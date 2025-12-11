using System.IO;
using Formats.OpenBve;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainEditor2.Models.Trains;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Trains.ExtensionsCfg
{
	internal static partial class ExtensionsCfg
	{
		internal static void Parse(string fileName, Train train)
		{
			ConfigFile<ExtensionCfgSection, ExtensionCfgKey> cfg = new ConfigFile<ExtensionCfgSection, ExtensionCfgKey>(File.ReadAllLines(fileName, TextEncoding.GetSystemEncodingFromFile(fileName)), Program.CurrentHost);
			string trainPath = Path.GetDirectoryName(fileName);
			while (cfg.RemainingSubBlocks > 0)
			{
				Block<ExtensionCfgSection, ExtensionCfgKey> block = cfg.ReadNextBlock();
				switch (block.Key)
				{
					case ExtensionCfgSection.Exterior:
						while (block.RemainingDataValues > 0)
						{
							if (block.GetIndexedPath(trainPath, out int carIndex, out string indexedPath))
							{
								for (int k = carIndex; k >= train.Cars.Count; k--)
								{
									train.Cars.Add(new TrailerCar());
									train.Couplers.Add(new Coupler());

									train.ApplyPowerNotchesToCar();
									train.ApplyBrakeNotchesToCar();
									train.ApplyLocoBrakeNotchesToCar();
								}

								train.Cars[carIndex].Object = indexedPath;
							}
						}
						break;
					case ExtensionCfgSection.Car:
						if (block.Index == -1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Invalid or missing CarIndex in file " + fileName);
							break;
						}
						for (int j = block.Index; j >= train.Cars.Count; j--)
						{
							train.Cars.Add(new TrailerCar());
							train.Couplers.Add(new Coupler());

							train.ApplyPowerNotchesToCar();
							train.ApplyBrakeNotchesToCar();
							train.ApplyLocoBrakeNotchesToCar();
						}

						if (block.GetPath(ExtensionCfgKey.Object, trainPath, out string objectPath))
						{
							train.Cars[block.Index].Object = objectPath;
						}

						if (block.GetValue(ExtensionCfgKey.Length, out double carLength, NumberRange.Positive))
						{
							train.Cars[block.Index].Length = carLength;
						}
						if (block.GetVector2(ExtensionCfgKey.Axles, ',', out Vector2 carAxles))
						{
							if (carAxles.X >= carAxles.Y)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front for Car " + block.Index + " in file " + fileName);
							}
							else
							{
								train.Cars[block.Index].RearAxle = carAxles.X;
								train.Cars[block.Index].FrontAxle = carAxles.Y;
								train.Cars[block.Index].DefinedAxles = true;
							}
						}

						block.GetValue(ExtensionCfgKey.Reversed, out bool carReversed);
						train.Cars[block.Index].Reversed = carReversed;
						block.GetValue(ExtensionCfgKey.LoadingSway, out bool loadingSway);
						train.Cars[block.Index].LoadingSway = loadingSway;
						break;
					case ExtensionCfgSection.Coupler:
						if (block.Index == -1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Invalid or missing CouplerIndex in file " + fileName);
							break;
						}
						for (int j = block.Index; j >= train.Couplers.Count; j--)
						{
							train.Cars.Add(new TrailerCar());
							train.Couplers.Add(new Coupler());

							train.ApplyPowerNotchesToCar();
							train.ApplyBrakeNotchesToCar();
							train.ApplyLocoBrakeNotchesToCar();
						}

						if (block.GetVector2(ExtensionCfgKey.Distances, ',', out Vector2 distances))
						{
							if (distances.X > distances.Y)
							{
								// NOTE: Current error is misleading...
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Minimum is expected to be less than Maximum in for Coupler " + block.Index + " in file " + fileName);
							}
							else
							{
								train.Couplers[block.Index].Min = distances.X;
								train.Couplers[block.Index].Max = distances.Y;
							}
						}
						if (block.GetPath(ExtensionCfgKey.Object, trainPath, out string couplerObject))
						{
							train.Couplers[block.Index].Object = couplerObject;
						}
						break;
					case ExtensionCfgSection.Bogie:
						if (block.Index == -1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Invalid or missing BogieIndex in file " + fileName);
							break;
						}
						bool isOdd = block.Index % 2 != 0;
						int bogieCarIndex = block.Index / 2;
						for (int j = bogieCarIndex; j >= train.Cars.Count; j--)
						{
							train.Cars.Add(new TrailerCar());
							train.Couplers.Add(new Coupler());

							train.ApplyPowerNotchesToCar();
							train.ApplyBrakeNotchesToCar();
							train.ApplyLocoBrakeNotchesToCar();
						}

						if (block.GetPath(ExtensionCfgKey.Object, trainPath, out string bogiePath))
						{
							if (isOdd)
							{
								train.Cars[bogieCarIndex].RearBogie.Object = bogiePath;
							}
							else
							{
								train.Cars[bogieCarIndex].FrontBogie.Object = bogiePath;
							}
						}

						if (block.GetVector2(ExtensionCfgKey.Axles, ',', out Vector2 bogieAxles))
						{
							if (bogieAxles.X >= bogieAxles.Y)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front for Car " + block.Index + " in file " + fileName);
							}
							else
							{
								if (isOdd)
								{
									train.Cars[bogieCarIndex].RearBogie.RearAxle = bogieAxles.X;
									train.Cars[bogieCarIndex].RearBogie.FrontAxle = bogieAxles.Y;
									train.Cars[bogieCarIndex].RearBogie.DefinedAxles = true;
								}
								else
								{
									train.Cars[bogieCarIndex].FrontBogie.RearAxle = bogieAxles.X;
									train.Cars[bogieCarIndex].FrontBogie.FrontAxle = bogieAxles.Y;
									train.Cars[bogieCarIndex].FrontBogie.DefinedAxles = true;
								}
							}
						}

						if (block.GetValue(ExtensionCfgKey.Reversed, out bool bogieReversed))
						{
							if (isOdd)
							{
								train.Cars[bogieCarIndex].FrontBogie.Reversed = bogieReversed;
							}
							else
							{
								train.Cars[bogieCarIndex].RearBogie.Reversed = bogieReversed;
							}
						}
						break;
				}

			}
		}
	}
}
