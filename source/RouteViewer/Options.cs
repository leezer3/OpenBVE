using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace OpenBve
{
    internal class Options
    {
        internal static void LoadOptions()
        {
            string optionsFolder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
            if (!System.IO.Directory.Exists(optionsFolder))
            {
                System.IO.Directory.CreateDirectory(optionsFolder);
            }
            CultureInfo Culture = CultureInfo.InvariantCulture;
            string configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_rv.cfg");
            if (!System.IO.File.Exists(configFile))
            {
                //Attempt to load and upgrade a prior configuration file
                string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                configFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "options_rv.cfg");

                if (!System.IO.File.Exists(configFile))
                {
                    //If no route viewer specific configuration file exists, then try the main OpenBVE configuration file
                    //Write out to a new routeviewer specific file though
                    configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg");
                }
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
                                case "display":
                                switch (Key) {
										case "vsync":
											Interface.CurrentOptions.VerticalSynchronization = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "windowwidth":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
													a = 960;
												}
												Renderer.ScreenWidth = a;
											} break;
										case "windowheight":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
													a = 600;
												}
												Renderer.ScreenHeight = a;
											} break;
									} break;
								case "quality":
									switch (Key) {
										case "interpolation":
											switch (Value.ToLowerInvariant()) {
													case "nearestneighbor": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.NearestNeighbor; break;
                                                    case "bilinear": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.Bilinear; break;
                                                    case "nearestneighbormipmapped": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.NearestNeighborMipmapped; break;
                                                    case "bilinearmipmapped": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.BilinearMipmapped; break;
                                                    case "trilinearmipmapped": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.TrilinearMipmapped; break;
                                                    case "anisotropicfiltering": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.AnisotropicFiltering; break;
                                                    default: Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.BilinearMipmapped; break;
											} break;
										case "anisotropicfilteringlevel":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.AnisotropicFilteringLevel = a;
											} break;
											case "antialiasinglevel":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.AntialiasingLevel = a;
											} break;
										case "transparencymode":
											switch (Value.ToLowerInvariant()) {
													case "sharp": Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Sharp; break;
													case "smooth": Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Smooth; break;
													default: {
														int a;
														if (int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
															Interface.CurrentOptions.TransparencyMode = (Renderer.TransparencyMode)a;
														} else {
															Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Smooth;
														}
														break;
													}
											} break;
									} break;
								case "loading":
									switch(Key)
									{
										case "showlogo":
											if(Value.Trim().ToLowerInvariant() == "true")
											{
												Interface.CurrentOptions.LoadingLogo = true;
											}
											else
											{
												Interface.CurrentOptions.LoadingLogo = false;
											}
											break;
										case "showprogressbar":
											if (Value.Trim().ToLowerInvariant() == "true")
											{
												Interface.CurrentOptions.LoadingProgressBar = true;
											}
											else
											{
												Interface.CurrentOptions.LoadingProgressBar = false;
											}
											break;
										case "showbackground":
											if (Value.Trim().ToLowerInvariant() == "true")
											{
												Interface.CurrentOptions.LoadingBackground = true;
											}
											else
											{
												Interface.CurrentOptions.LoadingBackground = false;
											}
											break;

									}
									break;
							}
                        }
                    }
                }
            }
        }

        internal static void SaveOptions()
        {
            CultureInfo Culture = CultureInfo.InvariantCulture;
            System.Text.StringBuilder Builder = new System.Text.StringBuilder();
            Builder.AppendLine("; Options");
            Builder.AppendLine("; =======");
            Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
            Builder.AppendLine("; Route Viewer specific options file");
            Builder.AppendLine();
            Builder.AppendLine("[display]");
            Builder.AppendLine("vsync = " + Interface.CurrentOptions.VerticalSynchronization);
            Builder.AppendLine("windowWidth = " + Renderer.ScreenWidth);
            Builder.AppendLine("windowHeight = " + Renderer.ScreenHeight);
            Builder.AppendLine();
            Builder.AppendLine("[quality]");
            Builder.AppendLine("interpolation = " + Interface.CurrentOptions.Interpolation);
            Builder.AppendLine("anisotropicfilteringlevel = " + Interface.CurrentOptions.AnisotropicFilteringLevel);
            Builder.AppendLine("antialiasinglevel = " + Interface.CurrentOptions.AntialiasingLevel);
            Builder.AppendLine("transparencymode = " + Interface.CurrentOptions.TransparencyMode);
            Builder.AppendLine();
            Builder.AppendLine("[loading]");
            Builder.AppendLine("showlogo = " + Interface.CurrentOptions.LoadingLogo.ToString());
            Builder.AppendLine("showprogressbar = " + Interface.CurrentOptions.LoadingProgressBar.ToString());
            Builder.AppendLine("showbackground = " + Interface.CurrentOptions.LoadingBackground.ToString());
            string configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_rv.cfg");
            System.IO.File.WriteAllText(configFile, Builder.ToString(), new System.Text.UTF8Encoding(true));
        }
    }
}
