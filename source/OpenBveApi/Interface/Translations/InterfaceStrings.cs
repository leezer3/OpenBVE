using System.Collections.Generic;
using OpenBveApi.Hosts;

namespace OpenBveApi.Interface
{
	public static partial class Translations
	{
		/// <summary>Stores the current language-code</summary>
		public static string CurrentLanguageCode = "en-GB";

		internal struct InterfaceString
		{
			/// <summary>The name of the string</summary>
			internal string Name;
			/// <summary>The translated string text</summary>
			internal string Text;
		}
		
		/// <summary>Sets the in-game language</summary>
		/// <param name="Language">The language string to set</param>
		public static void SetInGameLanguage(string Language)
		{
			//Set command infos to the translated strings
			for (int i = 0; i < AvailableLanguages.Count; i++)
			{
				//This is a hack, but the commandinfos are used in too many places to twiddle with easily
				if (AvailableLanguages[i].LanguageCode == Language)
				{
					CommandInfos = AvailableLanguages[i].myCommandInfos;
					QuickReferences = AvailableLanguages[i].myQuickReferences;
					TranslatedKeys = AvailableLanguages[i].KeyInfos;
					break;
				}
			}
		}

		/*
		 * This is used to very marginally speed up access in the array with some bitwise logic
		 * 
		 * I strongly suspect that any gains from this with our current string count
		 * (~500) will be un-noticable
		 *
		 * TODO: Consider removal for ease of maintanence
		 */
		private static int CurrentInterfaceStringIndex = 0;

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
		
		/// <summary>The quick-reference strings displayed in-game</summary>
		public struct InterfaceQuickReference
		{
			/// <summary>Reverser Forwards</summary>
			public string HandleForward;
			/// <summary>Reverser Neutral</summary>
			public string HandleNeutral;
			/// <summary>Reverser Reverse</summary>
			public string HandleBackward;
			/// <summary>Power P(n)</summary>
			public string HandlePower;
			/// <summary>Power Neutral</summary>
			public string HandlePowerNull;
			/// <summary>Brake B(n)</summary>
			public string HandleBrake;
			/// <summary>LocoBrake B(n)</summary>
			public string HandleLocoBrake;
			/// <summary>Brake / LocoBrake Neutral</summary>
			public string HandleBrakeNull;
			/// <summary>Air brake release</summary>
			public string HandleRelease;
			/// <summary>Air brake lap</summary>
			public string HandleLap;
			/// <summary>Air brake service</summary>
			public string HandleService;
			/// <summary>Brake emergency</summary>
			public string HandleEmergency;
			/// <summary>Hold brake applied</summary>
			public string HandleHoldBrake;
			/// <summary>Left Doors</summary>
			public string DoorsLeft;
			/// <summary>Right doors</summary>
			public string DoorsRight;
			/// <summary>Score (n)</summary>
			public string Score;
		}
		/// <summary>Holds the current set of interface quick reference strings</summary>
		public static InterfaceQuickReference QuickReferences;
		/// <summary>The number of score events to be displayed</summary>
		/// TODO: Appears to remain constant, investigate exact usages and whether we can dump
		public static int RatingsCount = 10;

	}
}
