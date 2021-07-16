using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using RouteManager2.SignalManager;
using CompatabilityHacks = OpenBveApi.Textures.CompatabilityHacks;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		internal Dictionary<string, RoutefilePatch> availableRoutefilePatches = new Dictionary<string, RoutefilePatch>();

		private void CheckForAvailablePatch(string FileName, ref RouteData Data, ref Expression[] Expressions, bool PreviewOnly)
		{
			if (Plugin.CurrentOptions.EnableBveTsHacks == false)
			{
				return;
			}

			string fileHash = Path.GetChecksum(FileName);
			if (availableRoutefilePatches.ContainsKey(fileHash))
			{
				RoutefilePatch patch = availableRoutefilePatches[fileHash];
				if (patch.Incompatible)
				{
					throw new Exception("This routefile is incompatible with OpenBVE: " + Environment.NewLine + Environment.NewLine + patch.LogMessage);
				}
				Data.LineEndingFix = patch.LineEndingFix;
				Data.IgnorePitchRoll = patch.IgnorePitchRoll;
				if (!string.IsNullOrEmpty(patch.LogMessage))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, patch.LogMessage);
				}

				EnabledHacks.CylinderHack = patch.CylinderHack;
				EnabledHacks.DisableSemiTransparentFaces = patch.DisableSemiTransparentFaces;
				Data.AccurateObjectDisposal = patch.AccurateObjectDisposal;
				for (int i = 0; i < patch.ExpressionFixes.Count; i++)
				{
					Expressions[patch.ExpressionFixes.ElementAt(i).Key].Text = patch.ExpressionFixes.ElementAt(i).Value;
				}

				if (patch.XParser != null)
				{
					Plugin.CurrentOptions.CurrentXParser = (XParsers) patch.XParser;
				}

				Plugin.CurrentOptions.Derailments = patch.Derailments;
				Plugin.CurrentOptions.Toppling = patch.Toppling;
				SplitLineHack = patch.SplitLineHack;
				AllowTrackPositionArguments = patch.AllowTrackPositionArguments;
				foreach (int i in patch.DummyRailTypes)
				{
					if (Data.Structure.RailObjects == null)
					{
						Data.Structure.RailObjects = new ObjectDictionary();
					}
					Data.Structure.RailObjects.Add(i, new StaticObject(Plugin.CurrentHost));
				}
				foreach (int i in patch.DummyGroundTypes)
				{
					if (Data.Structure.Ground == null)
					{
						Data.Structure.Ground = new ObjectDictionary();
					}
					Data.Structure.Ground.Add(i, new StaticObject(Plugin.CurrentHost));
				}

				if (!string.IsNullOrEmpty(patch.CompatibilitySignalSet) && !PreviewOnly)
				{
					CompatibilitySignalObject.ReadCompatibilitySignalXML(Plugin.CurrentHost, patch.CompatibilitySignalSet, out Data.CompatibilitySignals, out CompatibilityObjects.SignalPost, out Data.SignalSpeeds);
				}

				if (patch.ReducedColorTransparency)
				{
					for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
					{
						CompatabilityHacks hacks = new CompatabilityHacks { ReduceTransparencyColorDepth = true };
						if (Plugin.CurrentHost.Plugins[i].Texture != null)
						{
							Plugin.CurrentHost.Plugins[i].Texture.SetCompatabilityHacks(hacks);
						}
					}
				}

				if (patch.ViewingDistance != int.MaxValue)
				{
					Plugin.CurrentOptions.ViewingDistance = patch.ViewingDistance;
				}
			}
		}
	}

	/// <summary>Describes a set of patches to be applied to a routefile</summary>
	internal class RoutefilePatch
	{
		/// <summary>The file name</summary>
		internal string FileName;
		/// <summary>Whether line endings are to be fixed</summary>
		internal bool LineEndingFix = false;
		/// <summary>Whether the pitch / roll parameters are to be ignored</summary>
		/// <remarks>These were added by later versions of BVE2 / BVE4, and some files may have comments in this space</remarks>
		internal bool IgnorePitchRoll = false;
		/// <summary>The log message to be added when applying this fix</summary>
		internal string LogMessage;
		/// <summary>Whether the cylinder hack is to be applied</summary>
		/// <remarks>BVE2 / BVE4 constructed the internals of cylinders differently, used by some tunnels</remarks>
		internal bool CylinderHack;
		/// <summary>The list of expression fixes to be added</summary>
		internal Dictionary<int, string> ExpressionFixes = new Dictionary<int, string>();
		/// <summary>The X Parser to use</summary>
		/// <remarks>Set to null to retain the current selection</remarks>
		internal XParsers? XParser = null;
		/// <summary>Whether derailments are enabled</summary>
		internal bool Derailments = false;
		/// <summary>Whether toppling is enabled</summary>
		internal bool Toppling = false;
		/// <summary>Dummy railtypes to be added to fix broken cycles</summary>
		internal List<int> DummyRailTypes = new List<int>();
		/// <summary>Dummy ground types to be added to fix broken cycles</summary>
		internal List<int> DummyGroundTypes = new List<int>();
		/// <summary>The compatability signal file to use</summary>
		internal string CompatibilitySignalSet;
		/// <summary>Forces accurate object disposal on or off</summary>
		internal bool AccurateObjectDisposal;
		/// <summary>Whether certain lines should be split</summary>
		internal bool SplitLineHack;
		/// <summary>Allows arguments after track positions</summary>
		/// <remarks>Some files use these as comments</remarks>
		internal bool AllowTrackPositionArguments;
		/// <summary>Disables semi-transparent faces</summary>
		internal bool DisableSemiTransparentFaces;
		/// <summary>Whether reduced color transparency should be used</summary>
		internal bool ReducedColorTransparency;
		/// <summary>The viewing distance to use</summary>
		internal int ViewingDistance = int.MaxValue;
		/// <summary>Whether the route is incompatible with OpenBVE</summary>
		internal bool Incompatible = false;
	}
}
