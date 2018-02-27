using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Math;

namespace OpenBve
{
	internal static class AnimatedObjectParser
	{

		// parse animated object config
		/// <summary>Loads a collection of animated objects from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <param name="LoadMode">The texture load mode.</param>
		/// <returns>The collection of animated objects.</returns>
		internal static ObjectManager.AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection
			{
				Objects = new ObjectManager.AnimatedObject[4],
				Sounds = new ObjectManager.WorldObject[4]
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
				Lines[i] = j >= 0 ? Lines[i].Substring(0, j).Trim() : Lines[i].Trim();
				//Test whether RPN functions have been used
				rpnUsed = Lines[i].IndexOf("functionrpn", StringComparison.OrdinalIgnoreCase) >= 0;
			}
			if (rpnUsed)
			{
				Interface.AddMessage(Interface.MessageType.Error, false, "An animated object file contains RPN functions. These were never meant to be used directly, only for debugging. They won't be supported indefinately. Please get rid of them in file " + FileName);
			}
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Length != 0)
				{
					switch (Lines[i].ToLowerInvariant())
					{
						case "[include]":
							{
								i++;
								Vector3 position = new Vector3(0.0, 0.0, 0.0);
								ObjectManager.UnifiedObject[] obj = new OpenBve.ObjectManager.UnifiedObject[4];
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
											switch (a.ToLowerInvariant())
											{
												case "position":
													{
														string[] s = b.Split(',');
														if (s.Length == 3)
														{
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																position = new Vector3(x, y, z);
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												default:
													Interface.AddMessage(Interface.MessageType.Error, false, "The attribute " + a + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										}
										else
										{
											string Folder = System.IO.Path.GetDirectoryName(FileName);
											if (Path.ContainsInvalidChars(Lines[i]))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, Lines[i] + " contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											}
											else
											{
												string file = OpenBveApi.Path.CombineFile(Folder, Lines[i]);
												if (System.IO.File.Exists(file))
												{
													if (obj.Length == objCount)
													{
														Array.Resize<ObjectManager.UnifiedObject>(ref obj, obj.Length << 1);
													}
													obj[objCount] = ObjectManager.LoadObject(file, Encoding, LoadMode, false, false, false);
													objCount++;
												}
												else
												{
													Interface.AddMessage(Interface.MessageType.Error, true, "File " + file + " not found at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
										if (obj[j] is ObjectManager.StaticObject)
										{
											ObjectManager.StaticObject s = (ObjectManager.StaticObject)obj[j];
											s.Dynamic = true;
											if (ObjectCount >= Result.Objects.Length)
											{
												Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, Result.Objects.Length << 1);
											}
											ObjectManager.AnimatedObject a = new ObjectManager.AnimatedObject();
											ObjectManager.AnimatedObjectState aos = new ObjectManager.AnimatedObjectState
											{
												Object = s,
												Position = position
											};
											a.States = new ObjectManager.AnimatedObjectState[] { aos };
											Result.Objects[ObjectCount] = a;
											ObjectCount++;
										}
										else if (obj[j] is ObjectManager.AnimatedObjectCollection)
										{
											ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)obj[j];
											for (int k = 0; k < a.Objects.Length; k++)
											{
												if (ObjectCount >= Result.Objects.Length)
												{
													Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, Result.Objects.Length << 1);
												}
												for (int h = 0; h < a.Objects[k].States.Length; h++)
												{
													a.Objects[k].States[h].Position.X += position.X;
													a.Objects[k].States[h].Position.Y += position.Y;
													a.Objects[k].States[h].Position.Z += position.Z;
												}
												Result.Objects[ObjectCount] = a.Objects[k];
												ObjectCount++;
											}
										}
									}
								}
							}
							break;
						case "[object]":
							{
								i++;
								if (Result.Objects.Length == ObjectCount)
								{
									Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, Result.Objects.Length << 1);
								}
								Result.Objects[ObjectCount] = new ObjectManager.AnimatedObject
								{
									States = new ObjectManager.AnimatedObjectState[] {},
									CurrentState = -1,
									TranslateXDirection = new Vector3(1.0, 0.0, 0.0),
									TranslateYDirection = new Vector3(0.0, 1.0, 0.0),
									TranslateZDirection = new Vector3(0.0, 0.0, 1.0),
									RotateXDirection = new Vector3(1.0, 0.0, 0.0),
									RotateYDirection = new Vector3(0.0, 1.0, 0.0),
									RotateZDirection = new Vector3(0.0, 0.0, 1.0),
									TextureShiftXDirection = new Vector2(1.0, 0.0),
									TextureShiftYDirection = new Vector2(0.0, 1.0),
									RefreshRate = 0.0,
									ObjectIndex = -1
								};
								Vector3 Position = new Vector3(0.0, 0.0, 0.0);
								double RotateX = 0;
								bool StaticXRotation = false;
								double RotateY = 0;
								bool StaticYRotation = false;
								double RotateZ = 0;
								bool StaticZRotation = false;
								bool timetableUsed = false;
								string[] StateFiles = null;
								string StateFunctionRpn = null;
								int StateFunctionLine = -1;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									string Folder;
									if (Lines[i].Length != 0)
									{
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j > 0)
										{
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											switch (a.ToLowerInvariant())
											{
												case "position":
													{
														string[] s = b.Split(',');
														if (s.Length == 3)
														{
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																Position = new Vector3(x, y, z);
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "states":
													{
														string[] s = b.Split(',');
														if (s.Length >= 1)
														{
															Folder = System.IO.Path.GetDirectoryName(FileName);
															StateFiles = new string[s.Length];
															bool NullObject = true;
															for (int k = 0; k < s.Length; k++)
															{
																s[k] = s[k].Trim();
																if (s[k].Length == 0)
																{
																	Interface.AddMessage(Interface.MessageType.Error, false, "File" + k.ToString(Culture) + " is an empty string - did you mean something else? - in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	StateFiles[k] = null;
																}
																else if (Path.ContainsInvalidChars(s[k]))
																{
																	Interface.AddMessage(Interface.MessageType.Error, false, "File" + k.ToString(Culture) + " contains illegal characters in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	StateFiles[k] = null;
																}
																else
																{
																	StateFiles[k] = OpenBveApi.Path.CombineFile(Folder, s[k]);
																	if (!System.IO.File.Exists(StateFiles[k]))
																	{
																		Interface.AddMessage(Interface.MessageType.Error, true, "File " + StateFiles[k] + " not found in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		StateFiles[k] = null;
																	}
																}
																if (StateFiles[k] != null)
																{
																	NullObject = false;
																}	
															}
															if (NullObject == true)
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "None of the specified files were found in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																return null;
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "At least one argument is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															return null;
														}
													} break;
												case "statefunction":
													try
													{
														StateFunctionLine = i;
														StateFunctionRpn = FunctionScripts.GetPostfixNotationFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "statefunctionrpn":
													{
														StateFunctionLine = i;
														StateFunctionRpn = b;
													} break;
												case "translatexdirection":
												case "translateydirection":
												case "translatezdirection":
													{
														string[] s = b.Split(',');
														if (s.Length == 3)
														{
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																switch (a.ToLowerInvariant())
																{
																	case "translatexdirection":
																		Result.Objects[ObjectCount].TranslateXDirection = new Vector3(x, y, z);
																		break;
																	case "translateydirection":
																		Result.Objects[ObjectCount].TranslateYDirection = new Vector3(x, y, z);
																		break;
																	case "translatezdirection":
																		Result.Objects[ObjectCount].TranslateZDirection = new Vector3(x, y, z);
																		break;
																}
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "translatexfunction":
													try
													{
														double X;
														if (double.TryParse(b, NumberStyles.Float, Culture, out X))
														{
															Position.X = X;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translatexscript":
													try
													{
														Result.Objects[ObjectCount].TranslateXScriptFile = OpenBveApi.Path.CombineDirectory(System.IO.Path.GetDirectoryName(FileName), b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translateyfunction":
													try
													{
														double Y;
														if (double.TryParse(b, NumberStyles.Float, Culture, out Y))
														{
															Position.Y = Y;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translateyscript":
													try
													{
														Result.Objects[ObjectCount].TranslateYScriptFile = OpenBveApi.Path.CombineDirectory(System.IO.Path.GetDirectoryName(FileName), b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translatezfunction":
													try
													{
														double Z;
														if (double.TryParse(b, NumberStyles.Float, Culture, out Z))
														{
															Position.Z = Z;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translatezscript":
													try
													{
														Result.Objects[ObjectCount].TranslateZScriptFile = OpenBveApi.Path.CombineDirectory(System.IO.Path.GetDirectoryName(FileName), b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "trackfollowerfunction":
													try
													{
														Result.Objects[ObjectCount].TrackFollowerFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "axles":
													try
													{
														double FrontAxlePosition;
														double RearAxlePosition;
														var splitValue = b.Split(',');
														Double.TryParse(splitValue[0], out FrontAxlePosition);
														Double.TryParse(splitValue[1], out RearAxlePosition);
														if (FrontAxlePosition > RearAxlePosition)
														{
															Result.Objects[ObjectCount].FrontAxlePosition = FrontAxlePosition;
															Result.Objects[ObjectCount].RearAxlePosition = RearAxlePosition;
														}
														else if (FrontAxlePosition < RearAxlePosition)
														{
															Interface.AddMessage(Interface.MessageType.Error, false,"Rear is expected to be less than Front in " + a + " at line " + (i + 1).ToString(Culture) + " in file " +FileName);
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Rear must not equal Front in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}

													}
													catch(Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
													/*
													 * RPN Functions were added by Michelle, and she stated that they should not be used other than in debugging
													 * Not aware of any uses, but these should stay there anyway
													 * 
													 */
												case "translatexfunctionrpn":
													try
													{
														Result.Objects[ObjectCount].TranslateXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translateyfunctionrpn":
													try
													{
														Result.Objects[ObjectCount].TranslateYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translatezfunctionrpn":
													try
													{
														Result.Objects[ObjectCount].TranslateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatexdirection":
												case "rotateydirection":
												case "rotatezdirection":
													{
														string[] s = b.Split(',');
														if (s.Length == 3)
														{
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (x == 0.0 & y == 0.0 & z == 0.0)
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "The direction indicated by X, Y and Z is expected to be non-zero in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																switch (a.ToLowerInvariant())
																{
																	case "rotatexdirection":
																		Result.Objects[ObjectCount].RotateXDirection = new Vector3(x, y, z);
																		break;
																	case "rotateydirection":
																		Result.Objects[ObjectCount].RotateYDirection = new Vector3(x, y, z);
																		break;
																	case "rotatezdirection":
																		Result.Objects[ObjectCount].RotateZDirection = new Vector3(x, y, z);
																		break;
																}
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "rotatexfunction":
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out RotateX))
														{
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															StaticXRotation = true;
														}
														Result.Objects[ObjectCount].RotateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotateyfunction":
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out RotateY))
														{
															StaticYRotation = true;
														}
														Result.Objects[ObjectCount].RotateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatezfunction":
													try
													{
														if (double.TryParse(b, NumberStyles.Float, Culture, out RotateZ))
														{
															StaticZRotation = true;
														}
														Result.Objects[ObjectCount].RotateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatexfunctionrpn":
													try
													{
														Result.Objects[ObjectCount].RotateXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotateyfunctionrpn":
													try
													{
														Result.Objects[ObjectCount].RotateYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatezfunctionrpn":
													try
													{
														Result.Objects[ObjectCount].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatexdamping":
												case "rotateydamping":
												case "rotatezdamping":
													{
														string[] s = b.Split(',');
														if (s.Length == 2)
														{
															double nf, dr;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out nf))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "NaturalFrequency is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out dr))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "DampingRatio is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (nf <= 0.0)
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "NaturalFrequency is expected to be positive in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (dr <= 0.0)
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "DampingRatio is expected to be positive in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																switch (a.ToLowerInvariant())
																{
																	case "rotatexdamping":
																		Result.Objects[ObjectCount].RotateXDamping = new ObjectManager.Damping(nf, dr);
																		break;
																	case "rotateydamping":
																		Result.Objects[ObjectCount].RotateYDamping = new ObjectManager.Damping(nf, dr);
																		break;
																	case "rotatezdamping":
																		Result.Objects[ObjectCount].RotateZDamping = new ObjectManager.Damping(nf, dr);
																		break;
																}
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 2 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "textureshiftxdirection":
												case "textureshiftydirection":
													{
														string[] s = b.Split(',');
														if (s.Length == 2)
														{
															double x, y;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																switch (a.ToLowerInvariant())
																{
																	case "textureshiftxdirection":
																		Result.Objects[ObjectCount].TextureShiftXDirection = new Vector2(x, y);
																		break;
																	case "textureshiftydirection":
																		Result.Objects[ObjectCount].TextureShiftYDirection = new Vector2(x, y);
																		break;
																}
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 2 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "textureshiftxfunction":
													try
													{
														Result.Objects[ObjectCount].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "textureshiftyfunction":
													try
													{
														Result.Objects[ObjectCount].TextureShiftYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "textureshiftxfunctionrpn":
													try
													{
														Result.Objects[ObjectCount].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "textureshiftyfunctionrpn":
													try
													{
														Result.Objects[ObjectCount].TextureShiftYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "textureoverride":
													switch (b.ToLowerInvariant())
													{
														case "none":
															break;
														case "timetable":
															if (!timetableUsed)
															{
																Timetable.AddObjectForCustomTimetable(Result.Objects[ObjectCount]);
																timetableUsed = true;
															}
															break;
														default:
															Interface.AddMessage(Interface.MessageType.Error, false, "Unrecognized value in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															break;
													}
													break;
												case "refreshrate":
													{
														double r;
														if (!double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out r))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
														else if (r < 0.0)
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be non-negative in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
														else
														{
															Result.Objects[ObjectCount].RefreshRate = r;
														}
													} break;
												default:
													Interface.AddMessage(Interface.MessageType.Error, false, "The attribute " + a + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										}
										else
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
											Result.Objects[ObjectCount].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(StateFunctionRpn);
										}
										catch (Exception ex)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in StateFunction at line " + (StateFunctionLine + 1).ToString(Culture) + " in file " + FileName);
										}
									}
									Result.Objects[ObjectCount].States = new ObjectManager.AnimatedObjectState[StateFiles.Length];
									bool ForceTextureRepeatX = Result.Objects[ObjectCount].TextureShiftXFunction != null & Result.Objects[ObjectCount].TextureShiftXDirection.X != 0.0 |
									                           Result.Objects[ObjectCount].TextureShiftYFunction != null & Result.Objects[ObjectCount].TextureShiftYDirection.X != 0.0;
									bool ForceTextureRepeatY = Result.Objects[ObjectCount].TextureShiftXFunction != null & Result.Objects[ObjectCount].TextureShiftXDirection.Y != 0.0 |
									                           Result.Objects[ObjectCount].TextureShiftYFunction != null & Result.Objects[ObjectCount].TextureShiftYDirection.Y != 0.0;
									for (int k = 0; k < StateFiles.Length; k++)
									{
										Result.Objects[ObjectCount].States[k].Position = new Vector3(0.0, 0.0, 0.0);
										if (StateFiles[k] != null)
										{
											Result.Objects[ObjectCount].States[k].Object = ObjectManager.LoadStaticObject(StateFiles[k], Encoding, LoadMode, false, ForceTextureRepeatX, ForceTextureRepeatY);
											if (Result.Objects[ObjectCount].States[k].Object != null)
											{
												Result.Objects[ObjectCount].States[k].Object.Dynamic = true;
												for (int l = 0; l < Result.Objects[ObjectCount].States[k].Object.Mesh.Materials.Length; l++)
												{
													if (ForceTextureRepeatX && ForceTextureRepeatY)
													{
														Result.Objects[ObjectCount].States[k].Object.Mesh.Materials[l].WrapMode = Textures.OpenGlTextureWrapMode.RepeatRepeat;
													}
													else if (ForceTextureRepeatX)
													{
														
														switch (Result.Objects[ObjectCount].States[k].Object.Mesh.Materials[l].WrapMode)
														{
															case Textures.OpenGlTextureWrapMode.ClampRepeat:
																Result.Objects[ObjectCount].States[k].Object.Mesh.Materials[l].WrapMode = Textures.OpenGlTextureWrapMode.RepeatRepeat;
																break;
															case Textures.OpenGlTextureWrapMode.ClampClamp:
																Result.Objects[ObjectCount].States[k].Object.Mesh.Materials[l].WrapMode = Textures.OpenGlTextureWrapMode.RepeatClamp;
																break;
														}
													}
													else if (ForceTextureRepeatY)
													{
														
														switch (Result.Objects[ObjectCount].States[k].Object.Mesh.Materials[l].WrapMode)
														{
															case Textures.OpenGlTextureWrapMode.RepeatClamp:
																Result.Objects[ObjectCount].States[k].Object.Mesh.Materials[l].WrapMode = Textures.OpenGlTextureWrapMode.RepeatRepeat;
																break;
															case Textures.OpenGlTextureWrapMode.ClampClamp:
																Result.Objects[ObjectCount].States[k].Object.Mesh.Materials[l].WrapMode = Textures.OpenGlTextureWrapMode.ClampRepeat;
																break;
														}
													}
												}
											}
											
										}
										else
										{
											Result.Objects[ObjectCount].States[k].Object = null;
										}
										
									}
									for (int j = 0; j < Result.Objects[ObjectCount].States.Length; j++)
									{

										//Rotate X
										if (Result.Objects[ObjectCount].States[j].Object == null)
										{
											continue;
										}
										//Apply position
										Result.Objects[ObjectCount].States[j].Position = Position;
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
											ApplyStaticRotation(ref Result.Objects[ObjectCount].States[j].Object.Mesh, Result.Objects[ObjectCount].RotateXDirection, RotateX);
											Result.Objects[ObjectCount].RotateXFunction = null;
										}
										if (StaticYRotation)
										{
											ApplyStaticRotation(ref Result.Objects[ObjectCount].States[j].Object.Mesh, Result.Objects[ObjectCount].RotateYDirection, RotateY);
											Result.Objects[ObjectCount].RotateYFunction = null;
										}
										if (StaticZRotation)
										{
											ApplyStaticRotation(ref Result.Objects[ObjectCount].States[j].Object.Mesh, Result.Objects[ObjectCount].RotateZDirection, RotateZ);
											Result.Objects[ObjectCount].RotateZFunction = null;
										}
										
									}
								}
								else
								{
									Result.Objects[ObjectCount].States = new ObjectManager.AnimatedObjectState[] { };
								}
								ObjectCount++;
							}
							break;
						case "[sound]":
							{
								double pitch = 1.0, volume = 1.0, radius = 30.0;
								FunctionScripts.FunctionScript TrackFollowerFunction = null;
								FunctionScripts.FunctionScript PitchFunction = null;
								FunctionScripts.FunctionScript VolumeFunction = null;
								i++;
								if (Result.Sounds.Length >= SoundCount)
								{
									Array.Resize<ObjectManager.WorldObject>(ref Result.Sounds, Result.Sounds.Length << 1);
								}
								Vector3 Position = new Vector3(0.0, 0.0, 0.0);
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
											switch (a.ToLowerInvariant())
											{
												case "position":
													{
														string[] s = b.Split(',');
														if (s.Length == 3)
														{
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																Position = new Vector3(x, y, z);
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
													break;
												case "filename":
													{
														string Folder = System.IO.Path.GetDirectoryName(FileName);
														fileName = OpenBveApi.Path.CombineFile(Folder, b);
														if (!System.IO.File.Exists(fileName))
														{
															fileName = OpenBveApi.Path.CombineFile(CsvRwRouteParser.SoundPath, b);
															if (!System.IO.File.Exists(fileName))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Sound file " + b + " was not found at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																fileName = null;
															}
														}
													}
													break;
												case "radius":
													{
														if (!Double.TryParse(b, out radius))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															radius = 30.0;
														}
													}
													break;
												case "pitch":
													{
														if (!Double.TryParse(b, out pitch))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															pitch = 1.0;
														}
													}
													break;
												case "volume":
													{
														if (!Double.TryParse(b, out volume))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															volume = 1.0;
														}
													}
													break;
												case "translatexdirection":
												case "translateydirection":
												case "translatezdirection":
													{
														string[] s = b.Split(',');
														if (s.Length == 3)
														{
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																switch (a.ToLowerInvariant())
																{
																	case "translatexdirection":
																		Result.Objects[ObjectCount].TranslateXDirection = new Vector3(x, y, z);
																		break;
																	case "translateydirection":
																		Result.Objects[ObjectCount].TranslateYDirection = new Vector3(x, y, z);
																		break;
																	case "translatezdirection":
																		Result.Objects[ObjectCount].TranslateZDirection = new Vector3(x, y, z);
																		break;
																}
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
													break;
												case "translatexfunction":
													try
													{
														double X;
														if (double.TryParse(b, NumberStyles.Float, Culture, out X))
														{
															Position.X = X;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "translatexscript":
													try
													{
														Result.Objects[ObjectCount].TranslateXScriptFile = OpenBveApi.Path.CombineDirectory(System.IO.Path.GetDirectoryName(FileName), b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "translateyfunction":
													try
													{
														double Y;
														if (double.TryParse(b, NumberStyles.Float, Culture, out Y))
														{
															Position.Y = Y;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "translateyscript":
													try
													{
														Result.Objects[ObjectCount].TranslateYScriptFile = OpenBveApi.Path.CombineDirectory(System.IO.Path.GetDirectoryName(FileName), b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "translatezfunction":
													try
													{
														double Z;
														if (double.TryParse(b, NumberStyles.Float, Culture, out Z))
														{
															Position.Z = Z;
															//A function script must be evaluated every frame, no matter if it is a constant value
															//If we add this to the position instead, this gives a minor speedup
															break;
														}
														Result.Objects[ObjectCount].TranslateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "translatezscript":
													try
													{
														Result.Objects[ObjectCount].TranslateZScriptFile = OpenBveApi.Path.CombineDirectory(System.IO.Path.GetDirectoryName(FileName), b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "trackfollowerfunction":
													try
													{
														TrackFollowerFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "pitchfunction":
													try
													{
														PitchFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												case "volumefunction":
													try
													{
														VolumeFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													}
													catch (Exception ex)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
													break;
												default:
													Interface.AddMessage(Interface.MessageType.Error, false, "The attribute " + a + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										}
										else
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											return null;
										}
									}
									i++;
								}
								i--;
								if (fileName != null)
								{
									ObjectManager.WorldSound snd = new ObjectManager.WorldSound();
									snd.Buffer = Sounds.RegisterBuffer(fileName, radius);
									snd.currentPitch = pitch;
									snd.currentVolume = volume;
									snd.Position = Position;
									snd.TrackFollowerFunction = TrackFollowerFunction;
									snd.PitchFunction = PitchFunction;
									snd.VolumeFunction = VolumeFunction;
									Result.Sounds[SoundCount] = snd;
									SoundCount++;
								}
								
							}
							break;
						case "[statechangesound]":
							{
								double pitch = 1.0, volume = 1.0, radius = 30.0;
								bool singleBuffer = false, playOnShow = true, playOnHide = true;
								i++;
								if (Result.Sounds.Length == SoundCount)
								{
									Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, Result.Sounds.Length << 1);
								}
								Vector3 Position = new Vector3(0.0, 0.0, 0.0);
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
											switch (a.ToLowerInvariant())
											{
												case "position":
													{
														string[] s = b.Split(',');
														if (s.Length == 3)
														{
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																Position = new Vector3(x, y, z);
															}
														}
														else
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
													break;
												case "filename":
													{
														singleBuffer = true;
														string Folder = System.IO.Path.GetDirectoryName(FileName);
														fileNames = new string[] {OpenBveApi.Path.CombineFile(Folder, b)};
														if (!System.IO.File.Exists(fileNames[0]))
														{
															fileNames[0] = OpenBveApi.Path.CombineFile(CsvRwRouteParser.SoundPath, b);
															if (!System.IO.File.Exists(fileNames[0]))
															{
																Interface.AddMessage(Interface.MessageType.Error, false, "Sound file " + b + " was not found at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																fileNames[0] = null;
															}
														}
													}
													break;
												case "filenames":
													{
														string Folder = System.IO.Path.GetDirectoryName(FileName);
														string[] splitFiles = b.Split(',');
														fileNames = new string[splitFiles.Length];
														for (int k = 0; k < splitFiles.Length; k++)
														{
															if (splitFiles[k].Trim().Length == 0)
															{
																continue;
															}
															fileNames[k] = OpenBveApi.Path.CombineFile(Folder, splitFiles[k].Trim());
															if (!System.IO.File.Exists(fileNames[k]))
															{
																fileNames[k] = OpenBveApi.Path.CombineFile(CsvRwRouteParser.SoundPath, b);
																if (!System.IO.File.Exists(fileNames[k]))
																{
																	Interface.AddMessage(Interface.MessageType.Error, false, "Sound file " + b + " was not found at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	fileNames[k] = null;
																}
															}
														}
														
													}
													break;
												case "radius":
													{
														if (!Double.TryParse(b, out radius))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															radius = 30.0;
														}
													}
													break;
												case "pitch":
													{
														if (!Double.TryParse(b, out pitch))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															pitch = 1.0;
														}
													}
													break;
												case "volume":
													{
														if (!Double.TryParse(b, out volume))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Sound radius " + b + " was invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															volume = 1.0;
														}
													}
													break;
												case "playonshow":
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
												case "playonhide":
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
													Interface.AddMessage(Interface.MessageType.Error, false, "The attribute " + a + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										}
										else
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											return null;
										}
									}
									i++;
								}
								i--;
								if (fileNames.Length != 0 && ObjectCount > 0)
								{
									ObjectManager.AnimatedWorldObjectStateSound snd = new ObjectManager.AnimatedWorldObjectStateSound();
									snd.Object = Result.Objects[ObjectCount -1].Clone();
									snd.Buffers = new Sounds.SoundBuffer[fileNames.Length];
									for (int j = 0; j < fileNames.Length; j++)
									{
										if (fileNames[j] != null)
										{
											snd.Buffers[j] = Sounds.RegisterBuffer(fileNames[j], radius);
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
									Result.Objects[ObjectCount] = null;
									ObjectCount--;
								}

							}
							break;
						default:
							Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							return null;
					}
				}
			}
			Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, ObjectCount);
			return Result;
		}

		private static void ApplyStaticRotation(ref World.Mesh Mesh, Vector3 RotationDirection, double Angle)
		{
			//Update co-ords
			for (int i = 0; i < Mesh.Vertices.Length; i++)
			{
				World.Rotate(ref Mesh.Vertices[i].Coordinates, RotationDirection, Math.Cos(Angle), Math.Sin(Angle));
			}
			//Update normals
			for (int i = 0; i < Mesh.Faces.Length; i++)
			{
				for(int j = 0; j < Mesh.Faces[i].Vertices.Length; j++)
					if (!Vector3.IsZero(Mesh.Faces[i].Vertices[j].Normal))
					{
						World.Rotate(ref Mesh.Faces[i].Vertices[j].Normal, RotationDirection, Math.Cos(Angle), Math.Sin(Angle));
					}
			}
		}
	}
}
