using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using ObjectViewer.Graphics;
using OpenBveApi.Graphics;
using OpenBveApi.Objects;

namespace OpenBve
{
    internal class Options
    {
        internal static void LoadOptions()
        {
			Interface.CurrentOptions = new Interface.Options();
            string optionsFolder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
            if (!System.IO.Directory.Exists(optionsFolder))
            {
                System.IO.Directory.CreateDirectory(optionsFolder);
            }
            CultureInfo Culture = CultureInfo.InvariantCulture;
            string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_ov.cfg");
            if (!System.IO.File.Exists(configFile))
            {
                //Attempt to load and upgrade a prior configuration file
                string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                configFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "options_ov.cfg");

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
                    Lines[i] = Lines[i].Trim(new char[] { });
                    if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Lines[i].StartsWith("[", StringComparison.Ordinal) &
                            Lines[i].EndsWith("]", StringComparison.Ordinal))
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
                                case "display":
                                    switch (Key)
                                    {
                                        case "windowwidth":
                                            {
                                                int a;
                                                if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a) || a < 300)
                                                {
                                                    a = 960;
                                                }
                                                Program.Renderer.Screen.Width = a;
                                            } break;
                                        case "windowheight":
                                            {
                                                int a;
                                                if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a) || a < 300)
                                                {
                                                    a = 600;
                                                }
                                                Program.Renderer.Screen.Height = a;
                                            } break;
                                        case "isusenewrenderer":
	                                        Interface.CurrentOptions.IsUseNewRenderer = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
	                                        break;
                                    }
									break;
                                case "quality":
                                    switch (Key)
                                    {
                                        case "interpolation":
                                            switch (Value.ToLowerInvariant())
                                            {
                                                case "nearestneighbor": Interface.CurrentOptions.Interpolation = InterpolationMode.NearestNeighbor; break;
                                                case "bilinear": Interface.CurrentOptions.Interpolation = InterpolationMode.Bilinear; break;
                                                case "nearestneighbormipmapped": Interface.CurrentOptions.Interpolation = InterpolationMode.NearestNeighborMipmapped; break;
                                                case "bilinearmipmapped": Interface.CurrentOptions.Interpolation = InterpolationMode.BilinearMipmapped; break;
                                                case "trilinearmipmapped": Interface.CurrentOptions.Interpolation = InterpolationMode.TrilinearMipmapped; break;
                                                case "anisotropicfiltering": Interface.CurrentOptions.Interpolation = InterpolationMode.AnisotropicFiltering; break;
                                                default: Interface.CurrentOptions.Interpolation = InterpolationMode.BilinearMipmapped; break;
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
                                                Interface.CurrentOptions.AntiAliasingLevel = a;
                                            } break;
                                        case "transparencymode":
                                            switch (Value.ToLowerInvariant())
                                            {
                                                case "sharp": Interface.CurrentOptions.TransparencyMode = TransparencyMode.Performance; break;
                                                case "smooth": Interface.CurrentOptions.TransparencyMode = TransparencyMode.Quality; break;
                                                default:
                                                    {
                                                        int a;
                                                        if (int.TryParse(Value, NumberStyles.Integer, Culture, out a))
                                                        {
                                                            Interface.CurrentOptions.TransparencyMode = (TransparencyMode)a;
                                                        }
                                                        else
                                                        {
                                                            Interface.CurrentOptions.TransparencyMode = TransparencyMode.Quality;
                                                        }
                                                        break;
                                                    }
                                            } break;
                                    } break;
								case "parsers":
									switch (Key)
									{
										case "xobject":
                                            {
											    if (!Enum.TryParse(Value, out Interface.CurrentOptions.CurrentXParser))
											    {
												    Interface.CurrentOptions.CurrentXParser = XParsers.Original;
											    }
											    break;
                                            }
										case "objobject":
                                            {
	                                            if (!Enum.TryParse(Value, out Interface.CurrentOptions.CurrentObjParser))
											    {
												    Interface.CurrentOptions.CurrentObjParser = ObjParsers.Original;
											    }
	                                            break;
                                            }
									} break;
								case "objectoptimization":
									switch (Key)
									{
										case "mode":
											{
												ObjectOptimizationMode mode;
												if (Enum.TryParse(Value, out mode))
												{
													Interface.CurrentOptions.ObjectOptimizationMode = mode;
												}
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
            try
            {
                CultureInfo Culture = CultureInfo.InvariantCulture;
                System.Text.StringBuilder Builder = new System.Text.StringBuilder();
                Builder.AppendLine("; Options");
                Builder.AppendLine("; =======");
                Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
                Builder.AppendLine("; Object Viewer specific options file");
                Builder.AppendLine();
                Builder.AppendLine("[display]");
                Builder.AppendLine("windowWidth = " + Program.Renderer.Screen.Width.ToString(Culture));
                Builder.AppendLine("windowHeight = " + Program.Renderer.Screen.Height.ToString(Culture));
                Builder.AppendLine("isUseNewRenderer = " + (Interface.CurrentOptions.IsUseNewRenderer ? "true" : "false"));
                Builder.AppendLine();
                Builder.AppendLine("[quality]");
                {
                    string t; switch (Interface.CurrentOptions.Interpolation)
                    {
                        case InterpolationMode.NearestNeighbor: t = "nearestNeighbor"; break;
                        case InterpolationMode.Bilinear: t = "bilinear"; break;
                        case InterpolationMode.NearestNeighborMipmapped: t = "nearestNeighborMipmapped"; break;
                        case InterpolationMode.BilinearMipmapped: t = "bilinearMipmapped"; break;
                        case InterpolationMode.TrilinearMipmapped: t = "trilinearMipmapped"; break;
                        case InterpolationMode.AnisotropicFiltering: t = "anisotropicFiltering"; break;
                        default: t = "bilinearMipmapped"; break;
                    }
                    Builder.AppendLine("interpolation = " + t);
                }
                Builder.AppendLine("anisotropicfilteringlevel = " + Interface.CurrentOptions.AnisotropicFilteringLevel.ToString(Culture));
                Builder.AppendLine("antialiasinglevel = " + Interface.CurrentOptions.AntiAliasingLevel.ToString(Culture));
                Builder.AppendLine("transparencyMode = " + ((int)Interface.CurrentOptions.TransparencyMode).ToString(Culture));
                Builder.AppendLine();
                Builder.AppendLine("[Parsers]");
                Builder.AppendLine("xObject = " + Interface.CurrentOptions.CurrentXParser);
                Builder.AppendLine("objObject = " + Interface.CurrentOptions.CurrentObjParser);
                Builder.AppendLine();
				Builder.AppendLine("[objectOptimization]");
				Builder.AppendLine($"mode = {Interface.CurrentOptions.ObjectOptimizationMode}");
                string configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_ov.cfg");
                System.IO.File.WriteAllText(configFile, Builder.ToString(), new System.Text.UTF8Encoding(true));
            }
            catch
            {
                MessageBox.Show("An error occured whilst saving the options to disk." + Environment.NewLine +
                                "Please check you have write permission.");
            }
        }
    }
}
