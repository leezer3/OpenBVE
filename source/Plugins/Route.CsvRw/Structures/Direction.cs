using OpenBveApi.Interface;

namespace CsvRwRouteParser
{
	internal enum Direction
	{
		Left = -1,
		Both = 0,
		Right = 1,
		None = 2,
		Invalid = int.MaxValue
	}

	internal partial class Parser
	{
		internal static Direction FindDirection(string Direction, string Command, int Line, string File)
		{
			Direction = Direction.Trim();
			switch (Direction.ToUpperInvariant())
			{
				case "-1":
				case "L":
				case "LEFT":
					return CsvRwRouteParser.Direction.Left;
				case "0": 
				case "BOTH":
					return CsvRwRouteParser.Direction.Both;
				case "1":
				case "R":
				case "RIGHT":
					return CsvRwRouteParser.Direction.Right;
				case "N":
				case "NONE":
				case "NEITHER":
					return CsvRwRouteParser.Direction.None;
				default:
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Direction is invalid in " + Command + " at line " + Line + " in file " + File);
					return CsvRwRouteParser.Direction.Invalid;

			}
		}
	}
}
