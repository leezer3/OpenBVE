using OpenBveApi;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Formats.OpenBve;

namespace TrainEditor
{
	internal class Interface
	{
		internal class Options
		{
			/// <summary>The ISO 639-1 code for the current user interface language</summary>
			internal string LanguageCode;

			/// <summary>Creates a new instance of the options class with default values set</summary>
			internal Options()
			{
				this.LanguageCode = "en-US";
			}
		}
		/// <summary>The current game options</summary>
		internal static Options CurrentOptions;

		internal static void LoadOptions()
		{
			string optionsFolder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
			if (!Directory.Exists(optionsFolder))
			{
				Directory.CreateDirectory(optionsFolder);
			}
			CurrentOptions = new Options();
			string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_te.cfg");
			if (!File.Exists(configFile))
			{
				//If no route viewer specific configuration file exists, then try the main OpenBVE configuration file
				//Write out to a new routeviewer specific file though
				configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg");
			}

			if (File.Exists(configFile))
			{
				ConfigFile<OptionsSection, OptionsKey> cfg = new ConfigFile<OptionsSection, OptionsKey>(File.ReadAllLines(configFile, new UTF8Encoding()), Program.CurrentHost);

				while (cfg.RemainingSubBlocks > 0)
				{
					Block<OptionsSection, OptionsKey> block = cfg.ReadNextBlock();
					switch (block.Key)
					{
						case OptionsSection.Language:
							block.TryGetValue(OptionsKey.Code, ref CurrentOptions.LanguageCode);
							break;
					}
				}
			}
			else
			{
				// file not found
				string Code = CultureInfo.CurrentUICulture.Name;
				if (string.IsNullOrEmpty(Code)) Code = "en-US";
				string File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), Code + ".cfg");
				if (System.IO.File.Exists(File))
				{
					CurrentOptions.LanguageCode = Code;
				}
				else
				{
					try
					{
						int i = Code.IndexOf("-", StringComparison.Ordinal);
						if (i > 0)
						{
							Code = Code.Substring(0, i);
							File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), Code + ".cfg");
							if (System.IO.File.Exists(File))
							{
								CurrentOptions.LanguageCode = Code;
							}
						}
					}
					catch
					{
						CurrentOptions.LanguageCode = "en-US";
					}
				}
			}
		}

		internal static void SaveOptions()
		{
			try
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendLine("; Options");
				builder.AppendLine("; =======");
				builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
				builder.AppendLine("; Train Editor specific options file");
				builder.AppendLine();
				builder.AppendLine("[language]");
				builder.AppendLine("code = " + CurrentOptions.LanguageCode);
				string configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_te.cfg");
				File.WriteAllText(configFile, builder.ToString(), new UTF8Encoding(true));
			}
			catch
			{
				MessageBox.Show("An error occured whilst saving the options to disk." + Environment.NewLine +
								"Please check you have write permission.");
			}
		}
	}
}
