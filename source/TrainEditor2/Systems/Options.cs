using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OpenBveApi;

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
				// load options
				string[] Lines = File.ReadAllLines(configFile, new UTF8Encoding());
				string Section = "";

				for (int i = 0; i < Lines.Length; i++)
				{
					Lines[i] = Lines[i].Trim(new char[] { });

					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase))
					{
						if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))
						{
							Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim(new char[] { }).ToLowerInvariant();
						}
						else
						{
							int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
							string Key, Value;

							if (j >= 0)
							{
								Key = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
								Value = Lines[i].Substring(j + 1).TrimStart(new char[] { });
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
									}
									break;
							}
						}
					}
				}
			}
			else
			{
				// file not found
				string Code = CultureInfo.CurrentUICulture.Name;

				if (string.IsNullOrEmpty(Code))
				{
					Code = "en-US";
				}

				string FileName = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), Code + ".cfg");

				if (File.Exists(FileName))
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
							FileName = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), Code + ".cfg");

							if (File.Exists(FileName))
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
				StringBuilder Builder = new StringBuilder();
				Builder.AppendLine("; Options");
				Builder.AppendLine("; =======");
				Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
				Builder.AppendLine("; TrainEditor2 specific options file");
				Builder.AppendLine();
				Builder.AppendLine("[language]");
				Builder.AppendLine("code = " + CurrentOptions.LanguageCode);

				string configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_te2.cfg");
				File.WriteAllText(configFile, Builder.ToString(), new UTF8Encoding(true));
			}
			catch
			{
				MessageBox.Show($@"An error occured whilst saving the options to disk.{Environment.NewLine}Please check you have write permission.");
			}
		}
	}
}
