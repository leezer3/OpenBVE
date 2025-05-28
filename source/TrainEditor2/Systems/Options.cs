using Formats.OpenBve;
using OpenBveApi;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TrainEditor2.Systems
{
	internal static partial class Interface
	{
		internal class Options : BaseOptions
		{
			/// <summary>Creates a new instance of the options class with default values set</summary>
			internal Options()
			{
				LanguageCode = "en-US";
			}


			public override void Save(string fileName)
			{
				try
				{
					StringBuilder builder = new StringBuilder();
					builder.AppendLine("; Options");
					builder.AppendLine("; =======");
					builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
					builder.AppendLine("; TrainEditor2 specific options file");
					builder.AppendLine();
					builder.AppendLine("[language]");
					builder.AppendLine("code = " + LanguageCode);
					File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
				}
				catch
				{
					MessageBox.Show($@"An error occured whilst saving the options to disk.{Environment.NewLine}Please check you have write permission.");
				}
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
			string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_te2.cfg");

			if (!File.Exists(configFile))
			{
				//If no route viewer specific configuration file exists, then try the main OpenBVE configuration file
				//Write out to a new RouteViewer specific file though
				configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options.cfg");
			}

			if (File.Exists(configFile))
			{
				ConfigFile<OptionsSection, OptionsKey> cfg = new ConfigFile<OptionsSection, OptionsKey>(File.ReadAllLines(configFile, new UTF8Encoding()), Program.CurrentHost);
				while (cfg.RemainingSubBlocks > 0)
				{
					if (cfg.ReadBlock(OptionsSection.Language, out var block))
					{
						block.TryGetValue(OptionsKey.Code, ref CurrentOptions.LanguageCode);
					}

				}

				return;
			}

			// file not found
			string languageCode = CultureInfo.CurrentUICulture.Name;

			if (string.IsNullOrEmpty(languageCode))
			{
				languageCode = "en-US";
			}

			string fileName = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), languageCode + ".cfg");

			if (File.Exists(fileName))
			{
				CurrentOptions.LanguageCode = languageCode;
			}
			else
			{
				try
				{
					int i = languageCode.IndexOf("-", StringComparison.Ordinal);

					if (i > 0)
					{
						languageCode = languageCode.Substring(0, i);
						fileName = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), languageCode + ".cfg");

						if (File.Exists(fileName))
						{
							CurrentOptions.LanguageCode = languageCode;
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
}
