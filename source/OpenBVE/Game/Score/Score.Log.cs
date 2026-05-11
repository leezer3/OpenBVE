using System;
using System.Collections.Generic;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>The current route name used for score logging</summary>
		internal static string LogRouteName = "";
		/// <summary>The current train name used for score logging</summary>
		internal static string LogTrainName = "";
		/// <summary>The current time for score logging</summary>
		internal static DateTime LogDateTime = DateTime.Now;
		/// <summary>Holds all score log messages generated</summary>
		internal static List<ScoreLog> ScoreLogs = new List<ScoreLog>();

		/// <summary>Represents a score log message</summary>
		internal struct ScoreLog
		{
			/// <summary>The value for the score event</summary>
			internal int Value;
			/// <summary>The token for the score event which triggered this log message</summary>
			internal ScoreTextToken TextToken;
			/// <summary>The absolute track position (Front axle of the front car) which this log message was generated at</summary>
			internal double Position;
			/// <summary>The in-game time at which this log message was generated at</summary>
			internal double Time;

			internal ScoreLog(double time, double position, int value, ScoreTextToken token)
			{
				Time = time;
				Position = position;
				Value = value;
				TextToken = token;
			}
		}
		
	}
}
