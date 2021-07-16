using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace OpenBveApi.Interface {
	public static partial class Translations {

		/// <summary>Loads all available language files from the specificed folder</summary>
        public static void LoadLanguageFiles(string LanguageFolder) {
			if (!Directory.Exists(LanguageFolder))
			{
				MessageBox.Show(@"The default language files have been moved or deleted.");
				LoadEmbeddedLanguage();
				return;
			}
            try {
				string[] LanguageFiles = Directory.GetFiles(LanguageFolder, "*.xlf");
	            if (LanguageFiles.Length == 0)
	            {
		            MessageBox.Show(@"No valid language files were found.");
					LoadEmbeddedLanguage();
		            return;
	            }
                foreach (var File in LanguageFiles) {
	                try
	                {
		                using (FileStream stream = new FileStream(File, FileMode.Open, FileAccess.Read))
		                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
		                {
			                Language l = new Language(reader, System.IO.Path.GetFileNameWithoutExtension(File));
			                AvailableLanguages.Add(l);
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
			using (TextReader reader = new StringReader(Resource.en_US))
			{
				Language l = new Language(reader, "en-US");
				AvailableLanguages.Add(l);
			}
			CurrentLanguageCode = "en-US";
		}

		/// <summary>Populates a list of languages in a combobox</summary>
		/// <param name="comboboxLanguages">The combobox to populate</param>
        public static void ListLanguages(ComboBox comboboxLanguages) {
            comboboxLanguages.Items.Clear();
            //Load all available languages
	        int idx = -1;
			for (int i = 0; i < AvailableLanguages.Count; i++)
			{
				comboboxLanguages.Items.Add(AvailableLanguages[i]);
				if (AvailableLanguages[i].LanguageCode == CurrentLanguageCode)
				{
					idx = i;
				}
			}

			if (idx != -1)
			{
				comboboxLanguages.SelectedIndex = idx;
			}
		}

		/// <summary>Attempts to set the flag image for the selected language code</summary>
		/// <param name="FlagFolder">The folder containing flag images</param>
		/// <param name="CurrentLanguageCodeArgument">The language code we wish to get the flag for</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <param name="LanguageImage">The path to the returned language image</param>
		/// <returns>True if we have found and successfully loaded the flag image</returns>
		public static bool SelectedLanguage(string FlagFolder, ref string CurrentLanguageCodeArgument, ComboBox comboboxLanguages, out string LanguageImage)
		{
			LanguageImage = null;
			int i = comboboxLanguages.SelectedIndex;
			if (i != -1)
			{
				Language l = comboboxLanguages.Items[i] as Language;
				if (l == null)
				{
					return false;
				}
				CurrentLanguageCode = l.LanguageCode;
				CurrentLanguageCodeArgument = l.LanguageCode;
				string File = Path.CombineFile(FlagFolder, l.Flag);
				if (!System.IO.File.Exists(File)) {
					File = Path.CombineFile(FlagFolder, "unknown.png");
				}
				if (System.IO.File.Exists(File))
				{
					
					using (var fs = new System.IO.FileStream(File, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
						LanguageImage = File;
					}
				}
				return true;
			}
            return false;
        }

		/// <summary>Selects a language</summary>
		/// <param name="CurrentLanguageCodeArgument">The language code to select</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <returns>True if the language was found and selected successfully</returns>
        public static bool SelectedLanguage(ref string CurrentLanguageCodeArgument, ComboBox comboboxLanguages) {
            int i = comboboxLanguages.SelectedIndex;
			if (i != -1)
			{
				Language l = comboboxLanguages.Items[i] as Language;
				if (l == null)
				{
					return false;
				}
				CurrentLanguageCode = l.LanguageCode;
				CurrentLanguageCodeArgument = l.LanguageCode;
				return true;
			}
            return false;
        }

		private static readonly List<Language> AvailableLanguages = new List<Language>();

	    
    }
}
