namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>The default number of points lost when the doors are opened unexpectedly</summary>
		private const double ScoreFactorOpenedDoors = -10.0;
		/// <summary>The default number of points lost per second when running overspeed</summary>
		private const double ScoreFactorOverspeed = -1.0;
		/// <summary>The default number of points lost when toppling the train through overspeed</summary>
		private const double ScoreFactorToppling = -10.0;
		/// <summary>The default number of points lost per second late</summary>
		private const double ScoreFactorStationLate = -0.333333333333333;
		/// <summary>The default number of points lost when missing a station's defined stop point</summary>
		private const double ScoreFactorStationStop = -50.0;
		/// <summary>The default number of points lost when departing unexpectedly from a station</summary>
		private const double ScoreFactorStationDeparture = -1.5;
		/// <summary>The default number of points lost when the train is derailed</summary>
		private const int ScoreValueDerailment = -1000;
		/// <summary>The default number of points lost when a red signal is passed</summary>
		private const int ScoreValueRedSignal = -100;
		/// <summary>The default number of points gained when arriving at a station on time</summary>
		private const int ScoreValueStationPerfectTime = 15;
		/// <summary>The default number of points gained when stopping within tolerance of a station's defined stop point</summary>
		private const int ScoreValueStationPerfectStop = 15;
		/// <summary>The default number of points lost when the passengers are experiencing discomfort (Excessive speed through curves etc)</summary>
		private const int ScoreValuePassengerDiscomfort = -20;
		/// <summary>The default number of points gained when stopping at a scheduled station</summary>
		internal const int ScoreValueStationArrival = 100;
	}
}
