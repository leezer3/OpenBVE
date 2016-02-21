using System;

namespace OpenBve
{
    internal static partial class Interface
    {
        private struct InterfaceString
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

        /// <summary>Fetches a translated user interface string</summary>
        /// <param name="Name">The name of the string to fetch</param>
        /// <returns>The translated string</returns>
        internal static string GetInterfaceString(string Name)
        {
            int n = Name.Length;
            for (int k = 0; k < InterfaceStringCount; k++)
            {
                int i;
                if ((k & 1) == 0)
                {
                    i = (CurrentInterfaceStringIndex + (k >> 1) + InterfaceStringCount) % InterfaceStringCount;
                }
                else
                {
                    i = (CurrentInterfaceStringIndex - (k + 1 >> 1) + InterfaceStringCount) % InterfaceStringCount;
                }
                if (InterfaceStrings[i].Name.Length == n)
                {
                    if (InterfaceStrings[i].Name == Name)
                    {
                        CurrentInterfaceStringIndex = (i + 1) % InterfaceStringCount;
                        return InterfaceStrings[i].Text;
                    }
                }
            }
            //Default return type-
            //If the string does not exist in the current language, return the search string
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

        internal static string CurrentControl;
        internal static string CurrentControlDescription;
    }
}
