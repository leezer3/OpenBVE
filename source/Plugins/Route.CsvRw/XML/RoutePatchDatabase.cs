using Formats.OpenBve;
using Formats.OpenBve.XML;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Path = OpenBveApi.Path;

namespace CsvRwRouteParser
{
	internal class RoutePatchDatabaseParser
	{
		internal static void LoadRoutePatchDatabase(ref Dictionary<string, RoutefilePatch> routePatches, string databaseFile = "")
		{
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			
			if (databaseFile == string.Empty)
			{
				databaseFile = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility\\RoutePatches"), "database.xml");
				if (!File.Exists(databaseFile))
				{
					return;
				}
			}
			XMLFile<PatchDatabaseSection, PatchDatabaseKey> xmlFile = new XMLFile<PatchDatabaseSection, PatchDatabaseKey>(databaseFile, "/openBVE/RoutePatches", Plugin.CurrentHost);

			while (xmlFile.RemainingSubBlocks > 0)
			{
				Block<PatchDatabaseSection, PatchDatabaseKey> subBlock = xmlFile.ReadNextBlock();
				while (subBlock.RemainingSubBlocks > 0)
				{
					Block<PatchDatabaseSection, PatchDatabaseKey> patchBlock = subBlock.ReadNextBlock();
					ParsePatchNode(patchBlock, ref routePatches);
				}
				
			}
			currentXML.Load(databaseFile);
		}

		internal static void ParsePatchNode(Block<PatchDatabaseSection, PatchDatabaseKey> patchBlock, ref Dictionary<string, RoutefilePatch> routeFixes)
		{
			RoutefilePatch currentPatch = new RoutefilePatch();
			patchBlock.GetValue(PatchDatabaseKey.Hash, out string currentHash);
			patchBlock.GetValue(PatchDatabaseKey.FileName, out currentPatch.FileName);
			patchBlock.GetValue(PatchDatabaseKey.LineEndingFix, out currentPatch.LineEndingFix);
			patchBlock.GetValue(PatchDatabaseKey.ColonFix, out currentPatch.ColonFix);
			patchBlock.GetValue(PatchDatabaseKey.IgnorePitchRoll, out currentPatch.IgnorePitchRoll);
			patchBlock.GetValue(PatchDatabaseKey.LogMessage, out currentPatch.LogMessage);
			patchBlock.GetValue(PatchDatabaseKey.CylinderHack, out currentPatch.CylinderHack);
			patchBlock.GetValue(PatchDatabaseKey.Derailments, out currentPatch.Derailments);
			patchBlock.GetValue(PatchDatabaseKey.Toppling, out currentPatch.Toppling);
			patchBlock.GetValue(PatchDatabaseKey.AccurateObjectDisposal, out currentPatch.AccurateObjectDisposal);
			patchBlock.GetValue(PatchDatabaseKey.SplitLineHack, out currentPatch.SplitLineHack);
			patchBlock.GetValue(PatchDatabaseKey.AllowTrackPositionArguments, out currentPatch.AllowTrackPositionArguments);
			patchBlock.GetValue(PatchDatabaseKey.DisableSemiTransparentFaces, out currentPatch.DisableSemiTransparentFaces);
			patchBlock.GetValue(PatchDatabaseKey.ReducedColorTransparency, out currentPatch.ReducedColorTransparency);
			patchBlock.TryGetValue(PatchDatabaseKey.ViewingDistance, ref currentPatch.ViewingDistance, NumberRange.Positive);
			patchBlock.TryGetValue(PatchDatabaseKey.MaxViewingDistance, ref currentPatch.MaxViewingDistance, NumberRange.Positive);
			patchBlock.GetValue(PatchDatabaseKey.AggressiveRWBrackets, out currentPatch.AggressiveRwBrackets);
			patchBlock.GetValue(PatchDatabaseKey.Incompatible, out currentPatch.Incompatible);
			patchBlock.GetValue(PatchDatabaseKey.DelayedAnimatedUpdates, out currentPatch.DelayedAnimatedUpdates);
			patchBlock.GetValue(PatchDatabaseKey.AdhesionHack, out currentPatch.AdhesionHack);
			if (patchBlock.GetEnumValue(PatchDatabaseKey.XParser, out XParsers parser))
			{
				currentPatch.XParser = parser;
			}
			patchBlock.TryGetIntArray(PatchDatabaseKey.DummyRailTypes, ',', ref currentPatch.DummyRailTypes);
			patchBlock.TryGetIntArray(PatchDatabaseKey.DummyGroundTypes, ',', ref currentPatch.DummyGroundTypes);
			if (patchBlock.GetValue(PatchDatabaseKey.SignalSet, out string signalSet))
			{
				string signalFile = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility\\Signals"), signalSet);
				if (File.Exists(signalFile))
				{
					currentPatch.CompatibilitySignalSet = signalFile;
				}
			}

			while (patchBlock.RemainingIndexedValues > 0)
			{
				patchBlock.GetIndexedValue(out int expressionNumber, out string text);
				currentPatch.ExpressionFixes.Add(expressionNumber, text);
			}

			if (!routeFixes.ContainsKey(currentHash))
			{
				routeFixes.Add(currentHash, currentPatch);
			}
			else
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The RoutePatches database contains a duplicate entry with hash " + currentHash);
			}
		}
	}
}
