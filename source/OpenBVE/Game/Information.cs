namespace OpenBve
{
    internal static partial class Game
    {
        /// <summary>The current plugin debug message to be displayed</summary>
        internal static string InfoDebugString = "";
        /// <summary>The in-game menu system</summary>
		internal static readonly Menu Menu = Menu.Instance;
		/// <summary>The in-game overlay with route info drawings</summary>
		internal static readonly RouteInfoOverlay routeInfoOverlay = new RouteInfoOverlay();

		internal static readonly SwitchChangeDialog switchChangeDialog = new SwitchChangeDialog();
    }
}
