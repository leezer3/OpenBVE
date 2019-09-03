﻿using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Path = OpenBveApi.Path;

namespace CarXmlConvertor
{
	class ConvertExtensionsCfg
	{
		internal static string FileName;
		internal static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

		private static MainForm mainForm;

		internal static void Process(MainForm form)
		{
			mainForm = form;
			if (!System.IO.File.Exists(FileName))
			{
				mainForm.updateLogBoxText += "INFO: No extensions.cfg file was detected- Generating default XML." + Environment.NewLine;
				//No extensions.cfg file exists, so just spin up a default XML file
				GenerateDefaultXML();
			}
			else
			{
				mainForm.updateLogBoxText += "Loading existing extensions.cfg file " + ConvertExtensionsCfg.FileName + Environment.NewLine;
				CarInfos = new Car[ConvertTrainDat.NumberOfCars];
				Couplers = new Coupler[ConvertTrainDat.NumberOfCars - 1];
				ReadExtensionsCfg();
				GenerateExtensionsCfgXML();
			}
		}

		private struct Car
		{
			internal double Length;
			internal double FrontAxle;
			internal double RearAxle;
			internal bool AxlesDefined;
			internal bool Reversed;
			internal string Object;
			internal Bogie FrontBogie;
			internal Bogie RearBogie;
			internal bool LoadingSway;
		}

		private struct Bogie
		{
			//Need a second struct to avoid a circular reference
			internal double FrontAxle;
			internal double RearAxle;
			internal bool AxlesDefined;
			internal bool Reversed;
			internal string Object;
		}

		private class Coupler
		{
			internal double Min = 0.27;
			internal double Max = 0.33;
		}


		private static Car[] CarInfos;
		private static Coupler[] Couplers;
	

