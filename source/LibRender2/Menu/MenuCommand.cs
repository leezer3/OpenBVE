using OpenBveApi.Textures;

namespace LibRender2.Menu
{
	/// <summary>A menu entry which performs a command when selected</summary>
	public class MenuCommand : MenuEntry
	{
		/// <summary>The command tag describing the function of the menu entry</summary>
		public readonly MenuTag Tag;
		/// <summary>The optional data to be passed with the command</summary>
		public readonly object Data;

		public MenuCommand(string Text, MenuTag Tag, object Data)
		{
			this.Text = Text;
			this.Tag = Tag;
			this.Data = Data;
			this.Icon = null;
		}

		public MenuCommand(string Text, MenuTag Tag, object Data, Texture Icon)
		{
			this.Text = Text;
			this.Tag = Tag;
			this.Data = Data;
			this.Icon = Icon;
		}
	}
}
