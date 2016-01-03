using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace OpenBve
{
    /// <summary>Provides functions for handling crashes, and producing an appropriate error log</summary>
    class CrashHandler
    {
        static readonly System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
        static readonly string CrashLog = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder,"OpenBVE Crash- " + DateTime.Now.ToString("yyyy.M.dd[HH.mm]") + ".log");
        /// <summary>Catches all unhandled exceptions within the current appdomain</summary>
        internal static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                MessageBox.Show("Unhandled exception:\n\n" + ex.Message);
                LogCrash(ex.ToString());

            }
            catch (Exception exc)
            {
                try
                {
                    MessageBox.Show("A fatal exception occured inside the UnhandledExceptionHandler: \n\n"
                        + exc.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        LogCrash(exc.ToString());
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>Catches all unhandled exceptions within the current UI thread</summary>
        internal static void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            try
            {
                MessageBox.Show("Unhandled Windows Forms Exception");
                LogCrash(t.ToString());
            }
            catch (Exception exc)
            {
                try
                {
                    MessageBox.Show("A fatal exception occured inside the UIThreadException handler",
                        "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                        LogCrash(exc.ToString());
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
            Environment.Exit(0);
        }

        /// <summary>This function logs an unhandled crash to disk</summary>
        internal static void LogCrash(string ExceptionText)
        {
            using (StreamWriter outputFile = new StreamWriter(CrashLog))
            {
                //Basic information
                outputFile.WriteLine(DateTime.Now);
                outputFile.WriteLine("OpenBVE " + Application.ProductVersion + " Crash Log");
                var Platform = "Unknown";
                if (OpenTK.Configuration.RunningOnWindows)
                {
                    Platform = "Windows";
                }
                else if (OpenTK.Configuration.RunningOnLinux)
                {
                    Platform = "Linux";
                }
                else if (OpenTK.Configuration.RunningOnMacOS)
                {
                    Platform = "MacOS";
                }
                else if (OpenTK.Configuration.RunningOnSdl2)
                {
                    Platform = "SDL2";
                }
                outputFile.WriteLine("Program is running on the " + Platform + " backend");
                //Route and train
                outputFile.WriteLine("Current routefile is: "  + Game.RouteInformation.RouteFile);
                outputFile.WriteLine("Current train is: " + Game.RouteInformation.TrainFolder);
                outputFile.WriteLine("Current train plugin is: " + TrainManager.PlayerTrain.Plugin.PluginTitle);
                //Track position and viewing distance
                outputFile.WriteLine("Current track position is: " + World.CameraTrackFollower.TrackPosition.ToString("0.00", Culture) + " m");
                outputFile.WriteLine("Current viewing distance is: " + Interface.CurrentOptions.ViewingDistance);
                outputFile.WriteLine("The exception caught was as follows: ");
                outputFile.WriteLine(ExceptionText);
            }

        }

        /// <summary>This function logs an exception caught whilst loading a route/ train to disk</summary>
        internal static void LoadingCrash(string ExceptionText, bool Train)
        {
            using (StreamWriter outputFile = new StreamWriter(CrashLog))
            {
                //Basic information
                outputFile.WriteLine(DateTime.Now);
                outputFile.WriteLine("OpenBVE " + Application.ProductVersion + " Crash Log");
                var Platform = "Unknown";
                if (OpenTK.Configuration.RunningOnWindows)
                {
                    Platform = "Windows";
                }
                else if (OpenTK.Configuration.RunningOnLinux)
                {
                    Platform = "Linux";
                }
                else if (OpenTK.Configuration.RunningOnMacOS)
                {
                    Platform = "MacOS";
                }
                else if (OpenTK.Configuration.RunningOnSdl2)
                {
                    Platform = "SDL2";
                }
                outputFile.WriteLine("Program is running on the " + Platform + " backend");
                //Route and train
                outputFile.WriteLine("Current routefile is: " + Game.RouteInformation.RouteFile);
                outputFile.WriteLine("Current train is: " + Game.RouteInformation.TrainFolder);
                outputFile.WriteLine("Current train plugin is: " + TrainManager.PlayerTrain.Plugin.PluginTitle);
                //Track position and viewing distance
                if (Train)
                {
                    outputFile.WriteLine("The current train plugin caused the following exception: ");
                }
                else
                {
                    outputFile.WriteLine("The current routefile caused the following exception: ");
                }
                outputFile.WriteLine(ExceptionText);
            }

        }
    }
}
