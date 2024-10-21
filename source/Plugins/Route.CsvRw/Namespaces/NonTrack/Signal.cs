using System;
using System.Linq;
using System.Text;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using RouteManager2.SignalManager;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private void ParseSignalCommand(string Command, string[] Arguments, int Index, Encoding Encoding, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			switch (Command)
			{
				case "signal":
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 2 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments[0].EndsWith(".animated", StringComparison.OrdinalIgnoreCase))
							{
								if (Path.ContainsInvalidChars(Arguments[0]))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "AnimatedObjectFile contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									if (Arguments.Length > 1)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, Command + " is expected to have exactly 1 argument when using animated objects at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}

									string f = Path.CombineFile(ObjectPath, Arguments[0]);
									if (!System.IO.File.Exists(f))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, true, "SignalFileWithoutExtension " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									else
									{
										Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
										if (obj is AnimatedObjectCollection)
										{
											AnimatedObjectSignalData Signal = new AnimatedObjectSignalData(obj);
											Data.Signals[Index] = Signal;
										}
										else
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, true, "GlowFileWithoutExtension " + f + " is not a valid animated object in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										}
									}
								}
							}
							else
							{
								if (Path.ContainsInvalidChars(Arguments[0]))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									if (Arguments.Length > 2)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, Command + " is expected to have between 1 and 2 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}

									string f = Arguments[0];
									try
									{
										if (!LocateObject(ref f, ObjectPath))
										{
											string testPath = Path.CombineFile(ObjectPath, f);

											if (Plugin.CurrentHost.DetermineStaticObjectExtension(ref testPath))
											{
												f = testPath;
											}
											else
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension does not exist in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
												break;
											}
										}
									}
									catch
									{
										//NYCT-1 line has a comment containing SIGNAL, which is then misinterpreted by the parser here
										//Really needs commenting fixing, rather than hacks like this.....
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension does not contain a valid path in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										break;
									}

									Bve4SignalData Signal = new Bve4SignalData
									{
										BaseObject = LoadStaticObject(f, Encoding, false),
										GlowObject = null
									};
									string Folder = Path.GetDirectoryName(f);
									if (!System.IO.Directory.Exists(Folder))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The folder " + Folder + " could not be found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									else
									{
										Signal.SignalTextures = LoadAllTextures(f, false);
										Signal.GlowTextures = new OpenBveApi.Textures.Texture[] { };
										if (Arguments.Length >= 2 && Arguments[1].Length != 0)
										{
											if (Path.ContainsInvalidChars(Arguments[1]))
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
											}
											else
											{
												f = Arguments[1];
												bool glowFileFound = false;
												if (!System.IO.File.Exists(f) && System.IO.Path.HasExtension(f))
												{
													string ext = System.IO.Path.GetExtension(f);

													if (Plugin.CurrentHost.SupportedStaticObjectExtensions.Contains(ext.ToLowerInvariant()))
													{
														Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "GlowFileWithoutExtension should not supply a file extension in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
														f = Path.CombineFile(ObjectPath, f);
														glowFileFound = true;
													}
													else if (Plugin.CurrentHost.SupportedAnimatedObjectExtensions.Contains(ext.ToLowerInvariant()))
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension must be a static object in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
													}
													else
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
													}
												}

												if (!System.IO.File.Exists(f) && !System.IO.Path.HasExtension(f))
												{
													string testPath = Path.CombineFile(ObjectPath, f);

													if (Plugin.CurrentHost.DetermineStaticObjectExtension(ref testPath))
													{
														f = testPath;
														glowFileFound = true;
													}
													else
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension does not exist in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
														break;
													}
												}

												if (glowFileFound)
												{
													Plugin.CurrentHost.LoadStaticObject(f, Encoding, false, out Signal.GlowObject);
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
