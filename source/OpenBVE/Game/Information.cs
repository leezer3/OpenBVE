namespace OpenBve
{
    internal static partial class Game
    {
        /// <summary>The game's current framerate</summary>
        internal static double InfoFrameRate = 1.0;
        /// <summary>The current plugin debug message to be displayed</summary>
        internal static string InfoDebugString = "";
        /// <summary>The total number of OpenGL triangles in the current frame</summary>
        internal static int InfoTotalTriangles = 0;
        /// <summary>The total number of OpenGL triangle strips in the current frame</summary>
        internal static int InfoTotalTriangleStrip = 0;
        /// <summary>The total number of OpenGL quad strips in the current frame</summary>
        internal static int InfoTotalQuadStrip = 0;
        /// <summary>The total number of OpenGL quads in the current frame</summary>
        internal static int InfoTotalQuads = 0;
        /// <summary>The total number of OpenGL polygons in the current frame</summary>
        internal static int InfoTotalPolygon = 0;
        /// <summary>The total number of static opaque faces in the current frame</summary>
        internal static int InfoStaticOpaqueFaceCount = 0;

        /// <summary>The game's current interface type</summary>
        internal enum InterfaceType
        {
            /// <summary>The game is currently showing a normal display</summary>
            Normal,
            /// <summary>The game is currently showing the pause display</summary>
            Pause,
            /// <summary>The game is currently showing a menu</summary>
            Menu,
        }
        /// <summary>Holds a reference to the current interface type of the game (Used by the renderer)</summary>
        internal static InterfaceType CurrentInterface = InterfaceType.Normal;
		/// <summary>Holds a reference to the previous interface type of the game</summary>
		internal static InterfaceType PreviousInterface = InterfaceType.Normal;
		/// <summary>The in-game menu system</summary>
		internal static Menu				Menu				= Menu.Instance;
		/// <summary>The in-game overlay with route info drawings</summary>
		internal static RouteInfoOverlay	routeInfoOverlay	= new RouteInfoOverlay();
	}
}
