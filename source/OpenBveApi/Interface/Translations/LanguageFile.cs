
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpenBveApi.Interface {
	public static partial class Translations {

		/// <summary>Loads all available language files from the specificed folder</summary>
        public static void LoadLanguageFiles(string LanguageFolder) {
			if (AvailableNewLanguages.Count > 2)
			{
				// Don't re-load languages if already present, e.g. restart
				return;
			}
			if (!Directory.Exists(LanguageFolder))
			{
				MessageBox.Show(@"The default language files have been moved or deleted.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				LoadEmbeddedLanguage();
				return;
			}
            try {
				string[] LanguageFiles = Directory.GetFiles(LanguageFolder, "*.xlf");
	            if (LanguageFiles.Length == 0)
	            {
		            MessageBox.Show(@"No valid language files were found.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					LoadEmbeddedLanguage();
		            return;
	            }
                foreach (var File in LanguageFiles) {
	                try
	                {
		                using (FileStream stream = new FileStream(File, FileMode.Open, FileAccess.Read))
		                {
			                AvailableNewLanguages.Add(System.IO.Path.GetFileNameWithoutExtension(File), new NewLanguage(stream, File));
		                }
	                }
	                catch
	                {
						//Corrupt language file? Just ignore
	                }
                }
            } catch {
                MessageBox.Show(@"An error occured whilst attempting to load the default language files.");
	            LoadEmbeddedLanguage();
            }
        }

		/// <summary>Loads the embedded default language</summary>
		private static void LoadEmbeddedLanguage()
		{
			NewLanguage l = new NewLanguage(Resource.en_US);
			AvailableNewLanguages.Add("en-US", l);
			CurrentLanguageCode = "en-US";
		}

		/// <summary>Populates a list of languages in a combobox</summary>
		/// <param name="comboboxLanguages">The combobox to populate</param>
        public static void ListLanguages(ComboBox comboboxLanguages)
		{
			blockComboBox = true;
            comboboxLanguages.Items.Clear();

            comboboxLanguages.DataSource = new BindingSource(AvailableNewLanguages, null);
            comboboxLanguages.DisplayMember = "Value";
            comboboxLanguages.ValueMember = "Key";
            //Load all available languages
	        int idx = -1;
			for (int i = 0; i < AvailableNewLanguages.Count; i++)
			{
				string key = AvailableNewLanguages.ElementAt(i).Key;
				if (key == CurrentLanguageCode)
				{
					idx = i;
					break;
				}
			}

			if (idx != -1)
			{
				comboboxLanguages.SelectedIndex = idx;
			}

			blockComboBox = false;
		}

		private static bool blockComboBox = false;

		/// <summary>Attempts to set the flag image for the selected language code</summary>
		/// <param name="FlagFolder">The folder containing flag images</param>
		/// <param name="CurrentLanguageCodeArgument">The language code we wish to get the flag for</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <param name="LanguageImage">The path to the returned language image</param>
		/// <returns>True if we have found and successfully loaded the flag image</returns>
		public static bool SelectedLanguage(string FlagFolder, ref string CurrentLanguageCodeArgument, ComboBox comboboxLanguages, out string LanguageImage)
		{
			LanguageImage = Path.CombineFile(FlagFolder, "unknown.png");
			if (comboboxLanguages.SelectedIndex == -1)
			{
				return false;
			}
			KeyValuePair<string, NewLanguage> kvp = (KeyValuePair<string, NewLanguage>)comboboxLanguages.SelectedItem;
			if (!blockComboBox)
			{
				CurrentLanguageCode = kvp.Value.Code;
				CurrentLanguageCodeArgument = kvp.Value.Code;
			}
			
			LanguageImage = Path.CombineFile(FlagFolder, kvp.Value.Flag);
			if (!File.Exists(LanguageImage)) 
			{
				LanguageImage = Path.CombineFile(FlagFolder, "unknown.png");
			}
			return true;
        }

		/// <summary>Selects a language</summary>
		/// <param name="CurrentLanguageCodeArgument">The language code to select</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <returns>True if the language was found and selected successfully</returns>
        public static bool SelectedLanguage(ref string CurrentLanguageCodeArgument, ComboBox comboboxLanguages) {
			if (comboboxLanguages.SelectedIndex == -1)
			{
				return false;
			}
			KeyValuePair<string, NewLanguage> kvp = (KeyValuePair<string, NewLanguage>)comboboxLanguages.SelectedItem;
			if (!blockComboBox)
			{
				CurrentLanguageCode = kvp.Value.Code;
				CurrentLanguageCodeArgument = kvp.Value.Code;
			}
			
			return true;
		}

		internal static readonly Dictionary<string, NewLanguage> AvailableNewLanguages = new Dictionary<string, NewLanguage>();


	}
}
