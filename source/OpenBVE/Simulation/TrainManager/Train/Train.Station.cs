namespace OpenBve
{
    public static partial class TrainManager
    {
        internal class StationInformation
        {
            /// <summary>The index of the next station </summary>
            internal int NextStation;
            /// <summary>The index of the previous station</summary>
            internal int PreviousStation;
            /// <summary>The current station stop state</summary>
            internal TrainStopState CurrentStopState;
            /// <summary>The expected arrival time at the next stop</summary>
            internal double ExpectedArrivalTime;
            /// <summary>The expected departure time at the next stop</summary>
            internal double ExpectedDepartureTime;
            /// <summary>Whether the departure sound has been played for the current station stop</summary>
            internal bool DepartureSoundPlayed;
            /// <summary>Whether the stop position for the current station requires adjusting</summary>
            internal bool AdjustStopPosition;
            /// <summary>The distance to the stop position</summary>
            internal double DistanceToStopPosition;
        }
    }
}
