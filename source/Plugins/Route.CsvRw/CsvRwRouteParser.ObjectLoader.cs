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

	internal partial class Parser
	{
		private readonly List<PendingObject> pendingObjects = new List<PendingObject>();

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
				UnifiedObject[] results = new UnifiedObject[n];
				int dop = ComputeObjectLoadDop();
				long done = 0;
				long progressMax = 0;
				object progressLock = new object();

				System.Action reportOne = () =>
				{
					long count = Interlocked.Increment(ref done);
					// Workers finish out of order; only advance the bar monotonically.
					lock (progressLock)
					{
						if (count > progressMax)
						{
							progressMax = count;
							Plugin.CurrentProgress = progressBase + progressSpan * progressMax / n;
						}
					}
				};

				if (dop <= 1 || n < 2)
				{
					for (int i = 0; i < n; i++)
					{
						if (Plugin.Cancel)
						{
							Plugin.IsLoading = false;
							return;
						}
						results[i] = LoadSinglePending(pendingObjects[i], encoding);
						reportOne();
					}
				}
				else
				{
					// Thread-safety note: workers only call Host.LoadObject/LoadStaticObject,
					// whose shared state (object caches, failure sets, texture registration,
					// log messages) is guarded by fine-grained locks. Built-in
					// object plugins (CsvB3d/DirectX/Wavefront/Animated) are stateless per
					// call (per-file locals, read-only shared config). Third-party native
					// object plugins have no declared thread-safety contract; if one ever
					// proves non-reentrant, gate it back to the sequential path above.
					ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = dop };
					try
					{
						Parallel.For(0, n, options, i =>
						{
							if (Plugin.Cancel)
							{
								return;
							}
							results[i] = LoadSinglePending(pendingObjects[i], encoding);
							reportOne();
						});
					}
					catch (OperationCanceledException)
					{
						// Cancelled via plugin flag; handled below.
					}
				}

				if (Plugin.Cancel)
				{
					Plugin.IsLoading = false;
					return;
				}

				// Sequential commit in file order preserves "last declaration wins" duplicate semantics.
				for (int i = 0; i < n; i++)
				{
					if (results[i] == null)
					{
						continue;
					}
					CommitPending(pendingObjects[i], results[i]);
				}
			}
			finally
			{
				pendingObjects.Clear();
			}
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
