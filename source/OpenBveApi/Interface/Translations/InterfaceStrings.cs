using OpenBveApi.Hosts;

namespace OpenBveApi.Interface
{
	public static partial class Translations
	{
		/// <summary>Stores the current language-code</summary>
		public static string CurrentLanguageCode = "en-GB";
		
		/// <summary>Sets the in-game language</summary>
		/// <param name="Language">The language string to set</param>
		public static void SetInGameLanguage(string Language)
		{
			if (AvailableNewLanguages.ContainsKey(Language))
			{
				TranslatedKeys = AvailableNewLanguages[Language].KeyInfos;
				QuickReferences = AvailableNewLanguages[Language].QuickReferences;
			}
		}

		/// <summary>Fetches a translated user interface string</summary>
		/// <param name="Application">The application string database to search</param>
		/// <param name="parameters">A string array containing the group tree to retrieve the string</param>
		/// <returns>The translated string</returns>
		public static string GetInterfaceString(HostApplication Application, string[] parameters)
		{
			// note: languages may be zero at startup before things have spun up- winforms....
			if (AvailableNewLanguages.Count != 0)
			{
				return AvailableNewLanguages[CurrentLanguageCode].GetInterfaceString(Application, parameters);
			}

			return string.Empty;
		}
		
		/// <summary>Holds the current set of interface quick reference strings</summary>
		public static InterfaceQuickReference QuickReferences;
		/// <summary>The number of score events to be displayed</summary>
		/// TODO: Appears to remain constant, investigate exact usages and whether we can dump
		public static int RatingsCount = 10;

	}
}
