using OpenBveApi.Interface;
using RouteManager2.SignalManager;
using System;
using System.Globalization;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private static void ParseSafetySystem(string system, TrackCommand command, Expression expression, RouteData data, out SafetySystem device)
		{
			if (!Enum.TryParse(system, out device))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is invalid in " + command + " at line " + expression.Line.ToString(CultureInfo.InvariantCulture) + ", column " + expression.Column.ToString(CultureInfo.InvariantCulture) + " in file " + expression.File);
				device = data.IsHmmsim ? SafetySystem.Ats : SafetySystem.Any;
			}

			if (!data.IsHmmsim && device == SafetySystem.Any)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is not supported in " + command + " at line " + expression.Line.ToString(CultureInfo.InvariantCulture) + ", column " + expression.Column.ToString(CultureInfo.InvariantCulture) + " in file " + expression.File);
			}
		}
	}
}
