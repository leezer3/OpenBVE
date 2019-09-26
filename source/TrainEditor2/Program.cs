using System;
using System.Reactive.Concurrency;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using Reactive.Bindings;
using SoundManager;
using TrainEditor2.Audio;
using TrainEditor2.Graphics;
using TrainEditor2.Systems;
using TrainEditor2.Systems.Functions;
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
				FileSystem = FileSystem.FromCommandLineArgs(new string[0]);
				FileSystem.CreateFileSystem();
			}
			catch (Exception ex)
			{
				MessageBox.Show(Translations.GetInterfaceString("errors_filesystem_invalid") + Environment.NewLine + Environment.NewLine + ex.Message, Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}

			Interface.LoadOptions();

			Renderer = new NewRenderer();

			SoundApi = new SoundApi();
			SoundApi.Initialize(CurrentHost, SoundRange.Medium);

			if (!Plugins.LoadPlugins())
			{
				SoundApi.Deinitialize();
				return;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormEditor());

			Plugins.UnloadPlugins();
			SoundApi.Deinitialize();

			Interface.SaveOptions();
		}
	}
}
