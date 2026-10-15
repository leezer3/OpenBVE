using OpenBveApi.Interface;
using OpenBveApi.Math;
using RouteManager2.SignalManager;
using System;
using System.Globalization;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		// Location suffix shared by every Track error message
		private static string At(Expression expression) =>
			" at line " + expression.Line.ToString(CultureInfo.InvariantCulture)
			+ ", column " + expression.Column.ToString(CultureInfo.InvariantCulture)
			+ " in file " + expression.File;

		// Multiply millimeter values by this to get meters
		private const double MmToM = 0.001;
		// Signal height used when none is given
		private const double DefaultSignalHeight = 4.8;

		// Parse X/Y position plus yaw/pitch/roll rotation starting at the given argument
		private static void ParseOffset(string[] arguments, int firstIndex, string command, Expression expression, double[] unitOfLength, out double x, out double y, out double yaw, out double pitch, out double roll)
		{
			x = 0.0; y = 0.0; yaw = 0.0; pitch = 0.0; roll = 0.0;
			if (arguments.Length >= firstIndex + 1 && arguments[firstIndex].Length > 0 && !NumberFormats.TryParseDoubleVb6(arguments[firstIndex], unitOfLength, out x))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + command + At(expression));
				x = 0.0;
			}

			if (arguments.Length >= firstIndex + 2 && arguments[firstIndex + 1].Length > 0 && !NumberFormats.TryParseDoubleVb6(arguments[firstIndex + 1], unitOfLength, out y))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + command + At(expression));
				y = 0.0;
			}

			if (arguments.Length >= firstIndex + 3 && arguments[firstIndex + 2].Length > 0 && !NumberFormats.TryParseDoubleVb6(arguments[firstIndex + 2], out yaw))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in " + command + At(expression));
				yaw = 0.0;
			}

			if (arguments.Length >= firstIndex + 4 && arguments[firstIndex + 3].Length > 0 && !NumberFormats.TryParseDoubleVb6(arguments[firstIndex + 3], out pitch))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in " + command + At(expression));
				pitch = 0.0;
			}

			if (arguments.Length >= firstIndex + 5 && arguments[firstIndex + 4].Length > 0 && !NumberFormats.TryParseDoubleVb6(arguments[firstIndex + 4], out roll))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in " + command + At(expression));
				roll = 0.0;
			}
		}

		private static void ParseSafetySystem(string system, TrackCommand command, Expression expression, out SafetySystem device)
		{
			if (!Enum.TryParse(system, true, out device))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is invalid in " + command + At(expression));
				device = Data.FileFormat == RoutefileFormat.Hmmsim ? SafetySystem.Ats : SafetySystem.Any;
			}

			if (Data.FileFormat != RoutefileFormat.Hmmsim && device == SafetySystem.Any)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is not supported in " + command + At(expression));
			}
		}
	}
}
