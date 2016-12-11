using System;
using System.Collections.Generic;

namespace OpenBve
{
	internal static partial class Interface
	{
		/// <summary>Stores the current language-code</summary>
		internal static string CurrentLanguageCode = "en-GB";

		internal struct InterfaceString
		{
			/// <summary>The name of the string</summary>
			internal string Name;
			/// <summary>The translated string text</summary>
			internal string Text;
		}
		private static InterfaceString[] InterfaceStrings = new InterfaceString[16];
		private static int InterfaceStringCount = 0;
		private static int CurrentInterfaceStringIndex = 0;

		/// <summary>Adds a translated user interface string to the current list</summary>
		/// <param name="Name">The name of the string to add</param>
		/// <param name="Text">The translated text of the string to add</param>
		private static void AddInterfaceString(string Name, string Text)
		{
			if (InterfaceStringCount >= InterfaceStrings.Length)
			{
				Array.Resize<InterfaceString>(ref InterfaceStrings, InterfaceStrings.Length << 1);
			}
			InterfaceStrings[InterfaceStringCount].Name = Name;
			InterfaceStrings[InterfaceStringCount].Text = Text;
			InterfaceStringCount++;
		}

		internal static void SetInGameLanguage(string Language)
		{
			//Set command infos to the translated strings
			for (int i = 0; i < Interface.AvailableLangauges.Count; i++)
			{
				//This is a hack, but the commandinfos are used in too many places to twiddle with easily
				if (Interface.AvailableLangauges[i].LanguageCode == Language)
				{
					Interface.CommandInfos = Interface.AvailableLangauges[i].CommandInfos;
					Interface.QuickReferences = Interface.AvailableLangauges[i].QuickReferences;
					Interface.TranslatedKeys = Interface.AvailableLangauges[i].KeyInfos;
					break;
				}
			}
		}

		/// <summary>Fetches a translated user interface string</summary>
		/// <param name="Name">The name of the string to fetch</param>
		/// <returns>The translated string</returns>
		internal static string GetInterfaceString(string Name)
		{
			List<string> FallbackLanguages = new List<string>();
			//First, we need to find the default langauge file
			for (int i = 0; i < AvailableLangauges.Count; i++)
			{
				if (AvailableLangauges[i].LanguageCode == CurrentLanguageCode)
				{
					//Set the fallback languages
					FallbackLanguages = AvailableLangauges[i].FallbackCodes;
					int n = Name.Length;
					for (int k = 0; k < AvailableLangauges[i].InterfaceStringCount; k++)
					{
						int t;
						if ((k & 1) == 0)
						{
							t = (CurrentInterfaceStringIndex + (k >> 1) + AvailableLangauges[i].InterfaceStringCount) % AvailableLangauges[i].InterfaceStringCount;
						}
						else
						{
							t = (CurrentInterfaceStringIndex - (k + 1 >> 1) + AvailableLangauges[i].InterfaceStringCount) % AvailableLangauges[i].InterfaceStringCount;
						}
						if (AvailableLangauges[i].InterfaceStrings[t].Name.Length == n)
						{
							if (AvailableLangauges[i].InterfaceStrings[t].Name == Name)
							{
								CurrentInterfaceStringIndex = (t + 1) % AvailableLangauges[i].InterfaceStringCount;
								return AvailableLangauges[i].InterfaceStrings[t].Text;
							}
						}
					}
				}
			}
			//OK, so that didn't work- Try the fallback languages
			if (FallbackLanguages == null)
			{
				return Name;
			}
			for (int m = 0; m < FallbackLanguages.Count; m++)
			{
				for (int i = 0; i < AvailableLangauges.Count; i++)
				{
					if (AvailableLangauges[i].LanguageCode == FallbackLanguages[m])
					{
						int j = Name.Length;
						for (int k = 0; k < AvailableLangauges[i].InterfaceStringCount; k++)
						{
							int l;
							if ((k & 1) == 0)
							{
								l = (CurrentInterfaceStringIndex + (k >> 1) + AvailableLangauges[i].InterfaceStringCount) % AvailableLangauges[i].InterfaceStringCount;
							}
							else
							{
								l = (CurrentInterfaceStringIndex - (k + 1 >> 1) + AvailableLangauges[i].InterfaceStringCount) % AvailableLangauges[i].InterfaceStringCount;
							}
							if (AvailableLangauges[i].InterfaceStrings[l].Name.Length == j)
							{
								if (AvailableLangauges[i].InterfaceStrings[l].Name == Name)
								{
									CurrentInterfaceStringIndex = (l + 1) % AvailableLangauges[i].InterfaceStringCount;
									//We found the string in a fallback language, so let's return it!
									return AvailableLangauges[i].InterfaceStrings[l].Text;
								}
							}
						}
					}
				}
			}

			//Default return type-
			//If the string does not exist in the current language or any of the fallback options, return the search string
			return Name;
		}
		/// <summary>The quick-reference strings displayed in-game</summary>
		internal struct InterfaceQuickReference
		{
			internal string HandleForward;
			internal string HandleNeutral;
			internal string HandleBackward;
			internal string HandlePower;
			internal string HandlePowerNull;
			internal string HandleBrake;
			internal string HandleBrakeNull;
			internal string HandleRelease;
			internal string HandleLap;
			internal string HandleService;
			internal string HandleEmergency;
			internal string HandleHoldBrake;
			internal string DoorsLeft;
			internal string DoorsRight;
			internal string Score;
		}
		internal static InterfaceQuickReference QuickReferences;
		internal static int RatingsCount = 10;

	}
}
