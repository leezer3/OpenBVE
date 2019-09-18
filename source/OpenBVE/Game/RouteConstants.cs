namespace OpenBve
{
    internal static partial class Game
    {
        /// <summary>The route's comment (For display in the main menu)</summary>
        internal static string RouteComment = "";
        /// <summary>The route's image file (For display in the main menu)</summary>
        internal static string RouteImage = "";
        internal static double[] RouteUnitOfLength = new double[] { 1.0 };
        internal const double CoefficientOfGroundFriction = 0.5;
        /// <summary>The speed difference in m/s above which derailments etc. will occur</summary>
        internal const double CriticalCollisionSpeedDifference = 8.0;
        /// <summary>The number of pascals leaked by the brake pipe each second</summary>
        internal const double BrakePipeLeakRate = 500000.0;
    }
}
