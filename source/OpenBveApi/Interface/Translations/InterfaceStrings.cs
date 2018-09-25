﻿using System;
using System.Collections.Generic;

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

		public static void SetInGameLanguage(string Language)
		{
			//Set command infos to the translated strings
			for (int i = 0; i < AvailableLangauges.Count; i++)
			{
				//This is a hack, but the commandinfos are used in too many places to twiddle with easily
				if (AvailableLangauges[i].LanguageCode == Language)
				{
					CommandInfos = AvailableLangauges[i].CommandInfos;
					QuickReferences = AvailableLangauges[i].QuickReferences;
					TranslatedKeys = AvailableLangauges[i].KeyInfos;
					break;
				}
			}
		}

		/// <summary>Fetches a translated user interface string</summary>
		/// <param name="Name">The name of the string to fetch</param>
		/// <returns>The translated string</returns>
		public static string GetInterfaceString(string Name)
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
		public struct InterfaceQuickReference
		{
			public string HandleForward;
			public string HandleNeutral;
			public string HandleBackward;
			public string HandlePower;
			public string HandlePowerNull;
			public string HandleBrake;
			public string HandleLocoBrake;
			public string HandleBrakeNull;
			public string HandleRelease;
			public string HandleLap;
			public string HandleService;
			public string HandleEmergency;
			public string HandleHoldBrake;
			public string DoorsLeft;
			public string DoorsRight;
			public string Score;
		}
		public static InterfaceQuickReference QuickReferences;
		public static int RatingsCount = 10;

	}
}
