using OpenBveApi.Textures;

namespace OpenBve
{
	public sealed partial class Menu
	{
		/// <summary>A menu entry which performs a command when selected</summary>
		private class MenuCommand : MenuEntry
		{
			/// <summary>The command tag describing the function of the menu entry</summary>
			internal readonly MenuTag Tag;
			/// <summary>The optional data to be passed with the command</summary>
			internal readonly object Data;

			internal MenuCommand(string Text, MenuTag Tag, object Data)
			{
				this.Text = Text;
				this.Tag = Tag;
				this.Data = Data;
				this.Icon = null;
			}

			internal MenuCommand(string Text, MenuTag Tag, object Data, Texture Icon)
			{
				this.Text = Text;
				this.Tag = Tag;
				this.Data = Data;
				this.Icon = Icon;
			}
		}
	}
}
