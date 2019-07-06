using System;
using System.Globalization;
using System.IO;
using Path = OpenBveApi.Path;

namespace SoundEditor.Parsers.Sound
{
	internal static partial class SoundCfg
	{
		private static void ParseBve2(string trainFolder, Sounds result)
		{
			CheckFile(trainFolder, "Adjust.wav", result.Buzzer.Correct);
			CheckFile(trainFolder, "Brake.wav", result.Brake.BpDecomp);
			CheckFile(trainFolder, "Halt.wav", result.Others.Halt);
			CheckFile(trainFolder, "Klaxon.wav", result.PrimaryHorn.Loop);
			CheckFile(trainFolder, "Klaxon0.wav", result.PrimaryHorn.Loop);
			CheckFile(trainFolder, "Klaxon1.wav", result.SecondaryHorn.Loop);
			CheckFile(trainFolder, "Klaxon2.wav", result.MusicHorn.Loop);
			CheckFile(trainFolder, "Leave.wav", result.PilotLamp.On);
			CheckFile(trainFolder, "Air.wav", result.Brake.BcRelease);
			CheckFile(trainFolder, "AirHigh.wav", result.Brake.BcReleaseHigh);
			CheckFile(trainFolder, "AirZero.wav", result.Brake.BcReleaseFull);
			CheckFile(trainFolder, "CpEnd.wav", result.Compressor.Release);
			CheckFile(trainFolder, "CpLoop.wav", result.Compressor.Loop);
			CheckFile(trainFolder, "CpStart.wav", result.Compressor.Attack);
			CheckFile(trainFolder, "DoorClsL.wav", result.Door.CloseLeft);
			CheckFile(trainFolder, "DoorClsR.wav", result.Door.CloseRight);
			CheckFile(trainFolder, "DoorCls.wav", result.Door.CloseLeft);
			CheckFile(trainFolder, "DoorCls.wav", result.Door.CloseRight);
			CheckFile(trainFolder, "DoorOpnL.wav", result.Door.OpenLeft);
			CheckFile(trainFolder, "DoorOpnR.wav", result.Door.OpenRight);
			CheckFile(trainFolder, "DoorOpn.wav", result.Door.OpenLeft);
			CheckFile(trainFolder, "DoorOpn.wav", result.Door.OpenRight);
			CheckFile(trainFolder, "EmrBrake.wav", result.Brake.Emergency);
			CheckFiles(trainFolder, "Flange", ".wav", result.Flange);
			CheckFile(trainFolder, "Loop.wav", result.Others.Noise);

			IndexedSound point = new IndexedSound
			{
				Index = 0
			};
			CheckFile(trainFolder, "Point.wav", point);
			result.FrontSwitch.Add(point);

			CheckFile(trainFolder, "Rub.wav", result.Others.Shoe);
			CheckFiles(trainFolder, "Run", ".wav", result.Run);
			CheckFile(trainFolder, "SpringL.wav", result.Suspension.Left);
			CheckFile(trainFolder, "SpringR.wav", result.Suspension.Right);
			CheckFiles(trainFolder, "Motor", ".wav", result.Motor);
		}

		private static void CheckFile(string trainFolder, string fileName, Sound sound)
		{
			if (File.Exists(Path.CombineFile(trainFolder, fileName)))
			{
				sound.FileName = fileName;
			}
		}

		private static void CheckFiles(string trainFolder, string fileStart, string fileEnd, ListedSound sounds)
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
								sounds.Add(new IndexedSound
								{
									Index = n,
									FileName = a
								});
							}
						}
					}
				}
			}
		}
	}
}
