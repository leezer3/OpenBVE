using System.Drawing;

namespace OpenBve
{
    internal static partial class Game
    {
        internal static class RouteInformation
        {
	        private const int		DefaultRouteInfoSize	= 500;
            /// <summary>A bitmap storing the current route-map image</summary>
            internal static Bitmap RouteMap;
            /// <summary>A bitmap storing the current route gradient profile </summary>
            internal static Bitmap GradientProfile;
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

			/// <summary>Ranges of route info bitmaps</summary>
			internal static int GradientMinTrack, GradientMaxTrack;
			internal static int RouteMinX, RouteMaxX, RouteMinZ, RouteMaxZ;

			internal static void LoadInformation()
			{
				if (Loading.Cancel)
				{
					return;
				}
				lock (Illustrations.Locker)
				{
					RouteMap = Illustrations.CreateRouteMap(DefaultRouteInfoSize, DefaultRouteInfoSize, true);
					RouteMinX = Illustrations.LastRouteMinX;
					RouteMaxX = Illustrations.LastRouteMaxX;
					RouteMinZ = Illustrations.LastRouteMinZ;
					RouteMaxZ = Illustrations.LastRouteMaxZ;
					GradientProfile = Illustrations.CreateRouteGradientProfile(DefaultRouteInfoSize, DefaultRouteInfoSize, true);
					GradientMinTrack = Illustrations.LastGradientMinTrack;
					GradientMaxTrack = Illustrations.LastGradientMaxTrack;
				}
			}
        }
    }
}
