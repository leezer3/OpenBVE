namespace OpenBve
{
	public sealed partial class Menu
	{
		/// <summary>A caption to be rendered at the top of the menu</summary>
		private class MenuCaption : MenuEntry
		{
			internal MenuCaption(string Text)
			{
				this.Text = Text;
			}
		}
	}
}
