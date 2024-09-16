﻿using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using OpenBveApi.Graphics;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;

namespace RouteViewer
{
    internal class Options
    {
        internal static void LoadOptions()
        {
			Interface.CurrentOptions = new Interface.Options();
            string optionsFolder = Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
            if (!System.IO.Directory.Exists(optionsFolder))
            {
                System.IO.Directory.CreateDirectory(optionsFolder);
            }
            CultureInfo Culture = CultureInfo.InvariantCulture;
            string configFile = Path.CombineFile(optionsFolder, "options_rv.cfg");
            if (!System.IO.File.Exists(configFile))
            {
                //Attempt to load and upgrade a prior configuration file
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                configFile = Path.CombineFile(Path.CombineDirectory(Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "options_rv.cfg");

                if (!System.IO.File.Exists(configFile))
                {
                    //If no route viewer specific configuration file exists, then try the main OpenBVE configuration file
                    //Write out to a new routeviewer specific file though
                    configFile = Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg");
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
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out int a) || a < 300) {
													a = 960;
												}
												Interface.CurrentOptions.WindowWidth = a;
											} break;
										case "windowheight":
											{
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out int a) || a < 300) {
													a = 600;
												}
												Interface.CurrentOptions.WindowHeight = a;
											} break;
										case "isusenewrenderer":
											Interface.CurrentOptions.IsUseNewRenderer = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "viewingdistance":
											{
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out int a) || a < 300) {
													a = 600;
												}
												Interface.CurrentOptions.ViewingDistance = a;
											} break;
										case "quadleafsize":
											{
												if (int.TryParse(Value, NumberStyles.Integer, Culture, out int a))
												{
													if (a >= 50 && a <= 500)
													{
														Interface.CurrentOptions.QuadTreeLeafSize = a;
													}
												}
												Interface.CurrentOptions.QuadTreeLeafSize = a;
											} break;
					                } break;
								case "quality":
									switch (Key) {
										case "interpolation":
											switch (Value.ToLowerInvariant()) {
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
											switch (Value.ToLowerInvariant()) {
													case "sharp": Interface.CurrentOptions.TransparencyMode = TransparencyMode.Performance; break;
													case "smooth": Interface.CurrentOptions.TransparencyMode = TransparencyMode.Quality; break;
													default: {
														if (int.TryParse(Value, NumberStyles.Integer, Culture, out int a)) {
															Interface.CurrentOptions.TransparencyMode = (TransparencyMode)a;
														} else {
															Interface.CurrentOptions.TransparencyMode = TransparencyMode.Quality;
														}
														break;
													}
											} break;
									} break;
								case "loading":
									switch(Key)
									{
										case "showlogo":
											Interface.CurrentOptions.LoadingLogo = Value.Trim().ToLowerInvariant() == "true";
											break;
										case "showprogressbar":
											Interface.CurrentOptions.LoadingProgressBar = Value.Trim().ToLowerInvariant() == "true";
											break;
										case "showbackground":
											Interface.CurrentOptions.LoadingBackground = Value.Trim().ToLowerInvariant() == "true";
											break;

									}
									break;
								case "parsers":
									switch (Key)
									{
										case "xobject":
										{
											if (!int.TryParse(Value, NumberStyles.Integer, Culture, out int p) || p < 0 || p > 3)
											{
												Interface.CurrentOptions.CurrentXParser = XParsers.Original;
											}
											else
											{
												Interface.CurrentOptions.CurrentXParser = (XParsers)p;
											}
											break;
										}
										case "objobject":
										{
											if (!int.TryParse(Value, NumberStyles.Integer, Culture, out int p) || p < 0 || p > 2)
											{
												Interface.CurrentOptions.CurrentObjParser = ObjParsers.Original;
											}
											else
											{
												Interface.CurrentOptions.CurrentObjParser = (ObjParsers)p;
											}
											break;
										}
									}
									break;
								case "folders":
									switch (Key)
									{
										case "routesearch":
											if (Directory.Exists(Value))
											{
												Interface.CurrentOptions.RouteSearchDirectory = Value;
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
                Builder.AppendLine("; Route Viewer specific options file");
                Builder.AppendLine();
                Builder.AppendLine("[display]");
                Builder.AppendLine("vsync = " + (Interface.CurrentOptions.VerticalSynchronization ? "true" : "false"));
                Builder.AppendLine("windowWidth = " + Program.Renderer.Screen.Width.ToString(Culture));
                Builder.AppendLine("windowHeight = " + Program.Renderer.Screen.Height.ToString(Culture));
                Builder.AppendLine("isUseNewRenderer = " + (Interface.CurrentOptions.IsUseNewRenderer ? "true" : "false"));
                Builder.AppendLine("viewingdistance = " + Interface.CurrentOptions.ViewingDistance);
                Builder.AppendLine("quadleafsize = " + Interface.CurrentOptions.QuadTreeLeafSize);
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
                Builder.AppendLine("[loading]");
                Builder.AppendLine("showlogo = " + (Interface.CurrentOptions.LoadingLogo ? "true" : "false"));
                Builder.AppendLine("showprogressbar = " + (Interface.CurrentOptions.LoadingProgressBar ? "true" : "false"));
                Builder.AppendLine("showbackground = " + (Interface.CurrentOptions.LoadingBackground ? "true" : "false"));
                Builder.AppendLine("[parsers]");
                Builder.AppendLine("xObject = " + (int)Interface.CurrentOptions.CurrentXParser);
                Builder.AppendLine("objObject = " + (int)Interface.CurrentOptions.CurrentObjParser);
                Builder.AppendLine();
                Builder.AppendLine("[Folders]");
                Builder.AppendLine($"routesearch = {Interface.CurrentOptions.RouteSearchDirectory}");
				string configFile = Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_rv.cfg");
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
