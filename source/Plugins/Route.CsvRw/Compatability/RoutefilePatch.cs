using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		internal Dictionary<string, RoutefilePatch> availableRoutefilePatches = new Dictionary<string, RoutefilePatch>();
		private void CheckForAvailablePatch(string FileName, ref RouteData Data, ref Expression[] Expressions)
			{
				if (Plugin.CurrentOptions.EnableBveTsHacks == false)
				{
					return;
				}
				string fileHash = GetChecksum(FileName);
				if (availableRoutefilePatches.ContainsKey(fileHash))
				{
					RoutefilePatch patch = availableRoutefilePatches[fileHash];
					Data.LineEndingFix = patch.LineEndingFix;
					Data.IgnorePitchRoll = patch.IgnorePitchRoll;
					if (!string.IsNullOrEmpty(patch.LogMessage))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, patch.LogMessage);
					}
					CylinderHack = patch.CylinderHack;
					for (int i = 0; i < patch.ExpressionFixes.Count; i++)
					{
						Expressions[patch.ExpressionFixes.ElementAt(i).Key].Text = patch.ExpressionFixes.ElementAt(i).Value;
					}
					if (patch.XParser != null)
					{
						Plugin.CurrentOptions.CurrentXParser = (XParsers)patch.XParser;
					}
					Plugin.CurrentOptions.Derailments = patch.Derailments;
					Plugin.CurrentOptions.Toppling = patch.Toppling;
					foreach (int i in patch.DummyRailTypes)
					{
						Data.Structure.RailObjects.Add(i, new StaticObject(Plugin.CurrentHost));
					}
					foreach (int i in patch.DummyGroundTypes)
					{
						Data.Structure.Ground.Add(i, new StaticObject(Plugin.CurrentHost));
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

	}
}
