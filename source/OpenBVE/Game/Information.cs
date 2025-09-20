namespace OpenBve
{
    internal static partial class Game
    {
        /// <summary>The in-game menu system</summary>
		internal static readonly GameMenu Menu = GameMenu.Instance;
		/// <summary>The in-game overlay with route info drawings</summary>
		internal static readonly RouteInfoOverlay RouteInfoOverlay = new RouteInfoOverlay();

		internal static readonly SwitchChangeDialog SwitchChangeDialog = new SwitchChangeDialog();
    }
}
