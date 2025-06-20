using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenTK;
using Reactive.Bindings;
using SoundManager;
using System;
using System.Reactive.Concurrency;
using System.Windows.Forms;
using TrainEditor2.Audio;
using TrainEditor2.Graphics;
using TrainEditor2.Systems;
using TrainEditor2.Views;

namespace TrainEditor2
{
	internal static class Program
	{
		/// <summary>The host API used by this program.</summary>
		internal static Host CurrentHost;

		/// <summary>Information about the file system organization.</summary>
		internal static FileSystem FileSystem;

		internal static NewRenderer Renderer;

		internal static SoundApi SoundApi;

		internal static Simulation.TrainManager.TrainManager TrainManager;

		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		private static void Main()
		{
			ReactivePropertyScheduler.SetDefault(ImmediateScheduler.Instance);

			CurrentHost = new Host();

			try
			{
				FileSystem = FileSystem.FromCommandLineArgs(new string[0], CurrentHost);
				FileSystem.CreateFileSystem();
			}
			catch (Exception ex)
			{
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"errors","filesystem_invalid"}) + Environment.NewLine + Environment.NewLine + ex.Message, Translations.GetInterfaceString(HostApplication.TrainEditor2, new[] {"program","title"}), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}

			//Switch between SDL2 and native backends; use native backend by default
			var options = new ToolkitOptions();

			if (CurrentHost.Platform == HostPlatform.FreeBSD)
			{
				// The OpenTK X11 backend is broken on FreeBSD, so force SDL2
				options.Backend = PlatformBackend.Default;
			}
			Toolkit.Init(options);

			Interface.LoadOptions();

			Renderer = new NewRenderer(CurrentHost, Interface.CurrentOptions, FileSystem);

			SoundApi = new SoundApi(CurrentHost);
			SoundApi.Initialize(SoundRange.Medium);

			if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out string error, null, Renderer))
			{
				SoundApi.DeInitialize();
				MessageBox.Show(error, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			TrainManager = new Simulation.TrainManager.TrainManager(CurrentHost, null, null, FileSystem);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormEditor());

			CurrentHost.UnloadPlugins(out error);
			SoundApi.DeInitialize();

			Interface.CurrentOptions.Save(OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_te2.cfg"));
		}
	}
}
