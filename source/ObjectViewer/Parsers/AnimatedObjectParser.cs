using System;
using OpenBveApi;
using OpenBveApi.Math;

namespace OpenBve {
	internal static class AnimatedObjectParser {

		// parse animated object config
		/// <summary>Loads a collection of animated objects from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <param name="LoadMode">The texture load mode.</param>
		/// <returns>The collection of animated objects.</returns>
		internal static ObjectManager.AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
		    ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection
		    {
		        Objects = new ObjectManager.AnimatedObject[4]
		    };
		    int ObjectCount = 0;
			// load file
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			bool rpnUsed = false;
			for (int i = 0; i < Lines.Length; i++) {
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).Trim();
				} else {
					Lines[i] = Lines[i].Trim();
				}
				if (Lines[i].IndexOf("functionrpn", StringComparison.OrdinalIgnoreCase) >= 0) {
					rpnUsed = true;
				}
			}
			if (rpnUsed) {
				Interface.AddMessage(Interface.MessageType.Error, false, "An animated object file contains RPN functions. These were never meant to be used directly, only for debugging. They won't be supported indefinately. Please get rid of them in file " + FileName);
			}
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length != 0) {
					switch (Lines[i].ToLowerInvariant()) {
						case "[include]":
							{
								i++;
								Vector3 position = new Vector3(0.0, 0.0, 0.0);
								ObjectManager.UnifiedObject[] obj = new OpenBve.ObjectManager.UnifiedObject[4];
								int objCount = 0;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
									if (Lines[i].Length != 0) {
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j > 0) {
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											switch (a.ToLowerInvariant()) {
												case "position":
													{
														string[] s = b.Split(',');
														if (s.Length == 3) {
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else {
																position = new Vector3(x, y, z);
															}
														} else {
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												default:
													Interface.AddMessage(Interface.MessageType.Error, false, "The attribute " + a + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										} else {
											string Folder = System.IO.Path.GetDirectoryName(FileName);
											if (Path.ContainsInvalidChars(Lines[i])) {
												Interface.AddMessage(Interface.MessageType.Error, false, Lines[i] + " contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											} else {
												string file = OpenBveApi.Path.CombineFile(Folder, Lines[i]);
												if (System.IO.File.Exists(file)) {
													if (obj.Length == objCount) {
														Array.Resize<ObjectManager.UnifiedObject>(ref obj, obj.Length << 1);
													}
													obj[objCount] = ObjectManager.LoadObject(file, Encoding, LoadMode, false, false, false);
													objCount++;
												} else {
													Interface.AddMessage(Interface.MessageType.Error, true, "File " + file + " not found at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												}
											}
										}
									}
									i++;
								}
								i--;
								for (int j = 0; j < objCount; j++) {
									if (obj[j] != null) {
										if (obj[j] is ObjectManager.StaticObject) {
											ObjectManager.StaticObject s = (ObjectManager.StaticObject)obj[j];
											s.Dynamic = true;
											if (ObjectCount >= Result.Objects.Length) {
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
										} else if (obj[j] is ObjectManager.AnimatedObjectCollection) {
											ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)obj[j];
											for (int k = 0; k < a.Objects.Length; k++) {
												if (ObjectCount >= Result.Objects.Length) {
													Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, Result.Objects.Length << 1);
												}
												for (int h = 0; h < a.Objects[k].States.Length; h++) {
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
								if (Result.Objects.Length == ObjectCount) {
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
								bool timetableUsed = false;
								string[] StateFiles = null;
								string StateFunctionRpn = null;
								int StateFunctionLine = -1;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
									if (Lines[i].Length != 0) {
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j > 0) {
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											switch (a.ToLowerInvariant()) {
												case "position":
													{
														string[] s = b.Split(',');
														if (s.Length == 3) {
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else {
																Position = new Vector3(x, y, z);
															}
														} else {
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "states":
													{
														string[] s = b.Split(',');
														if (s.Length >= 1) {
															string Folder = System.IO.Path.GetDirectoryName(FileName);
															StateFiles = new string[s.Length];
															for (int k = 0; k < s.Length; k++) {
																s[k] = s[k].Trim();
																if (s[k].Length == 0) {
																	Interface.AddMessage(Interface.MessageType.Error, false, "File" + k.ToString(Culture) + " is an empty string - did you mean something else? - in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	StateFiles[k] = null;
																} else if (Path.ContainsInvalidChars(s[k])) {
																	Interface.AddMessage(Interface.MessageType.Error, false, "File" + k.ToString(Culture) + " contains illegal characters in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	StateFiles[k] = null;
																} else {
																	StateFiles[k] = OpenBveApi.Path.CombineFile(Folder, s[k]);
																	if (!System.IO.File.Exists(StateFiles[k])) {
																		Interface.AddMessage(Interface.MessageType.Error, true, "File " + StateFiles[k] + " not found in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		StateFiles[k] = null;
																	}
																}
															}
														} else {
															Interface.AddMessage(Interface.MessageType.Error, false, "At least one argument is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															return null;
														}
													} break;
												case "statefunction":
													try {
														StateFunctionLine = i;
														StateFunctionRpn = FunctionScripts.GetPostfixNotationFromInfixNotation(b);
													} catch (Exception ex) {
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
														if (s.Length == 3) {
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else {
																switch (a.ToLowerInvariant()) {
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
														} else {
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "translatexfunction":
													try {
														Result.Objects[ObjectCount].TranslateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translateyfunction":
													try {
														Result.Objects[ObjectCount].TranslateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translatezfunction":
													try {
														Result.Objects[ObjectCount].TranslateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translatexfunctionrpn":
													try {
														Result.Objects[ObjectCount].TranslateXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translateyfunctionrpn":
													try {
														Result.Objects[ObjectCount].TranslateYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "translatezfunctionrpn":
													try {
														Result.Objects[ObjectCount].TranslateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatexdirection":
												case "rotateydirection":
												case "rotatezdirection":
													{
														string[] s = b.Split(',');
														if (s.Length == 3) {
															double x, y, z;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (x == 0.0 & y == 0.0 & z == 0.0) {
																Interface.AddMessage(Interface.MessageType.Error, false, "The direction indicated by X, Y and Z is expected to be non-zero in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else {
																switch (a.ToLowerInvariant()) {
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
														} else {
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "rotatexfunction":
													try {
														Result.Objects[ObjectCount].RotateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotateyfunction":
													try {
														Result.Objects[ObjectCount].RotateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatezfunction":
													try {
														Result.Objects[ObjectCount].RotateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatexfunctionrpn":
													try {
														Result.Objects[ObjectCount].RotateXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotateyfunctionrpn":
													try {
														Result.Objects[ObjectCount].RotateYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatezfunctionrpn":
													try {
														Result.Objects[ObjectCount].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "rotatexdamping":
												case "rotateydamping":
												case "rotatezdamping":
													{
														string[] s = b.Split(',');
														if (s.Length == 2) {
															double nf, dr;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out nf)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "NaturalFrequency is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out dr)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "DampingRatio is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (nf <= 0.0) {
																Interface.AddMessage(Interface.MessageType.Error, false, "NaturalFrequency is expected to be positive in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (dr <= 0.0) {
																Interface.AddMessage(Interface.MessageType.Error, false, "DampingRatio is expected to be positive in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else {
																switch (a.ToLowerInvariant()) {
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
														} else {
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 2 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "textureshiftxdirection":
												case "textureshiftydirection":
													{
														string[] s = b.Split(',');
														if (s.Length == 2) {
															double x, y;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
																Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															} else {
																switch (a.ToLowerInvariant()) {
																	case "textureshiftxdirection":
																		Result.Objects[ObjectCount].TextureShiftXDirection = new Vector2(x, y);
																		break;
																	case "textureshiftydirection":
																		Result.Objects[ObjectCount].TextureShiftYDirection = new Vector2(x, y);
																		break;
																}
															}
														} else {
															Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 2 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													} break;
												case "textureshiftxfunction":
													try {
														Result.Objects[ObjectCount].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "textureshiftyfunction":
													try {
														Result.Objects[ObjectCount].TextureShiftYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "textureshiftxfunctionrpn":
													try {
														Result.Objects[ObjectCount].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "textureshiftyfunctionrpn":
													try {
														Result.Objects[ObjectCount].TextureShiftYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
													} catch (Exception ex) {
														Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} break;
												case "textureoverride":
													switch (b.ToLowerInvariant()) {
														case "none":
															break;
														case "timetable":
															if (!timetableUsed) {
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
														if (!double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out r)) {
															Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														} else if (r < 0.0) {
															Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be non-negative in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														} else {
															Result.Objects[ObjectCount].RefreshRate = r;
														}
													} break;
												default:
													Interface.AddMessage(Interface.MessageType.Error, false, "The attribute " + a + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										} else {
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											return null;
										}
									}
									i++;
								}
								i--;
								if (StateFiles != null) {
									// create the object
									if (timetableUsed) {
										if (StateFunctionRpn != null) {
											StateFunctionRpn = "timetable 0 == " + StateFunctionRpn + " -1 ?";
										} else {
											StateFunctionRpn = "timetable";
										}
									}
									if (StateFunctionRpn != null) {
										try {
											Result.Objects[ObjectCount].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(StateFunctionRpn);
										} catch (Exception ex) {
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
												Result.Objects[ObjectCount].States[k].Position = Position;
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
								} else {
									Result.Objects[ObjectCount].States = new ObjectManager.AnimatedObjectState[] { };
								}
								ObjectCount++;
							}
							break;
						case "[sound]":
						case "[statechangesound]":
							//Only show the sound nag once per route, otherwise this could cause spam...
							if (!Program.SoundError)
							{
								Interface.AddMessage(Interface.MessageType.Information, false, "Animated objects containing sounds are only supported in openBVE v1.5.2.4+");
								Interface.AddMessage(Interface.MessageType.Information, false, "Object Viewer does not support sounds. Please use the main game to test these!");
								Program.SoundError = true;
							}
							i++;
							while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
							{
								i++;
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

	}
}
