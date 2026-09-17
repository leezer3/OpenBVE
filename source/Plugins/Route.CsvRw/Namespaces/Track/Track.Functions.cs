using OpenBveApi.Interface;
using RouteManager2.SignalManager;
using System;
using System.Globalization;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private static void ParseSafetySystem(string system, TrackCommand command, Expression expression, out SafetySystem device)
		{
			if (!Enum.TryParse(system, true, out device))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is invalid in " + command + " at line " + expression.Line.ToString(CultureInfo.InvariantCulture) + ", column " + expression.Column.ToString(CultureInfo.InvariantCulture) + " in file " + expression.File);
				device = Data.FileFormat == RoutefileFormat.Hmmsim ? SafetySystem.Ats : SafetySystem.Any;
			}

			if (Data.FileFormat != RoutefileFormat.Hmmsim && device == SafetySystem.Any)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is not supported in " + command + " at line " + expression.Line.ToString(CultureInfo.InvariantCulture) + ", column " + expression.Column.ToString(CultureInfo.InvariantCulture) + " in file " + expression.File);
			}
		}
	}
}
