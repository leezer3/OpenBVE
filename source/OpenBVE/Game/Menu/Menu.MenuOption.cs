namespace OpenBve
{
	public sealed partial class Menu
	{
		private class MenuOption : MenuEntry
		{
			/// <summary>Holds the textual strings for all options</summary>
			internal readonly string[] Entries;
			/// <summary>Gets the text for the currently displayed option</summary>
			internal string OptionText => Entries[CurrentOption];
			/// <summary>Gets the index of the currently displayed option</summary>
			internal int CurrentOption
			{
				get
				{
					return _CurrentOption;
				}
				private set
				{
					_CurrentOption = value;
				}
			}
			/// <summary>Backing property storing the current option</summary>
			internal int _CurrentOption;

			internal MenuOption(string[] entries)
			{
				Entries = entries;
				CurrentOption = 0;
			}

			/// <summary>Flips to the next option</summary>
			internal void Flip()
			{
				if (_CurrentOption < Entries.Length - 1)
				{
					_CurrentOption++;
				}
				else
				{
					_CurrentOption = 0;
				}
			}
		}

	}
}
