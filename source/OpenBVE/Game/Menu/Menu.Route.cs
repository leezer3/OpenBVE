using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using LibRender2.Menu;
using LibRender2.Primitives;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Packages;
using OpenBveApi.Textures;
using RouteManager2;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	public partial class GameMenu
	{
		private static BackgroundWorker routeWorkerThread;
		private static BackgroundWorker packageWorkerThread;
		private static string SearchDirectory;
		private static string PreviousSearchDirectory;
		private static string currentFile;
		private static Encoding RouteEncoding;
		private static RouteState RoutefileState;
		private static readonly Picturebox routePictureBox = new Picturebox(Program.Renderer);
		private static readonly Textbox routeDescriptionBox = new Textbox(Program.Renderer, Program.Renderer.Fonts.NormalFont, Color128.White, Color128.Black);
		private static readonly Dictionary<string, Texture> iconCache = new Dictionary<string, Texture>();
		private static Package currentPackage;
		private static PackageOperation currentOperation;
		private static bool packagePreview;
		private static string installedFiles;
		private static readonly Picturebox switchMainPictureBox = new Picturebox(Program.Renderer);
		private static readonly Picturebox switchSettingPictureBox = new Picturebox(Program.Renderer);
		private static readonly Picturebox switchMapPictureBox = new Picturebox(Program.Renderer);
		private static readonly Button nextImageButton = new Button(Program.Renderer, "->");
		private static readonly Button previousImageButton = new Button(Program.Renderer, "<-");

		private static Texture routeImageTexture;
		private static Texture routeMapTexture;

		private static void packageWorkerThread_doWork(object sender, DoWorkEventArgs e)
		{
			if (string.IsNullOrEmpty(currentFile))
			{
				return;
			}
			
			switch (currentOperation)
			{
				case PackageOperation.Installing:
					if (packagePreview)
					{
						try
						{
							currentPackage = Manipulation.ReadPackage(currentFile);
						}
						catch
						{
							// Ignored
						}
						
					}
					break;
			}
		}

		private static void nextImageButton_Click(object sender, EventArgs e)
		{
			routePictureBox.Texture = routePictureBox.Texture == routeImageTexture ? routeMapTexture : routeImageTexture;
		}

		private static void packageWorkerThread_completed(object sender, RunWorkerCompletedEventArgs e)
		{
			if (currentPackage != null)
			{
				routePictureBox.Texture = new Texture(new Bitmap(currentPackage.PackageImage));
				routeDescriptionBox.Text = currentPackage.Description;
				packagePreview = false;
			}
			Database.SaveDatabase();
		}

		private static void routeWorkerThread_doWork(object sender, DoWorkEventArgs e)
		{
			if (string.IsNullOrEmpty(currentFile))
			{
				return;
			}

			nextImageButton.IsVisible = false;
			previousImageButton.IsVisible = false;
			RouteEncoding = TextEncoding.GetSystemEncodingFromFile(currentFile);
			Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\loading.png"), TextureParameters.NoChange, out routePictureBox.Texture);
			routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_processing"});
			Game.Reset(false);
			bool loaded = false;
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(currentFile))
				{
					// ReSharper disable once RedundantCast
					object Route = (object)Program.CurrentRoute; // must cast to allow us to use the ref keyword correctly.
					string RailwayFolder = Loading.GetRailwayFolder(currentFile);
					string ObjectFolder = Path.CombineDirectory(RailwayFolder, "Object");
					string SoundFolder = Path.CombineDirectory(RailwayFolder, "Sound");
					if (Program.CurrentHost.Plugins[i].Route.LoadRoute(currentFile, RouteEncoding, null, ObjectFolder, SoundFolder, true, ref Route))
					{
						Program.CurrentRoute = (CurrentRoute) Route;
					}
					else
					{
						if (Program.CurrentHost.Plugins[i].Route.LastException != null)
						{
							throw Program.CurrentHost.Plugins[i].Route.LastException; //Re-throw last exception generated by the route parser plugin so that the UI thread captures it
						}
						routeDescriptionBox.Text = "An unknown error was enountered whilst attempting to parse the routefile " + currentFile;
						RoutefileState = RouteState.Error;
					}
					loaded = true;
					break;
				}
			}

			if (!loaded)
			{
				throw new Exception("No plugins capable of loading routefile " + currentFile + " were found.");
			}
		}

		private static void routeWorkerThread_completed(object sender, RunWorkerCompletedEventArgs e)
		{
			RoutefileState = RouteState.Processed;
			if (e.Error != null || Program.CurrentRoute == null)
			{
				Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\route_error.png"), TextureParameters.NoChange, out routePictureBox.Texture);
				if (e.Error != null)
				{
					routeDescriptionBox.Text = e.Error.Message;
					RoutefileState = RouteState.Error;
				}
				routeWorkerThread.Dispose();
				return;
			}
			try
			{
				// image
				if (!string.IsNullOrEmpty(Program.CurrentRoute.Image))
				{

					try
					{
						if (File.Exists(Program.CurrentRoute.Image))
						{
							Program.CurrentHost.RegisterTexture(Program.CurrentRoute.Image, TextureParameters.NoChange, out routeImageTexture);
						}
						else
						{
							Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\route_unknown.png"), TextureParameters.NoChange, out routeImageTexture);
						}

						
					}
					catch
					{
						routeImageTexture = null;
					}
				}
				else
				{
					string[] f = {".png", ".bmp", ".gif", ".tiff", ".tif", ".jpeg", ".jpg"};
					int i;
					for (i = 0; i < f.Length; i++)
					{
						string g = Path.CombineFile(Path.GetDirectoryName(currentFile),
							System.IO.Path.GetFileNameWithoutExtension(currentFile) + f[i]);
						if (!File.Exists(g)) continue;
						Program.CurrentHost.RegisterTexture(g, TextureParameters.NoChange, out routeImageTexture);
						break;
					}
					if (i == f.Length)
					{
						Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\route_unknown.png"), TextureParameters.NoChange, out routeImageTexture);
					}
				}

				routeMapTexture = new Texture(Illustrations.CreateRouteMap((int)routePictureBox.Size.X, (int)routePictureBox.Size.Y, false, out _));
				nextImageButton.IsVisible = true;
				previousImageButton.IsVisible = true;
				routePictureBox.Texture = routeImageTexture;

				// description
				string Description = Program.CurrentRoute.Comment.ConvertNewlinesToCrLf();
				routeDescriptionBox.Text = Description.Length != 0 ? Description : System.IO.Path.GetFileNameWithoutExtension(currentFile);
			}
			catch (Exception ex)
			{
				Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\route_error.png"), TextureParameters.NoChange, out routePictureBox.Texture);
				routeDescriptionBox.Text = ex.Message;
				currentFile = null;
			}
		}

		private static void OnWorkerProgressChanged(object sender, ProgressReport e)
		{
			routeDescriptionBox.Text = "Processing:" + Environment.NewLine + e.Progress + "%" + Environment.NewLine + Environment.NewLine + e.CurrentFile;
		}

		private static void OnWorkerReportsProblem(object sender, ProblemReport e)
		{
			routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_failure_error"}) + Environment.NewLine;
			if (e.Exception is UnauthorizedAccessException && currentOperation != PackageOperation.Creating)
			{
				//User attempted to install in a directory which requires UAC access
				routeDescriptionBox.Text += e.Exception.Message + Environment.NewLine + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","security_checkaccess"});
				if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows)
				{
					routeDescriptionBox.Text += Environment.NewLine + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","security_badlocation"});
				}
			}
			else
			{
				//Non-localised string as this is a specific error message
				routeDescriptionBox.Text += e.Exception + @"\r\n \r\n encountered whilst processing the following file: \r\n\r\n" +
				                             e.CurrentFile + @" at " + e.Progress + @"% completion.";
				//Create crash dump file
				CrashHandler.LogCrash(e.Exception + Environment.StackTrace);
			}
		}

		private static void OnPackageOperationCompleted(object sender, CompletionReport e)
		{
			switch (e.Operation)
			{
				case PackageOperation.Installing:
					routeDescriptionBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_success"}) + Environment.NewLine + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_success_files"}) + Environment.NewLine + installedFiles;
					currentFile = string.Empty;
					installedFiles = string.Empty;
					switch (currentPackage.PackageType)
					{
						case PackageType.Route:
							Database.currentDatabase.InstalledRoutes.Remove(currentPackage);
							Database.currentDatabase.InstalledRoutes.Add(currentPackage);
							break;
						case PackageType.Train:
							Database.currentDatabase.InstalledTrains.Remove(currentPackage);
							Database.currentDatabase.InstalledTrains.Add(currentPackage);
							break;
						default:
							Database.currentDatabase.InstalledOther.Remove(currentPackage);
							Database.currentDatabase.InstalledOther.Add(currentPackage);
							break;
					}
					currentPackage = null;
					Database.SaveDatabase();
					break;
			}
		}

		
	}
}
