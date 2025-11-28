using System.IO;

namespace OpenBve
{
	internal struct BlackBoxEntry
	{
		internal double Time;
		internal double Position;
		internal float Speed;
		internal float Acceleration;
		internal short ReverserDriver;
		internal short ReverserSafety;
		internal BlackBoxPower PowerDriver;
		internal BlackBoxPower PowerSafety;
		internal BlackBoxBrake BrakeDriver;
		internal BlackBoxBrake BrakeSafety;

		internal BlackBoxEntry(BinaryReader Reader)
		{
			Time = Reader.ReadDouble();
			Position = Reader.ReadDouble();
			Speed = Reader.ReadSingle();
			Acceleration = Reader.ReadSingle();
			ReverserDriver = Reader.ReadInt16();
			ReverserSafety = Reader.ReadInt16();
			PowerDriver = (BlackBoxPower)Reader.ReadInt16();
			PowerSafety = (BlackBoxPower)Reader.ReadInt16();
			BrakeDriver = (BlackBoxBrake)Reader.ReadInt16();
			BrakeSafety = (BlackBoxBrake)Reader.ReadInt16();
			Reader.ReadInt16(); // reserved
		}
	}
}
