using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Textures;

namespace OpenBve
{
	public sealed partial class Menu
	{
		/// <summary>The base abstract Menu Entry class</summary>
		private abstract class MenuEntry
		{
			/// <summary>The translation path</summary>
			internal string[] TranslationPath;
			/// <summary>The base text of the menu entry</summary>
			internal string Text;
			/// <summary>The language code for the cached string</summary>
			internal string languageCode;
			/// <summary>The display text of the menu entry</summary>
			internal string DisplayText(double TimeElapsed)
			{
				if (TranslationPath != null && Interface.CurrentOptions.LanguageCode != languageCode)
				{
					// user has changed the in-game language, so we'll need to update our cached string
					Text = Translations.GetInterfaceString(HostApplication.OpenBve, TranslationPath);
					languageCode = Interface.CurrentOptions.LanguageCode;
				}
				if (DisplayLength == 0)
				{
					return Text;
				}
				timer += TimeElapsed;
				if (timer > 0.5)
				{
					if (pause)
					{
						pause = false;
						return _displayText;
					}
					timer = 0;
					scroll++;
					if (scroll == Text.Length)
					{
						scroll = 0;
						pause = true;
					}
					_displayText = Text.Substring(scroll);
					if (_displayText.Length > _displayLength)
					{
						_displayText = _displayText.Substring(0, _displayLength);
					}
				}
				return _displayText;
			}
			/// <summary>Backing property for display text</summary>
			private string _displayText;
			/// <summary>Backing property for display length</summary>
			private int _displayLength;
			/// <summary>The length to display</summary>
			internal int DisplayLength
			{
				get
				{
					return _displayLength;
				}
				set
				{
					_displayLength = value;
					_displayText = Text.Substring(0, value);
					timer = 0;
				}
			}
			/// <summary>The icon to draw for this menu entry</summary>
			internal Texture Icon;
			//Properties used for controlling the scrolling text if overlong
			private double timer;
			private int scroll;
			private bool pause;

		}
	}
}
