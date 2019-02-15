using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenBveApi;
using OpenBveApi.Objects;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static class ExtensionsCfgParser
	{
		internal static void ParseExtensionsConfig(string filePath, Encoding encoding, out UnifiedObject[] carObjects, out UnifiedObject[] bogieObjects, out TrainManager.Train train, bool loadObjects)
		{
			carObjects = new UnifiedObject[] { };
			bogieObjects = new UnifiedObject[] { };
			train = new TrainManager.Train();

			if (!System.IO.File.Exists(filePath))
			{
				return;
			}

			train.Cars = new TrainManager.Car[] { };
			bool[] carObjectsReversed = new bool[train.Cars.Length];
			bool[] bogieObjectsReversed = new bool[train.Cars.Length * 2];
			bool[] carsDefined = new bool[train.Cars.Length];
			bool[] bogiesDefined = new bool[train.Cars.Length * 2];

			string trainPath = System.IO.Path.GetDirectoryName(filePath);
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			TextEncoding.Encoding newEncoding = TextEncoding.GetEncodingFromFile(filePath);
			if (newEncoding != TextEncoding.Encoding.Unknown)
			{
				switch (newEncoding)
				{
					case TextEncoding.Encoding.Utf7:
						encoding = Encoding.UTF7;
						break;
					case TextEncoding.Encoding.Utf8:
						encoding = Encoding.UTF8;
						break;
					case TextEncoding.Encoding.Utf16Le:
						encoding = Encoding.Unicode;
						break;
					case TextEncoding.Encoding.Utf16Be:
						encoding = Encoding.BigEndianUnicode;
						break;
					case TextEncoding.Encoding.Utf32Le:
						encoding = Encoding.UTF32;
						break;
					case TextEncoding.Encoding.Utf32Be:
						encoding = Encoding.GetEncoding(12001);
						break;
					case TextEncoding.Encoding.Shift_JIS:
						encoding = Encoding.GetEncoding(932);
						break;
				}
			}

			string[] lines = System.IO.File.ReadAllLines(filePath, encoding);
			for (int i = 0; i < lines.Length; i++)
			{
				int j = lines[i].IndexOf(';');
				if (j >= 0)
				{
					lines[i] = lines[i].Substring(0, j).Trim();
				}
				else
				{
					lines[i] = lines[i].Trim();
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
										string a = lines[i].Substring(0, j).TrimEnd();
										string b = lines[i].Substring(j + 1).TrimStart();
										int n;
										if (int.TryParse(a, System.Globalization.NumberStyles.Integer, culture, out n))
										{
											if (n >= 0)
											{
												if (n >= train.Cars.Length)
												{
													Array.Resize(ref train.Cars, n + 1);
													Array.Resize(ref carObjects, n + 1);
													Array.Resize(ref bogieObjects, (n + 1) * 2);
													Array.Resize(ref carObjectsReversed, n + 1);
													Array.Resize(ref bogieObjectsReversed, (n + 1) * 2);
													Array.Resize(ref carsDefined, n + 1);
													Array.Resize(ref bogiesDefined, (n + 1) * 2);
												}
												if (Path.ContainsInvalidChars(b))
												{
													Interface.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(culture) + " in file " + filePath);
												}
												else
												{
													string file = OpenBveApi.Path.CombineFile(trainPath, b);
													if (System.IO.File.Exists(file))
													{
														if (loadObjects)
														{
															carObjects[n] = ObjectManager.LoadObject(file, encoding, false, false, false);
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, true, "The car object " + file + " does not exist at line " + (i + 1).ToString(culture) + " in file " + filePath);
													}
												}
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, "The car index " + a + " does not reference an existing car at line " + (i + 1).ToString(culture) + " in file " + filePath);
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(culture) + " in file " + filePath);
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, "Invalid statement " + lines[i] + " encountered at line " + (i + 1).ToString(culture) + " in file " + filePath);
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
								if (int.TryParse(t, System.Globalization.NumberStyles.Integer, culture, out n))
								{
									if (n >= 0)
									{
										if (n >= train.Cars.Length)
										{
											Array.Resize(ref train.Cars, n + 1);
											Array.Resize(ref carObjects, n + 1);
											Array.Resize(ref bogieObjects, (n + 1) * 2);
											Array.Resize(ref carObjectsReversed, n + 1);
											Array.Resize(ref bogieObjectsReversed, (n + 1) * 2);
											Array.Resize(ref carsDefined, n + 1);
											Array.Resize(ref bogiesDefined, (n + 1) * 2);
										}
										if (carsDefined[n])
										{
											Interface.AddMessage(MessageType.Error, false, "Car " + n.ToString(culture) + " has already been declared at line " + (i + 1).ToString(culture) + " in file " + filePath);
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
													string a = lines[i].Substring(0, j).TrimEnd();
													string b = lines[i].Substring(j + 1).TrimStart();
													switch (a.ToLowerInvariant())
													{
														case "object":
															if (string.IsNullOrEmpty(b))
															{
																Interface.AddMessage(MessageType.Error, true, "An empty car object was supplied at line " + (i + 1).ToString(culture) + " in file " + filePath);
																break;
															}
															if (Path.ContainsInvalidChars(b))
															{
																Interface.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(culture) + " in file " + filePath);
															}
															else
															{
																string file = OpenBveApi.Path.CombineFile(trainPath, b);
																if (System.IO.File.Exists(file))
																{
																	if (loadObjects)
																	{
																		carObjects[n] = ObjectManager.LoadObject(file, encoding, false, false, false);
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, true, "The car object " + file + " does not exist at line " + (i + 1).ToString(culture) + " in file " + filePath);
																}
															}
															break;
														case "length":
															{
																double m;
																if (double.TryParse(b, System.Globalization.NumberStyles.Float, culture, out m))
																{
																	if (m > 0.0)
																	{
																		train.Cars[n].Length = m;
																	}
																	else
																	{
																		Interface.AddMessage(MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(culture) + " in file " + filePath);
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(culture) + " in file " + filePath);
																}
															}
															break;
														case "reversed":
															carObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
														case "axles":
															Interface.AddMessage(MessageType.Information, false, "Axle declaration for car " + n + " will be ignored whilst viewing an extensions.cfg in file " + filePath);
															break;
														default:
															Interface.AddMessage(MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(culture) + " in file " + filePath);
															break;
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, "Invalid statement " + lines[i] + " encountered at line " + (i + 1).ToString(culture) + " in file " + filePath);
												}
											}
											i++;
										}
										i--;
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, "The car index " + t + " does not reference an existing car at line " + (i + 1).ToString(culture) + " in file " + filePath);
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(culture) + " in file " + filePath);
								}
							}
							else if (lines[i].StartsWith("[bogie", StringComparison.OrdinalIgnoreCase) & lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// car
								string t = lines[i].Substring(6, lines[i].Length - 7);
								int n;
								if (int.TryParse(t, System.Globalization.NumberStyles.Integer, culture, out n))
								{
									if (n >= train.Cars.Length * 2)
									{
										Array.Resize(ref train.Cars, n / 2 + 1);
										Array.Resize(ref carObjects, n / 2 + 1);
										Array.Resize(ref bogieObjects, n + 2);
										Array.Resize(ref carObjectsReversed, n / 2 + 1);
										Array.Resize(ref bogieObjectsReversed, n + 2);
										Array.Resize(ref carsDefined, n / 2 + 1);
										Array.Resize(ref bogiesDefined, n + 2);
									}

									if (n > bogiesDefined.Length - 1)
									{
										continue;
									}
									if (bogiesDefined[n])
									{
										Interface.AddMessage(MessageType.Error, false, "Bogie " + n.ToString(culture) + " has already been declared at line " + (i + 1).ToString(culture) + " in file " + filePath);
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
													string a = lines[i].Substring(0, j).TrimEnd();
													string b = lines[i].Substring(j + 1).TrimStart();
													switch (a.ToLowerInvariant())
													{
														case "object":
															if (Path.ContainsInvalidChars(b))
															{
																Interface.AddMessage(MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(culture) + " in file " + filePath);
															}
															else
															{
																if (string.IsNullOrEmpty(b))
																{
																	Interface.AddMessage(MessageType.Error, true, "An empty bogie object was supplied at line " + (i + 1).ToString(culture) + " in file " + filePath);
																	break;
																}
																string file = OpenBveApi.Path.CombineFile(trainPath, b);
																if (System.IO.File.Exists(file))
																{
																	if (loadObjects)
																	{
																		bogieObjects[n] = ObjectManager.LoadObject(file, encoding, false, false, false);
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, true, "The bogie object " + file + " does not exist at line " + (i + 1).ToString(culture) + " in file " + filePath);
																}
															}
															break;
														case "length":
															{
																Interface.AddMessage(MessageType.Error, false, "A defined length is not supported for bogies at line " + (i + 1).ToString(culture) + " in file " + filePath);
															}
															break;
														case "reversed":
															bogieObjectsReversed[n] = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
														default:
															Interface.AddMessage(MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(culture) + " in file " + filePath);
															break;
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, "Invalid statement " + lines[i] + " encountered at line " + (i + 1).ToString(culture) + " in file " + filePath);
												}
											}
											i++;
										}
										i--;
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, "The car index " + t + " does not reference an existing car at line " + (i + 1).ToString(culture) + " in file " + filePath);
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(culture) + " in file " + filePath);
								}
							}
							else if (lines[i].StartsWith("[coupler", StringComparison.OrdinalIgnoreCase) & lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								i++;
								while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
								{
									/*
									 * Coupler statments are currently not supported in Object Viewer
									 */
									i++;
								}

								i--;
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
									ParseExtensionsConfig(filePath, Encoding.GetEncoding(1252), out carObjects, out bogieObjects, out train, loadObjects);
									return;
								}
								Interface.AddMessage(MessageType.Error, false, "Invalid statement " + lines[i] + " encountered at line " + (i + 1).ToString(culture) + " in file " + filePath);
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
						if (carObjects[i] is ObjectManager.StaticObject)
						{
							ObjectManager.StaticObject obj = (ObjectManager.StaticObject)carObjects[i];
							obj.ApplyScale(-1.0, 1.0, -1.0);
						}
						else if (carObjects[i] is ObjectManager.AnimatedObjectCollection)
						{
							ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)carObjects[i];
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

			//Check for bogie objects and reverse if necessary.....
			int bogieObjectsCount = 0;
			for (int i = 0; i < train.Cars.Length * 2; i++)
			{
				if (bogieObjects[i] != null)
				{
					bogieObjectsCount++;
					if (bogieObjectsReversed[i] && loadObjects)
					{
						if (bogieObjects[i] is ObjectManager.StaticObject)
						{
							ObjectManager.StaticObject obj = (ObjectManager.StaticObject)bogieObjects[i];
							obj.ApplyScale(-1.0, 1.0, -1.0);
						}
						else if (bogieObjects[i] is ObjectManager.AnimatedObjectCollection)
						{
							ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)bogieObjects[i];
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

			if (carObjectsCount > 0 & carObjectsCount < train.Cars.Length)
			{
				Interface.AddMessage(MessageType.Warning, false, "An incomplete set of exterior objects was provided in file " + filePath);
			}
			
			if (bogieObjectsCount > 0 & bogieObjectsCount < train.Cars.Length * 2)
			{
				Interface.AddMessage(MessageType.Warning, false, "An incomplete set of bogie objects was provided in file " + filePath);
			}
		}
	}
}
