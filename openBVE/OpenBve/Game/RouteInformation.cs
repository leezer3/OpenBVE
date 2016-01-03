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
            /// <summary>A string storing the absolute on-disk path to a .RTF or .TXT document describing the briefing for the current scenario</summary>
            internal static string RouteBriefing;
            /// <summary>A string storing the absolute on-disk path to the current route file</summary>
            internal static string RouteFile;
            /// <summary>A string storing the absolute on-disk path to the current train folder</summary>
            internal static string TrainFolder;
            /// <summary>The number of files not found</summary>
            internal static string FilesNotFound;
            /// <summary>The number of errors and warnings</summary>
            internal static string ErrorsAndWarnings;

            internal static void LoadInformation()
            {
                RouteMap = Illustrations.CreateRouteMap(500, 500);
                GradientProfile = Illustrations.CreateRouteGradientProfile(500, 500);
            }
        }
    }
}
