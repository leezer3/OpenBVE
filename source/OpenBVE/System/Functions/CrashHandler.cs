using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using OpenBveApi.Hosts;
// ReSharper disable LocalizableElement
// Note: Crashes may occur before languages have been loaded, so this file cannot be localised
namespace OpenBve
{
    /// <summary>Provides functions for handling crashes, and producing an appropriate error log</summary>
    internal class CrashHandler
    {
        private static readonly string CrashLog = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder,"OpenBVE Crash- " + DateTime.Now.ToString("yyyy.M.dd[HH.mm]") + ".log");
        /// <summary>Catches all unhandled exceptions within the current appdomain</summary>
        internal static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
	        if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size !=4)
	        {
				Console.WriteLine("UNHANDLED EXCEPTION:");
				Console.WriteLine("--------------------");
				Console.WriteLine(e.ExceptionObject);
				Environment.Exit(0);
	        }
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                if (ex is ArgumentOutOfRangeException && ex.Message == "Specified argument was out of the range of valid values.\r\nParameter name: button")
                {
                    //If a joystick with an excessive number of axis or buttons is connected, at the least show a nice error message, rather than simply dissapearing
                    Program.ShowMessageBox("An unsupported joystick is connected: \n \n Too many buttons. \n \n Please unplug all USB joysticks & gamepads and try again.", Application.ProductName);
                    Environment.Exit(0);
                }
                if (ex is ArgumentOutOfRangeException && ex.Message == "Specified argument was out of the range of valid values.\r\nParameter name: axis")
                {
                    //If a joystick with an excessive number of axis or buttons is connected, at the least show a nice error message, rather than simply dissapearing
                    MessageBox.Show("An unsupported joystick is connected: \n \n Too many axis. \n \n Please unplug all USB joysticks & gamepads and try again.");
                    Environment.Exit(0);
                }
                Program.ShowMessageBox("Unhandled exception:\n\n" + ex.Message, Application.ProductName);
                LogCrash(ex + Environment.StackTrace);

            }
            catch (Exception exc)
            {
                try
                {
                    Program.ShowMessageBox("A fatal exception occured inside the UnhandledExceptionHandler:" + Environment.NewLine + Environment.NewLine
                        + exc.Message, Application.ProductName);
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
	        if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size !=4)
	        {
		        Console.WriteLine("UNHANDLED EXCEPTION:");
		        Console.WriteLine("--------------------");
		        Console.WriteLine(t.Exception);
		        Environment.Exit(0);
	        }
            try
            {
                MessageBox.Show("An unhandled Windows Forms Exception occured. \r\n\r\n OpenBVE will now exit. Please consider reporting this with the \"Report Problem\" button on the bottom left of the main menu.", "OpenBVE Crashed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                LogCrash(t.Exception + Environment.NewLine + Environment.StackTrace);
            }
            catch (Exception exc)
            {
                try
                {
                    MessageBox.Show("Fatal Error: A fatal exception occured inside the UIThreadException handler");
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
        internal static void LogCrash(string exceptionText)
        {
			Program.FileSystem.AppendToLogFile("WARNING: Program crashing. Creating CrashLog file: " + CrashLog);
			using (StreamWriter outputFile = new StreamWriter(CrashLog))
            {
                //Basic information
                outputFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
	            if (Program.CurrentRoute.Information.RouteFile != null)
	            {
		            outputFile.WriteLine("Current routefile is: " + Program.CurrentRoute.Information.RouteFile);
	            }
	            if (Program.CurrentRoute.Information.TrainFolder != null)
	            {
		            outputFile.WriteLine("Current train is: " + Program.CurrentRoute.Information.TrainFolder);
	            }
	            if (TrainManager.PlayerTrain != null && TrainManager.PlayerTrain.Plugin != null)
	            {
		            outputFile.WriteLine("Current train plugin is: " + TrainManager.PlayerTrain.Plugin.PluginTitle);
	            }
	            //Errors and Warnings
                if (Program.CurrentRoute.Information.FilesNotFound != null)
                {
                    outputFile.WriteLine(Program.CurrentRoute.Information.FilesNotFound);
                }
                if (Program.CurrentRoute.Information.ErrorsAndWarnings != null)
                {
                    outputFile.WriteLine(Program.CurrentRoute.Information.ErrorsAndWarnings);
                }
                //Track position and viewing distance
                try
                {
	                outputFile.WriteLine("Current track position is: " + Program.Renderer.CameraTrackFollower.TrackPosition.ToString("0.00", CultureInfo.InvariantCulture) + " m");
	                outputFile.WriteLine("Current viewing distance is: " + Interface.CurrentOptions.ViewingDistance);
                }
                catch
                {
					//Most likely died before the render init
                }
                
                outputFile.WriteLine("The exception caught was as follows: ");
                outputFile.WriteLine(exceptionText);
                try
                {
	                double MemoryUsed;
	                using (Process proc = Process.GetCurrentProcess())
	                {
		                MemoryUsed = proc.PrivateMemorySize64;
	                }
	                outputFile.WriteLine("Current program memory usage: " + Math.Round((MemoryUsed / 1024 / 1024), 2) + "mb");
	                var freeRamCounter = new PerformanceCounter("Memory", "Available MBytes");
	                outputFile.WriteLine("System memory free: " + freeRamCounter.NextValue() + "mb");
                }
                catch
                {
	                outputFile.WriteLine("Unable to determine the current used / free memory figures.");
	                outputFile.WriteLine("This may indicate a wider system issue.");
                }
               
            }

        }

        /// <summary>This function logs an exception caught whilst loading a route/ train to disk</summary>
        internal static void LoadingCrash(string ExceptionText, bool Train)
        {
			Program.FileSystem.AppendToLogFile("WARNING: Program crashing. Creating CrashLog file: " + CrashLog);
			using (StreamWriter outputFile = new StreamWriter(CrashLog))
            {
                //Basic information
                outputFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
                    outputFile.WriteLine("Current routefile is: " + Program.CurrentRoute.Information.RouteFile);
                    outputFile.WriteLine("Current train is: " + Program.CurrentRoute.Information.TrainFolder);
                    outputFile.WriteLine("Current train plugin is: " + TrainManager.PlayerTrain.Plugin.PluginTitle);
                }
                catch
                {
					//Ignored
                }
                //Errors and Warnings
                if (Program.CurrentRoute.Information.FilesNotFound != null)
                {
                    outputFile.WriteLine(Program.CurrentRoute.Information.FilesNotFound);
                }
                if (Program.CurrentRoute.Information.ErrorsAndWarnings != null)
                {
                    outputFile.WriteLine(Program.CurrentRoute.Information.ErrorsAndWarnings);
                }

                outputFile.WriteLine(Train ? "The current train plugin caused the following exception: " : "The current routefile caused the following exception: ");
                outputFile.WriteLine(ExceptionText);
                double MemoryUsed;
                using (Process proc = Process.GetCurrentProcess())
                {
                    MemoryUsed = proc.PrivateMemorySize64;
                }
                outputFile.WriteLine("Current program memory usage: " + Math.Round((MemoryUsed / 1024 / 1024),2) + "mb");
                try
                {
	                var freeRamCounter = new PerformanceCounter("Memory", "Available MBytes");
	                outputFile.WriteLine("System memory free: " + freeRamCounter.NextValue() + "mb");
                }
                catch
                {
	                outputFile.WriteLine("Unable to determine the current used / free memory figures.");
	                outputFile.WriteLine("This may indicate a wider system issue.");
                }
            }
            // Attempt to gracefully shutdown the renderer to terminate hanging threads etc.
            Loading.Cancel = true;
		}
    }
}
