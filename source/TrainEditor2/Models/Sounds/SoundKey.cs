using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainEditor2.Models.Sounds
{
	internal static class SoundKey
	{
		internal enum Brake
		{
			BcReleaseHigh,
			BcRelease,
			BcReleaseFull,
			Emergency,
			BpDecomp
		}

		internal enum BrakeHandle
		{
			Apply,
			ApplyFast,
			Release,
			ReleaseFast,
			Min,
			Max
		}

		internal enum Breaker
		{
			On,
			Off
		}

		internal enum Buzzer
		{
			Correct
		}

		internal enum Compressor
		{
			Attack,
			Loop,
			Release
		}

		internal enum Door
		{
			OpenLeft,
			CloseLeft,
			OpenRight,
			CloseRight
		}

		internal enum Horn
		{
			Start,
			Loop,
			End
		}

		internal enum MasterController
		{
			Up,
			UpFast,
			Down,
			DownFast,
			Min,
			Max
		}

		internal enum Others
		{
			Noise,
			Shoe,
			Halt
		}

		internal enum PilotLamp
		{
			On,
			Off
		}

		internal enum RequestStop
		{
			Stop,
			Pass,
			Ignored
		}

		internal enum Reverser
		{
			On,
			Off
		}

		internal enum Suspension
		{
			Left,
			Right
		}

		private struct TypeValuePair
		{
			internal Type Type
			{
				get;
			}

			internal Enum Value
			{
				get;
			}

			internal TypeValuePair(Type type, Enum value)
			{
				Type = type;
				Value = value;
			}
		}

		private static readonly Dictionary<TypeValuePair, List<string>> rewordsDictionary = new Dictionary<TypeValuePair, List<string>>();

		static SoundKey()
		{
			AddReword(Brake.BcReleaseHigh, "Bc Release High", "ReleaseHigh");
			AddReword(Brake.BcRelease, "Bc Release", "Release");
			AddReword(Brake.BcReleaseFull, "Bc Release Full", "ReleaseFull");
			AddReword(Brake.Emergency, "Emergency");
			AddReword(Brake.BpDecomp, "BP Decomp", "Application");
			AddReword(BrakeHandle.Apply, "Apply");
			AddReword(BrakeHandle.ApplyFast, "ApplyFast");
			AddReword(BrakeHandle.Release, "Release");
			AddReword(BrakeHandle.ReleaseFast, "ReleaseFast");
			AddReword(BrakeHandle.Min, "Min", "Minimum");
			AddReword(BrakeHandle.Max, "Max", "Maximum");
			AddReword(Breaker.On, "On");
			AddReword(Breaker.Off, "Off");
			AddReword(Buzzer.Correct, "Correct");
			AddReword(Compressor.Attack, "Attack", "Start");
			AddReword(Compressor.Loop, "Loop");
			AddReword(Compressor.Release, "Release", "Stop", "End");
			AddReword(Door.OpenLeft, "Open Left", "OpenLeft", "LeftOpen");
			AddReword(Door.CloseLeft, "Close Left", "CloseLeft", "LeftClose");
			AddReword(Door.OpenRight, "Open Right", "OpenRight", "RightOpen");
			AddReword(Door.CloseRight, "Close Right", "CloseRight", "RightClose");
			AddReword(Horn.Start, "Start");
			AddReword(Horn.Loop, "Loop");
			AddReword(Horn.End, "End", "Release", "Stop");
			AddReword(MasterController.Up, "Up", "Increase");
			AddReword(MasterController.UpFast, "UpFast", "IncreaseFast");
			AddReword(MasterController.Down, "Down", "Decrease");
			AddReword(MasterController.DownFast, "DownFast", "DecreaseFast");
			AddReword(MasterController.Min, "Min", "Minimum");
			AddReword(MasterController.Max, "Max", "Maximum");
			AddReword(Others.Noise, "Noise");
			AddReword(Others.Shoe, "Shoe");
			AddReword(Others.Halt, "Halt");
			AddReword(PilotLamp.On, "On");
			AddReword(PilotLamp.Off, "Off");
			AddReword(RequestStop.Stop, "Stop");
			AddReword(RequestStop.Pass, "Pass");
			AddReword(RequestStop.Ignored, "Ignored");
			AddReword(Reverser.On, "On");
			AddReword(Reverser.Off, "Off");
			AddReword(Suspension.Left, "Left");
			AddReword(Suspension.Right, "Right");
		}

		private static void AddReword(Enum keyValue, params string[] rewords)
		{
			TypeValuePair pair = new TypeValuePair(keyValue.GetType(), keyValue);

			if (!rewordsDictionary.ContainsKey(pair))
			{
				rewordsDictionary.Add(pair, new List<string>());
			}

			rewordsDictionary[pair].AddRange(rewords);
		}

		internal static bool TryParse<T>(string text, bool ignoreCase, out T keyValue) where T : struct
		{
			if (Enum.TryParse(text, ignoreCase, out keyValue))
			{
				return true;
			}

			Enum[] rewords = rewordsDictionary.Where(x => x.Key.Type == typeof(T) && x.Value.Any(y => string.Equals(y, text, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))).Select(x => x.Key.Value).ToArray();

			if (rewords.Any())
			{
				keyValue = (T)(object)rewords.First();
				return true;
			}

			keyValue = default(T);
			return false;
		}

		internal static T Parse<T>(string text, bool ignoreCase) where T : struct
		{
			T keyValue;

			if (!TryParse(text, ignoreCase, out keyValue))
			{
				throw new FormatException();
			}

			return keyValue;
		}

		internal static string[] GetRewords(Enum keyValue)
		{
			return rewordsDictionary[new TypeValuePair(keyValue.GetType(), keyValue)].ToArray();
		}
	}
}
