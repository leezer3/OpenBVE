using System;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Interface;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Trains.ExtensionsCfg
{
	internal static partial class ExtensionsCfg
	{
		internal static void Parse(string fileName, Train train)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;
			string[] lines = File.ReadAllLines(fileName, TextEncoding.GetSystemEncodingFromFile(fileName));

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
				if (lines[i].Any())
				{
					switch (lines[i].ToLowerInvariant())
					{
						case "[exterior]":
							i++;

							while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								if (lines[i].Any())
								{
									int j = lines[i].IndexOf("=", StringComparison.Ordinal);

									if (j >= 0)
									{
										string a = lines[i].Substring(0, j).TrimEnd();
										string b = lines[i].Substring(j + 1).TrimStart();
										int n;

										if (int.TryParse(a, NumberStyles.Integer, culture, out n))
										{
											if (n >= 0)
											{
												for (int k = n; k >= train.Cars.Count; k--)
												{
													train.Cars.Add(new TrailerCar());
													train.Couplers.Add(new Coupler());

													train.ApplyPowerNotchesToCar();
													train.ApplyBrakeNotchesToCar();
													train.ApplyLocoBrakeNotchesToCar();
												}

												if (string.IsNullOrEmpty(b))
												{
													Interface.AddMessage(MessageType.Error, true, $"An empty car object was supplied at line {(i + 1).ToString(culture)} in file {fileName}");
												}
												else if (Path.ContainsInvalidChars(b))
												{
													Interface.AddMessage(MessageType.Error, false, $"File contains illegal characters at line {(i + 1).ToString(culture)} in file {fileName}");
												}
												else
												{
													string file = Path.CombineFile(System.IO.Path.GetDirectoryName(fileName), b);

													if (!File.Exists(file))
													{
														Interface.AddMessage(MessageType.Warning, true, $"The car object {file} does not exist at line {(i + 1).ToString(culture)} in file {fileName}");
													}

													train.Cars[n].Object = file;
												}
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, $"The car index {n} does not reference an existing car at line {(i + 1).ToString(culture)} in file {fileName}");
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, $"The car index is expected to be an integer at line {(i + 1).ToString(culture)} in file {fileName}");
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
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
										for (int j = n; j >= train.Cars.Count; j--)
										{
											train.Cars.Add(new TrailerCar());
											train.Couplers.Add(new Coupler());

											train.ApplyPowerNotchesToCar();
											train.ApplyBrakeNotchesToCar();
											train.ApplyLocoBrakeNotchesToCar();
										}

										i++;

										while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (lines[i].Any())
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
																Interface.AddMessage(MessageType.Error, true, $"An empty car object was supplied at line {(i + 1).ToString(culture)} in file {fileName}");
															}
															else if (Path.ContainsInvalidChars(b))
															{
																Interface.AddMessage(MessageType.Error, false, $"File contains illegal characters at line {(i + 1).ToString(culture)} in file {fileName}");
															}
															else
															{
																string file = Path.CombineFile(System.IO.Path.GetDirectoryName(fileName), b);

																if (!File.Exists(file))
																{
																	Interface.AddMessage(MessageType.Warning, true, $"The car object {file} does not exist at line {(i + 1).ToString(culture)} in file {fileName}");
																}

																train.Cars[n].Object = file;
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
																		Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a positive floating-point number in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a positive floating-point number in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																}
															}
															break;
														case "axles":
															int k = b.IndexOf(',');

															if (k >= 0)
															{
																string c = b.Substring(0, k).TrimEnd();
																string d = b.Substring(k + 1).TrimStart();
																double rear, front;

																if (!double.TryParse(c, NumberStyles.Float, culture, out rear))
																{
																	Interface.AddMessage(MessageType.Error, false, $"Rear is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																}
																else if (!double.TryParse(d, NumberStyles.Float, culture, out front))
																{
																	Interface.AddMessage(MessageType.Error, false, $"Front is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																}
																else if (rear >= front)
																{
																	Interface.AddMessage(MessageType.Error, false, $"Rear is expected to be less than Front in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																}
																else
																{
																	train.Cars[n].RearAxle = rear;
																	train.Cars[n].FrontAxle = front;
																	train.Cars[n].DefinedAxles = true;
																}
															}
															else
															{
																Interface.AddMessage(MessageType.Error, false, $"An argument-separating comma is expected in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
															}
															break;
														case "reversed":
															train.Cars[n].Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
														case "loadingsway":
															train.Cars[n].LoadingSway = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
														default:
															Interface.AddMessage(MessageType.Warning, false, $"Unsupported key-value pair {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
															break;
													}
												}
											}

											i++;
										}

										i--;
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"The car index {t} does not reference an existing car at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, $"The car index is expected to be an integer at line {(i + 1).ToString(culture)} in file {fileName}");
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
										for (int j = n; j >= train.Couplers.Count; j--)
										{
											train.Cars.Add(new TrailerCar());
											train.Couplers.Add(new Coupler());

											train.ApplyPowerNotchesToCar();
											train.ApplyBrakeNotchesToCar();
											train.ApplyLocoBrakeNotchesToCar();
										}

										i++;

										while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (lines[i].Any())
											{
												int j = lines[i].IndexOf("=", StringComparison.Ordinal);

												if (j >= 0)
												{
													string a = lines[i].Substring(0, j).TrimEnd();
													string b = lines[i].Substring(j + 1).TrimStart();

													switch (a.ToLowerInvariant())
													{
														case "distances":
															{
																int k = b.IndexOf(',');

																if (k >= 0)
																{
																	string c = b.Substring(0, k).TrimEnd();
																	string d = b.Substring(k + 1).TrimStart();
																	double min, max;

																	if (!double.TryParse(c, NumberStyles.Float, culture, out min))
																	{
																		Interface.AddMessage(MessageType.Error, false, $"Minimum is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																	}
																	else if (!double.TryParse(d, NumberStyles.Float, culture, out max))
																	{
																		Interface.AddMessage(MessageType.Error, false, $"Maximum is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																	}
																	else if (min > max)
																	{
																		Interface.AddMessage(MessageType.Error, false, $"Minimum is expected to be less than Maximum in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																	}
																	else
																	{
																		train.Couplers[n].Min = min;
																		train.Couplers[n].Max = max;
																	}
																}
																else
																{
																	Interface.AddMessage(MessageType.Error, false, $"An argument-separating comma is expected in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																}
															}
															break;
														case "object":
															if (string.IsNullOrEmpty(b))
															{
																Interface.AddMessage(MessageType.Error, true, $"An empty coupler object was supplied at line {(i + 1).ToString(culture)} in file {fileName}");
															}
															else if (Path.ContainsInvalidChars(b))
															{
																Interface.AddMessage(MessageType.Error, false, $"File contains illegal characters at line {(i + 1).ToString(culture)} in file {fileName}");
															}
															else
															{
																string file = Path.CombineFile(System.IO.Path.GetDirectoryName(fileName), b);

																if (!File.Exists(file))
																{
																	Interface.AddMessage(MessageType.Warning, true, $"The coupler object {file} does not exist at line {(i + 1).ToString(culture)} in file {fileName}");
																}

																train.Couplers[n].Object = file;
															}
															break;
														default:
															Interface.AddMessage(MessageType.Warning, false, $"Unsupported key-value pair {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
															break;
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
												}
											}

											i++;
										}

										i--;
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"The coupler index {t} does not reference an existing coupler at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, $"The coupler index is expected to be an integer at line {(i + 1).ToString(culture)} in file {fileName}");
								}
							}
							else if (lines[i].StartsWith("[bogie", StringComparison.OrdinalIgnoreCase) & lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// bogie
								string t = lines[i].Substring(6, lines[i].Length - 7);
								int n;

								if (int.TryParse(t, NumberStyles.Integer, culture, out n))
								{
									//Assuming that there are two bogies per car
									bool IsOdd = (n % 2 != 0);
									int CarIndex = n / 2;

									if (n >= 0)
									{
										for (int j = CarIndex; j >= train.Cars.Count; j--)
										{
											train.Cars.Add(new TrailerCar());
											train.Couplers.Add(new Coupler());

											train.ApplyPowerNotchesToCar();
											train.ApplyBrakeNotchesToCar();
											train.ApplyLocoBrakeNotchesToCar();
										}

										i++;

										while (i < lines.Length && !lines[i].StartsWith("[", StringComparison.Ordinal) & !lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (lines[i].Any())
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
																Interface.AddMessage(MessageType.Error, true, $"An empty bogie object was supplied at line {(i + 1).ToString(culture)} in file {fileName}");
															}
															else if (Path.ContainsInvalidChars(b))
															{
																Interface.AddMessage(MessageType.Error, false, $"File contains illegal characters at line {(i + 1).ToString(culture)} in file {fileName}");
															}
															else
															{
																string file = Path.CombineFile(System.IO.Path.GetDirectoryName(fileName), b);

																if (!File.Exists(file))
																{
																	Interface.AddMessage(MessageType.Warning, true, $"The bogie object {file} does not exist at line {(i + 1).ToString(culture)} in file {fileName}");
																}

																if (IsOdd)
																{
																	train.Cars[CarIndex].RearBogie.Object = b;
																}
																else
																{
																	train.Cars[CarIndex].FrontBogie.Object = b;
																}
															}
															break;
														case "axles":
															int k = b.IndexOf(',');

															if (k >= 0)
															{
																string c = b.Substring(0, k).TrimEnd();
																string d = b.Substring(k + 1).TrimStart();
																double rear, front;

																if (!double.TryParse(c, NumberStyles.Float, culture, out rear))
																{
																	Interface.AddMessage(MessageType.Error, false, $"Rear is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																}
																else if (!double.TryParse(d, NumberStyles.Float, culture, out front))
																{
																	Interface.AddMessage(MessageType.Error, false, $"Front is expected to be a floating-point number in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																}
																else if (rear >= front)
																{
																	Interface.AddMessage(MessageType.Error, false, $"Rear is expected to be less than Front in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
																}
																else
																{
																	if (IsOdd)
																	{
																		train.Cars[CarIndex].RearBogie.RearAxle = rear;
																		train.Cars[CarIndex].RearBogie.FrontAxle = front;
																		train.Cars[CarIndex].RearBogie.DefinedAxles = true;
																	}
																	else
																	{
																		train.Cars[CarIndex].FrontBogie.RearAxle = rear;
																		train.Cars[CarIndex].FrontBogie.FrontAxle = front;
																		train.Cars[CarIndex].FrontBogie.DefinedAxles = true;
																	}
																}
															}
															else
															{
																Interface.AddMessage(MessageType.Error, false, $"An argument-separating comma is expected in {a} at line {(i + 1).ToString(culture)} in file {fileName}");
															}
															break;
														case "reversed":
															if (IsOdd)
															{
																train.Cars[CarIndex].FrontBogie.Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															}
															else
															{
																train.Cars[CarIndex].RearBogie.Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															}
															break;
														default:
															Interface.AddMessage(MessageType.Warning, false, $"Unsupported key-value pair {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
															break;
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
												}
											}

											i++;
										}

										i--;
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"The bogie index {t} does not reference an existing car at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, $"The bogie index is expected to be an integer at line {(i + 1).ToString(culture)} in file {fileName}");
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Invalid statement {lines[i]} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
							}
							break;
					}
				}
			}
		}
	}
}
