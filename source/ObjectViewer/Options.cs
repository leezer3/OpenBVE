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
            string configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_ov.cfg");
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
                                    switch (Key)
                                    {
                                        case "windowwidth":
                                            {
                                                int a;
                                                if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a))
                                                {
                                                    a = 960;
                                                }
                                                Renderer.ScreenWidth = a;
                                            } break;
                                        case "windowheight":
                                            {
                                                int a;
                                                if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a))
                                                {
                                                    a = 600;
                                                }
                                                Renderer.ScreenHeight = a;
                                            } break;
                                    } break;
                                case "quality":
                                    switch (Key)
                                    {
                                        case "interpolation":
                                            switch (Value.ToLowerInvariant())
                                            {
                                                case "nearestneighbor": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.NearestNeighbor; break;
                                                case "bilinear": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.Bilinear; break;
                                                case "nearestneighbormipmapped": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.NearestNeighborMipmapped; break;
                                                case "bilinearmipmapped": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.BilinearMipmapped; break;
                                                case "trilinearmipmapped": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.TrilinearMipmapped; break;
                                                case "anisotropicfiltering": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.AnisotropicFiltering; break;
                                                default: Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.BilinearMipmapped; break;
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
                                            switch (Value.ToLowerInvariant())
                                            {
                                                case "sharp": Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Sharp; break;
                                                case "smooth": Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Smooth; break;
                                                default:
                                                    {
                                                        int a;
                                                        if (int.TryParse(Value, NumberStyles.Integer, Culture, out a))
                                                        {
                                                            Interface.CurrentOptions.TransparencyMode = (Renderer.TransparencyMode)a;
                                                        }
                                                        else
                                                        {
                                                            Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Smooth;
                                                        }
                                                        break;
                                                    }
                                            } break;
                                    } break;
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
            Builder.AppendLine("; Object Viewer specific options file");
            Builder.AppendLine();
            Builder.AppendLine("[display]");
            Builder.AppendLine("windowWidth = " + Renderer.ScreenWidth.ToString(Culture));
            Builder.AppendLine("windowHeight = " + Renderer.ScreenHeight.ToString(Culture));
            Builder.AppendLine();
            Builder.AppendLine("[quality]");
            {
                string t; switch (Interface.CurrentOptions.Interpolation)
                {
                    case Interface.InterpolationMode.NearestNeighbor: t = "nearestNeighbor"; break;
                    case Interface.InterpolationMode.Bilinear: t = "bilinear"; break;
                    case Interface.InterpolationMode.NearestNeighborMipmapped: t = "nearestNeighborMipmapped"; break;
                    case Interface.InterpolationMode.BilinearMipmapped: t = "bilinearMipmapped"; break;
                    case Interface.InterpolationMode.TrilinearMipmapped: t = "trilinearMipmapped"; break;
                    case Interface.InterpolationMode.AnisotropicFiltering: t = "anisotropicFiltering"; break;
                    default: t = "bilinearMipmapped"; break;
                }
                Builder.AppendLine("interpolation = " + t);
            }
            Builder.AppendLine("anisotropicfilteringlevel = " + Interface.CurrentOptions.AnisotropicFilteringLevel.ToString(Culture));
            Builder.AppendLine("antialiasinglevel = " + Interface.CurrentOptions.AntialiasingLevel.ToString(Culture));
            Builder.AppendLine("transparencyMode = " + ((int)Interface.CurrentOptions.TransparencyMode).ToString(Culture));
            string configFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_ov.cfg");
            System.IO.File.WriteAllText(configFile, Builder.ToString(), new System.Text.UTF8Encoding(true));
        }
    }
}
