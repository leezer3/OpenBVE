using System.Drawing;

namespace OpenBve
{
    internal static partial class Game
    {
        internal static class RouteInformation
        {
            /// <summary>A bitmap storing the current route-map image</summary>
            internal static Bitmap RouteMap;
            /// <summary>A bitmap storing the current route gradient profile </summary>
            internal static Bitmap GradientProfile;
            /// <summary>A bitmap storing the current auto-generated timetable</summary>
            internal static Bitmap TimeTable;


            internal static void LoadInformation()
            {
                RouteMap = Illustrations.CreateRouteMap(500, 500);
                GradientProfile = Illustrations.CreateRouteGradientProfile(500, 500);
            }
        }
    }
}