		internal static void ReadExtensionsCfg()
		{
			string[] Lines = System.IO.File.ReadAllLines(FileName, System.Text.Encoding.Default);
			for (int i = 0; i < Lines.Length; i++)
			{
				int j = Lines[i].IndexOf(';');
				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).Trim(new char[] {' '});
				}
				else
				{
					Lines[i] = Lines[i].Trim(new char[] {' '});
				}
			}
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Length != 0)
				{
					switch (Lines[i].ToLowerInvariant())
					{
						case "[exterior]":
							// exterior
							i++;
							while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								if (Lines[i].Length != 0)
								{
									int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
									if (j >= 0)
									{
										string a = Lines[i].Substring(0, j).TrimEnd(new char[] {' '});
										string b = Lines[i].Substring(j + 1).TrimStart(new char[] {' '});
										int n;
										if (int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out n))
										{
											if (n >= 0 & n < ConvertTrainDat.NumberOfCars)
											{
												if (!String.IsNullOrEmpty(b) && !Path.ContainsInvalidChars(b))
												{
													string File = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), b);
													if (System.IO.File.Exists(File))
													{
														CarInfos[n].Object = b;
													}
												}
											}
										}
									}
								}
								i++;
							}
							i--;
							break;
						default:
							if (Lines[i].StartsWith("[car", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// car
								string t = Lines[i].Substring(4, Lines[i].Length - 5);
								int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n))
								{
									if (n >= 0 & n < ConvertTrainDat.NumberOfCars)
									{
										i++;
										while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (Lines[i].Length != 0)
											{
												int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													string a = Lines[i].Substring(0, j).TrimEnd(new char[] {' '});
													string b = Lines[i].Substring(j + 1).TrimStart(new char[] {' '});
													switch (a.ToLowerInvariant())
													{
														case "object":
															if (!String.IsNullOrEmpty(b) && !Path.ContainsInvalidChars(b))
															{
																string File = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), b);
																if (System.IO.File.Exists(File))
																{
																	CarInfos[n].Object = b;
																}
															}
															break;
														case "length":
														{
															double m;
															if (double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out m))
															{
																if (m > 0.0)
																{
																	CarInfos[n].Length = m;
																}
															}
														}
															break;
														case "axles":
														int k = b.IndexOf(',');
															if (k >= 0)
															{
																string c = b.Substring(0, k).TrimEnd(new char[] {' '});
																string d = b.Substring(k + 1).TrimStart(new char[] {' '});
																double rear, front;
																if (double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out rear) && double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front))
																{
																	CarInfos[n].RearAxle = rear;
																	CarInfos[n].FrontAxle = front;
																	CarInfos[n].AxlesDefined = true;
																}
															}
															break;
														case "reversed":
															CarInfos[n].Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
														case "loadingsway":
															CarInfos[n].LoadingSway = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															break;
													}
												}
											}
											i++;
										}
										i--;
									}
								}
							}
							else if (Lines[i].StartsWith("[coupler", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// coupler
								string t = Lines[i].Substring(8, Lines[i].Length - 9);
								int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n))
								{
									if (n >= 0 & n < Couplers.Length)
									{
										i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (Lines[i].Length != 0)
											{
												int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													string a = Lines[i].Substring(0, j).TrimEnd(new char[] {' '});
													string b = Lines[i].Substring(j + 1).TrimStart(new char[] {' '});
													switch (a.ToLowerInvariant())
													{
														case "distances":
														{
															int k = b.IndexOf(',');
															if (k >= 0)
															{
																string c = b.Substring(0, k).TrimEnd(new char[] {' '});
																string d = b.Substring(k + 1).TrimStart(new char[] {' '});
																double min, max;
																if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out min))
																{
																}
																else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out max))
																{
																}
																else
																{
																	Couplers[n] = new Coupler { Min = min, Max = max };
																}
															}
														}
															break;
													}
												}
											}
											i++;
										}
										i--;
									}
								}
							}
							else if (Lines[i].StartsWith("[bogie", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								// car
								string t = Lines[i].Substring(6, Lines[i].Length - 7);
								int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n))
								{
									//Assuming that there are two bogies per car
									bool IsOdd = (n % 2 != 0);
									int CarIndex = n / 2;
									if (n >= 0 & n < ConvertTrainDat.NumberOfCars * 2)
									{
										i++;
										while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal))
										{
											if (Lines[i].Length != 0)
											{
												int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
												if (j >= 0)
												{
													string a = Lines[i].Substring(0, j).TrimEnd(new char[] {' '});
													string b = Lines[i].Substring(j + 1).TrimStart(new char[] {' '});
													switch (a.ToLowerInvariant())
													{
														case "object":
															if (!String.IsNullOrEmpty(b) && !Path.ContainsInvalidChars(b))
															{
																string File = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), b);
																if (System.IO.File.Exists(File))
																{
																	if (IsOdd)
																	{
																		CarInfos[CarIndex].RearBogie.Object = b;
																	}
																	else
																	{
																		CarInfos[CarIndex].FrontBogie.Object = b;
																	}
																}
															}
															break;
														case "axles":
															int k = b.IndexOf(',');
															if (k >= 0)
															{
																string c = b.Substring(0, k).TrimEnd(new char[] {' '});
																string d = b.Substring(k + 1).TrimStart(new char[] {' '});
																double rear, front;
																if (double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out rear) && double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out front))
																{
																	if (IsOdd)
																	{
																		CarInfos[CarIndex].RearBogie.RearAxle = rear;
																		CarInfos[CarIndex].RearBogie.FrontAxle = front;
																		CarInfos[CarIndex].RearBogie.AxlesDefined = true;
																	}
																	else
																	{
																		CarInfos[CarIndex].FrontBogie.RearAxle = rear;
																		CarInfos[CarIndex].FrontBogie.FrontAxle = front;
																		CarInfos[CarIndex].FrontBogie.AxlesDefined = true;
																	}
																}
															}
															break;
														case "reversed":
															if (IsOdd)
															{
																CarInfos[CarIndex].FrontBogie.Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															}
															else
															{
																CarInfos[CarIndex].RearBogie.Reversed = b.Equals("true", StringComparison.OrdinalIgnoreCase);
															}
															break;
													}
												}
											}
											i++;
										}
										i--;
										
									}
								}
							}
							break;
					}
				}
			}
		}

		internal static bool SingleFile = false;

		internal static void GenerateExtensionsCfgXML()
		{
			TabbedList newLines = new TabbedList();
			newLines.Add("<Train>");
			for (int i = 0; i < ConvertTrainDat.NumberOfCars; i++)
			{
				if (SingleFile == true)
				{
					GenerateCarXML(ref newLines, i);
				}
				else
				{
					TabbedList carLines = new TabbedList();
					GenerateCarXML(ref carLines, i);
					carLines.Add("</openBVE>");
					string fileOut = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileName), "Car" + i + ".xml");
					try
					{
						
						using (StreamWriter sw = new StreamWriter(fileOut))
						{
							foreach (String s in carLines.Lines)
								sw.WriteLine(s);
						}
						newLines.Add("<Car>"+ "Car" + i + ".xml</Car>");
					}
					catch
					{
						mainForm.updateLogBoxText += "Error writing file " + fileOut + Environment.NewLine;
						MessageBox.Show("An error occured whilst writing the new XML file for car " + i + ". \r\n Please check for write permissions.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return;
					}
				}
				
			}
			newLines.Add("</Train>");
			newLines.Add("</openBVE>");
			try
			{
				string fileOut = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileName), "Train.xml");
				using (StreamWriter sw = new StreamWriter(fileOut))
				{
					foreach (String s in newLines.Lines)
						sw.WriteLine(s);
				}
			}
			catch
			{
				MessageBox.Show("An error occured whilst writing the new XML file. \r\n Please check for write permissions.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			MessageBox.Show("Conversion succeeded.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		internal static void GenerateCarXML(ref TabbedList newLines, int i)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			newLines.Add("<Car>");
			if (CarInfos[i].Length != 0.0)
			{
				newLines.Add("<Length>" + CarInfos[i].Length + "</Length>");
			}
			else
			{
				newLines.Add("<Length>" + ConvertTrainDat.CarLength + "</Length>");
			}
			newLines.Add("<Width>" + ConvertTrainDat.CarWidth + "</Width>");
			newLines.Add("<Height>" + ConvertTrainDat.CarHeight + "</Height>");
			if (ConvertTrainDat.MotorCars[i] == true)
			{
				newLines.Add("<MotorCar>True</MotorCar>");
				newLines.Add("<Mass>" + ConvertTrainDat.MotorCarMass + "</Mass>");
				switch (ConvertTrainDat.ReadhesionDeviceType)
				{
					case 0:
						newLines.Add("<ReadhesionDevice>TypeA</ReadhesionDevice>");
						break;
					case 1:
						newLines.Add("<ReadhesionDevice>TypeB</ReadhesionDevice>");
						break;
					case 2:
						newLines.Add("<ReadhesionDevice>TypeC</ReadhesionDevice>");
						break;
					case 3:
						newLines.Add("<ReadhesionDevice>TypeD</ReadhesionDevice>");
						break;
					default:
						newLines.Add("<ReadhesionDevice>NotFitted</ReadhesionDevice>");
						break;
				}
			}
			else
			{
				newLines.Add("<MotorCar>False</MotorCar>");
				newLines.Add("<Mass>" + ConvertTrainDat.TrailerCarMass + "</Mass>");
			}
			if (CarInfos[i].AxlesDefined == true)
			{
				newLines.Add("<FrontAxle>" + CarInfos[i].FrontAxle + "</FrontAxle>");
				newLines.Add("<RearAxle>" + CarInfos[i].RearAxle + "</RearAxle>");
			}
			else
			{
				newLines.Add("<FrontAxle>" + 0.4 * ConvertTrainDat.CarLength + "</FrontAxle>");
				newLines.Add("<RearAxle>" + -(0.4 * ConvertTrainDat.CarLength) + "</RearAxle>");
			}
			if (!String.IsNullOrEmpty(CarInfos[i].Object))
			{
				newLines.Add("<Object>" + CarInfos[i].Object + "</Object>");
			}
			newLines.Add("<Reversed>" + CarInfos[i].Reversed + "</Reversed>");
			newLines.Add("<LoadingSway>" + CarInfos[i].Reversed + "</LoadingSway>");
			if (CarInfos[i].FrontBogie.AxlesDefined == true || !string.IsNullOrEmpty(CarInfos[i].FrontBogie.Object))
			{
				newLines.Add("<FrontBogie>");
				newLines.Add("<FrontAxle>" + CarInfos[i].FrontBogie.FrontAxle + "</FrontAxle>");
				newLines.Add("<RearAxle>" + CarInfos[i].FrontBogie.RearAxle + "</RearAxle>");
				newLines.Add("<Object>" + CarInfos[i].FrontBogie.Object + "</Object>");
				newLines.Add("<Reversed>" + CarInfos[i].FrontBogie.Reversed + "</Reversed>");
				newLines.Add("</FrontBogie>");
			}

			if (CarInfos[i].RearBogie.AxlesDefined == true || !string.IsNullOrEmpty(CarInfos[i].RearBogie.Object))
			{
				newLines.Add("<RearBogie>");
				newLines.Add("<FrontAxle>" + CarInfos[i].RearBogie.FrontAxle + "</FrontAxle>");
				newLines.Add("<RearAxle>" + CarInfos[i].RearBogie.RearAxle + "</RearAxle>");
				newLines.Add("<Object>" + CarInfos[i].RearBogie.Object + "</Object>");
				newLines.Add("<Reversed>" + CarInfos[i].RearBogie.Reversed + "</Reversed>");
				newLines.Add("</RearBogie>");
			}
			if (i == ConvertTrainDat.DriverCar)
			{
				if(System.IO.File.Exists(OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), "panel.animated")))
				{
					newLines.Add("<InteriorView>panel.animated</InteriorView>" );
					newLines.Add("<DriverPosition>" + ConvertSoundCfg.DriverPosition.X + "," + ConvertSoundCfg.DriverPosition.Y + "," + ConvertSoundCfg.DriverPosition.Z + "</DriverPosition>");
				}
				else if (System.IO.File.Exists(OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), "panel2.cfg")))
				{
					newLines.Add("<InteriorView>panel.xml</InteriorView>");
					newLines.Add("<DriverPosition>" + ConvertSoundCfg.DriverPosition.X + "," + ConvertSoundCfg.DriverPosition.Y + "," + ConvertSoundCfg.DriverPosition.Z + "</DriverPosition>");
				}
			}
			newLines.Add("<Brake>");
			if (ConvertTrainDat.MotorCars[i] == true)
			{

				newLines.Add("<Compressor>");
				newLines.Add("<Rate>5000.0</Rate>");
				newLines.Add("</Compressor>");
			}

			newLines.Add("<MainReservoir>");
			newLines.Add("<MinimumPressure>" + ConvertTrainDat.MainReservoirMinimumPressure + "</MinimumPressure>");
			newLines.Add("<MaximumPressure>" + ConvertTrainDat.MainReservoirMaximumPressure + "</MaximumPressure>");
			newLines.Add("</MainReservoir>");
			newLines.Add("<AuxiliaryReservoir>");
			newLines.Add("<ChargeRate>200000.0</ChargeRate>");
			newLines.Add("</AuxiliaryReservoir>");
			newLines.Add("<EqualizingReservoir>");
			newLines.Add("<ServiceRate>50000.0</ServiceRate>");
			newLines.Add("<EmergencyRate>250000.0</EmergencyRate>");
			newLines.Add("<ChargeRate>200000.0</ChargeRate>");
			newLines.Add("</EqualizingReservoir>");
			newLines.Add("<BrakePipe>");
			newLines.Add("<NormalPressure>" + ConvertTrainDat.BrakePipePressure + "</NormalPressure>");
			newLines.Add("<ServiceRate>1500000.0</ServiceRate>");
			newLines.Add("<EmergencyRate>5000000.0</EmergencyRate>");
			newLines.Add("<ChargeRate>10000000.0</ChargeRate>");
			newLines.Add("</BrakePipe>");
			newLines.Add("<StraightAirPipe>");
			newLines.Add("<ServiceRate>300000.0</ServiceRate>");
			newLines.Add("<EmergencyRate>400000.0</EmergencyRate>");
			newLines.Add("<ReleaseRate>200000.0</ReleaseRate>");
			newLines.Add("</StraightAirPipe>");
			newLines.Add("<BrakeCylinder>");
			newLines.Add("<ServiceMaximumPressure>" + ConvertTrainDat.BrakeCylinderServiceMaximumPressure + "</ServiceMaximumPressure>");
			newLines.Add("<EmergencyMaximumPressure>" + ConvertTrainDat.BrakeCylinderEmergencyMaximumPressure + "</EmergencyMaximumPressure>");
			newLines.Add("<EmergencyRate>" + ConvertTrainDat.BrakeCylinderEmergencyRate + "</EmergencyRate>");
			newLines.Add("<ReleaseRate>" + ConvertTrainDat.BrakeCylinderReleaseRate + "</ReleaseRate>");
			newLines.Add("</BrakeCylinder>");
			newLines.Add("</Brake>");
			newLines.Add("</Car>");
			if (i < Couplers.Length)
			{
				if (Couplers[i] != null)
				{
					newLines.Add("<Coupler>");
					newLines.Add("<Minimum>" + Couplers[i].Min + "</Minimum>");
					newLines.Add("<Maximum>" + Couplers[i].Max + "</Maximum>");
					newLines.Add("</Coupler>");
				}
			}
		}

		/// <summary>Generates a train.xml file using the values / assumptions contained within the train.dat file</summary>
		internal static void GenerateDefaultXML()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			TabbedList newLines = new TabbedList();
			newLines.Add("<Train>");
			for (int i = 0; i < ConvertTrainDat.NumberOfCars; i++)
			{
				newLines.Add("<Car>");
				newLines.Add("<Length>" + ConvertTrainDat.CarLength + "</Length>");
				newLines.Add("<Width>" + ConvertTrainDat.CarWidth + "</Width>");
				newLines.Add("<Height>" + ConvertTrainDat.CarHeight + "</Height>");
				if (ConvertTrainDat.MotorCars[i] == true)
				{
					newLines.Add("<MotorCar>True</MotorCar>");
					newLines.Add("<Mass>" + ConvertTrainDat.MotorCarMass + "</Mass>");
				}
				else
				{
					newLines.Add("<MotorCar>False</MotorCar>");
					newLines.Add("<Mass>" + ConvertTrainDat.TrailerCarMass + "</Mass>");
				}
				newLines.Add("<FrontAxle>" + 0.4 * ConvertTrainDat.CarLength + "</FrontAxle>");
				newLines.Add("<RearAxle>" + -(0.4 * ConvertTrainDat.CarLength) + "</RearAxle>");
				newLines.Add("</Car>");
			}
			newLines.Add("</Train>");
			newLines.Add("</openBVE>");
			string fileOut = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileName), "Train.xml");
			try
			{
				using (StreamWriter sw = new StreamWriter(fileOut))
				{
					foreach (String s in newLines.Lines)
						sw.WriteLine(s);
				}
			}
			catch
			{
				mainForm.updateLogBoxText += "Error writing file " + fileOut + Environment.NewLine;
				MessageBox.Show("An error occured whilst writing the new XML file. \r\n Please check for write permissions.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
	}
}
