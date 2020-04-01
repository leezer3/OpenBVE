using System;
using System.Globalization;
using System.Linq;
using System.Text;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private static void ParseSignalCommand(string Command, string[] Arguments, int Index, Encoding Encoding, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			switch (Command)
			{
				case "signal":
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 2 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments[0].EndsWith(".animated", StringComparison.OrdinalIgnoreCase))
							{
								if (Path.ContainsInvalidChars(Arguments[0]))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "AnimatedObjectFile contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									if (Arguments.Length > 1)
									{
										Program.CurrentHost.AddMessage(MessageType.Warning, false, Command + " is expected to have exactly 1 argument when using animated objects at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}

									string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
									if (!System.IO.File.Exists(f))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, true, "SignalFileWithoutExtension " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									else
									{
										UnifiedObject obj;
										Program.CurrentHost.LoadObject(f, Encoding, out obj);
										if (obj is AnimatedObjectCollection)
										{
											AnimatedObjectSignalData Signal = new AnimatedObjectSignalData();
											Signal.Objects = obj;
											Data.Signals[Index] = Signal;
										}
										else
										{
											Program.CurrentHost.AddMessage(MessageType.Error, true, "GlowFileWithoutExtension " + f + " is not a valid animated object in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										}
									}
								}
							}
							else
							{
								if (Path.ContainsInvalidChars(Arguments[0]))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									if (Arguments.Length > 2)
									{
										Program.CurrentHost.AddMessage(MessageType.Warning, false, Command + " is expected to have between 1 and 2 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}

									string f = Arguments[0];
									try
									{
										if (!LocateObject(ref f, ObjectPath))
										{
											string testPath = Path.CombineFile(ObjectPath, f);

											if (Program.CurrentHost.DetermineStaticObjectExtension(ref testPath))
											{
												f = testPath;
											}
											else
											{
												Program.CurrentHost.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension does not exist in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
												break;
											}
										}
									}
									catch
									{
										//NYCT-1 line has a comment containing SIGNAL, which is then misinterpreted by the parser here
										//Really needs commenting fixing, rather than hacks like this.....
										Program.CurrentHost.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension does not contain a valid path in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										break;
									}

									Bve4SignalData Signal = new Bve4SignalData
									{
										BaseObject = LoadStaticObject(f, Encoding, false),
										GlowObject = null
									};
									string Folder = System.IO.Path.GetDirectoryName(f);
									if (!System.IO.Directory.Exists(Folder))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, true, "The folder " + Folder + " could not be found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									else
									{
										Signal.SignalTextures = LoadAllTextures(f, false);
										Signal.GlowTextures = new OpenBveApi.Textures.Texture[] { };
										if (Arguments.Length >= 2 && Arguments[1].Length != 0)
										{
											if (Path.ContainsInvalidChars(Arguments[1]))
											{
												Program.CurrentHost.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
											}
											else
											{
												f = Arguments[1];
												bool glowFileFound = false;
												if (!System.IO.File.Exists(f) && System.IO.Path.HasExtension(f))
												{
													string ext = System.IO.Path.GetExtension(f);

													if (Program.CurrentHost.SupportedStaticObjectExtensions.Contains(ext.ToLowerInvariant()))
													{
														Program.CurrentHost.AddMessage(MessageType.Warning, false, "GlowFileWithoutExtension should not supply a file extension in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
														f = Path.CombineFile(ObjectPath, f);
														glowFileFound = true;
													}
													else if (Program.CurrentHost.SupportedAnimatedObjectExtensions.Contains(ext.ToLowerInvariant()))
													{
														Program.CurrentHost.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension must be a static object in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
													}
													else
													{
														Program.CurrentHost.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
													}
												}

												if (!System.IO.File.Exists(f) && !System.IO.Path.HasExtension(f))
												{
													string testPath = Path.CombineFile(ObjectPath, f);

													if (Program.CurrentHost.DetermineStaticObjectExtension(ref testPath))
													{
														f = testPath;
														glowFileFound = true;
													}
													else
													{
														Program.CurrentHost.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension does not exist in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
														break;
													}
												}

												if (glowFileFound)
												{
													Program.CurrentHost.LoadStaticObject(f, Encoding, false, out Signal.GlowObject);
													if (Signal.GlowObject != null)
													{
														Signal.GlowTextures = LoadAllTextures(f, true);
														for (int p = 0; p < Signal.GlowObject.Mesh.Materials.Length; p++)
														{
															Signal.GlowObject.Mesh.Materials[p].BlendMode = MeshMaterialBlendMode.Additive;
															Signal.GlowObject.Mesh.Materials[p].GlowAttenuationData = Glow.GetAttenuationData(200.0, GlowAttenuationMode.DivisionExponent4);
														}
													}
												}
											}
										}

										Data.Signals.Add(Index, Signal);
									}
								}
							}
						}
					}

					break;
			}
		}
	}
}
