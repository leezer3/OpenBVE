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

			CheckFile<BuzzerElement, SoundKey.Buzzer>(sound.SoundElements, trainFolder, "Adjust.wav", SoundKey.Buzzer.Correct);
			CheckFile<BrakeElement, SoundKey.Brake>(sound.SoundElements, trainFolder, "Brake.wav", SoundKey.Brake.BpDecomp);
			CheckFile<OthersElement, SoundKey.Others>(sound.SoundElements, trainFolder, "Halt.wav", SoundKey.Others.Halt);
			CheckFile<PrimaryHornElement, SoundKey.Horn>(sound.SoundElements, trainFolder, "Klaxon.wav", SoundKey.Horn.Loop);
			CheckFile<PrimaryHornElement, SoundKey.Horn>(sound.SoundElements, trainFolder, "Klaxon0.wav", SoundKey.Horn.Loop);
			CheckFile<SecondaryHornElement, SoundKey.Horn>(sound.SoundElements, trainFolder, "Klaxon1.wav", SoundKey.Horn.Loop);
			CheckFile<MusicHornElement, SoundKey.Horn>(sound.SoundElements, trainFolder, "Klaxon2.wav", SoundKey.Horn.Loop);
			CheckFile<PilotLampElement, SoundKey.PilotLamp>(sound.SoundElements, trainFolder, "Leave.wav", SoundKey.PilotLamp.On);
			CheckFile<BrakeElement, SoundKey.Brake>(sound.SoundElements, trainFolder, "Air.wav", SoundKey.Brake.BcRelease);
			CheckFile<BrakeElement, SoundKey.Brake>(sound.SoundElements, trainFolder, "AirHigh.wav", SoundKey.Brake.BcReleaseHigh);
			CheckFile<BrakeElement, SoundKey.Brake>(sound.SoundElements, trainFolder, "AirZero.wav", SoundKey.Brake.BcReleaseFull);
			CheckFile<CompressorElement, SoundKey.Compressor>(sound.SoundElements, trainFolder, "CpEnd.wav", SoundKey.Compressor.Release);
			CheckFile<CompressorElement, SoundKey.Compressor>(sound.SoundElements, trainFolder, "CpLoop.wav", SoundKey.Compressor.Loop);
			CheckFile<CompressorElement, SoundKey.Compressor>(sound.SoundElements, trainFolder, "CpStart.wav", SoundKey.Compressor.Attack);
			CheckFile<DoorElement, SoundKey.Door>(sound.SoundElements, trainFolder, "DoorClsL.wav", SoundKey.Door.CloseLeft);
			CheckFile<DoorElement, SoundKey.Door>(sound.SoundElements, trainFolder, "DoorClsR.wav", SoundKey.Door.CloseRight);
			CheckFile<DoorElement, SoundKey.Door>(sound.SoundElements, trainFolder, "DoorCls.wav", SoundKey.Door.CloseLeft);
			CheckFile<DoorElement, SoundKey.Door>(sound.SoundElements, trainFolder, "DoorCls.wav", SoundKey.Door.CloseRight);
			CheckFile<DoorElement, SoundKey.Door>(sound.SoundElements, trainFolder, "DoorOpnL.wav", SoundKey.Door.OpenLeft);
			CheckFile<DoorElement, SoundKey.Door>(sound.SoundElements, trainFolder, "DoorOpnR.wav", SoundKey.Door.OpenRight);
			CheckFile<DoorElement, SoundKey.Door>(sound.SoundElements, trainFolder, "DoorOpn.wav", SoundKey.Door.OpenLeft);
			CheckFile<DoorElement, SoundKey.Door>(sound.SoundElements, trainFolder, "DoorOpn.wav", SoundKey.Door.OpenRight);
			CheckFile<BrakeElement, SoundKey.Brake>(sound.SoundElements, trainFolder, "EmrBrake.wav", SoundKey.Brake.Emergency);
			CheckFiles<FlangeElement>(sound.SoundElements, trainFolder, "Flange", ".wav");
			CheckFile<OthersElement, SoundKey.Others>(sound.SoundElements, trainFolder, "Loop.wav", SoundKey.Others.Noise);
			CheckFile<FrontSwitchElement, int>(sound.SoundElements, trainFolder, "Point.wav", 0);

			CheckFile<OthersElement, SoundKey.Others>(sound.SoundElements, trainFolder, "Rub.wav", SoundKey.Others.Shoe);
			CheckFiles<RunElement>(sound.SoundElements, trainFolder, "Run", ".wav");
			CheckFile<SuspensionElement, SoundKey.Suspension>(sound.SoundElements, trainFolder, "SpringL.wav", SoundKey.Suspension.Left);
			CheckFile<SuspensionElement, SoundKey.Suspension>(sound.SoundElements, trainFolder, "SpringR.wav", SoundKey.Suspension.Right);
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
