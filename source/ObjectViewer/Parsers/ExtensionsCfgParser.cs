using System;
using System.Globalization;
using System.IO;
using System.Text;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	internal static class ExtensionsCfgParser
	{
		internal static void ParseExtensionsConfig(string filePath, out UnifiedObject[] carObjects, out UnifiedObject[] bogieObjects, out UnifiedObject[] couplerObjects, out double[] axleLocations, out double[] couplerDistances, out TrainManager.Train train, bool loadObjects)
		{
			ParseExtensionsConfig(filePath, null, out carObjects, out bogieObjects, out couplerObjects, out axleLocations, out couplerDistances, out train, loadObjects);
		}

		private static void ParseExtensionsConfig(string filePath, Encoding encoding, out UnifiedObject[] carObjects, out UnifiedObject[] bogieObjects, out UnifiedObject[] couplerObjects, out double[] axleLocations, out double[] couplerDistances, out TrainManager.Train train, bool loadObjects)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;
			carObjects = new UnifiedObject[0];
			bogieObjects = new UnifiedObject[0];
			couplerObjects = new UnifiedObject[0];
			axleLocations = new double[0];
			couplerDistances = new double[0];
			train = new TrainManager.Train { Cars = new TrainManager.Car[0] };

			if (!File.Exists(filePath))
			{
				return;
			}

			bool[] carObjectsReversed = new bool[0];
			bool[] bogieObjectsReversed = new bool[0];
			bool[] carsDefined = new bool[0];
			bool[] bogiesDefined = new bool[0];
			bool[] couplerDefined = new bool[0];

			string trainPath = System.IO.Path.GetDirectoryName(filePath);

			if (encoding == null)
			{
				encoding = TextEncoding.GetSystemEncodingFromFile(trainPath);
			}

			string[] lines = File.ReadAllLines(filePath, encoding);
			for (int i = 0; i < lines.Length; i++)
			{
				int j = lines[i].IndexOf(';');
				if (j >= 0)
				{
					lines[i] = lines[i].Substring(0, j).Trim(new char[] { });
				}
				else
				{
					lines[i] = lines[i].Trim(new char[] { });
				}
			}

			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Length != 0)
				{
					switch (lines[i].ToLowerInvariant())
					{
						case "[exterior]":
							// exterior
							i++;
							while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								if (lines[i].Length != 0)
								{
									int j = lines[i].IndexOf("=", StringComparison.Ordinal);
									if (j >= 0)
									{
										// ReSharper disable RedundantExplicitParamsArrayCreation
										string a = lines[i].Substring(0, j).TrimEnd(new char[] { });
										string b = lines[i].Substring(j + 1).TrimStart(new char[] { });
										// ReSharper restore RedundantExplicitParamsArrayCreation

										int n;
										if (int.TryParse(a, NumberStyles.Integer, culture, out n))
										{
											if (n >= 0)
											{
												if (n >= train.Cars.Length)
												{
													Array.Resize(ref train.Cars, n + 1);
													train.Cars[n] = new TrainManager.Car(train);
													Array.Resize(ref carObjects, n + 1);
													Array.Resize(ref bogieObjects, (n + 1) * 2);
													Array.Resize(ref couplerObjects, n);
													Array.Resize(ref carObjectsReversed, n + 1);
													Array.Resize(ref bogieObjectsReversed, (n + 1) * 2);
													Array.Resize(ref carsDefined, n + 1);
													Array.Resize(ref bogiesDefined, (n + 1) * 2);
													Array.Resize(ref couplerDefined, n);
													Array.Resize(ref axleLocations, (n + 1) * 2);
													Array.Resize(ref couplerDistances, n);
												}
												if (Path.ContainsInvalidChars(b))
												{
													Interface.AddMessage(MessageType.Error, false, $"File contains illegal characters at line {(i + 1).ToString(culture)} in file {filePath}");
												}
												else
												{
													string file = Path.CombineFile(trainPath, b);
													if (File.Exists(file))
													{
														if (loadObjects)
														{
															Program.CurrentHost.LoadObject(file, encoding, out carObjects[n]);
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, true, $"The car object {file} does not exist at line {(i + 1).ToString(culture)} in file {filePath}");
													}
												}
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, $"The car index {a} does not reference an existing car at line {(i + 1).ToString(culture)} in file {filePath}");
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, $"The car index is expected to be an integer at line {(i + 1).ToString(culture)} in file {filePath}");
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {filePath}");
									}
								}
								i++;
							}
							i--;
							break;
						default:
							if (lines[i].StartsWith("[car", StringComparison.OrdinalIgnoreCase) & lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// car
								string t = lines[i].Substring(4, lines[i].Length - 5);
								int n;
								if (int.TryParse(t, NumberStyles.Integer, culture, out n))
								{
									if (n >= 0)
									{
										if (n >= train.Cars.Length)
										{
											Array.Resize(ref train.Cars, n + 1);
											train.Cars[n] = new TrainManager.Car(train);
											Array.Resize(ref carObjects, n + 1);
											Array.Resize(ref bogieObjects, (n + 1) * 2);
											Array.Resize(ref carObjectsReversed, n + 1);
											Array.Resize(ref bogieObjectsReversed, (n + 1) * 2);
											Array.Resize(ref couplerObjects, n);
											Array.Resize(ref carsDefined, n + 1);
											Array.Resize(ref bogiesDefined, (n + 1) * 2);
											Array.Resize(ref couplerDefined, n);
											Array.Resize(ref axleLocations, (n + 1) * 2);
											Array.Resize(ref couplerDistances, n);
										}
										if (carsDefined[n])
										{
											Interface.AddMessage(MessageType.Error, false, $"Car {n.ToString(culture)} has already been declared at line {(i + 1).ToString(culture)} in file {filePath}");
										}
										carsDefined[n] = true;
										i++;
										while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (lines[i].Length != 0)
											{
												int j = lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													// ReSharper disable RedundantExplicitParamsArrayCreation
													string a = lines[i].Substring(0, j).TrimEnd(new char[] { });
													string b = lines[i].Substring(j + 1).TrimStart(new char[] { });
													// ReSharper restore RedundantExplicitParamsArrayCreation

													switch (a.ToLowerInvariant())
													{
														case "object":
															if (string.IsNullOrEmpty(b))
															{
																Interface.AddMessage(MessageType.Error, true, $"An empty car object was supplied at line {(i + 1).ToString(culture)} in file {filePath}");
																break;
															}
															if (Path.ContainsInvalidChars(b))
															{
																Interface.AddMessage(MessageType.Error, false, $"File contains illegal characters at line {(i + 1).ToString(culture)} in file {filePath}");
															}
															else
															{
																string file = Path.CombineFile(trainPath, b);
																if (File.Exists(file))
																{
																	if (loadObjects)
																	{
																		Program.CurrentHost.LoadObject(file, encoding, out carObjects[n]);
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, true, $"The car object {file} does not exist at line {(i + 1).ToString(culture)} in file {filePath}");
																}
															}
															break;
														case "length":
															{
																double m;
																if (double.TryParse(b, NumberStyles.Float, culture, out m))
																{
																	if (m > 0.0)
																	{
																		train.Cars[n].Length = m;
																	}
																	else
																	{
																		Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a positive floating-point number in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a positive floating-point number in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																}
															}
															break;
														case "reversed":
															carObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
														case "axles":
															int k = b.IndexOf(',');
															if (k >= 0)
															{
																// ReSharper disable RedundantExplicitParamsArrayCreation
																string c = b.Substring(0, k).TrimEnd(new char[] { });
																string d = b.Substring(k + 1).TrimStart(new char[] { });
																// ReSharper restore RedundantExplicitParamsArrayCreation

																double rear, front;
																if (!double.TryParse(c, NumberStyles.Float, culture, out rear))
																{
																	Interface.AddMessage(MessageType.Error, false, $"Rear is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																}
																else if (!double.TryParse(d, NumberStyles.Float, culture, out front))
																{
																	Interface.AddMessage(MessageType.Error, false, $"Front is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																}
																else if (rear >= front)
																{
																	Interface.AddMessage(MessageType.Error, false, $"Rear is expected to be less than Front in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																}
																else
																{
																	if (n == 0)
																	{
																		axleLocations[n] = rear;
																		axleLocations[n + 1] = front;
																	}
																	else
																	{
																		axleLocations[n * 2] = rear;
																		axleLocations[n * 2 + 1] = front;
																	}


																}
															}
															else
															{
																Interface.AddMessage(MessageType.Error, false, $"An argument-separating comma is expected in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
															}
															break;
														default:
															Interface.AddMessage(MessageType.Warning, false, $"Unsupported key-value pair {a} encountered at line {(i + 1).ToString(culture)} in file {filePath}");
															break;
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {filePath}");
												}
											}
											i++;
										}
										i--;
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"The car index {t} does not reference an existing car at line {(i + 1).ToString(culture)} in file {filePath}");
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, $"The car index is expected to be an integer at line {(i + 1).ToString(culture)} in file {filePath}");
								}
							}
							else if (lines[i].StartsWith("[bogie", StringComparison.OrdinalIgnoreCase) & lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// bogie
								string t = lines[i].Substring(6, lines[i].Length - 7);
								int n;
								if (int.TryParse(t, NumberStyles.Integer, culture, out n))
								{
									if (n >= train.Cars.Length * 2)
									{
										Array.Resize(ref train.Cars, n / 2 + 1);
										if (n == 0)
										{
											train.Cars[0] = new TrainManager.Car(train);
											Array.Resize(ref axleLocations, 2);
										}
										else
										{
											train.Cars[n / 2] = new TrainManager.Car(train);
											Array.Resize(ref axleLocations, (n / 2 + 1) * 2);
										}

										Array.Resize(ref carObjects, n / 2 + 1);
										Array.Resize(ref bogieObjects, n + 2);
										Array.Resize(ref couplerObjects, n / 2);
										Array.Resize(ref carObjectsReversed, n / 2 + 1);
										Array.Resize(ref bogieObjectsReversed, n + 2);
										Array.Resize(ref carsDefined, n / 2 + 1);
										Array.Resize(ref bogiesDefined, n + 2);
										Array.Resize(ref couplerDefined, n / 2);
										Array.Resize(ref couplerDistances, n / 2);
									}

									if (n > bogiesDefined.Length - 1)
									{
										continue;
									}
									if (bogiesDefined[n])
									{
										Interface.AddMessage(MessageType.Error, false, $"Bogie {n.ToString(culture)} has already been declared at line {(i + 1).ToString(culture)} in file {filePath}");
									}
									bogiesDefined[n] = true;
									//Assuming that there are two bogies per car
									if (n >= 0 & n < train.Cars.Length * 2)
									{
										i++;
										while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (lines[i].Length != 0)
											{
												int j = lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													// ReSharper disable RedundantExplicitParamsArrayCreation
													string a = lines[i].Substring(0, j).TrimEnd(new char[] { });
													string b = lines[i].Substring(j + 1).TrimStart(new char[] { });
													// ReSharper restore RedundantExplicitParamsArrayCreation

													switch (a.ToLowerInvariant())
													{
														case "object":
															if (Path.ContainsInvalidChars(b))
															{
																Interface.AddMessage(MessageType.Error, false, $"File contains illegal characters at line {(i + 1).ToString(culture)} in file {filePath}");
															}
															else
															{
																if (string.IsNullOrEmpty(b))
																{
																	Interface.AddMessage(MessageType.Error, true, $"An empty bogie object was supplied at line {(i + 1).ToString(culture)} in file {filePath}");
																	break;
																}
																string file = Path.CombineFile(trainPath, b);
																if (File.Exists(file))
																{
																	if (loadObjects)
																	{
																		Program.CurrentHost.LoadObject(file, encoding, out bogieObjects[n]);
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, true, $"The bogie object {file} does not exist at line {(i + 1).ToString(culture)} in file {filePath}");
																}
															}
															break;
														case "length":
															{
																Interface.AddMessage(MessageType.Error, false, $"A defined length is not supported for bogies at line {(i + 1).ToString(culture)} in file {filePath}");
															}
															break;
														case "reversed":
															bogieObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
														case "axles":
															//Axles aren't used in bogie positioning, just in rotation
															break;
														default:
															Interface.AddMessage(MessageType.Warning, false, $"Unsupported key-value pair {a} encountered at line {(i + 1).ToString(culture)} in file {filePath}");
															break;
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {filePath}");
												}
											}
											i++;
										}
										i--;
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"The bogie index {t} does not reference an existing bogie at line {(i + 1).ToString(culture)} in file {filePath}");
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, $"The bogie index is expected to be an integer at line {(i + 1).ToString(culture)} in file {filePath}");
								}
							}
							else if (lines[i].StartsWith("[coupler", StringComparison.OrdinalIgnoreCase) & lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// coupler
								string t = lines[i].Substring(8, lines[i].Length - 9);
								int n;
								if (int.TryParse(t, NumberStyles.Integer, culture, out n))
								{
									if (n >= 0)
									{
										if (n >= train.Cars.Length - 1)
										{
											Array.Resize(ref train.Cars, n + 2);
											train.Cars[n + 1] = new TrainManager.Car(train);
											Array.Resize(ref carObjects, n + 2);
											Array.Resize(ref bogieObjects, (n + 2) * 2);
											Array.Resize(ref carObjectsReversed, n + 2);
											Array.Resize(ref bogieObjectsReversed, (n + 2) * 2);
											Array.Resize(ref couplerObjects, n + 1);
											Array.Resize(ref carsDefined, n + 2);
											Array.Resize(ref bogiesDefined, (n + 2) * 2);
											Array.Resize(ref couplerDefined, n + 1);
											Array.Resize(ref axleLocations, (n + 2) * 2);
											Array.Resize(ref couplerDistances, n + 1);
										}
										if (couplerDefined[n])
										{
											Interface.AddMessage(MessageType.Error, false, $"Coupler {n.ToString(culture)} has already been declared at line {(i + 1).ToString(culture)} in file {filePath}");
										}
										couplerDefined[n] = true;
										i++;
										while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (lines[i].Length != 0)
											{
												int j = lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													// ReSharper disable RedundantExplicitParamsArrayCreation
													string a = lines[i].Substring(0, j).TrimEnd(new char[] { });
													string b = lines[i].Substring(j + 1).TrimStart(new char[] { });
													// ReSharper restore RedundantExplicitParamsArrayCreation

													switch (a.ToLowerInvariant())
													{
														case "distances":
															{
																int k = b.IndexOf(',');
																if (k >= 0)
																{
																	// ReSharper disable RedundantExplicitParamsArrayCreation
																	string c = b.Substring(0, k).TrimEnd(new char[] { });
																	string d = b.Substring(k + 1).TrimStart(new char[] { });
																	// ReSharper restore RedundantExplicitParamsArrayCreation

																	double min, max;
																	if (!double.TryParse(c, NumberStyles.Float, culture, out min))
																	{
																		Interface.AddMessage(MessageType.Error, false, $"Minimum is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																	}
																	else if (!double.TryParse(d, NumberStyles.Float, culture, out max))
																	{
																		Interface.AddMessage(MessageType.Error, false, $"Maximum is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																	}
																	else if (min > max)
																	{
																		Interface.AddMessage(MessageType.Error, false, $"Minimum is expected to be less than Maximum in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																	}
																	else
																	{
																		couplerDistances[n] = 0.5 * (min + max);
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, false, $"An argument-separating comma is expected in {a} at line {(i + 1).ToString(culture)} in file {filePath}");
																}
															}
															break;
														case "object":
															if (string.IsNullOrEmpty(b))
															{
																Interface.AddMessage(MessageType.Error, true, $"An empty coupler object was supplied at line {(i + 1).ToString(culture)} in file {filePath}");
																break;
															}
															if (Path.ContainsInvalidChars(b))
															{
																Interface.AddMessage(MessageType.Error, false, $"File contains illegal characters at line {(i + 1).ToString(culture)} in file {filePath}");
															}
															else
															{
																string file = Path.CombineFile(trainPath, b);
																if (File.Exists(file))
																{
																	if (loadObjects)
																	{
																		Program.CurrentHost.LoadObject(file, encoding, out couplerObjects[n]);
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, true, $"The coupler object {file} does not exist at line {(i + 1).ToString(culture)} in file {filePath}");
																}
															}
															break;
														default:
															Interface.AddMessage(MessageType.Warning, false, $"Unsupported key-value pair {a} encountered at line {(i + 1).ToString(culture)} in file {filePath}");
															break;
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {filePath}");
												}
											}
											i++;
										}
										i--;
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"The coupler index {t} does not reference an existing coupler at line {(i + 1).ToString(culture)} in file {filePath}");
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, $"The coupler index is expected to be an integer at line {(i + 1).ToString(culture)} in file {filePath}");
								}
							}
							else
							{
								// default
								if (lines.Length == 1 && encoding.Equals(Encoding.Unicode))
								{
									/*
									 * If only one line, there's a good possibility that our file is NOT Unicode at all
									 * and that the misdetection has turned it into garbage
									 *
									 * Try again with ASCII instead
									 */
									ParseExtensionsConfig(filePath, Encoding.GetEncoding(1252), out carObjects, out bogieObjects, out couplerObjects, out axleLocations, out couplerDistances, out train, loadObjects);
									return;
								}
								Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {filePath}");
							}
							break;
					}
				}
			}

			// check for car objects and reverse if necessary
			int carObjectsCount = 0;
			for (int i = 0; i < train.Cars.Length; i++)
			{
				if (carObjects[i] != null)
				{
					carObjectsCount++;
					if (carObjectsReversed[i] && loadObjects)
					{
						if (carObjects[i] is StaticObject)
						{
							StaticObject obj = (StaticObject)carObjects[i];
							obj.ApplyScale(-1.0, 1.0, -1.0);
						}
						else if (carObjects[i] is AnimatedObjectCollection)
						{
							AnimatedObjectCollection obj = (AnimatedObjectCollection)carObjects[i];
							foreach (AnimatedObject animatedObj in obj.Objects)
							{
								foreach (ObjectState state in animatedObj.States)
								{
									state.Prototype.ApplyScale(-1.0, 1.0, -1.0);
									Matrix4D t = state.Translation;
									t.Row3.X *= -1.0f;
									t.Row3.Z *= -1.0f;
									state.Translation = t;
								}
								animatedObj.TranslateXDirection.X *= -1.0;
								animatedObj.TranslateXDirection.Z *= -1.0;
								animatedObj.TranslateYDirection.X *= -1.0;
								animatedObj.TranslateYDirection.Z *= -1.0;
								animatedObj.TranslateZDirection.X *= -1.0;
								animatedObj.TranslateZDirection.Z *= -1.0;
							}
						}
						else
						{
							throw new NotImplementedException();
						}
					}
				}
			}

			//Check for bogie objects and reverse if necessary.....
			int bogieObjectsCount = 0;
			for (int i = 0; i < train.Cars.Length * 2; i++)
			{
				if (bogieObjects[i] != null)
				{
					bogieObjectsCount++;
					if (bogieObjectsReversed[i] && loadObjects)
					{
						if (bogieObjects[i] is StaticObject)
						{
							StaticObject obj = (StaticObject)bogieObjects[i];
							obj.ApplyScale(-1.0, 1.0, -1.0);
						}
						else if (bogieObjects[i] is AnimatedObjectCollection)
						{
							AnimatedObjectCollection obj = (AnimatedObjectCollection)bogieObjects[i];
							foreach (AnimatedObject animatedObj in obj.Objects)
							{
								foreach (ObjectState state in animatedObj.States)
								{
									state.Prototype.ApplyScale(-1.0, 1.0, -1.0);
									Matrix4D t = state.Translation;
									t.Row3.X *= -1.0f;
									t.Row3.Z *= -1.0f;
									state.Translation = t;
								}
								animatedObj.TranslateXDirection.X *= -1.0;
								animatedObj.TranslateXDirection.Z *= -1.0;
								animatedObj.TranslateYDirection.X *= -1.0;
								animatedObj.TranslateYDirection.Z *= -1.0;
								animatedObj.TranslateZDirection.X *= -1.0;
								animatedObj.TranslateZDirection.Z *= -1.0;
							}
						}
						else
						{
							throw new NotImplementedException();
						}
					}
				}
			}

			if (carObjectsCount > 0 & carObjectsCount < train.Cars.Length)
			{
				Interface.AddMessage(MessageType.Warning, false, $"An incomplete set of exterior objects was provided in file {filePath}");
			}

			if (bogieObjectsCount > 0 & bogieObjectsCount < train.Cars.Length * 2)
			{
				Interface.AddMessage(MessageType.Warning, false, $"An incomplete set of bogie objects was provided in file {filePath}");
			}
		}
	}
}
