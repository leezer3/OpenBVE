
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenBveApi.Interface {
	public static partial class Translations {

		/// <summary>Loads all available language files from the specified folder</summary>
        public static void LoadLanguageFiles(string languageFolder) {
			if (AvailableNewLanguages.Count > 2)
			{
				// Don't re-load languages if already present, e.g. restart
				return;
			}
			if (!Directory.Exists(languageFolder))
			{
				MessageBox.Show(@"The default language files have been moved or deleted.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				LoadEmbeddedLanguage();
				return;
			}
            try {
				string[] languageFiles = Directory.GetFiles(languageFolder, "*.xlf");
	            if (languageFiles.Length == 0)
	            {
		            MessageBox.Show(@"No valid language files were found.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					LoadEmbeddedLanguage();
		            return;
	            }
                //Load the fallback language first, as the language constructors may reference it for missing translations
                string[] fallbackLanguage = languageFiles.Where(f => string.Equals(System.IO.Path.GetFileNameWithoutExtension(f), "en-US", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                if (fallbackLanguage.Length > 0)
                {
	                try
	                {
		                using (FileStream stream = new FileStream(fallbackLanguage[0], FileMode.Open, FileAccess.Read))
		                {
			                AvailableNewLanguages.Add("en-US", new NewLanguage(stream, fallbackLanguage[0]));
		                }
	                }
	                catch
	                {
						//Corrupt language file? Just ignore
	                }
                }
                //Parse the remaining language files in parallel, then add them to the dictionary once all parsing is complete
                //(the language constructors may read the available languages dictionary as a fallback, so it must not be modified whilst they are running)
                List<KeyValuePair<string, NewLanguage>> loadedLanguages = new List<KeyValuePair<string, NewLanguage>>();
                Parallel.ForEach(languageFiles, language => {
	                try
	                {
		                using (FileStream stream = new FileStream(language, FileMode.Open, FileAccess.Read))
		                {
			                NewLanguage newLanguage = new NewLanguage(stream, language);
			                lock (loadedLanguages)
			                {
				                loadedLanguages.Add(new KeyValuePair<string, NewLanguage>(System.IO.Path.GetFileNameWithoutExtension(language), newLanguage));
			                }
		                }
	                }
	                catch
	                {
						//Corrupt language file? Just ignore
	                }
                });
                foreach (KeyValuePair<string, NewLanguage> language in loadedLanguages)
                {
	                if (!AvailableNewLanguages.ContainsKey(language.Key))
	                {
		                AvailableNewLanguages.Add(language.Key, language.Value);
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
		/// <param name="flagFolder">The folder containing flag images</param>
		/// <param name="currentLanguageCodeArgument">The language code we wish to get the flag for</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <param name="languageImage">The path to the returned language image</param>
		/// <returns>True if we have found and successfully loaded the flag image</returns>
		public static bool SelectedLanguage(string flagFolder, ref string currentLanguageCodeArgument, ComboBox comboboxLanguages, out string languageImage)
		{
			languageImage = Path.CombineFile(flagFolder, "unknown.png");
			if (comboboxLanguages.SelectedIndex == -1)
			{
				return false;
			}
			KeyValuePair<string, NewLanguage> kvp = (KeyValuePair<string, NewLanguage>)comboboxLanguages.SelectedItem;
			if (!blockComboBox)
			{
				CurrentLanguageCode = kvp.Value.Code;
				currentLanguageCodeArgument = kvp.Value.Code;
			}
			
			languageImage = Path.CombineFile(flagFolder, kvp.Value.Flag);
			if (!File.Exists(languageImage)) 
			{
				languageImage = Path.CombineFile(flagFolder, "unknown.png");
			}
			return true;
        }

		/// <summary>Selects a language</summary>
		/// <param name="currentLanguageCodeArgument">The language code to select</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <returns>True if the language was found and selected successfully</returns>
        public static bool SelectedLanguage(ref string currentLanguageCodeArgument, ComboBox comboboxLanguages) {
			if (comboboxLanguages.SelectedIndex == -1)
			{
				return false;
			}
			KeyValuePair<string, NewLanguage> kvp = (KeyValuePair<string, NewLanguage>)comboboxLanguages.SelectedItem;
			if (!blockComboBox)
			{
				CurrentLanguageCode = kvp.Value.Code;
				currentLanguageCodeArgument = kvp.Value.Code;
			}
			
			return true;
		}

		internal static readonly Dictionary<string, NewLanguage> AvailableNewLanguages = new Dictionary<string, NewLanguage>();


	}
}
