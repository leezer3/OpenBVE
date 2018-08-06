using System;
using System.Threading;
using OpenBve.Parsers.Train;

namespace OpenBve
{
	internal static partial class Loading
	{
		/// <summary>Loads bogus train data into the specified train</summary>
		/// <param name="Train">The train</param>
		private static void LoadBogusTrain(ref TrainManager.Train Train)
		{
			string TrainData = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility", "PreTrain"), "train.dat");
			TrainDatParser.ParseTrainData(TrainData, System.Text.Encoding.UTF8, Train);
			Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1);
			Train.InitializeCarSounds();
			Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1);
			TrainProgressCurrentWeight = 0.3 / TrainProgressMaximum;
			TrainProgressCurrentSum += TrainProgressCurrentWeight;
		}

		/// <summary>Loads BVE4 / openBVE data into the specified train</summary>
		/// <param name="Train">The specified train</param>
		private static void LoadBve4Train(ref TrainManager.Train Train)
		{
			Train.TrainFolder = CurrentTrainFolder;
			TrainProgressCurrentWeight = 0.1 / TrainProgressMaximum;
			string TrainData = OpenBveApi.Path.CombineFile(Train.TrainFolder, "train.dat");
			TrainDatParser.ParseTrainData(TrainData, CurrentTrainEncoding, Train);
			TrainProgressCurrentSum += TrainProgressCurrentWeight;
			Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1);
			TrainProgressCurrentWeight = 0.2 / TrainProgressMaximum;
			SoundCfgParser.ParseSoundConfig(Train.TrainFolder, CurrentTrainEncoding, Train);
			TrainProgressCurrentSum += TrainProgressCurrentWeight;
			Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1);
			// door open/close speed
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (Train.Cars[i].Specs.DoorOpenFrequency <= 0.0)
				{
					if (Train.Cars[i].Doors[0].OpenSound.Buffer != null & Train.Cars[i].Doors[1].OpenSound.Buffer != null)
					{
						Sounds.LoadBuffer(Train.Cars[i].Doors[0].OpenSound.Buffer);
						Sounds.LoadBuffer(Train.Cars[i].Doors[1].OpenSound.Buffer);
						double a = Train.Cars[i].Doors[0].OpenSound.Buffer.Duration;
						double b = Train.Cars[i].Doors[1].OpenSound.Buffer.Duration;
						Train.Cars[i].Specs.DoorOpenFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
					}
					else if (Train.Cars[i].Doors[0].OpenSound.Buffer != null)
					{
						Sounds.LoadBuffer(Train.Cars[i].Doors[0].OpenSound.Buffer);
						double a = Train.Cars[i].Doors[0].OpenSound.Buffer.Duration;
						Train.Cars[i].Specs.DoorOpenFrequency = a > 0.0 ? 1.0 / a : 0.8;
					}
					else if (Train.Cars[i].Doors[1].OpenSound.Buffer != null)
					{
						Sounds.LoadBuffer(Train.Cars[i].Doors[0].OpenSound.Buffer);
						double b = Train.Cars[i].Doors[1].OpenSound.Buffer.Duration;
						Train.Cars[i].Specs.DoorOpenFrequency = b > 0.0 ? 1.0 / b : 0.8;
					}
					else
					{
						Train.Cars[i].Specs.DoorOpenFrequency = 0.8;
					}
				}

				if (Train.Cars[i].Specs.DoorCloseFrequency <= 0.0)
				{
					if (Train.Cars[i].Doors[0].CloseSound.Buffer != null & Train.Cars[i].Doors[1].CloseSound.Buffer != null)
					{
						Sounds.LoadBuffer(Train.Cars[i].Doors[0].CloseSound.Buffer);
						Sounds.LoadBuffer(Train.Cars[i].Doors[1].CloseSound.Buffer);
						double a = Train.Cars[i].Doors[0].CloseSound.Buffer.Duration;
						double b = Train.Cars[i].Doors[1].CloseSound.Buffer.Duration;
						Train.Cars[i].Specs.DoorCloseFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
					}
					else if (Train.Cars[i].Doors[0].CloseSound.Buffer != null)
					{
						Sounds.LoadBuffer(Train.Cars[i].Doors[0].CloseSound.Buffer);
						double a = Train.Cars[i].Doors[0].CloseSound.Buffer.Duration;
						Train.Cars[i].Specs.DoorCloseFrequency = a > 0.0 ? 1.0 / a : 0.8;
					}
					else if (Train.Cars[i].Doors[1].CloseSound.Buffer != null)
					{
						Sounds.LoadBuffer(Train.Cars[i].Doors[0].CloseSound.Buffer);
						double b = Train.Cars[i].Doors[1].CloseSound.Buffer.Duration;
						Train.Cars[i].Specs.DoorCloseFrequency = b > 0.0 ? 1.0 / b : 0.8;
					}
					else
					{
						Train.Cars[i].Specs.DoorCloseFrequency = 0.8;
					}
				}

				const double f = 0.015;
				const double g = 2.75;
				Train.Cars[i].Specs.DoorOpenPitch = Math.Exp(f * Math.Tan(g * (Program.RandomNumberGenerator.NextDouble() - 0.5)));
				Train.Cars[i].Specs.DoorClosePitch = Math.Exp(f * Math.Tan(g * (Program.RandomNumberGenerator.NextDouble() - 0.5)));
				Train.Cars[i].Specs.DoorOpenFrequency /= Train.Cars[i].Specs.DoorOpenPitch;
				Train.Cars[i].Specs.DoorCloseFrequency /= Train.Cars[i].Specs.DoorClosePitch;
				/* 
				 * Remove the following two lines, then the pitch at which doors play
				 * takes their randomized opening and closing times into account.
				 * */
				Train.Cars[i].Specs.DoorOpenPitch = 1.0;
				Train.Cars[i].Specs.DoorClosePitch = 1.0;
			}

			if (Train == TrainManager.PlayerTrain)
			{
				World.CameraCar = Train.DriverCar;
				TrainProgressCurrentWeight = 0.7 / TrainProgressMaximum;
				TrainManager.ParsePanelConfig(Train.TrainFolder, CurrentTrainEncoding, Train);
				TrainProgressCurrentSum += TrainProgressCurrentWeight;
				Thread.Sleep(1);
				if (Cancel) return;
				Program.AppendToLogFile("Train panel loaded sucessfully.");
			}

			if (Train.State != TrainManager.TrainState.Bogus)
			{
				bool LoadObjects = false;
				if (CarObjects == null)
				{
					CarObjects = new ObjectManager.UnifiedObject[Train.Cars.Length];
					BogieObjects = new ObjectManager.UnifiedObject[Train.Cars.Length * 2];
					LoadObjects = true;
				}

				string tXml = OpenBveApi.Path.CombineFile(Train.TrainFolder, "train.xml");
				if (System.IO.File.Exists(tXml))
				{
					TrainXmlParser.Parse(tXml, Train, ref CarObjects, ref BogieObjects);
				}
				else
				{
					ExtensionsCfgParser.ParseExtensionsConfig(Train.TrainFolder, CurrentTrainEncoding, ref CarObjects, ref BogieObjects, Train, LoadObjects);
				}
				Thread.Sleep(1);
				if (Cancel) return;
				//Stores the current array index of the bogie object to add
				//Required as there are two bogies per car, and we're using a simple linear array....
				int currentBogieObject = 0;
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					if (CarObjects[i] == null)
					{
						// load default exterior object
						string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
						ObjectManager.StaticObject so = ObjectManager.LoadStaticObject(file, System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
						if (so == null)
						{
							CarObjects[i] = null;
						}
						else
						{
							double sx = Train.Cars[i].Width;
							double sy = Train.Cars[i].Height;
							double sz = Train.Cars[i].Length;
							so.ApplyScale(sx, sy, sz);
							CarObjects[i] = so;
						}
					}
					if (CarObjects[i] != null)
					{
						// add object
						Train.Cars[i].LoadCarSections(CarObjects[i]);
					}
					//Load bogie objects
					if (BogieObjects[currentBogieObject] != null)
					{
						Train.Cars[i].FrontBogie.LoadCarSections(BogieObjects[currentBogieObject]);
					}
					currentBogieObject++;
					if (BogieObjects[currentBogieObject] != null)
					{
						Train.Cars[i].RearBogie.LoadCarSections(BogieObjects[currentBogieObject]);
					}
					currentBogieObject++;
				}
			}
		}
	}
}
