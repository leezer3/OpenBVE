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


			//Switch between SDL2 and native backends; use native backend by default
			var options = new ToolkitOptions();

			if (CurrentHost.Platform == HostPlatform.FreeBSD)
			{
				// The OpenTK X11 backend is broken on FreeBSD, so force SDL2
				options.Backend = PlatformBackend.Default;
			}
			else if (CurrentHost.Platform == HostPlatform.GNULinux)
			{
				// Otherwise, prefer native where possible
				// (timing seems to work a little better- https://github.com/leezer3/OpenBVE/issues/1335 )
				options.Backend = PlatformBackend.PreferNative;
			}
			
			Toolkit.Init(options);

			Interface.CurrentOptions = new Interface.Options();
			Interface.CurrentOptions.Load();

			Renderer = new NewRenderer(CurrentHost);

			SoundApi = new SoundApi(CurrentHost);
			SoundApi.Initialize(SoundRange.Medium);

			if (!CurrentHost.LoadPlugins(Interface.CurrentOptions, out string error, null, Renderer))
			{
				SoundApi.DeInitialize();
				MessageBox.Show(error, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			TrainManager = new Simulation.TrainManager.TrainManager(CurrentHost, null);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormEditor());

			CurrentHost.UnloadPlugins(out error);
			SoundApi.DeInitialize();

			Interface.CurrentOptions.Save(OpenBveApi.Path.CombineFile(CurrentHost.FileSystem.SettingsFolder, "1.5.0/options_te2.cfg"));
		}
	}
}
