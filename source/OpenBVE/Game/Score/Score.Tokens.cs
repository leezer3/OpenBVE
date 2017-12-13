namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Holds the different score tokens</summary>
		internal enum ScoreTextToken : short
		{
			Invalid = 0,
			Overspeed = 1,
			PassedRedSignal = 2,
			Toppling = 3,
			Derailed = 4,
			PassengerDiscomfort = 5,
			DoorsOpened = 6,
			ArrivedAtStation = 7,
			PerfectTimeBonus = 8,
			Late = 9,
			PerfectStopBonus = 10,
			Stop = 11,
			PrematureDeparture = 12,
			Total = 13
		}
	}
}
