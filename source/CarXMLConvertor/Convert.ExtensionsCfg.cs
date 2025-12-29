using Formats.OpenBve;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
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
			if (!File.Exists(FileName))
			{
				mainForm.updateLogBoxText += "INFO: No extensions.cfg file was detected- Generating default XML." + Environment.NewLine;
				//No extensions.cfg file exists, so just spin up a default XML file
				GenerateDefaultXML();
			}
			else
			{
				mainForm.updateLogBoxText += "Loading existing extensions.cfg file " + FileName + Environment.NewLine;
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
			internal bool VisibleFromInterior;
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
			internal string Object;
		}


		private static Car[] CarInfos;
		private static Coupler[] Couplers;
	

		internal static void ReadExtensionsCfg()
		{
			ConfigFile<ExtensionCfgSection, ExtensionCfgKey> cfg = new ConfigFile<ExtensionCfgSection, ExtensionCfgKey>(FileName, Program.CurrentHost);

			while (cfg.RemainingSubBlocks > 0)
			{
				Block<ExtensionCfgSection, ExtensionCfgKey> block = cfg.ReadNextBlock();
				string carObject;
				switch (block.Key)
				{
					case ExtensionCfgSection.Exterior:
						while (block.RemainingDataValues > 0 && block.GetIndexedPath(Path.GetDirectoryName(FileName), out var carIndex, out carObject) && carIndex < ConvertTrainDat.NumberOfCars)
						{
							CarInfos[carIndex].Object = carObject;
						}
						break;
					case ExtensionCfgSection.Car:
						if (block.Index == -1)
						{
							break;
						}

						if (block.Index >= ConvertTrainDat.NumberOfCars)
						{
							break;
						}
						
						if (block.GetPath(ExtensionCfgKey.Object, Path.GetDirectoryName(FileName), out carObject))
						{
							CarInfos[block.Index].Object = carObject;
						}

						if (block.GetValue(ExtensionCfgKey.Length, out double carLength, NumberRange.Positive))
						{
							CarInfos[block.Index].Length = carLength;
						}
						block.GetValue(ExtensionCfgKey.Reversed, out CarInfos[block.Index].Reversed);
						block.GetValue(ExtensionCfgKey.VisibleFromInterior, out CarInfos[block.Index].VisibleFromInterior);
						block.GetValue(ExtensionCfgKey.LoadingSway, out CarInfos[block.Index].LoadingSway);
						if (block.GetVector2(ExtensionCfgKey.Axles, ',', out Vector2 carAxles))
						{
							if (carAxles.X >= carAxles.Y)
							{
								CarInfos[block.Index].RearAxle = carAxles.X;
								CarInfos[block.Index].FrontAxle = carAxles.Y;
								CarInfos[block.Index].AxlesDefined = true;
							}
						}
						break;
					case ExtensionCfgSection.Coupler:
						if (block.Index == -1 || block.Index >= CarInfos.Length)
						{
							break;
						}
						if (block.GetVector2(ExtensionCfgKey.Distances, ',', out Vector2 distances))
						{
							if (distances.X > distances.Y)
							{
								Couplers[block.Index].Min = distances.X;
								Couplers[block.Index].Max = distances.Y;
							}
						}
						if (block.GetPath(ExtensionCfgKey.Object, Path.GetDirectoryName(FileName), out string couplerObject))
						{
							Couplers[block.Index].Object = couplerObject;
						}
						break;
					case ExtensionCfgSection.Bogie:
						if (block.Index == -1)
						{
							break;
						}

						if (block.Index > ConvertTrainDat.NumberOfCars * 2)
						{
							break;
						}

						//Assuming that there are two bogies per car
						bool IsOdd = (block.Index % 2 != 0);
						int CarIndex = block.Index / 2;

						if (block.GetPath(ExtensionCfgKey.Object, Path.GetDirectoryName(FileName), out string bogieObject))
						{
							if (IsOdd)
							{
								CarInfos[CarIndex].RearBogie.Object = bogieObject;
							}
							else
							{
								CarInfos[CarIndex].FrontBogie.Object = bogieObject;
							}
						}
						block.GetValue(ExtensionCfgKey.Reversed, out bool bogieReversed);
						if (IsOdd)
						{
							CarInfos[CarIndex].RearBogie.Reversed = bogieReversed;
						}
						else
						{
							CarInfos[CarIndex].FrontBogie.Reversed = bogieReversed;
						}
						if (block.GetVector2(ExtensionCfgKey.Axles, ',', out Vector2 bogieAxles))
						{
							if (bogieAxles.X >= bogieAxles.Y)
							{
								if (IsOdd)
								{
									CarInfos[CarIndex].FrontBogie.RearAxle = bogieAxles.X;
									CarInfos[CarIndex].FrontBogie.FrontAxle = bogieAxles.Y;
								}
								else
								{
									CarInfos[CarIndex].RearBogie.RearAxle = bogieAxles.X;
									CarInfos[CarIndex].RearBogie.FrontAxle = bogieAxles.Y;
								}
							}
						}
						break;
				}
			}
		}

		internal static bool SingleFile = false;

		internal static void GenerateExtensionsCfgXML()
		{
			TabbedList newLines = new TabbedList();
			newLines.Add("<Train>");
			try
			{
				FileVersionInfo programVersion = FileVersionInfo.GetVersionInfo("OpenBve.exe");
				newLines.Add("<ConvertorVersion>" + programVersion.FileVersion + "</ConvertorVersion>");
			}
			catch
			{
				// Ignore- Most likely the convertor has been copied elsewhere
			}
			newLines.Add("<DriverCar>" + ConvertTrainDat.DriverCar + "</DriverCar>");
			newLines.Add("<DriverBody>");
			newLines.Add("<ShoulderHeight>0.6</ShoulderHeight>");
			newLines.Add("<HeadHeight>0.1</HeadHeight>");
			newLines.Add("</DriverBody>");
			for (int i = 0; i < ConvertTrainDat.NumberOfCars; i++)
			{
				if (SingleFile)
				{
					GenerateCarXML(ref newLines, i);
				}
				else
				{
					TabbedList carLines = new TabbedList();
					GenerateCarXML(ref carLines, i);
					carLines.Add("</openBVE>");
					string fileOut = System.IO.Path.Combine(Path.GetDirectoryName(FileName), "Car" + i + ".xml");
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
						MessageBox.Show("An error occured whilst writing the new XML file for car " + i + ". \r\n Please check for write permissions.", @"CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return;
					}
				}
				
			}

			string pluginFile = ConvertAts.DllPath(Path.GetDirectoryName(FileName));
			if (!string.IsNullOrEmpty(pluginFile))
			{
				newLines.Add("<Plugin>" + pluginFile + "</Plugin>");
			}
			newLines.Add("<HeadlightStates>1</HeadlightStates>");
			string trainTxt = Path.CombineFile(Path.GetDirectoryName(FileName), "train.txt");
			if (File.Exists(trainTxt))
			{
				string desc = File.ReadAllText(trainTxt, OpenBveApi.TextEncoding.GetSystemEncodingFromFile(trainTxt));
				newLines.Add("<Description>" + SecurityElement.Escape(desc) + "</Description>");
			}
			newLines.Add("</Train>");
			newLines.Add("</openBVE>");
			try
			{
				string fileOut = System.IO.Path.Combine(Path.GetDirectoryName(FileName), "Train.xml");
				using (StreamWriter sw = new StreamWriter(fileOut))
				{
					foreach (String s in newLines.Lines)
						sw.WriteLine(s);
				}
			}
			catch
			{
				MessageBox.Show("An error occured whilst writing the new XML file. \r\n Please check for write permissions.", @"CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			MessageBox.Show("Conversion succeeded.", @"CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
			if (ConvertTrainDat.MotorCars[i])
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
				newLines.Add("<Power>");
				newLines.Add("<Notches>" + ConvertTrainDat.PowerNotches + "</Notches>");
				newLines.Add("<!-- Note that XML figures are per-car as opposed to a blended figure for the complete train in Train.dat -->");
				newLines.Add("<AccelerationCurves>");
				foreach (ConvertTrainDat.AccelerationCurve curve in ConvertTrainDat.AccelerationCurves)
				{
					newLines.Add("<OpenBVE>");
					newLines.Add("<StageZeroAcceleration>" + curve.StageZeroAcceleration + "</StageZeroAcceleration>");
					newLines.Add("<StageOneAcceleration>" + curve.StageOneAcceleration + "</StageOneAcceleration>");
					newLines.Add("<StageOneSpeed>" + curve.StageOneSpeed + "</StageOneSpeed>");
					newLines.Add("<StageTwoSpeed>" + curve.StageTwoSpeed + "</StageTwoSpeed>");
					newLines.Add("<StageTwoExponent>" + curve.StageTwoExponent + "</StageTwoExponent>");
					newLines.Add("<!-- If manually setting the acceleration figures per motor car, you will normally want a multiplier of 1.0 -->");
					newLines.Add("<Multiplier>" + curve.Multiplier + "</Multiplier>");
					newLines.Add("</OpenBVE>");
				}
				newLines.Add("</AccelerationCurves>");
				newLines.Add("</Power>");
			}
			else
			{
				newLines.Add("<MotorCar>False</MotorCar>");
				newLines.Add("<Mass>" + ConvertTrainDat.TrailerCarMass + "</Mass>");
			}
			// BVE has all cars as passenger carrying by default
			newLines.Add("<Cargo>Passengers</Cargo>");
			if (CarInfos[i].AxlesDefined)
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
				newLines.Add("<Object>" + CarInfos[i].Object.Escape() + "</Object>");
			}
			newLines.Add("<Reversed>" + CarInfos[i].Reversed + "</Reversed>");
			newLines.Add("<VisibleFromInterior>" + CarInfos[i].VisibleFromInterior + "</VisibleFromInterior>");
			newLines.Add("<LoadingSway>" + CarInfos[i].LoadingSway + "</LoadingSway>");
			if (CarInfos[i].FrontBogie.AxlesDefined || !string.IsNullOrEmpty(CarInfos[i].FrontBogie.Object))
			{
				newLines.Add("<FrontBogie>");
				newLines.Add("<FrontAxle>" + CarInfos[i].FrontBogie.FrontAxle + "</FrontAxle>");
				newLines.Add("<RearAxle>" + CarInfos[i].FrontBogie.RearAxle + "</RearAxle>");
				newLines.Add("<Object>" + CarInfos[i].FrontBogie.Object.Escape() + "</Object>");
				newLines.Add("<Reversed>" + CarInfos[i].FrontBogie.Reversed + "</Reversed>");
				newLines.Add("</FrontBogie>");
			}

			if (CarInfos[i].RearBogie.AxlesDefined || !string.IsNullOrEmpty(CarInfos[i].RearBogie.Object))
			{
				newLines.Add("<RearBogie>");
				newLines.Add("<FrontAxle>" + CarInfos[i].RearBogie.FrontAxle + "</FrontAxle>");
				newLines.Add("<RearAxle>" + CarInfos[i].RearBogie.RearAxle + "</RearAxle>");
				newLines.Add("<Object>" + CarInfos[i].RearBogie.Object.Escape() + "</Object>");
				newLines.Add("<Reversed>" + CarInfos[i].RearBogie.Reversed + "</Reversed>");
				newLines.Add("</RearBogie>");
			}
			if (i == ConvertTrainDat.DriverCar)
			{
				if(File.Exists(Path.CombineFile(Path.GetDirectoryName(FileName), "panel.animated")))
				{
					newLines.Add("<InteriorView>panel.animated</InteriorView>" );
					newLines.Add("<DriverPosition>" + ConvertTrainDat.DriverPosition + "</DriverPosition>");
				}
				else if (File.Exists(Path.CombineFile(Path.GetDirectoryName(FileName), "panel2.cfg")))
				{
					newLines.Add("<InteriorView>panel.xml</InteriorView>");
					newLines.Add("<DriverPosition>" + ConvertTrainDat.DriverPosition + "</DriverPosition>");
				}
			}
			newLines.Add("<Brake>");
			if (i == ConvertTrainDat.DriverCar)
			{
				newLines.Add("<Handle>");
				newLines.Add("<Notches>" + ConvertTrainDat.BrakeNotches + "</Notches>");
				newLines.Add("</Handle>");
			}
			newLines.Add("<!-- Pressures are in kPa -->");
			if (ConvertTrainDat.MotorCars[i])
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
			newLines.Add("<LegacyPressureDistribution>true</LegacyPressureDistribution>");
			newLines.Add("</Brake>");
			newLines.Add("<Doors>");
			newLines.Add("<Width>" + ConvertTrainDat.DoorWidth / 1000.0 + "</Width>");
			newLines.Add("<Tolerance>" + ConvertTrainDat.DoorTolerance / 1000.0 + "</Tolerance>");
			newLines.Add("</Doors>");
			newLines.Add("</Car>");
			if (i < Couplers.Length)
			{
				if (Couplers[i] != null)
				{
					newLines.Add("<Coupler>");
					newLines.Add("<Minimum>" + Couplers[i].Min + "</Minimum>");
					newLines.Add("<Maximum>" + Couplers[i].Max + "</Maximum>");
					newLines.Add("<CanUncouple>true</CanUncouple>");
					if (!string.IsNullOrEmpty(Couplers[i].Object))
					{
						newLines.Add("<Object>" + Couplers[i].Object.Escape() + "</Object>");
					}
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
				newLines.Add("<CenterOfGravityHeight>" + ConvertTrainDat.CenterOfGravityHeight + "</CenterOfGravityHeight>");
				if (ConvertTrainDat.ExposedFrontalArea != -1)
				{
					newLines.Add("<ExposedFrontalArea>" + ConvertTrainDat.ExposedFrontalArea + "</ExposedFrontalArea>");
				}
				if (ConvertTrainDat.UnexposedFrontalArea != -1)
				{
					newLines.Add("<UnexposedFrontalArea>" + ConvertTrainDat.UnexposedFrontalArea + "</UnexposedFrontalArea>");
				}
				if (ConvertTrainDat.MotorCars[i])
				{
					newLines.Add("<MotorCar>True</MotorCar>");
					newLines.Add("<!-- Masses are in kg -->");
					newLines.Add("<Mass>" + ConvertTrainDat.MotorCarMass + "</Mass>");
				}
				else
				{
					newLines.Add("<MotorCar>False</MotorCar>");
					newLines.Add("<!-- Masses are in kg -->");
					newLines.Add("<Mass>" + ConvertTrainDat.TrailerCarMass + "</Mass>");
				}
				newLines.Add("<FrontAxle>" + 0.4 * ConvertTrainDat.CarLength + "</FrontAxle>");
				newLines.Add("<RearAxle>" + -(0.4 * ConvertTrainDat.CarLength) + "</RearAxle>");
				newLines.Add("</Car>");
			}
			newLines.Add("<DriverCar>" + ConvertTrainDat.DriverCar + "</DriverCar>");
			string pluginFile = ConvertAts.DllPath(Path.GetDirectoryName(FileName));
			if (!string.IsNullOrEmpty(pluginFile))
			{
				newLines.Add("<Plugin>");
				newLines.Add("<File>" + pluginFile + "</File>");
				newLines.Add("<LoadForAI>false</LoadForAI>");
				newLines.Add("</Plugin>");
			}
			newLines.Add("<HeadlightStates>1</HeadlightStates>");
			string trainTxt = Path.CombineFile(Path.GetDirectoryName(FileName), "train.txt");
			if (File.Exists(trainTxt))
			{
				string desc = File.ReadAllText(trainTxt);
				newLines.Add("<Description>" + desc + "</Description>");
			}
			newLines.Add("</Train>");
			newLines.Add("</openBVE>");
			// ReSharper disable once AssignNullToNotNullAttribute
			string fileOut = System.IO.Path.Combine(Path.GetDirectoryName(FileName), "Train.xml");
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
				MessageBox.Show("An error occured whilst writing the new XML file. \r\n Please check for write permissions.", @"CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
	}
}
