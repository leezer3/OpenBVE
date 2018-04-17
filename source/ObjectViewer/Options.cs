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
            CultureInfo Culture = CultureInfo.InvariantCulture;
            string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "options_ov.cfg");
            if (!System.IO.File.Exists(configFile))
            {
                //If no route viewer specific configuration file exists, then try the main OpenBVE configuration file
                //Write out to a new routeviewer specific file though
                configFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "options.cfg");
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
            string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string optionsFolder = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(assemblyFolder, "UserData"),"Settings");
            string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_ov.cfg");
            try
            {
                //Delete original options file
                if (System.IO.File.Exists(configFile))
                {
                    System.IO.File.Delete(configFile);
                }
                if (!System.IO.Directory.Exists(optionsFolder))
                {
                    Directory.CreateDirectory(optionsFolder);
                }
                using(StreamWriter sw = new StreamWriter(configFile))
                {
                    sw.WriteLine("; Options");
                    sw.WriteLine("; =======");
                    sw.WriteLine("; This file was automatically generated. Please modify only if you know what you're doing.");
                    sw.WriteLine("; Route Viewer specific options file");
                    sw.WriteLine();
                    sw.WriteLine("[display]");
                    sw.WriteLine("windowWidth = " + Renderer.ScreenWidth);
                    sw.WriteLine("windowHeight = " + Renderer.ScreenHeight);
                    sw.WriteLine();
                    sw.WriteLine("[quality]");
                    sw.WriteLine("interpolation = " + Interface.CurrentOptions.Interpolation);
                    sw.WriteLine("anisotropicfilteringlevel = " + Interface.CurrentOptions.AnisotropicFilteringLevel);
                    sw.WriteLine("antialiasinglevel = " + Interface.CurrentOptions.AntialiasingLevel);
                    sw.WriteLine("transparencymode = " + Interface.CurrentOptions.TransparencyMode);
                }
            }
            catch
            {
                MessageBox.Show("An error occured whilst saving the options to disk." + System.Environment.NewLine +
                                "Please check you have write permission.");
            }
        }
    }
}
