using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace OpenBve
{
	public sealed partial class Menu
	{
		/// <summary>A caption to be rendered at the top of the menu</summary>
		private class MenuCaption : MenuEntry
		{
			internal MenuCaption(string[] translationPath)
			{
				this.TranslationPath = translationPath;
				languageCode = Interface.CurrentOptions.LanguageCode;
				this.Text = Translations.GetInterfaceString(HostApplication.OpenBve, translationPath);
			}
			internal MenuCaption(string Text)
			{
				this.Text = Text;
			}
		}
	}
}
