namespace OpenBve
{
	internal static partial class Game
	{
		internal partial class Score
		{
			/// <summary>The default number of points lost when the doors are opened unexpectedly</summary>
			private const double FactorOpenedDoors = -10.0;
			/// <summary>The default number of points lost per second when running overspeed</summary>
			private const double FactorOverspeed = -1.0;
			/// <summary>The default number of points lost when toppling the train through overspeed</summary>
			private const double FactorToppling = -10.0;
			/// <summary>The default number of points lost per second late</summary>
			private const double FactorStationLate = -0.333333333333333;
			/// <summary>The default number of points lost when missing a station's defined stop point</summary>
			private const double FactorStationStop = -50.0;
			/// <summary>The default number of points lost when departing unexpectedly from a station</summary>
			private const double FactorStationDeparture = -1.5;
			/// <summary>The default number of points lost when the train is derailed</summary>
			private const int ValueDerailment = -1000;
			/// <summary>The default number of points lost when a red signal is passed</summary>
			private const int ValueRedSignal = -100;
			/// <summary>The default number of points gained when arriving at a station on time</summary>
			private const int ValueStationPerfectTime = 15;
			/// <summary>The default number of points gained when stopping within tolerance of a station's defined stop point</summary>
			private const int ValueStationPerfectStop = 15;
			/// <summary>The default number of points lost when the passengers are experiencing discomfort (Excessive speed through curves etc)</summary>
			private const int ValuePassengerDiscomfort = -20;
			/// <summary>The default number of points gained when stopping at a scheduled station</summary>
			internal const int ValueStationArrival = 100;
		}
	}
}
