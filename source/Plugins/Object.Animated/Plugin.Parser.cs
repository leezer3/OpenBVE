using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Input;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;

namespace Plugin
{
	public partial class Plugin
	{
		/// <summary>Loads a collection of animated objects from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <returns>The collection of animated objects.</returns>
		private static AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding)
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			AnimatedObjectCollection Result = new AnimatedObjectCollection(currentHost)
			{
				Objects = new AnimatedObject[4],
				Sounds = new WorldObject[4]
			};
			int ObjectCount = 0;
			int SoundCount = 0;
			// load file
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			bool rpnUsed = false;
			for (int i = 0; i < Lines.Length; i++)
			{
				int j = Lines[i].IndexOf(';');
				//Trim out comments
				Lines[i] = j >= 0 ? Lines[i].Substring(0, j).Trim(new char[] { }) : Lines[i].Trim(new char[] { });
				//Test whether RPN functions have been used
				rpnUsed = Lines[i].IndexOf("functionrpn", StringComparison.OrdinalIgnoreCase) >= 0;
			}
			if (rpnUsed)
			{
				currentHost.AddMessage(MessageType.Error, false, "An animated object file contains RPN functions. These were never meant to be used directly, only for debugging. They won't be supported indefinately. Please get rid of them in file " + FileName);
			}
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Length != 0)
				{
					string sct = Lines[i].Trim().Trim('[', ']');
					Enum.TryParse(sct, true, out AnimatedSection Section);
					switch (Section)
					{
						case AnimatedSection.Include:
							{
								i++;
								Vector3 position = Vector3.Zero;
								UnifiedObject[] obj = new UnifiedObject[4];
								int objCount = 0;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									if (Lines[i].Length != 0)
									{
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j > 0)
										{
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											if (!Enum.TryParse(a, true, out AnimatedKey key))
											{
												currentHost.AddMessage(MessageType.Error, false, "Unknown key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
												i++;
												continue;
											}
											switch (key)
											{
												case AnimatedKey.Position:
													position = ParseVector3(b, key, i, Section, FileName);
													break;
												default:
													currentHost.AddMessage(MessageType.Error, false, "The attribute " + key + " is not supported in a " + Section + " section at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													break;
											}
										}
										else
										{
											string Folder = Path.GetDirectoryName(FileName);
											if (Path.ContainsInvalidChars(Lines[i]))
											{
												currentHost.AddMessage(MessageType.Error, false, Lines[i] + " contains illegal characters at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
											}
											else
											{
												string file = Path.CombineFile(Folder, Lines[i]);
												if (System.IO.File.Exists(file))
												{
													if (obj.Length == objCount)
													{
														Array.Resize(ref obj, obj.Length << 1);
													}
													currentHost.LoadObject(file, Encoding, out obj[objCount]);
													objCount++;
												}
												else
												{
													currentHost.AddMessage(MessageType.Error, true, "File " + file + " not found at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
												}
											}
										}
									}
									i++;
								}
								i--;
								for (int j = 0; j < objCount; j++)
								{
									if (obj[j] != null)
									{
										if (obj[j] is StaticObject)
										{
											StaticObject s = (StaticObject)obj[j];
											s.Dynamic = true;
											if (ObjectCount >= Result.Objects.Length)
											{
												Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
											}
											AnimatedObject a = new AnimatedObject(currentHost);
											ObjectState aos = new ObjectState
											{
												Prototype = s,
												Translation = Matrix4D.CreateTranslation(position.X, position.Y, -position.Z)
											};
											a.States = new[] { aos };
											Result.Objects[ObjectCount] = a;
											ObjectCount++;
										}
										else if (obj[j] is AnimatedObjectCollection)
										{
											AnimatedObjectCollection a = (AnimatedObjectCollection)obj[j].Clone();
											for (int k = 0; k < a.Objects.Length; k++)
											{
												if (ObjectCount >= Result.Objects.Length)
												{
													Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
												}
												for (int h = 0; h < a.Objects[k].States.Length; h++)
												{
													a.Objects[k].States[h].Translation *= Matrix4D.CreateTranslation(position.X, position.Y, -position.Z);
												}
												Result.Objects[ObjectCount] = a.Objects[k];
												ObjectCount++;
											}
											for (int kk = 0; kk < a.Sounds.Length; kk++)
											{
												if (SoundCount >= Result.Sounds.Length)
												{
													Array.Resize(ref Result.Sounds, Result.Sounds.Length << 1);
												}
												Result.Sounds[SoundCount] = a.Sounds[kk];
												SoundCount++;
											}
										}
									}
								}
							}
							break;
						case AnimatedSection.Curve:
							{
								i++;
								double radius = 0;
								int segments = 1;
								double segmentLength = 0;
								UnifiedObject[] obj = new UnifiedObject[4];
								int objCount = 0;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									if (Lines[i].Length != 0)
									{
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j > 0)
										{
											string a = Lines[i].Substring(0, j).TrimEnd(new char[] { });
											string b = Lines[i].Substring(j + 1).TrimStart(new char[] { });
											switch (a.ToLowerInvariant())
											{
												case "radius":
													if (!double.TryParse(b, NumberStyles.Float, Culture, out radius))
													{
														currentHost.AddMessage(MessageType.Error, false, "Invalid curved object radius " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "segments":
													if (!int.TryParse(b, NumberStyles.Float, Culture, out segments))
													{
														currentHost.AddMessage(MessageType.Error, false, "Invalid curved object radius " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "segmentlength":
													if (!double.TryParse(b, NumberStyles.Float, Culture, out segmentLength))
													{
														currentHost.AddMessage(MessageType.Error, false, "Invalid curved object radius " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												default:
													currentHost.AddMessage(MessageType.Error, false, "The attribute " + a + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										}
										else
										{
											string Folder = System.IO.Path.GetDirectoryName(FileName);
											if (OpenBveApi.Path.ContainsInvalidChars(Lines[i]))
											{
												currentHost.AddMessage(MessageType.Error, false, Lines[i] + " contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											}
											else
											{
												string file = OpenBveApi.Path.CombineFile(Folder, Lines[i]);
												if (System.IO.File.Exists(file))
												{
													if (obj.Length == objCount)
													{
														Array.Resize<UnifiedObject>(ref obj, obj.Length << 1);
													}
													currentHost.LoadObject(file, Encoding, out obj[objCount]);
													objCount++;
												}
												else
												{
													currentHost.AddMessage(MessageType.Error, true, "File " + file + " not found at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												}
											}
										}
									}
									i++;
								}
								i--;
								for (int j = 0; j < objCount; j++)
								{
									if (obj[j] != null)
									{
										if (obj[j] is StaticObject)
										{
											StaticObject s = (StaticObject)obj[j];
											if (segmentLength == 0)
											{
												double min = 0, max = 0;
												for (int k = 0; k < s.Mesh.Vertices.Length; k++)
												{
													if (s.Mesh.Vertices[k].Coordinates.Z < min)
													{
														min = s.Mesh.Vertices[k].Coordinates.Z;
													}
													if (s.Mesh.Vertices[k].Coordinates.Z > max)
													{
														max = s.Mesh.Vertices[k].Coordinates.Z;
													}
												}

												segmentLength = max - min;
												if (segmentLength == 0)
												{
													currentHost.AddMessage(MessageType.Error, false, "Zero length segment supplied at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													continue;
												}
											}

											for (int k = 0; k < segments; k++)
											{
												StaticObject ss = (StaticObject) s.Clone();
												ss.Dynamic = true;
												ss.ApplyTranslation(0,0,k * segmentLength);
												ss.ApplyCurve(radius);
												if (ObjectCount >= Result.Objects.Length)
												{
													Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
												}
												AnimatedObject a = new AnimatedObject(currentHost);
												ObjectState aos = new ObjectState
												{
													Prototype = ss,
													Translation = Matrix4D.CreateTranslation(Vector3.Zero)
												};
												a.States = new ObjectState[] { aos };
												Result.Objects[ObjectCount] = a;
												ObjectCount++;
											}
											//s = s.CreateCurvedObject(segments, 5.0, radius);
											
										}
										else if (obj[j] is AnimatedObjectCollection)
										{
											currentHost.AddMessage(MessageType.Error, false, "Unable to create a curved object from an animated object collection at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									}
								}
							}
							break;
						case AnimatedSection.Object:
							{
								i++;
								if (Result.Objects.Length == ObjectCount)
								{
									Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
								}
								Result.Objects[ObjectCount] = new AnimatedObject(currentHost)
								{
									CurrentState = -1,
									TranslateXDirection = Vector3.Right,
									TranslateYDirection = Vector3.Down,
									TranslateZDirection = Vector3.Forward,
									RotateXDirection = Vector3.Right,
									RotateYDirection = Vector3.Down,
									RotateZDirection = Vector3.Forward,
									TextureShiftXDirection = Vector2.Right,
									TextureShiftYDirection = Vector2.Down,
									RefreshRate = 0.0,
								};
								Vector3 Position = Vector3.Zero;
								double RotateX = 0;
								bool StaticXRotation = false;
								double RotateY = 0;
								bool StaticYRotation = false;
								double RotateZ = 0;
								bool StaticZRotation = false;
								bool timetableUsed = false;
								string[] StateFiles = null;
								string StateFunctionRpn = null;
								bool StateFunctionIsPostfix = false;
								int StateFunctionLine = -1;
								Vector3 Scale = Vector3.One;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									if (Lines[i].Length != 0)
									{
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j > 0)
										{
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											if(!Enum.TryParse(a, true, out AnimatedKey key))
											{
												currentHost.AddMessage(MessageType.Error, false, "Unknown key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
												i++;
												continue;
											}
											switch (key)
											{
												case AnimatedKey.Position:
													Position = ParseVector3(b, key, i, Section, FileName);
													break;
												case AnimatedKey.States:
													{
														string[] s = b.Split(',');
														if (s.Length >= 1)
														{
															string Folder = Path.GetDirectoryName(FileName);
															StateFiles = new string[s.Length];
															bool NullObject = true;
															for (int k = 0; k < s.Length; k++)
															{
																s[k] = s[k].Trim(new char[] { });
																if (s[k].Length == 0)
																{
																	if (k == s.Length - 1)
																	{
																		// empty final argument, so assume misplaced final coma as empty arguments just get ignored
																		Array.Resize(ref s, s.Length - 1);
																	}
																	else
																	{
																		currentHost.AddMessage(MessageType.Error, false, "File " + k.ToString(Culture) + " is an empty string - did you mean something else? - in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
																		StateFiles[k] = null;	
																	}
																	
																}
																else if (Path.ContainsInvalidChars(s[k]))
																{
																	currentHost.AddMessage(MessageType.Error, false, "File " + k.ToString(Culture) + " contains illegal characters in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
																	StateFiles[k] = null;
																}
																else
																{
																	StateFiles[k] = Path.CombineFile(Folder, s[k]);
																	if (!System.IO.File.Exists(StateFiles[k]))
																	{
																		currentHost.AddMessage(MessageType.Error, true, "File " + StateFiles[k] + " not found in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
																		StateFiles[k] = null;
																	}
																}
																if (StateFiles[k] != null)
																{
																	NullObject = false;
																}	
															}
															if (NullObject && Result.Objects[0] == null)
															{
																currentHost.AddMessage(MessageType.Error, false, "None of the specified files were found in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
																return null;
															}
														}
														else
														{
															currentHost.AddMessage(MessageType.Error, false, "At least one argument is expected in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															return null;
														}
													} break;
												case AnimatedKey.StateFunction:
												case AnimatedKey.StateFunctionRPN:
													StateFunctionLine = i;
													StateFunctionRpn = b;
													StateFunctionIsPostfix = key == AnimatedKey.StateFunctionRPN;
													break;
												case AnimatedKey.TranslateXDirection:
												case AnimatedKey.TranslateYDirection:
												case AnimatedKey.TranslateZDirection:
													switch (key)
													{
														case AnimatedKey.TranslateXDirection:
															Result.Objects[ObjectCount].TranslateXDirection = ParseVector3(b, key, i, Section, FileName);
															break;
														case AnimatedKey.TranslateYDirection:
															Result.Objects[ObjectCount].TranslateYDirection = ParseVector3(b, key, i, Section, FileName);
															break;
														case AnimatedKey.TranslateZDirection:
															Result.Objects[ObjectCount].TranslateZDirection = ParseVector3(b, key, i, Section, FileName);
															break;
													}
													break;
												case AnimatedKey.TranslateXFunction:
												case AnimatedKey.TranslateXFunctionRPN:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out double X))
														{
															Position.X = X;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateXFunction = new FunctionScript(currentHost, b, key == AnimatedKey.TranslateXFunction);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.TranslateXScript:
													try
													{
														Result.Objects[ObjectCount].TranslateXFunction = new CSAnimationScript(currentHost, 
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.TranslateYFunction:
												case AnimatedKey.TranslateYFunctionRPN:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out double Y))
														{
															Position.Y = Y;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateYFunction = new FunctionScript(currentHost, b, key == AnimatedKey.TranslateYFunction);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.TranslateYScript:
													try
													{
														Result.Objects[ObjectCount].TranslateYFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.TranslateZFunction:
												case AnimatedKey.TranslateZFunctionRPN:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out double Z))
														{
															Position.Z = Z;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateZFunction = new FunctionScript(currentHost, b, key == AnimatedKey.TranslateZFunction);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.TranslateZScript:
													try
													{
														Result.Objects[ObjectCount].TranslateZFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.TrackFollowerFunction:
													try
													{
														Result.Objects[ObjectCount].TrackFollowerFunction = new FunctionScript(currentHost, b, true);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.Axles:
													try
													{
														var splitValue = b.Split(',');
														if (!double.TryParse(splitValue[0], out double FrontAxlePosition))
														{
															currentHost.AddMessage(MessageType.Error, false,"Invalid FrontAxlePosition in " + key + " at line " + (i + 1).ToString(Culture) + " in file " +FileName);
														}
														if(!double.TryParse(splitValue[1], out double RearAxlePosition))
														{
															currentHost.AddMessage(MessageType.Error, false,"Invalid RearAxlePosition in " + key + " at line " + (i + 1).ToString(Culture) + " in file " +FileName);
														}
														if (FrontAxlePosition > RearAxlePosition)
														{
															Result.Objects[ObjectCount].FrontAxlePosition = FrontAxlePosition;
															Result.Objects[ObjectCount].RearAxlePosition = RearAxlePosition;
														}
														else if (FrontAxlePosition < RearAxlePosition)
														{
															currentHost.AddMessage(MessageType.Error, false,"Rear is expected to be less than Front in " + key + " at line " + (i + 1).ToString(Culture) + " in file " +FileName);
														}
														else
														{
															currentHost.AddMessage(MessageType.Error, false, "Rear must not equal Front in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
														}

													}
													catch(Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.RotateXDirection:
												case AnimatedKey.RotateYDirection:
												case AnimatedKey.RotateZDirection:
													switch (key)
													{
														case AnimatedKey.RotateXDirection:
															Result.Objects[ObjectCount].RotateXDirection = ParseVector3(b, key, i, Section, FileName);
															break;
														case AnimatedKey.RotateYDirection:
															Result.Objects[ObjectCount].RotateYDirection = ParseVector3(b, key, i, Section, FileName);
															break;
														case AnimatedKey.RotateZDirection:
															Result.Objects[ObjectCount].RotateZDirection = ParseVector3(b, key, i, Section, FileName);
															break;
													}break;
												case AnimatedKey.RotateXFunction:
												case AnimatedKey.RotateXFunctionRPN:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out RotateX))
														{
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															StaticXRotation = true;
														}
														Result.Objects[ObjectCount].RotateXFunction = new FunctionScript(currentHost, b, key == AnimatedKey.RotateXFunction);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.RotateXScript:
													try {
														Result.Objects[ObjectCount].RotateXFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													} catch (Exception ex) {
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.RotateYFunction:
												case AnimatedKey.RotateYFunctionRPN:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out RotateY))
														{
															StaticYRotation = true;
														}
														Result.Objects[ObjectCount].RotateYFunction = new FunctionScript(currentHost, b, key == AnimatedKey.RotateYFunction);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.RotateYScript:
													try {
														Result.Objects[ObjectCount].RotateYFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													} catch (Exception ex) {
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.RotateZFunction:
												case AnimatedKey.RotateZFunctionRPN:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out RotateZ))
														{
															StaticZRotation = true;
														}
														Result.Objects[ObjectCount].RotateZFunction = new FunctionScript(currentHost, b, key == AnimatedKey.RotateZFunction);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.RotateZScript:
													try {
														Result.Objects[ObjectCount].RotateZFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													} catch (Exception ex) {
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.RotateXDamping:
												case AnimatedKey.RotateYDamping:
												case AnimatedKey.RotateZDamping:
													{
														string[] s = b.Split(',');
														if (s.Length == 2)
														{
															if (!double.TryParse(s[0], NumberStyles.Float, Culture, out double nf))
															{
																currentHost.AddMessage(MessageType.Error, false, "NaturalFrequency is invalid in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], NumberStyles.Float, Culture, out double dr))
															{
																currentHost.AddMessage(MessageType.Error, false, "DampingRatio is invalid in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															}
															else if (nf <= 0.0)
															{
																currentHost.AddMessage(MessageType.Error, false, "NaturalFrequency is expected to be positive in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															}
															else if (dr <= 0.0)
															{
																currentHost.AddMessage(MessageType.Error, false, "DampingRatio is expected to be positive in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															}
															else
															{
																switch (key)
																{
																	case AnimatedKey.RotateXDamping:
																		Result.Objects[ObjectCount].RotateXDamping = new Damping(nf, dr);
																		break;
																	case AnimatedKey.RotateYDamping:
																		Result.Objects[ObjectCount].RotateYDamping = new Damping(nf, dr);
																		break;
																	case AnimatedKey.RotateZDamping:
																		Result.Objects[ObjectCount].RotateZDamping = new Damping(nf, dr);
																		break;
																}
															}
														}
														else
														{
															currentHost.AddMessage(MessageType.Error, false, "Exactly 2 arguments are expected in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
														}
													} break;
												case AnimatedKey.ScaleXFunction:
													try
													{
														if (!double.TryParse(b, NumberStyles.Float, Culture, out Scale.X))
														{
															Result.Objects[ObjectCount].ScaleXFunction = new FunctionScript(currentHost, b, true);
														}
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case AnimatedKey.ScaleXScript:
													try
													{
														Result.Objects[ObjectCount].ScaleXFunction = new CSAnimationScript(currentHost, 
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case AnimatedKey.ScaleYFunction:
													try
													{
														if (!double.TryParse(b, NumberStyles.Float, Culture, out Scale.Y))
														{
															Result.Objects[ObjectCount].ScaleYFunction = new FunctionScript(currentHost, b, true);	
														}
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case AnimatedKey.ScaleYScript:
													try
													{
														Result.Objects[ObjectCount].ScaleYFunction = new CSAnimationScript(currentHost, 
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case AnimatedKey.ScaleZFunction:
													try
													{
														if (!double.TryParse(b, NumberStyles.Float, Culture, out Scale.Z))
														{
															Result.Objects[ObjectCount].ScaleXFunction = new FunctionScript(currentHost, b, true);
														}
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case AnimatedKey.ScaleZScript:
													try
													{
														Result.Objects[ObjectCount].ScaleXFunction = new CSAnimationScript(currentHost, 
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case AnimatedKey.TextureShiftXDirection:
												case AnimatedKey.TextureShiftYDirection:
													{
														string[] s = b.Split(',');
														if (s.Length == 2)
														{
															if (!double.TryParse(s[0], NumberStyles.Float, Culture, out double x))
															{
																currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], NumberStyles.Float, Culture, out double y))
															{
																currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															}
															else
															{
																switch (key)
																{
																	case AnimatedKey.TextureShiftXDirection:
																		Result.Objects[ObjectCount].TextureShiftXDirection = new Vector2(x, y);
																		break;
																	case AnimatedKey.TextureShiftYDirection:
																		Result.Objects[ObjectCount].TextureShiftYDirection = new Vector2(x, y);
																		break;
																}
															}
														}
														else
														{
															currentHost.AddMessage(MessageType.Error, false, "Exactly 2 arguments are expected in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
														}
													} break;
												case AnimatedKey.TextureShiftXFunction:
												case AnimatedKey.TextureShiftXFunctionRPN:
													try
													{
														Result.Objects[ObjectCount].TextureShiftXFunction = new FunctionScript(currentHost, b, key == AnimatedKey.TextureShiftXFunction);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.TextureShiftXScript:
													try {
														Result.Objects[ObjectCount].TextureShiftXFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													} catch (Exception ex) {
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.TextureShiftYFunction:
												case AnimatedKey.TextureShiftYFunctionRPN:
													try
													{
														Result.Objects[ObjectCount].TextureShiftYFunction = new FunctionScript(currentHost, b, key == AnimatedKey.TextureShiftYFunction);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													} break;
												case AnimatedKey.TextureShiftYScript:
													try {
														Result.Objects[ObjectCount].TextureShiftYFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													} catch (Exception ex) {
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.TextureOverride:
													switch (b.ToLowerInvariant())
													{
														case "none":
															break;
														case "timetable":
															if (!timetableUsed)
															{
																currentHost.AddObjectForCustomTimeTable(Result.Objects[ObjectCount]);
																timetableUsed = true;
															}
															break;
														default:
															currentHost.AddMessage(MessageType.Error, false, "Unrecognized value in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															break;
													}
													break;
												case AnimatedKey.RefreshRate:
													{
														if (!double.TryParse(b, NumberStyles.Float, Culture, out double r))
														{
															currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
														}
														else if (r < 0.0)
														{
															currentHost.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
														}
														else
														{
															Result.Objects[ObjectCount].RefreshRate = r;
														}
													} break;
												default:
													currentHost.AddMessage(MessageType.Error, false, "The attribute " + key + " is not supported for Objects at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													break;
											}
										}
										else
										{
											currentHost.AddMessage(MessageType.Error, false, "A key-value pair was expected at line " + (i + 1).ToString(Culture) + ", but found " + Lines[i] + " in the Section " + Section + " in file " + FileName);
											return null;
										}
									}
									i++;
								}
								i--;
								
								if (StateFiles != null)
								{
									// create the object
									if (timetableUsed)
									{
										if (StateFunctionRpn != null)
										{
											StateFunctionRpn = "timetable 0 == " + StateFunctionRpn + " -1 ?";
										}
										else
										{
											StateFunctionRpn = "timetable";
										}
									}
									if (StateFunctionRpn != null)
									{
										try
										{
											Result.Objects[ObjectCount].StateFunction = new FunctionScript(currentHost, StateFunctionRpn, !StateFunctionIsPostfix);
										}
										catch (Exception ex)
										{
											currentHost.AddMessage(MessageType.Error, false, ex.Message + " in StateFunction at line " + (StateFunctionLine + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
										}
									}
									Result.Objects[ObjectCount].States = new ObjectState[StateFiles.Length];
									bool ForceTextureRepeatX = Result.Objects[ObjectCount].TextureShiftXFunction != null & Result.Objects[ObjectCount].TextureShiftXDirection.X != 0.0 |
									                           Result.Objects[ObjectCount].TextureShiftYFunction != null & Result.Objects[ObjectCount].TextureShiftYDirection.X != 0.0;
									bool ForceTextureRepeatY = Result.Objects[ObjectCount].TextureShiftXFunction != null & Result.Objects[ObjectCount].TextureShiftXDirection.Y != 0.0 |
									                           Result.Objects[ObjectCount].TextureShiftYFunction != null & Result.Objects[ObjectCount].TextureShiftYDirection.Y != 0.0;
									for (int k = 0; k < StateFiles.Length; k++)
									{
										Result.Objects[ObjectCount].States[k] = new ObjectState();
										if (StateFiles[k] != null)
										{
											currentHost.LoadObject(StateFiles[k], Encoding, out UnifiedObject currentObject);
											if (currentObject is StaticObject)
											{
												if (Scale != Vector3.One)
												{
													StaticObject obj = (StaticObject)currentObject.Clone();
													obj.ApplyScale(Scale);
													Result.Objects[ObjectCount].States[k].Prototype = obj;
												}
												else
												{
													Result.Objects[ObjectCount].States[k].Prototype = (StaticObject) currentObject;
												}

											}
											else if (currentObject is AnimatedObjectCollection)
											{
												currentHost.AddMessage(MessageType.Error, false, "Attempted to load the animated object " + StateFiles[k] + " where only static objects are allowed at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
												continue;
											}
											/*
											 * NOTE: If loading the object fails, or the prototype is null (empty), then we will get a
											 * non-shown state
											 */
											if (Result.Objects[ObjectCount].States[k].Prototype != null)
											{
												Result.Objects[ObjectCount].States[k].Prototype.Dynamic = true;
												for (int l = 0; l < Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials.Length; l++)
												{
													if (ForceTextureRepeatX && ForceTextureRepeatY)
													{
														Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
													}
													else if (ForceTextureRepeatX)
													{
														
														switch (Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode)
														{
															case OpenGlTextureWrapMode.ClampRepeat:
																Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
																break;
															case OpenGlTextureWrapMode.ClampClamp:
																Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatClamp;
																break;
														}
													}
													else if (ForceTextureRepeatY)
													{
														
														switch (Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode)
														{
															case OpenGlTextureWrapMode.RepeatClamp:
																Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
																break;
															case OpenGlTextureWrapMode.ClampClamp:
																Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.ClampRepeat;
																break;
														}
													}
												}
											}
											
										}
										else
										{
											Result.Objects[ObjectCount].States[k].Prototype = null;
										}
										
									}
									for (int j = 0; j < Result.Objects[ObjectCount].States.Length; j++)
									{

										//Rotate X
										if (Result.Objects[ObjectCount].States[j].Prototype == null)
										{
											continue;
										}
										//Apply position
										Result.Objects[ObjectCount].States[j].Translation = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
										//Test whether the object contains non static rotation functions
										//If so, the results may be off so don't optimise
										if (!StaticXRotation)
										{
											if (Result.Objects[ObjectCount].RotateXFunction != null)
											{
												continue;
											}
										}
										if (!StaticYRotation)
										{
											if (Result.Objects[ObjectCount].RotateYFunction != null)
											{
												continue;
											}
										}
										if (!StaticZRotation)
										{
											if (Result.Objects[ObjectCount].RotateZFunction != null)
											{
												continue;
											}
										}
										if (StaticXRotation)
										{
											Result.Objects[ObjectCount].States[j].Prototype = (StaticObject)Result.Objects[ObjectCount].States[j].Prototype.Clone();
											ApplyStaticRotation(ref Result.Objects[ObjectCount].States[j].Prototype.Mesh, Result.Objects[ObjectCount].RotateXDirection, RotateX);
											Result.Objects[ObjectCount].RotateXFunction = null;
										}
										if (StaticYRotation)
										{
											Result.Objects[ObjectCount].States[j].Prototype = (StaticObject)Result.Objects[ObjectCount].States[j].Prototype.Clone();
											ApplyStaticRotation(ref Result.Objects[ObjectCount].States[j].Prototype.Mesh, Result.Objects[ObjectCount].RotateYDirection, RotateY);
											Result.Objects[ObjectCount].RotateYFunction = null;
										}
										if (StaticZRotation)
										{
											Result.Objects[ObjectCount].States[j].Prototype = (StaticObject)Result.Objects[ObjectCount].States[j].Prototype.Clone();
											ApplyStaticRotation(ref Result.Objects[ObjectCount].States[j].Prototype.Mesh, Result.Objects[ObjectCount].RotateZDirection, RotateZ);
											Result.Objects[ObjectCount].RotateZFunction = null;
										}
										
									}
								}
								else
								{
									Result.Objects[ObjectCount].States = new ObjectState[] { };
								}
								ObjectCount++;
							}
							break;
						case AnimatedSection.Sound:
							{
								double pitch = 1.0, volume = 1.0, radius = 30.0;
								FunctionScript TrackFollowerFunction = null;
								FunctionScript PitchFunction = null;
								FunctionScript VolumeFunction = null;
								i++;
								if (Result.Sounds.Length >= SoundCount)
								{
									Array.Resize(ref Result.Sounds, Result.Sounds.Length << 1);
								}
								Vector3 Position = Vector3.Zero;
								string fileName = null;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									if (Lines[i].Length != 0)
									{
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j > 0)
										{
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											if (!Enum.TryParse(a, true, out AnimatedKey key))
											{
												currentHost.AddMessage(MessageType.Error, false, "Unknown key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
												i++;
												continue;
											}
											switch (key)
											{
												case AnimatedKey.Position:
													Position = ParseVector3(b, key, i, Section, FileName);
													break;
												case AnimatedKey.FileName:
													{
														string Folder = Path.GetDirectoryName(FileName);
														fileName = Path.CombineFile(Folder, b);
														if (!System.IO.File.Exists(fileName))
														{
															if (currentSoundFolder != null)
															{
																fileName = Path.CombineFile(currentSoundFolder, b);
															}

															if (!System.IO.File.Exists(fileName))
															{
																currentHost.AddMessage(MessageType.Error, false, "Sound file " + b + " was not found at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
																fileName = null;
															}
														}
													}
													break;
												case AnimatedKey.Radius:
													{
														if (!double.TryParse(b, out radius))
														{
															currentHost.AddMessage(MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															radius = 30.0;
														}
													}
													break;
												case AnimatedKey.Pitch:
													{
														if (!double.TryParse(b, out pitch))
														{
															currentHost.AddMessage(MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															pitch = 1.0;
														}
													}
													break;
												case AnimatedKey.Volume:
													{
														if (!double.TryParse(b, out volume))
														{
															currentHost.AddMessage(MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															volume = 1.0;
														}
													}
													break;
												case AnimatedKey.TranslateXDirection:
												case AnimatedKey.TranslateYDirection:
												case AnimatedKey.TranslateZDirection:
													switch (key)
													{
														case AnimatedKey.TranslateXDirection:
															Result.Objects[ObjectCount].TranslateXDirection = ParseVector3(b, key, i, Section, FileName);
															break;
														case AnimatedKey.TranslateYDirection:
															Result.Objects[ObjectCount].TranslateYDirection = ParseVector3(b, key, i, Section, FileName);
															break;
														case AnimatedKey.TranslateZDirection:
															Result.Objects[ObjectCount].TranslateZDirection = ParseVector3(b, key, i, Section, FileName);
															break;
													}
													break;
												case AnimatedKey.TranslateXFunction:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out double X))
														{
															Position.X = X;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateXFunction = new FunctionScript(currentHost, b, true);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.TranslateXScript:
													try
													{
														Result.Objects[ObjectCount].TranslateXFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.TranslateYFunction:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out double Y))
														{
															Position.Y = Y;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateYFunction = new FunctionScript(currentHost, b, true);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.TranslateYScript:
													try
													{
														Result.Objects[ObjectCount].TranslateYFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.TranslateZFunction:
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out double Z))
														{
															Position.Z = Z;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateZFunction = new FunctionScript(currentHost, b, true);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.TranslateZScript:
													try
													{
														Result.Objects[ObjectCount].TranslateZFunction = new CSAnimationScript(currentHost,
															Path.CombineDirectory(Path.GetDirectoryName(FileName), b, true));
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.TrackFollowerFunction:
													try
													{
														TrackFollowerFunction = new FunctionScript(currentHost, b, true);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.PitchFunction:
													try
													{
														PitchFunction = new FunctionScript(currentHost, b, true);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												case AnimatedKey.VolumeFunction:
													try
													{
														VolumeFunction = new FunctionScript(currentHost, b, true);
													}
													catch (Exception ex)
													{
														currentHost.AddMessage(MessageType.Error, false, ex.Message + " in " + key + " at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													}
													break;
												default:
													currentHost.AddMessage(MessageType.Error, false, "The attribute " + key + " is not supported for Sounds at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													break;
											}
										}
										else
										{
											currentHost.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
											return null;
										}
									}
									i++;
								}
								i--;
								if (fileName != null)
								{
									currentHost.RegisterSound(fileName, radius, out SoundHandle currentSound);
									WorldSound snd = new WorldSound(currentHost, currentSound)
									{
										currentPitch = pitch,
										currentVolume = volume,
										Position = Position,
										TrackFollowerFunction = TrackFollowerFunction,
										PitchFunction = PitchFunction,
										VolumeFunction = VolumeFunction
									};
									Result.Sounds[SoundCount] = snd;
									SoundCount++;
								}
								
							}
							break;
						case AnimatedSection.StateChangeSound:
							{
								double pitch = 1.0, volume = 1.0, radius = 30.0;
								bool singleBuffer = false, playOnShow = true, playOnHide = true;
								i++;
								if (Result.Sounds.Length == SoundCount)
								{
									Array.Resize(ref Result.Objects, Result.Sounds.Length << 1);
								}
								Vector3 Position = Vector3.Zero;
								string[] fileNames = new string[0];
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									if (Lines[i].Length != 0)
									{
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j > 0)
										{
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											if (!Enum.TryParse(a, true, out AnimatedKey key))
											{
												currentHost.AddMessage(MessageType.Error, false, "Unknown key " + key + " encountered at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
												i++;
												continue;
											}
											switch (key)
											{
												case AnimatedKey.Position:
													Position = ParseVector3(b, key, i, Section, FileName);
													break;
												case AnimatedKey.FileName:
													{
														singleBuffer = true;
														string Folder = Path.GetDirectoryName(FileName);
														fileNames = new[] {Path.CombineFile(Folder, b)};
														if (!System.IO.File.Exists(fileNames[0]))
														{
															if (currentSoundFolder != null)
															{
																fileNames[0] = Path.CombineFile(currentSoundFolder, b);
															}

															if (!System.IO.File.Exists(fileNames[0]))
															{
																currentHost.AddMessage(MessageType.Error, false, "Sound file " + b + " was not found at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
																fileNames[0] = null;
															}
														}
													}
													break;
												case AnimatedKey.FileNames:
													{
														string Folder = Path.GetDirectoryName(FileName);
														string[] splitFiles = b.Split(',');
														fileNames = new string[splitFiles.Length];
														for (int k = 0; k < splitFiles.Length; k++)
														{
															if (splitFiles[k].Trim(new char[] { }).Length == 0)
															{
																continue;
															}
															fileNames[k] = Path.CombineFile(Folder, splitFiles[k].Trim(new char[] { }));
															if (!System.IO.File.Exists(fileNames[k]))
															{
																if (currentSoundFolder != null)
																{
																	fileNames[k] = Path.CombineFile(currentSoundFolder, splitFiles[k]);
																}
																if (!System.IO.File.Exists(fileNames[k]))
																{
																	currentHost.AddMessage(MessageType.Error, false, "Sound file " + splitFiles[k] + " was not found at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
																	fileNames[k] = null;
																}
															}
														}
														
													}
													break;
												case AnimatedKey.Radius:
													{
														if (!double.TryParse(b, out radius))
														{
															currentHost.AddMessage(MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															radius = 30.0;
														}
													}
													break;
												case AnimatedKey.Pitch:
													{
														if (!double.TryParse(b, out pitch))
														{
															currentHost.AddMessage(MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															pitch = 1.0;
														}
													}
													break;
												case AnimatedKey.Volume:
													{
														if (!double.TryParse(b, out volume))
														{
															currentHost.AddMessage(MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
															volume = 1.0;
														}
													}
													break;
												case AnimatedKey.PlayOnShow:
													{
														if (b.ToLowerInvariant() == "true" || b == "1")
														{
															playOnShow = true;
														}
														else
														{
															playOnShow = false;
														}
													}
													break;
												case AnimatedKey.PlayOnHide:
													{
														if (b.ToLowerInvariant() == "true" || b == "1")
														{
															playOnHide = true;
														}
														else
														{
															playOnHide = false;
														}
													}
													break;
												default:
													currentHost.AddMessage(MessageType.Error, false, "The attribute " + key + " is not supported for StateChangeSounds at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
													break;
											}
										}
										else
										{
											currentHost.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
											return null;
										}
									}
									i++;
								}
								i--;
								if (fileNames.Length != 0 && ObjectCount > 0)
								{
									AnimatedWorldObjectStateSound snd = new AnimatedWorldObjectStateSound(currentHost)
									{
										Object = Result.Objects[ObjectCount - 1].Clone(),
										Buffers = new SoundHandle[fileNames.Length]
									};
									for (int j = 0; j < fileNames.Length; j++)
									{
										if (fileNames[j] != null)
										{
											currentHost.RegisterSound(fileNames[j], radius, out snd.Buffers[j]);
										}
									}
									snd.currentPitch = pitch;
									snd.currentVolume = volume;
									snd.Position = Position;
									snd.SingleBuffer = singleBuffer;
									snd.PlayOnShow = playOnShow;
									snd.PlayOnHide = playOnHide;
									Result.Sounds[SoundCount] = snd;
									SoundCount++;
									Result.Objects[ObjectCount - 1] = null;
									ObjectCount--;
								}

							}
							break;
						default:
							if (Lines.Length == 1 && Encoding.Equals(System.Text.Encoding.Unicode))
							{
								/*
								 * If only one line, there's a good possibility that our file is NOT Unicode at all
								 * and that the misdetection has turned it into garbage
								 *
								 * Try again with ASCII instead
								 */
								return ReadObject(FileName, System.Text.Encoding.GetEncoding(1252));
							}

							if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								/*
								 * Unknown section- skip it
								 * Either going to be a typo, or something introduced in a newer version
								 */
								currentHost.AddMessage(MessageType.Error, false, "Unknown section " + sct + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								while (i + 1 < Lines.Length && !(Lines[i + 1].StartsWith("[", StringComparison.Ordinal) & Lines[i + 1].EndsWith("]", StringComparison.Ordinal)))
								{
									i++;
								}
							}
							else
							{
								currentHost.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in the Section " + Section + " in file " + FileName);
								return null;	
							}
							break;
					}
				}
			}
			Array.Resize(ref Result.Objects, ObjectCount);
			return Result;
		}

		private static Vector3 ParseVector3(string b, AnimatedKey key, int i, AnimatedSection Section, string FileName)
		{
			Vector3 v = new Vector3();
			string[] s = b.Split(',');
			if (s.Length == 3)
			{
				if (!double.TryParse(s[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x))
				{
					currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + key + " at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in the Section " + Section + " in file " + FileName);
				}
				else if (!double.TryParse(s[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
				{
					currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + key + " at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in the Section " + Section + " in file " + FileName);
				}
				else if (!double.TryParse(s[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double z))
				{
					currentHost.AddMessage(MessageType.Error, false, "Z is invalid in " + key + " at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in the Section " + Section + " in file " + FileName);
				}
				else
				{
					v = new Vector3(x, y, z);
				}
			}
			else
			{
				currentHost.AddMessage(MessageType.Error, false, "Exactly 3 arguments are expected in " + key + " at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in the Section " + Section + " in file " + FileName);
			}
			return v;
		}

		private static void ApplyStaticRotation(ref Mesh Mesh, Vector3 RotationDirection, double Angle)
		{
			//Update co-ords
			for (int i = 0; i < Mesh.Vertices.Length; i++)
			{
				Mesh.Vertices[i].Coordinates.Rotate(RotationDirection, Angle);
			}
			//Update normals
			for (int i = 0; i < Mesh.Faces.Length; i++)
			{
				for(int j = 0; j < Mesh.Faces[i].Vertices.Length; j++)
					if (!Vector3.IsZero(Mesh.Faces[i].Vertices[j].Normal))
					{
						Mesh.Faces[i].Vertices[j].Normal.Rotate(RotationDirection, Angle);
					}
			}
		}
	}
}
