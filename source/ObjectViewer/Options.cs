using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ObjectViewer.Graphics;
using OpenBveApi.Graphics;
using OpenBveApi.Input;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;

namespace ObjectViewer
{
    internal class Options
    {
        internal static void LoadOptions()
        {
	        Interface.CurrentOptions = new Interface.Options
	        {
		        ViewingDistance = 1000, // fixed
				CameraMoveLeft = Key.A,
				CameraMoveRight = Key.D,
				CameraMoveUp = Key.W,
				CameraMoveDown = Key.S,
				CameraMoveForward = Key.Q,
				CameraMoveBackward = Key.E
	        };
            string optionsFolder = Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
            if (!Directory.Exists(optionsFolder))
            {
                Directory.CreateDirectory(optionsFolder);
            }
            CultureInfo Culture = CultureInfo.InvariantCulture;
            string configFile = Path.CombineFile(optionsFolder, "options_ov.cfg");
            if (!File.Exists(configFile))
            {
                //Attempt to load and upgrade a prior configuration file
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                configFile = Path.CombineFile(Path.CombineDirectory(Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "options_ov.cfg");

                if (!File.Exists(configFile))
                {
                    //If no route viewer specific configuration file exists, then try the main OpenBVE configuration file
                    //Write out to a new routeviewer specific file though
                    configFile = Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg");
                }
            }

            if (File.Exists(configFile))
            {
                // load options
                string[] Lines = File.ReadAllLines(configFile, new System.Text.UTF8Encoding());
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
                                                if (!int.TryParse(Value, NumberStyles.Integer, Culture, out int a) || a < 300)
                                                {
                                                    a = 960;
                                                }
                                                Interface.CurrentOptions.WindowWidth = a;
                                            } break;
                                        case "windowheight":
                                            {
                                                if (!int.TryParse(Value, NumberStyles.Integer, Culture, out int a) || a < 300)
                                                {
                                                    a = 600;
                                                }
                                                Interface.CurrentOptions.WindowHeight = a;
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
                                                int.TryParse(Value, NumberStyles.Integer, Culture, out int a);
                                                Interface.CurrentOptions.AnisotropicFilteringLevel = a;
                                            } break;
                                        case "antialiasinglevel":
                                            {
                                                int.TryParse(Value, NumberStyles.Integer, Culture, out int a);
                                                Interface.CurrentOptions.AntiAliasingLevel = a;
                                            } break;
                                        case "transparencymode":
                                            switch (Value.ToLowerInvariant())
                                            {
                                                case "sharp": Interface.CurrentOptions.TransparencyMode = TransparencyMode.Performance; break;
                                                case "smooth": Interface.CurrentOptions.TransparencyMode = TransparencyMode.Quality; break;
                                                default:
                                                    {
                                                        if (int.TryParse(Value, NumberStyles.Integer, Culture, out int a))
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
										case "gdiplus":
											Interface.CurrentOptions.UseGDIDecoders = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
									} break;
								case "objectoptimization":
									switch (Key)
									{
										case "mode":
											{
												if (Enum.TryParse(Value, out ObjectOptimizationMode mode))
												{
													Interface.CurrentOptions.ObjectOptimizationMode = mode;
												}
											}
											break;
									}
									break;
								case "folders":
									switch (Key)
									{
										case "objectsearch":
											if (Directory.Exists(Value))
											{
												Interface.CurrentOptions.ObjectSearchDirectory = Value;
											}
											break;
									}
									break;
								case "keys":
									switch (Key)
									{
										case "left":
											if (!Enum.TryParse(Value, out Interface.CurrentOptions.CameraMoveLeft))
											{
												Interface.CurrentOptions.CameraMoveLeft = OpenBveApi.Input.Key.A;
											}
											break;
										case "right":
											if (!Enum.TryParse(Value, out Interface.CurrentOptions.CameraMoveRight))
											{
												Interface.CurrentOptions.CameraMoveRight = OpenBveApi.Input.Key.D;
											}
											break;
										case "up":
											if (!Enum.TryParse(Value, out Interface.CurrentOptions.CameraMoveUp))
											{
												Interface.CurrentOptions.CameraMoveUp = OpenBveApi.Input.Key.W;
											}
											break;
										case "down":
											if (!Enum.TryParse(Value, out Interface.CurrentOptions.CameraMoveDown))
											{
												Interface.CurrentOptions.CameraMoveDown = OpenBveApi.Input.Key.S;
											}
											break;
										case "forward":
											if (!Enum.TryParse(Value, out Interface.CurrentOptions.CameraMoveForward))
											{
												Interface.CurrentOptions.CameraMoveForward = OpenBveApi.Input.Key.Q;
											}
											break;
										case "backward":
											if (!Enum.TryParse(Value, out Interface.CurrentOptions.CameraMoveBackward))
											{
												Interface.CurrentOptions.CameraMoveBackward = OpenBveApi.Input.Key.E;
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
				Builder.AppendLine();
				Builder.AppendLine("[Folders]");
				Builder.AppendLine($"objectsearch = {Interface.CurrentOptions.ObjectSearchDirectory}");
				Builder.AppendLine("[Keys]");
				Builder.AppendLine("left = " + Interface.CurrentOptions.CameraMoveLeft);
				Builder.AppendLine("right = " + Interface.CurrentOptions.CameraMoveRight);
				Builder.AppendLine("up = " + Interface.CurrentOptions.CameraMoveUp);
				Builder.AppendLine("down = " + Interface.CurrentOptions.CameraMoveDown);
				Builder.AppendLine("forward = " + Interface.CurrentOptions.CameraMoveForward);
				Builder.AppendLine("backward = " + Interface.CurrentOptions.CameraMoveBackward);
				string configFile = Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_ov.cfg");
                File.WriteAllText(configFile, Builder.ToString(), new System.Text.UTF8Encoding(true));
            }
            catch
            {
                MessageBox.Show("An error occured whilst saving the options to disk." + Environment.NewLine +
                                "Please ensure you have write permission.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
