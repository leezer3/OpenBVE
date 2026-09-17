using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenBveApi.Objects;

namespace CsvRwRouteParser
{
	/// <summary>Identifies which Structure dictionary a pending object load targets.</summary>
	internal enum StructureTarget
	{
		RailObjects,
		Beacon,
		Pole,
		Ground,
		WallL,
		WallR,
		DikeL,
		DikeR,
		FormL,
		FormR,
		FormCL,
		FormCR,
		RoofL,
		RoofR,
		RoofCL,
		RoofCR,
		CrackL,
		CrackR,
		FreeObjects,
		WeatherObjects
	}

	/// <summary>A single deferred object load collected during Structure parsing.</summary>
	internal struct PendingObject
	{
		public StructureTarget Target;
		public int Index1;
		public int Index2;
		public string Path;
		public bool IsStatic;
		public bool PreserveVertices;
		public string TypeName;
	}

	/// <summary>A single deferred Cycle command, validated after objects are committed.</summary>
	internal struct PendingCycle
	{
		public CycleCommand Command;
		public string[] Arguments;
		public int Index;
		public int Line;
		public int Column;
		public string File;
	}

	internal partial class Parser
	{
		private readonly List<PendingObject> pendingObjects = new List<PendingObject>();
		private readonly List<PendingCycle> pendingCycles = new List<PendingCycle>();

		private void QueuePending(StructureTarget target, int index1, int index2, string path, bool isStatic, bool preserveVertices, string typeName)
		{
			pendingObjects.Add(new PendingObject
			{
				Target = target,
				Index1 = index1,
				Index2 = index2,
				Path = path,
				IsStatic = isStatic,
				PreserveVertices = preserveVertices,
				TypeName = typeName
			});
		}

		private void QueueCycle(CycleCommand command, string[] arguments, int index, Expression expression)
		{
			pendingCycles.Add(new PendingCycle
			{
				Command = command,
				Arguments = (string[])arguments.Clone(),
				Index = index,
				Line = expression.Line,
				Column = expression.Column,
				File = expression.File
			});
		}

		/// <summary>Replays deferred Cycle commands sequentially after objects are committed.</summary>
		private void CommitPendingCycles(bool previewOnly)
		{
			try
			{
				for (int i = 0; i < pendingCycles.Count; i++)
				{
					if (Plugin.Cancel)
					{
						Plugin.IsLoading = false;
						return;
					}
					PendingCycle cycle = pendingCycles[i];
					Expression expression = new Expression(cycle.File, string.Empty, cycle.Line, cycle.Column, 0.0);
					ParseCycleCommand(cycle.Command, cycle.Arguments, cycle.Index, expression, ref Data, previewOnly);
				}
			}
			finally
			{
				pendingCycles.Clear();
			}
		}

		/// <summary>Computes worker count from machine capability, reserving one thread for the loading screen on small machines.</summary>
		internal static int ComputeObjectLoadDop()
		{
			int cpu = Environment.ProcessorCount;
			if (cpu <= 4)
			{
				return Math.Max(1, cpu - 1);
			}
			return Math.Min(8, cpu);
		}

		/// <summary>Loads all pending structure objects in parallel, then commits them sequentially in file order.</summary>
		private void LoadAndCommitPendingObjects(System.Text.Encoding encoding, double progressBase, double progressSpan)
		{
			int n = pendingObjects.Count;
			if (n == 0 || Plugin.Cancel)
			{
				pendingObjects.Clear();
				return;
			}

			try
			{
				// Dedupe by file: each unique file loads once, clones are handed out below.
				List<PendingObject> uniqueObjects = new List<PendingObject>();
				Dictionary<ValueTuple<string, bool, bool>, int> uniqueIndex = new Dictionary<ValueTuple<string, bool, bool>, int>();
				int[] prototypeIndex = new int[n];
				for (int i = 0; i < n; i++)
				{
					PendingObject pending = pendingObjects[i];
					ValueTuple<string, bool, bool> key = ValueTuple.Create(pending.Path.ToLowerInvariant(), pending.IsStatic, pending.PreserveVertices);
					if (!uniqueIndex.TryGetValue(key, out int u))
					{
						u = uniqueObjects.Count;
						uniqueIndex.Add(key, u);
						uniqueObjects.Add(pending);
					}
					prototypeIndex[i] = u;
				}

				int m = uniqueObjects.Count;
				UnifiedObject[] prototypes = new UnifiedObject[m];
				int dop = ComputeObjectLoadDop();
				long done = 0;

				if (dop <= 1 || m < 2)
				{
					for (int u = 0; u < m && !Plugin.Cancel; u++)
					{
						prototypes[u] = LoadSinglePending(uniqueObjects[u], encoding);
						ReportLoadProgress(ref done, m, progressBase, progressSpan);
					}
				}
				else
				{
					// Workers handle distinct files via thread-safe host loads.
					ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = dop };
					Parallel.For(0, m, options, u =>
					{
						if (Plugin.Cancel)
						{
							return;
						}
						prototypes[u] = LoadSinglePending(uniqueObjects[u], encoding);
						ReportLoadProgress(ref done, m, progressBase, progressSpan);
					});
				}

				if (Plugin.Cancel)
				{
					Plugin.IsLoading = false;
					return;
				}

				// Commit in file order; each occurrence gets its own clone.
				for (int i = 0; i < n; i++)
				{
					UnifiedObject prototype = prototypes[prototypeIndex[i]];
					if (prototype == null)
					{
						continue;
					}
					UnifiedObject obj;
					try
					{
						obj = prototype.Clone();
					}
					catch
					{
						continue;
					}
					CommitPending(pendingObjects[i], obj);
				}
			}
			finally
			{
				pendingObjects.Clear();
			}
		}

