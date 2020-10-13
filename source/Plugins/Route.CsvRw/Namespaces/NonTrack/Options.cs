using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private void ParseOptionCommand(OptionsCommand Command, string[] Arguments, double[] UnitOfLength, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			switch (Command)
			{
				case OptionsCommand.BlockLength:
				{
					double length = 25.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out length))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Length is invalid in Options.BlockLength at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						length = 25.0;
					}

					Data.BlockInterval = length;
				}
					break;
				case OptionsCommand.XParser:
					if (!PreviewOnly)
					{
						int parser = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out parser) | parser < 0 | parser > 3)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "XParser is invalid in Options.XParser at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
							{
								if (Plugin.CurrentHost.Plugins[i].Object != null)
								{
									Plugin.CurrentHost.Plugins[i].Object.SetObjectParser((XParsers) parser); //Remember that this will be ignored if not the X plugin!
								}
							}

						}
					}

					break;
				case OptionsCommand.ObjParser:
					if (!PreviewOnly)
					{
						int parser = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out parser) | parser < 0 | parser > 2)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ObjParser is invalid in Options.ObjParser at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
							{
								if (Plugin.CurrentHost.Plugins[i].Object != null)
								{
									Plugin.CurrentHost.Plugins[i].Object.SetObjectParser((ObjParsers) parser); //Remember that this will be ignored if not the Obj plugin!
								}
							}
						}
					}

					break;
				case OptionsCommand.UnitOfLength:
				case OptionsCommand.UnitOfSpeed:
				case OptionsCommand.ObjectVisibility:
					break;
				case OptionsCommand.SectionBehaviour:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int a;
						if (!NumberFormats.TryParseIntVb6(Arguments[0], out a))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a != 0 & a != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Data.ValueBasedSections = a == 1;
						}
					}

					break;
				case OptionsCommand.CantBehaviour:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int a;
						if (!NumberFormats.TryParseIntVb6(Arguments[0], out a))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a != 0 & a != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Data.SignedCant = a == 1;
						}
					}

					break;
				case OptionsCommand.FogBehaviour:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int a;
						if (!NumberFormats.TryParseIntVb6(Arguments[0], out a))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a != 0 & a != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Data.FogTransitionMode = a == 1;
						}
					}
					break;
			}
		}
	}
}
