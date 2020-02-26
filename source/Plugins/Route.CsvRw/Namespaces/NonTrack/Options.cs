using System.Globalization;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private static void ParseOptionCommand(string Command, string[] Arguments, double[] UnitOfLength, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			switch (Command)
			{
				case "options.blocklength":
				{
					double length = 25.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out length))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Length is invalid in Options.BlockLength at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						length = 25.0;
					}

					Data.BlockInterval = length;
				}
					break;
				case "options.xparser":
					if (!PreviewOnly)
					{
						int parser = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out parser) | parser < 0 | parser > 3)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "XParser is invalid in Options.XParser at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
							{
								if (Program.CurrentHost.Plugins[i].Object != null)
								{
									Program.CurrentHost.Plugins[i].Object.SetObjectParser((XParsers) parser); //Remember that this will be ignored if not the X plugin!
								}
							}

						}
					}

					break;
				case "options.objparser":
					if (!PreviewOnly)
					{
						int parser = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out parser) | parser < 0 | parser > 2)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ObjParser is invalid in Options.ObjParser at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
							{
								if (Program.CurrentHost.Plugins[i].Object != null)
								{
									Program.CurrentHost.Plugins[i].Object.SetObjectParser((ObjParsers) parser); //Remember that this will be ignored if not the Obj plugin!
								}
							}
						}
					}

					break;
				case "options.unitoflength":
				case "options.unitofspeed":
				case "options.objectvisibility":
					break;
				case "options.sectionbehavior":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int a;
						if (!NumberFormats.TryParseIntVb6(Arguments[0], out a))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a != 0 & a != 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Data.ValueBasedSections = a == 1;
						}
					}

					break;
				case "options.cantbehavior":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int a;
						if (!NumberFormats.TryParseIntVb6(Arguments[0], out a))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a != 0 & a != 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Data.SignedCant = a == 1;
						}
					}

					break;
				case "options.fogbehavior":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int a;
						if (!NumberFormats.TryParseIntVb6(Arguments[0], out a))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a != 0 & a != 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
