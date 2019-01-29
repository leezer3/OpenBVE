using System;
using System.Diagnostics;
using System.IO;
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
                if (ex is ArgumentOutOfRangeException && ex.Message == "Specified argument was out of the range of valid values.\r\nParameter name: button")
                {
                    //If a joystick with an excessive number of axis or buttons is connected, at the least show a nice error message, rather than simply dissapearing
                    MessageBox.Show("An unsupported joystick is connected: \n \n Too many buttons. \n \n Please unplug all USB joysticks & gamepads and try again.");
                    Environment.Exit(0);
                }
                if (ex is ArgumentOutOfRangeException && ex.Message == "Specified argument was out of the range of valid values.\r\nParameter name: axis")
                {
                    //If a joystick with an excessive number of axis or buttons is connected, at the least show a nice error message, rather than simply dissapearing
                    MessageBox.Show("An unsupported joystick is connected: \n \n Too many axis. \n \n Please unplug all USB joysticks & gamepads and try again.");
                    Environment.Exit(0);
                }
                MessageBox.Show("Unhandled exception:\n\n" + ex.Message);
                LogCrash(ex + Environment.StackTrace);

            }
            catch (Exception exc)
            {
                try
                {
                    MessageBox.Show("A fatal exception occured inside the UnhandledExceptionHandler: \n\n"
                        + exc.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        LogCrash(exc + Environment.StackTrace);
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
                LogCrash(t + Environment.StackTrace);
            }
            catch (Exception exc)
            {
                try
                {
                    MessageBox.Show("A fatal exception occured inside the UIThreadException handler",
                        "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                        LogCrash(exc + Environment.StackTrace);
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
			Program.FileSystem.AppendToLogFile("WARNING: Program crashing. Creating CrashLog file: " + CrashLog);
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
                if (Interface.CurrentOptions.FullscreenMode)
                {
                    outputFile.WriteLine("Current screen resolution is: Full-screen " + Interface.CurrentOptions.FullscreenWidth + "px x " + Interface.CurrentOptions.FullscreenHeight + "px " + Interface.CurrentOptions.FullscreenBits + "bit color-mode");
                }
                else
                {
                    outputFile.WriteLine("Current screen resolution is: Windowed " + Interface.CurrentOptions.WindowWidth + "px x " + Interface.CurrentOptions.WindowHeight + "px ");
                }
                //Route and train
	            if (Game.RouteInformation.RouteFile != null)
	            {
		            outputFile.WriteLine("Current routefile is: " + Game.RouteInformation.RouteFile);
	            }
	            if (Game.RouteInformation.TrainFolder != null)
	            {
		            outputFile.WriteLine("Current train is: " + Game.RouteInformation.TrainFolder);
	            }
	            if (TrainManager.PlayerTrain != null && TrainManager.PlayerTrain.Plugin != null)
	            {
		            outputFile.WriteLine("Current train plugin is: " + TrainManager.PlayerTrain.Plugin.PluginTitle);
	            }
	            //Errors and Warnings
                if (Game.RouteInformation.FilesNotFound != null)
                {
                    outputFile.WriteLine(Game.RouteInformation.FilesNotFound);
                }
                if (Game.RouteInformation.ErrorsAndWarnings != null)
                {
                    outputFile.WriteLine(Game.RouteInformation.ErrorsAndWarnings);
                }
                //Track position and viewing distance
                outputFile.WriteLine("Current track position is: " + World.CameraTrackFollower.TrackPosition.ToString("0.00", Culture) + " m");
                outputFile.WriteLine("Current viewing distance is: " + Interface.CurrentOptions.ViewingDistance);
                outputFile.WriteLine("The exception caught was as follows: ");
                outputFile.WriteLine(ExceptionText);
                double MemoryUsed;
                using (Process proc = Process.GetCurrentProcess())
                {
                    MemoryUsed = proc.PrivateMemorySize64;
                }
                outputFile.WriteLine("Current program memory usage: " + Math.Round((MemoryUsed / 1024 / 1024), 2) + "mb");
                var freeRamCounter = new PerformanceCounter("Memory", "Available MBytes");
                outputFile.WriteLine("System memory free: " + freeRamCounter.NextValue() + "mb");
            }

        }

        /// <summary>This function logs an exception caught whilst loading a route/ train to disk</summary>
        internal static void LoadingCrash(string ExceptionText, bool Train)
        {
			Program.FileSystem.AppendToLogFile("WARNING: Program crashing. Creating CrashLog file: " + CrashLog);
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
                if (Interface.CurrentOptions.FullscreenMode)
                {
                    outputFile.WriteLine("Current screen resolution is: Full-screen " + Interface.CurrentOptions.FullscreenWidth + "px X " + Interface.CurrentOptions.FullscreenHeight + "px " + Interface.CurrentOptions.FullscreenBits + "bit color-mode");
                }
                else
                {
                    outputFile.WriteLine("Current screen resolution is: Windowed " + Interface.CurrentOptions.WindowWidth + "px X " + Interface.CurrentOptions.WindowHeight + "px ");
                }
                //Route and train information
                try
                {
                    //We need the try/ catch block in order to catch errors which may have occured before initing the current route, train or plugin
                    //These may occur if we feed dud data to the sim
                    outputFile.WriteLine("Current routefile is: " + Game.RouteInformation.RouteFile);
                    outputFile.WriteLine("Current train is: " + Game.RouteInformation.TrainFolder);
                    outputFile.WriteLine("Current train plugin is: " + TrainManager.PlayerTrain.Plugin.PluginTitle);
                }
                catch
                {
                }
                //Errors and Warnings
                if (Game.RouteInformation.FilesNotFound != null)
                {
                    outputFile.WriteLine(Game.RouteInformation.FilesNotFound);
                }
                if (Game.RouteInformation.ErrorsAndWarnings != null)
                {
                    outputFile.WriteLine(Game.RouteInformation.ErrorsAndWarnings);
                }
                if (Train)
                {
                    outputFile.WriteLine("The current train plugin caused the following exception: ");
                }
                else
                {
                    outputFile.WriteLine("The current routefile caused the following exception: ");
                }
                outputFile.WriteLine(ExceptionText);
                double MemoryUsed;
                using (Process proc = Process.GetCurrentProcess())
                {
                    MemoryUsed = proc.PrivateMemorySize64;
                }
                outputFile.WriteLine("Current program memory usage: " + Math.Round((MemoryUsed / 1024 / 1024),2) + "mb");
                var freeRamCounter = new PerformanceCounter("Memory", "Available MBytes");
                outputFile.WriteLine("System memory free: " + freeRamCounter.NextValue() + "mb");
            }

        }
    }
}
