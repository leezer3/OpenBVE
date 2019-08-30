using System;
using System.IO;
using System.Linq;
using System.Text;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Sounds;

namespace TrainEditor2.IO.Sounds.Bve4
{
	internal static partial class SoundCfgBve4
	{
		internal static void Write(string fileName, Sound sound)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Version 1.0");

			builder.AppendLine("[Run]");

			foreach (RunElement element in sound.SoundElements.OfType<RunElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Flange]");

			foreach (FlangeElement element in sound.SoundElements.OfType<FlangeElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Motor]");

			foreach (MotorElement element in sound.SoundElements.OfType<MotorElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Switch]");

			foreach (FrontSwitchElement element in sound.SoundElements.OfType<FrontSwitchElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Brake]");

			foreach (BrakeElement element in sound.SoundElements.OfType<BrakeElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Compressor]");

			foreach (CompressorElement element in sound.SoundElements.OfType<CompressorElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Suspension]");

			foreach (SuspensionElement element in sound.SoundElements.OfType<SuspensionElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Horn]");

			foreach (PrimaryHornElement element in sound.SoundElements.OfType<PrimaryHornElement>())
			{
				WriteKey(fileName, builder, element, "Primary");
			}

			foreach (SecondaryHornElement element in sound.SoundElements.OfType<SecondaryHornElement>())
			{
				WriteKey(fileName, builder, element, "Secondary");
			}

			foreach (MusicHornElement element in sound.SoundElements.OfType<MusicHornElement>())
			{
				WriteKey(fileName, builder, element, "Music");
			}

			builder.AppendLine("[Door]");

			foreach (DoorElement element in sound.SoundElements.OfType<DoorElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Ats]");

			foreach (AtsElement element in sound.SoundElements.OfType<AtsElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Buzzer]");

			foreach (BuzzerElement element in sound.SoundElements.OfType<BuzzerElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Pilot Lamp]");

			foreach (PilotLampElement element in sound.SoundElements.OfType<PilotLampElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Brake Handle]");

			foreach (BrakeHandleElement element in sound.SoundElements.OfType<BrakeHandleElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Master Controller]");

			foreach (MasterControllerElement element in sound.SoundElements.OfType<MasterControllerElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Reverser]");

			foreach (ReverserElement element in sound.SoundElements.OfType<ReverserElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Breaker]");

			foreach (BreakerElement element in sound.SoundElements.OfType<BreakerElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Request Stop]");

			foreach (RequestStopElement element in sound.SoundElements.OfType<RequestStopElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Touch]");

			foreach (TouchElement element in sound.SoundElements.OfType<TouchElement>())
			{
				WriteKey(fileName, builder, element);
			}

			builder.AppendLine("[Others]");

			foreach (OthersElement element in sound.SoundElements.OfType<OthersElement>())
			{
				WriteKey(fileName, builder, element);
			}

			File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
		}

		private static void WriteKey(string fileName, StringBuilder builder, SoundElement element, string keyPrefix = "")
		{
			if (!string.IsNullOrEmpty(element.FilePath))
			{
				builder.AppendLine($"{keyPrefix}{(element.Key is Enum ? ((Enum)element.Key).GetStringValues().First() : element.Key.ToString())} = {Utilities.MakeRelativePath(fileName, element.FilePath)}");
			}
		}
	}
}
