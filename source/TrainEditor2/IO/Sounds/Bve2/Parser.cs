using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using TrainEditor2.Models.Sounds;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Sounds.Bve2
{
	internal static class SoundCfgBve2
	{
		internal static void Parse(string trainFolder, out Sound sound)
		{
			sound = new Sound();

			CheckFile<BuzzerElement, BuzzerKey>(sound.SoundElements, trainFolder, "Adjust.wav", BuzzerKey.Correct);
			CheckFile<BrakeElement, BrakeKey>(sound.SoundElements, trainFolder, "Brake.wav", BrakeKey.BpDecomp);
			CheckFile<OthersElement, OthersKey>(sound.SoundElements, trainFolder, "Halt.wav", OthersKey.Halt);
			CheckFile<PrimaryHornElement, HornKey>(sound.SoundElements, trainFolder, "Klaxon.wav", HornKey.Loop);
			CheckFile<PrimaryHornElement, HornKey>(sound.SoundElements, trainFolder, "Klaxon0.wav", HornKey.Loop);
			CheckFile<SecondaryHornElement, HornKey>(sound.SoundElements, trainFolder, "Klaxon1.wav", HornKey.Loop);
			CheckFile<MusicHornElement, HornKey>(sound.SoundElements, trainFolder, "Klaxon2.wav", HornKey.Loop);
			CheckFile<PilotLampElement, PilotLampKey>(sound.SoundElements, trainFolder, "Leave.wav", PilotLampKey.On);
			CheckFile<BrakeElement, BrakeKey>(sound.SoundElements, trainFolder, "Air.wav", BrakeKey.BcRelease);
			CheckFile<BrakeElement, BrakeKey>(sound.SoundElements, trainFolder, "AirHigh.wav", BrakeKey.BcReleaseHigh);
			CheckFile<BrakeElement, BrakeKey>(sound.SoundElements, trainFolder, "AirZero.wav", BrakeKey.BcReleaseFull);
			CheckFile<CompressorElement, CompressorKey>(sound.SoundElements, trainFolder, "CpEnd.wav", CompressorKey.Release);
			CheckFile<CompressorElement, CompressorKey>(sound.SoundElements, trainFolder, "CpLoop.wav", CompressorKey.Loop);
			CheckFile<CompressorElement, CompressorKey>(sound.SoundElements, trainFolder, "CpStart.wav", CompressorKey.Attack);
			CheckFile<DoorElement, DoorKey>(sound.SoundElements, trainFolder, "DoorClsL.wav", DoorKey.CloseLeft);
			CheckFile<DoorElement, DoorKey>(sound.SoundElements, trainFolder, "DoorClsR.wav", DoorKey.CloseRight);
			CheckFile<DoorElement, DoorKey>(sound.SoundElements, trainFolder, "DoorCls.wav", DoorKey.CloseLeft);
			CheckFile<DoorElement, DoorKey>(sound.SoundElements, trainFolder, "DoorCls.wav", DoorKey.CloseRight);
			CheckFile<DoorElement, DoorKey>(sound.SoundElements, trainFolder, "DoorOpnL.wav", DoorKey.OpenLeft);
			CheckFile<DoorElement, DoorKey>(sound.SoundElements, trainFolder, "DoorOpnR.wav", DoorKey.OpenRight);
			CheckFile<DoorElement, DoorKey>(sound.SoundElements, trainFolder, "DoorOpn.wav", DoorKey.OpenLeft);
			CheckFile<DoorElement, DoorKey>(sound.SoundElements, trainFolder, "DoorOpn.wav", DoorKey.OpenRight);
			CheckFile<BrakeElement, BrakeKey>(sound.SoundElements, trainFolder, "EmrBrake.wav", BrakeKey.Emergency);
			CheckFiles<FlangeElement>(sound.SoundElements, trainFolder, "Flange", ".wav");
			CheckFile<OthersElement, OthersKey>(sound.SoundElements, trainFolder, "Loop.wav", OthersKey.Noise);
			CheckFile<FrontSwitchElement, int>(sound.SoundElements, trainFolder, "Point.wav", 0);

			CheckFile<OthersElement, OthersKey>(sound.SoundElements, trainFolder, "Rub.wav", OthersKey.Shoe);
			CheckFiles<RunElement>(sound.SoundElements, trainFolder, "Run", ".wav");
			CheckFile<SuspensionElement, SuspensionKey>(sound.SoundElements, trainFolder, "SpringL.wav", SuspensionKey.Left);
			CheckFile<SuspensionElement, SuspensionKey>(sound.SoundElements, trainFolder, "SpringR.wav", SuspensionKey.Right);
			CheckFiles<MotorElement>(sound.SoundElements, trainFolder, "Motor", ".wav");

			sound.SoundElements = new ObservableCollection<SoundElement>(sound.SoundElements.GroupBy(x => new { Type = x.GetType(), x.Key }).Select(x => x.First()));
		}

		private static void CheckFile<T, U>(ICollection<SoundElement> elements, string trainFolder, string fileName, U key) where T : SoundElement<U>, new()
		{
			string filePath = Path.CombineFile(trainFolder, fileName);

			if (File.Exists(filePath))
			{
				elements.Add(new T { Key = key, FilePath = filePath });
			}
		}

		private static void CheckFiles<T>(ICollection<SoundElement> elements, string trainFolder, string fileStart, string fileEnd) where T : SoundElement<int>, new()
		{
			string[] files = Directory.GetFiles(trainFolder);

			foreach (string file in files)
			{
				string a = System.IO.Path.GetFileName(file);

				if (string.IsNullOrEmpty(a))
				{
					continue;
				}

				if (a.Length > fileStart.Length + fileEnd.Length)
				{
					if (a.StartsWith(fileStart, StringComparison.OrdinalIgnoreCase) & a.EndsWith(fileEnd, StringComparison.OrdinalIgnoreCase))
					{
						string b = a.Substring(fileStart.Length, a.Length - fileEnd.Length - fileStart.Length);
						int n;

						if (int.TryParse(b, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
						{
							if (n >= 0)
							{
								elements.Add(new T { Key = n, FilePath = a });
							}
						}
					}
				}
			}
		}
	}
}
