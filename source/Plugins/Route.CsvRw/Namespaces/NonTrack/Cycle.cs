using System;
using OpenBveApi.Interface;
using OpenBveApi.Math;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private void ParseCycleCommand(CycleCommand Command, string[] Arguments, int Index, Expression Expression, ref RouteData Data, bool previewOnly)
		{
			switch (Command)
			{
				case CycleCommand.Ground:
					if (!previewOnly)
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The index of " + (k + 1).ToString(Culture) + " is invalid in Cycle." + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								ix = 0;
							}

							if (ix < 0 | !Data.Structure.Ground.ContainsKey(ix))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GroundStructure with an index of " + ix + " is out of range in Cycle." + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								ix = 0;
							}

							Data.Structure.Cycles[Index][k] = ix;
						}
					}

					break;
				// rail cycle
				case CycleCommand.Rail:
					if (!previewOnly)
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The index of " + (k + 1).ToString(Culture) + " is invalid in Cycle." + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								ix = 0;
							}

							if (ix < 0 | !Data.Structure.RailObjects.ContainsKey(ix))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructure with an index of " + ix + " is out of range in Cycle." + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
