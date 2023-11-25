using System;
using System.Text;
using Formats.OpenBve;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
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

		internal static bool unicodeCheck;

		// parse extensions config
		internal void ParseExtensionsConfig(string TrainPath, Encoding Encoding, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, ref UnifiedObject[] CouplerObjects, out bool[] VisibleFromInterior, TrainBase Train)
		{
			VisibleFromInterior = new bool[Train.Cars.Length];
			bool[] CarObjectsReversed = new bool[Train.Cars.Length];
			bool[] BogieObjectsReversed = new bool[Train.Cars.Length * 2];
			bool[] CarsDefined = new bool[Train.Cars.Length];
			bool[] BogiesDefined = new bool[Train.Cars.Length * 2];
			string FileName = Path.CombineFile(TrainPath, "extensions.cfg");
			if (System.IO.File.Exists(FileName)) {
				Encoding = TextEncoding.GetSystemEncodingFromFile(FileName, Encoding);

				string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
				if (Lines.Length == 1)
				{
					if (unicodeCheck)
					{
						return;
					}

					unicodeCheck = true;
					/*
					 * If only one line, there's a good possibility that our file is NOT Unicode at all
					 * and that the misdetection has turned it into garbage
					 *
					 * Try again with ASCII instead
					 */
					ParseExtensionsConfig(TrainPath, Encoding.GetEncoding(1252), ref CarObjects, ref BogieObjects, ref CouplerObjects, out VisibleFromInterior, Train);
					return;
				}
				ConfigFile<ExtensionCfgSection, ExtensionCfgKey> cfg = new ConfigFile<ExtensionCfgSection, ExtensionCfgKey>(Lines, Plugin.currentHost);
				while (cfg.RemainingSubBlocks > 0)
				{
					Block<ExtensionCfgSection, ExtensionCfgKey> block = cfg.ReadNextBlock();
					switch (block.Key)
					{
						case ExtensionCfgSection.Exterior:
							while (block.RemainingDataValues > 0 && block.GetIndexedValue(out var carIndex, out var fileName))
							{
								string File = Path.CombineFile(TrainPath, fileName);
								if (System.IO.File.Exists(File))
								{
									Plugin.currentHost.LoadObject(File, Encoding, out CarObjects[carIndex]);
								}
								else
								{
									Plugin.currentHost.AddMessage(MessageType.Error, true, "The car object " + File + " for Car " + carIndex + " does not exist in file " + FileName);
								}
							}
							break;
						case ExtensionCfgSection.Car:
							if (block.Index == -1)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid or missing CarIndex in file " + FileName);
								break;
							}

							if (block.Index > Train.Cars.Length)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "CarIndex " + block.Index + " does not reference an existing car in in file " + FileName);
								break;
							}

							if (CarsDefined[block.Index])
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "CarIndex " + block.Index + " has already been declared in in file " + FileName);
							}

							CarsDefined[block.Index] = true;
							if (block.GetValue(ExtensionCfgKey.Object, out string carObject))
							{
								if (!Path.ContainsInvalidChars(carObject))
								{
									string File = Path.CombineFile(TrainPath, carObject);
									if (System.IO.File.Exists(File))
									{
										Plugin.currentHost.LoadObject(File, Encoding, out CarObjects[block.Index]);
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, true, "The object " + File + " for Car " + block.Index + " does not exist in file " + FileName);
									}
								}
							}

							if (block.GetValue(ExtensionCfgKey.Length, out double carLength))
							{
								Train.Cars[block.Index].Length = carLength;
							}
							block.GetValue(ExtensionCfgKey.Reversed, out CarObjectsReversed[block.Index]);
							block.GetValue(ExtensionCfgKey.VisibleFromInterior, out VisibleFromInterior[block.Index]);
							block.GetValue(ExtensionCfgKey.LoadingSway, out Train.Cars[block.Index].EnableLoadingSway);
							if (block.GetVector2(ExtensionCfgKey.Axles, ',', out Vector2 carAxles))
							{
								if (carAxles.X >= carAxles.Y)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front for Car " + block.Index + " in file " + FileName);
								}
								else
								{
									Train.Cars[block.Index].RearAxle.Position = carAxles.X;	
									Train.Cars[block.Index].FrontAxle.Position = carAxles.Y;
								}
								
							}
							break;
						case ExtensionCfgSection.Coupler:
							if (block.GetVector2(ExtensionCfgKey.Axles, ',', out Vector2 distances))
							{
								if (distances.X >= distances.Y)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Minimum is expected to be less than Maximum in for Coupler " + block.Index + " in file " + FileName);
								}
								else
								{
									Train.Cars[block.Index].Coupler.MinimumDistanceBetweenCars = distances.X;
									Train.Cars[block.Index].Coupler.MaximumDistanceBetweenCars = distances.Y;
								}
							}
							if (block.GetValue(ExtensionCfgKey.Object, out string couplerObject))
							{
								if (!Path.ContainsInvalidChars(couplerObject))
								{
									string File = Path.CombineFile(TrainPath, couplerObject);
									if (System.IO.File.Exists(File))
									{
										Plugin.currentHost.LoadObject(File, Encoding, out CouplerObjects[block.Index]);
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, true, "The object " + File + " for Coupler " + block.Index + " does not exist in file " + FileName);
									}
								}
							}
							break;
						case ExtensionCfgSection.Bogie:
							if (block.Index == -1)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid or missing BogieIndex in file " + FileName);
								break;
							}

							if (block.Index > Train.Cars.Length * 2)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "BogieIndex " + block.Index + " does not reference an existing bogie in in file " + FileName);
								break;
							}

							if (BogiesDefined[block.Index])
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "BogieIndex " + block.Index + " has already been declared in in file " + FileName);
							}
							BogiesDefined[block.Index] = true;
							//Assuming that there are two bogies per car
							bool IsOdd = (block.Index % 2 != 0);
							int CarIndex = block.Index / 2;
							bool DefinedAxles = false;

							if (block.GetValue(ExtensionCfgKey.Object, out string bogieObject))
							{
								if (!Path.ContainsInvalidChars(bogieObject))
								{
									string File = Path.CombineFile(TrainPath, bogieObject);
									if (System.IO.File.Exists(File))
									{
										Plugin.currentHost.LoadObject(File, Encoding, out BogieObjects[block.Index]);
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, true, "The object " + File + " for Bogie " + block.Index + " does not exist in file " + FileName);
									}
								}
							}
							block.GetValue(ExtensionCfgKey.Reversed, out BogieObjectsReversed[block.Index]);
							if (block.GetVector2(ExtensionCfgKey.Axles, ',', out Vector2 bogieAxles))
							{
								if (bogieAxles.X >= bogieAxles.Y)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front for Bogie " + block.Index + " in file " + FileName);
								}
								else
								{
									if (IsOdd)
									{
										Train.Cars[CarIndex].FrontBogie.RearAxle.Position = bogieAxles.X;
										Train.Cars[CarIndex].FrontBogie.FrontAxle.Position = bogieAxles.Y;
									}
									else
									{
										Train.Cars[CarIndex].RearBogie.RearAxle.Position = bogieAxles.X;
										Train.Cars[CarIndex].RearBogie.FrontAxle.Position = bogieAxles.Y;
									}
									DefinedAxles = true;
								}
							}
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
							break;
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