		/// <summary>Reports one completed load.</summary>
		private void ReportLoadProgress(ref long done, int total, double progressBase, double progressSpan)
		{
			long count = Interlocked.Increment(ref done);
			Plugin.CurrentProgress = progressBase + progressSpan * count / total;
		}

		private UnifiedObject LoadSinglePending(PendingObject pending, System.Text.Encoding encoding)
		{
			try
			{
				if (pending.IsStatic)
				{
					if (Plugin.CurrentHost.LoadStaticObject(pending.Path, encoding, pending.PreserveVertices, out StaticObject staticObject))
					{
						return staticObject;
					}
					return null;
				}
				if (Plugin.CurrentHost.LoadObject(pending.Path, encoding, out UnifiedObject obj))
				{
					return obj;
				}
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private void CommitPending(PendingObject pending, UnifiedObject obj)
		{
			switch (pending.Target)
			{
				case StructureTarget.RailObjects:
					Data.Structure.RailObjects.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.Beacon:
					Data.Structure.Beacon.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.Pole:
					if (!Data.Structure.Poles.ContainsKey(pending.Index1))
					{
						Data.Structure.Poles.Add(pending.Index1, new ObjectDictionary());
					}
					bool overwriteDefault = pending.Index2 >= 0 && pending.Index2 >= 3;
					Data.Structure.Poles[pending.Index1].Add(pending.Index2, obj, overwriteDefault);
					break;
				case StructureTarget.Ground:
					Data.Structure.Ground.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.WallL:
					Data.Structure.WallL.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.WallR:
					Data.Structure.WallR.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.DikeL:
					Data.Structure.DikeL.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.DikeR:
					Data.Structure.DikeR.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.FormL:
					Data.Structure.FormL.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.FormR:
					Data.Structure.FormR.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.FormCL:
					Data.Structure.FormCL.Add(pending.Index1, (StaticObject)obj, pending.TypeName);
					break;
				case StructureTarget.FormCR:
					Data.Structure.FormCR.Add(pending.Index1, (StaticObject)obj, pending.TypeName);
					break;
				case StructureTarget.RoofL:
					Data.Structure.RoofL.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.RoofR:
					Data.Structure.RoofR.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.RoofCL:
					Data.Structure.RoofCL.Add(pending.Index1, (StaticObject)obj, pending.TypeName);
					break;
				case StructureTarget.RoofCR:
					Data.Structure.RoofCR.Add(pending.Index1, (StaticObject)obj, pending.TypeName);
					break;
				case StructureTarget.CrackL:
					Data.Structure.CrackL.Add(pending.Index1, (StaticObject)obj, pending.TypeName);
					break;
				case StructureTarget.CrackR:
					Data.Structure.CrackR.Add(pending.Index1, (StaticObject)obj, pending.TypeName);
					break;
				case StructureTarget.FreeObjects:
					Data.Structure.FreeObjects.Add(pending.Index1, obj, pending.TypeName);
					break;
				case StructureTarget.WeatherObjects:
					Data.Structure.WeatherObjects.Add(pending.Index1, obj, pending.TypeName);
					break;
			}
		}
	}
}
