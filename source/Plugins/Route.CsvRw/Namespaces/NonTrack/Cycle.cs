using System;
using OpenBveApi.Interface;
using OpenBveApi.Math;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private void ParseCycleCommand(string Command, string[] Arguments, int Index, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			switch (Command)
			{
				case "ground":
					if (!PreviewOnly)
					{
						if (Index >= Data.Structure.Cycles.Length)
						{
							Array.Resize(ref Data.Structure.Cycles, Index + 1);
						}

						Data.Structure.Cycles[Index] = new int[Arguments.Length];
						for (int k = 0; k < Arguments.Length; k++)
						{
							int ix = 0;
							if (Arguments[k].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[k], out ix))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GroundStructureIndex " + (k + 1).ToString(Culture) + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								ix = 0;
							}

							if (ix < 0 | !Data.Structure.Ground.ContainsKey(ix))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GroundStructureIndex " + (k + 1).ToString(Culture) + " is out of range in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								ix = 0;
							}

							Data.Structure.Cycles[Index][k] = ix;
						}
					}

					break;
				// rail cycle
				case "rail":
					if (!PreviewOnly)
					{
						if (Index >= Data.Structure.RailCycles.Length)
						{
							Array.Resize(ref Data.Structure.RailCycles, Index + 1);
						}

						Data.Structure.RailCycles[Index] = new int[Arguments.Length];
						for (int k = 0; k < Arguments.Length; k++)
						{
							int ix = 0;
							if (Arguments[k].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[k], out ix))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex " + (k + 1).ToString(Culture) + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								ix = 0;
							}

							if (ix < 0 | !Data.Structure.RailObjects.ContainsKey(ix))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex " + (k + 1).ToString(Culture) + " is out of range in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								ix = 0;
							}

							Data.Structure.RailCycles[Index][k] = ix;
						}
					}

					break;
			}
		}
	}
}
