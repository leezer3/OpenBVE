using OpenBveApi.Interface;
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
