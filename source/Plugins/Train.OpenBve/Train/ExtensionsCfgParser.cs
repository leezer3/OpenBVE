using System;
using System.Text;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal class ExtensionsCfgParser
	{
		internal readonly Plugin Plugin;

		internal ExtensionsCfgParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		// parse extensions config
		internal void ParseExtensionsConfig(string TrainPath, Encoding Encoding, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, ref UnifiedObject[] CouplerObjects, out bool[] VisibleFromInterior, TrainBase Train)
		{
			VisibleFromInterior = new bool[Train.Cars.Length];
			bool[] CarObjectsReversed = new bool[Train.Cars.Length];
			bool[] BogieObjectsReversed = new bool[Train.Cars.Length * 2];
			bool[] CarsDefined = new bool[Train.Cars.Length];
			bool[] BogiesDefined = new bool[Train.Cars.Length * 2];
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = Path.CombineFile(TrainPath, "extensions.cfg");
			if (System.IO.File.Exists(FileName)) {
				Encoding = TextEncoding.GetSystemEncodingFromFile(FileName, Encoding);

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
										if (j >= 0)
										{
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											int n;
											if (int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out n)) {
												if (n >= 0 & n < Train.Cars.Length) {
													if (Path.ContainsInvalidChars(b)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} else {
														string File = Path.CombineFile(TrainPath, b);
														if (System.IO.File.Exists(File)) {
															Plugin.currentHost.LoadObject(File, Encoding, out CarObjects[n]);
														} else {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "The car object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
												} else {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "The car index " + a + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												}
											} else {
												Plugin.currentHost.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											}
										} else {
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Car " + n.ToString(Culture) + " has already been declared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											}
											CarsDefined[n] = true;
											bool DefinedLength = false;
											bool DefinedAxles = false;
											i++;
											while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
												if (Lines[i].Length != 0) {
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0)
													{
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant()) {
															case "object":
																if (string.IsNullOrEmpty(b))
																{
																	Plugin.currentHost.AddMessage(MessageType.Error, true, "An empty car object was supplied at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	break;
																}
																if (Path.ContainsInvalidChars(b)) {
																	Plugin.currentHost.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																} else {
																	string File = Path.CombineFile(TrainPath, b);
																	if (System.IO.File.Exists(File)) {
																		Plugin.currentHost.LoadObject(File, Encoding, out CarObjects[n]);
																	} else {
																		Plugin.currentHost.AddMessage(MessageType.Error, true, "The car object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																	} else {
																		Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
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
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out rear)) {
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Rear is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front)) {
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Front is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (rear >= front) {
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else {
																			Train.Cars[n].RearAxle.Position = rear;
																			Train.Cars[n].FrontAxle.Position = front;
																			DefinedAxles = true;
																		}
																	} else {
																		Plugin.currentHost.AddMessage(MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "reversed":
																CarObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
																break;
															case "loadingsway":
																Train.Cars[n].EnableLoadingSway = b.Equals("true", StringComparison.OrdinalIgnoreCase);
																break;
															case "visiblefrominterior":
																VisibleFromInterior[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
																break;
															default:
																Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
											Plugin.currentHost.AddMessage(MessageType.Error, false, "The car index " + t + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									} else {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else if (Lines[i].StartsWith("[coupler", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									// coupler
									string t = Lines[i].Substring(8, Lines[i].Length - 9);
									int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n)) {
										if (n >= 0 & n < Train.Cars.Length -1) {
											i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
												if (Lines[i].Length != 0) {
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0)
													{
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant()) {
															case "distances":
																{
																	int k = b.IndexOf(',');
																	if (k >= 0)
																	{
																		string c = b.Substring(0, k).TrimEnd();
																		string d = b.Substring(k + 1).TrimStart();
																		double min, max;
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out min)) {
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Minimum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out max)) {
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Maximum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (min > max) {
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Minimum is expected to be less than Maximum in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else {
																			Train.Cars[n].Coupler.MinimumDistanceBetweenCars = min;
																			Train.Cars[n].Coupler.MaximumDistanceBetweenCars = max;
																		}
																	} else {
																		Plugin.currentHost.AddMessage(MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																} break;
															case "object":
																if (string.IsNullOrEmpty(b))
																{
																	Plugin.currentHost.AddMessage(MessageType.Error, true, "An empty coupler object was supplied at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	break;
																}
																if (Path.ContainsInvalidChars(b)) {
																	Plugin.currentHost.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																} else {
																	string File = Path.CombineFile(TrainPath, b);
																	if (System.IO.File.Exists(File)) {
																		Plugin.currentHost.LoadObject(File, Encoding, out CouplerObjects[n]);
																	} else {
																		Plugin.currentHost.AddMessage(MessageType.Error, true, "The coupler object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															default:
																Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} i++;
											} i--;
										} else {
											Plugin.currentHost.AddMessage(MessageType.Error, false, "The coupler index " + t + " does not reference an existing coupler at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									} else {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "The coupler index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Bogie " + n.ToString(Culture) + " has already been declared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
																	Plugin.currentHost.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																}
																else
																{
																	if (string.IsNullOrEmpty(b))
																	{
																		Plugin.currentHost.AddMessage(MessageType.Error, true, "An empty bogie object was supplied at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		break;
																	}
																	string File = Path.CombineFile(TrainPath, b);
																	if (System.IO.File.Exists(File))
																	{
																		Plugin.currentHost.LoadObject(File, Encoding, out BogieObjects[n]);
																	}
																	else
																	{
																		Plugin.currentHost.AddMessage(MessageType.Error, true, "The bogie object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "length":
																{
																	Plugin.currentHost.AddMessage(MessageType.Error, false, "A defined length is not supported for bogies at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Rear is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																		else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front))
																		{
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Front is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																		else if (rear >= front)
																		{
																			Plugin.currentHost.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
																		Plugin.currentHost.AddMessage(MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "reversed":
																BogieObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
																break;
															default:
																Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													}
													else
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
											Plugin.currentHost.AddMessage(MessageType.Error, false, "The car index " + t + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
										ParseExtensionsConfig(TrainPath, Encoding.GetEncoding(1252), ref CarObjects, ref BogieObjects, ref CouplerObjects, out VisibleFromInterior, Train);
										return;
									}
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
						if (CarObjectsReversed[i]) {
							{
								// reverse axle positions
								double temp = Train.Cars[i].FrontAxle.Position;
								Train.Cars[i].FrontAxle.Position = -Train.Cars[i].RearAxle.Position;
								Train.Cars[i].RearAxle.Position = -temp;
							}
							if (CarObjects[i] is StaticObject) {
								StaticObject obj = (StaticObject)CarObjects[i].Clone();
								obj.ApplyScale(-1.0, 1.0, -1.0);
								CarObjects[i] = obj;
							} else if (CarObjects[i] is AnimatedObjectCollection) {
								AnimatedObjectCollection obj = (AnimatedObjectCollection)CarObjects[i].Clone();
								obj.Reverse();
								CarObjects[i] = obj;
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
						if (BogieObjectsReversed[i])
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
							if (BogieObjects[i] is StaticObject)
							{
								StaticObject obj = (StaticObject)BogieObjects[i].Clone();
								obj.ApplyScale(-1.0, 1.0, -1.0);
								BogieObjects[i] = obj;
							}
							else if (BogieObjects[i] is AnimatedObjectCollection)
							{
								AnimatedObjectCollection obj = (AnimatedObjectCollection)BogieObjects[i].Clone();
								obj.Reverse();
								BogieObjects[i] = obj;
							}
							else
							{
								throw new NotImplementedException();
							}
						}
					}
				}
				
				if (carObjects > 0 & carObjects < Train.Cars.Length) {
					Plugin.currentHost.AddMessage(MessageType.Warning, false, "An incomplete set of exterior objects was provided in file " + FileName);
				}
				
				if (bogieObjects > 0 & bogieObjects < Train.Cars.Length * 2)
				{
					Plugin.currentHost.AddMessage(MessageType.Warning, false, "An incomplete set of bogie objects was provided in file " + FileName);
				}
			}
		}

	}
}
