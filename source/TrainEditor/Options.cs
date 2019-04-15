using System;
using System.Globalization;
using System.Windows.Forms;

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
			if (!System.IO.Directory.Exists(optionsFolder))
			{
				System.IO.Directory.CreateDirectory(optionsFolder);
			}
			CurrentOptions = new Options();
			CultureInfo Culture = CultureInfo.InvariantCulture;
			string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_te.cfg");
			if (!System.IO.File.Exists(configFile))
			{
				//If no route viewer specific configuration file exists, then try the main OpenBVE configuration file
				//Write out to a new routeviewer specific file though
				configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg");
			}

			if (System.IO.File.Exists(configFile))
			{
				// load options
				string[] Lines = System.IO.File.ReadAllLines(configFile, new System.Text.UTF8Encoding());
				string Section = "";
				for (int i = 0; i < Lines.Length; i++)
				{
					Lines[i] = Lines[i].Trim();
					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase))
					{
						if (Lines[i].StartsWith("[", StringComparison.Ordinal) &
							Lines[i].EndsWith("]", StringComparison.Ordinal))
						{
							Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();
						}
						else
						{
							int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
							string Key, Value;
							if (j >= 0)
							{
								Key = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
								Value = Lines[i].Substring(j + 1).TrimStart();
							}
							else
							{
								Key = "";
								Value = Lines[i];
							}
							switch (Section)
							{
								case "language":
									switch (Key)
									{
										case "code":
											CurrentOptions.LanguageCode = Value.Length != 0 ? Value : "en-US";
											break;
									} break;
							}
						}
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
				CultureInfo Culture = CultureInfo.InvariantCulture;
				System.Text.StringBuilder Builder = new System.Text.StringBuilder();
				Builder.AppendLine("; Options");
				Builder.AppendLine("; =======");
				Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
				Builder.AppendLine("; Train Editor specific options file");
				Builder.AppendLine();
				Builder.AppendLine("[language]");
				Builder.AppendLine("code = " + CurrentOptions.LanguageCode);
				string configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_te.cfg");
				System.IO.File.WriteAllText(configFile, Builder.ToString(), new System.Text.UTF8Encoding(true));
			}
			catch
			{
				MessageBox.Show("An error occured whilst saving the options to disk." + System.Environment.NewLine +
								"Please check you have write permission.");
			}
		}
	}
}
