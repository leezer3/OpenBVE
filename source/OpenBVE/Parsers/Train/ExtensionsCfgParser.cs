using System;
using System.Text;
using OpenBveApi;
using OpenBveApi.Objects;
using OpenBveApi.Interface;

namespace OpenBve {
	internal static class ExtensionsCfgParser {

		// parse extensions config
		internal static void ParseExtensionsConfig(string TrainPath, System.Text.Encoding Encoding, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, TrainManager.Train Train, bool LoadObjects)
		{
			bool[] CarObjectsReversed = new bool[Train.Cars.Length];
			bool[] BogieObjectsReversed = new bool[Train.Cars.Length * 2];

			bool[] CarsDefined = new bool[Train.Cars.Length];
			bool[] BogiesDefined = new bool[Train.Cars.Length * 2];
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "extensions.cfg");
			if (System.IO.File.Exists(FileName)) {
				TextEncoding.Encoding newEncoding = TextEncoding.GetEncodingFromFile(FileName);
				if (newEncoding != TextEncoding.Encoding.Unknown)
				{
					switch (newEncoding)
					{
						case TextEncoding.Encoding.Utf7:
							Encoding = System.Text.Encoding.UTF7;
							break;
						case TextEncoding.Encoding.Utf8:
							Encoding = System.Text.Encoding.UTF8;
							break;
						case TextEncoding.Encoding.Utf16Le:
							Encoding = System.Text.Encoding.Unicode;
							break;
						case TextEncoding.Encoding.Utf16Be:
							Encoding = System.Text.Encoding.BigEndianUnicode;
							break;
						case TextEncoding.Encoding.Utf32Le:
							Encoding = System.Text.Encoding.UTF32;
							break;
						case TextEncoding.Encoding.Utf32Be:
							Encoding = System.Text.Encoding.GetEncoding(12001);
							break;
						case TextEncoding.Encoding.Shift_JIS:
							Encoding = System.Text.Encoding.GetEncoding(932);
							break;
					}
				}

				string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
				for (int i = 0; i < Lines.Length; i++) {
					int j = Lines[i].IndexOf(';');
					if (j >= 0) {
						Lines[i] = Lines[i].Substring(0, j).Trim();
					} else {
						Lines[i] = Lines[i].Trim();
					}
				}
				for (int i = 0; i < Lines.Length; i++) {
					if (Lines[i].Length != 0) {
						switch (Lines[i].ToLowerInvariant()) {
							case "[exterior]":
								// exterior
								i++;
								while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									if (Lines[i].Length != 0) {
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j >= 0) {
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											int n;
											if (int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out n)) {
												if (n >= 0 & n < Train.Cars.Length) {
													if (Path.ContainsInvalidChars(b)) {
														Interface.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} else {
														string File = OpenBveApi.Path.CombineFile(TrainPath, b);
														if (System.IO.File.Exists(File)) {
															if (LoadObjects)
															{
																CarObjects[n] = ObjectManager.LoadObject(File, Encoding, false, false, false);
															}
														} else {
															Interface.AddMessage(MessageType.Error, true, "The car object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
												} else {
													Interface.AddMessage(MessageType.Error, false, "The car index " + a + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												}
											} else {
												Interface.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											}
										} else {
											Interface.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									}
									i++;
								}
								i--;
								break;
							default:
								if (Lines[i].StartsWith("[car", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									// car
									string t = Lines[i].Substring(4, Lines[i].Length - 5);
									int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n)) {
										if (n >= 0 & n < Train.Cars.Length)
										{
											if (CarsDefined[n])
											{
												Interface.AddMessage(MessageType.Error, false, "Car " + n.ToString(Culture) + " has already been declared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											}
											CarsDefined[n] = true;
											bool DefinedLength = false;
											bool DefinedAxles = false;
											i++;
											while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
												if (Lines[i].Length != 0) {
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0) {
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant()) {
															case "object":
																if (string.IsNullOrEmpty(b))
																{
																	Interface.AddMessage(MessageType.Error, true, "An empty car object was supplied at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	break;
																}
																if (Path.ContainsInvalidChars(b)) {
																	Interface.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																} else {
																	string File = OpenBveApi.Path.CombineFile(TrainPath, b);
																	if (System.IO.File.Exists(File)) {
																		if (LoadObjects)
																		{
																			CarObjects[n] = ObjectManager.LoadObject(File, Encoding, false, false, false);
																		}
																	} else {
																		Interface.AddMessage(MessageType.Error, true, "The car object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "length":
																{
																	double m;
																	if (double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out m)) {
																		if (m > 0.0) {
																			Train.Cars[n].Length = m;
																			Train.Cars[n].BeaconReceiverPosition = 0.5 * m;
																			DefinedLength = true;
																		} else {
																			Interface.AddMessage(MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																	} else {
																		Interface.AddMessage(MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "axles":
																{
																	int k = b.IndexOf(',');
																	if (k >= 0) {
																		string c = b.Substring(0, k).TrimEnd();
																		string d = b.Substring(k + 1).TrimStart();
																		double rear, front;
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out rear)) {
																			Interface.AddMessage(MessageType.Error, false, "Rear is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front)) {
																			Interface.AddMessage(MessageType.Error, false, "Front is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (rear >= front) {
																			Interface.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else {
																			Train.Cars[n].RearAxle.Position = rear;
																			Train.Cars[n].FrontAxle.Position = front;
																			DefinedAxles = true;
																		}
																	} else {
																		Interface.AddMessage(MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "reversed":
																CarObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
																break;
															default:
																Interface.AddMessage(MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												}
												i++;
											}
											i--;
											if (DefinedLength & !DefinedAxles) {
												double AxleDistance = 0.4 * Train.Cars[n].Length;
												Train.Cars[n].RearAxle.Position = -AxleDistance;
												Train.Cars[n].FrontAxle.Position = AxleDistance;
											}
										} else {
											Interface.AddMessage(MessageType.Error, false, "The car index " + t + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									} else {
										Interface.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else if (Lines[i].StartsWith("[coupler", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									// coupler
									string t = Lines[i].Substring(8, Lines[i].Length - 9);
									int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n)) {
										if (n >= 0 & n < Train.Couplers.Length) {
											i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
												if (Lines[i].Length != 0) {
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0) {
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant()) {
															case "distances":
																{
																	int k = b.IndexOf(',');
																	if (k >= 0) {
																		string c = b.Substring(0, k).TrimEnd();
																		string d = b.Substring(k + 1).TrimStart();
																		double min, max;
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out min)) {
																			Interface.AddMessage(MessageType.Error, false, "Minimum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out max)) {
																			Interface.AddMessage(MessageType.Error, false, "Maximum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (min > max) {
																			Interface.AddMessage(MessageType.Error, false, "Minimum is expected to be less than Maximum in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else {
																			Train.Couplers[n].MinimumDistanceBetweenCars = min;
																			Train.Couplers[n].MaximumDistanceBetweenCars = max;
																		}
																	} else {
																		Interface.AddMessage(MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																} break;
															default:
																Interface.AddMessage(MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} i++;
											} i--;
										} else {
											Interface.AddMessage(MessageType.Error, false, "The coupler index " + t + " does not reference an existing coupler at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									} else {
										Interface.AddMessage(MessageType.Error, false, "The coupler index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
								else if (Lines[i].StartsWith("[bogie", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal))
								{
									// car
									string t = Lines[i].Substring(6, Lines[i].Length - 7);
									int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n))
									{
										if (n > BogiesDefined.Length -1)
										{
											continue;
										}
										if (BogiesDefined[n])
										{
											Interface.AddMessage(MessageType.Error, false, "Bogie " + n.ToString(Culture) + " has already been declared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
										BogiesDefined[n] = true;
										//Assuming that there are two bogies per car
										bool IsOdd = (n % 2 != 0);
										int CarIndex = n / 2;
										if (n >= 0 & n < Train.Cars.Length * 2)
										{
											bool DefinedAxles = false;
											i++;
											while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
											{
												if (Lines[i].Length != 0)
												{
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0)
													{
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant())
														{
															case "object":
																if (Path.ContainsInvalidChars(b))
																{
																	Interface.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																}
																else
																{
																	if (string.IsNullOrEmpty(b))
																	{
																		Interface.AddMessage(MessageType.Error, true, "An empty bogie object was supplied at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		break;
																	}
																	string File = OpenBveApi.Path.CombineFile(TrainPath, b);
																	if (System.IO.File.Exists(File))
																	{
																		if (LoadObjects)
																		{
																			BogieObjects[n] = ObjectManager.LoadObject(File, Encoding, false, false, false);
																		}
																	}
																	else
																	{
																		Interface.AddMessage(MessageType.Error, true, "The bogie object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "length":
																{
																	Interface.AddMessage(MessageType.Error, false, "A defined length is not supported for bogies at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																}
																break;
															case "axles":
																{
																	int k = b.IndexOf(',');
																	if (k >= 0)
																	{
																		string c = b.Substring(0, k).TrimEnd();
																		string d = b.Substring(k + 1).TrimStart();
																		double rear, front;
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out rear))
																		{
																			Interface.AddMessage(MessageType.Error, false, "Rear is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																		else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front))
																		{
																			Interface.AddMessage(MessageType.Error, false, "Front is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																		else if (rear >= front)
																		{
																			Interface.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																		else
																		{
																			if (IsOdd)
																			{
																				Train.Cars[CarIndex].FrontBogie.RearAxle.Position = rear;
																				Train.Cars[CarIndex].FrontBogie.FrontAxle.Position = front;
																			}
																			else
																			{
																				Train.Cars[CarIndex].RearBogie.RearAxle.Position = rear;
																				Train.Cars[CarIndex].RearBogie.FrontAxle.Position = front;
																			}
																			DefinedAxles = true;
																		}
																	}
																	else
																	{
																		Interface.AddMessage(MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "reversed":
																BogieObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
																break;
															default:
																Interface.AddMessage(MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												}
												i++;
											}
											i--;
											if (!DefinedAxles)
											{
												if (IsOdd)
												{
													double AxleDistance = 0.4 * Train.Cars[CarIndex].FrontBogie.Length;
													Train.Cars[CarIndex].FrontBogie.RearAxle.Position = -AxleDistance;
													Train.Cars[CarIndex].FrontBogie.FrontAxle.Position = AxleDistance;
												}
												else
												{
													double AxleDistance = 0.4 * Train.Cars[CarIndex].RearBogie.Length;
													Train.Cars[CarIndex].RearBogie.RearAxle.Position = -AxleDistance;
													Train.Cars[CarIndex].RearBogie.FrontAxle.Position = AxleDistance;
												}
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "The car index " + t + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
								else
								{
									// default
									if (Lines.Length == 1 && Encoding.Equals(Encoding.Unicode))
									{
										/*
										 * If only one line, there's a good possibility that our file is NOT Unicode at all
										 * and that the misdetection has turned it into garbage
										 *
										 * Try again with ASCII instead
										 */
										ParseExtensionsConfig(TrainPath, Encoding.GetEncoding(1252), ref CarObjects, ref BogieObjects, Train, LoadObjects);
										return;
									}
									Interface.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								break;
						}
					}
				}

				// check for car objects and reverse if necessary
				int carObjects = 0;
				for (int i = 0; i < Train.Cars.Length; i++) {
					if (CarObjects[i] != null) {
						carObjects++;
						if (CarObjectsReversed[i] && LoadObjects) {
							{
								// reverse axle positions
								double temp = Train.Cars[i].FrontAxle.Position;
								Train.Cars[i].FrontAxle.Position = -Train.Cars[i].RearAxle.Position;
								Train.Cars[i].RearAxle.Position = -temp;
							}
							if (CarObjects[i] is ObjectManager.StaticObject) {
								ObjectManager.StaticObject obj = (ObjectManager.StaticObject)CarObjects[i];
								obj.ApplyScale(-1.0, 1.0, -1.0);
							} else if (CarObjects[i] is ObjectManager.AnimatedObjectCollection) {
								ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)CarObjects[i];
								for (int j = 0; j < obj.Objects.Length; j++) {
									for (int h = 0; h < obj.Objects[j].States.Length; h++) {
										obj.Objects[j].States[h].Object.ApplyScale(-1.0, 1.0, -1.0);
										obj.Objects[j].States[h].Position.X *= -1.0;
										obj.Objects[j].States[h].Position.Z *= -1.0;
									}
									obj.Objects[j].TranslateXDirection.X *= -1.0;
									obj.Objects[j].TranslateXDirection.Z *= -1.0;
									obj.Objects[j].TranslateYDirection.X *= -1.0;
									obj.Objects[j].TranslateYDirection.Z *= -1.0;
									obj.Objects[j].TranslateZDirection.X *= -1.0;
									obj.Objects[j].TranslateZDirection.Z *= -1.0;
								}
							} else {
								throw new NotImplementedException();
							}
						}
					}
				}
				
				//Check for bogie objects and reverse if necessary.....
				int bogieObjects = 0;
				for (int i = 0; i < Train.Cars.Length * 2; i++)
				{
					bool IsOdd = (i % 2 != 0);
					int CarIndex = i/2;
					if (BogieObjects[i] != null)
					{
						bogieObjects++;
						if (BogieObjectsReversed[i] && LoadObjects)
						{
							{
								// reverse axle positions
								if (IsOdd)
								{
									double temp = Train.Cars[CarIndex].FrontBogie.FrontAxle.Position;
									Train.Cars[CarIndex].FrontBogie.FrontAxle.Position = -Train.Cars[CarIndex].FrontBogie.RearAxle.Position;
									Train.Cars[CarIndex].FrontBogie.RearAxle.Position = -temp;
								}
								else
								{
									double temp = Train.Cars[CarIndex].RearBogie.FrontAxle.Position;
									Train.Cars[CarIndex].RearBogie.FrontAxle.Position = -Train.Cars[CarIndex].RearBogie.RearAxle.Position;
									Train.Cars[CarIndex].RearBogie.RearAxle.Position = -temp;
								}
							}
							if (BogieObjects[i] is ObjectManager.StaticObject)
							{
								ObjectManager.StaticObject obj = (ObjectManager.StaticObject)BogieObjects[i];
								obj.ApplyScale(-1.0, 1.0, -1.0);
							}
							else if (BogieObjects[i] is ObjectManager.AnimatedObjectCollection)
							{
								ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)BogieObjects[i];
								for (int j = 0; j < obj.Objects.Length; j++)
								{
									for (int h = 0; h < obj.Objects[j].States.Length; h++)
									{
										obj.Objects[j].States[h].Object.ApplyScale(-1.0, 1.0, -1.0);
										obj.Objects[j].States[h].Position.X *= -1.0;
										obj.Objects[j].States[h].Position.Z *= -1.0;
									}
									obj.Objects[j].TranslateXDirection.X *= -1.0;
									obj.Objects[j].TranslateXDirection.Z *= -1.0;
									obj.Objects[j].TranslateYDirection.X *= -1.0;
									obj.Objects[j].TranslateYDirection.Z *= -1.0;
									obj.Objects[j].TranslateZDirection.X *= -1.0;
									obj.Objects[j].TranslateZDirection.Z *= -1.0;
								}
							}
							else
							{
								throw new NotImplementedException();
							}
						}
					}
				}
				
				if (carObjects > 0 & carObjects < Train.Cars.Length) {
					Interface.AddMessage(MessageType.Warning, false, "An incomplete set of exterior objects was provided in file " + FileName);
				}
				
				if (bogieObjects > 0 & bogieObjects < Train.Cars.Length * 2)
				{
					Interface.AddMessage(MessageType.Warning, false, "An incomplete set of bogie objects was provided in file " + FileName);
				}
			}
		}

	}
}
