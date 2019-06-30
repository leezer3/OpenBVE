using System;
using System.Windows.Forms;
using MotorSoundEditor.Audio;
using MotorSoundEditor.Systems;
using MotorSoundEditor.Systems.Functions;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using SoundManager;

namespace MotorSoundEditor
{
	internal static class Program
	{
		/// <summary>The host API used by this program.</summary>
		internal static Host CurrentHost;

		/// <summary>Information about the file system organization.</summary>
		internal static FileSystem FileSystem;

		internal static Sounds Sounds;

		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		private static void Main()
		{
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

			Sounds = new Sounds();
			Sounds.Initialize(CurrentHost, SoundsBase.SoundRange.Medium);

			if (!Plugins.LoadPlugins())
			{
				Sounds.Deinitialize();
				return;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormEditor());

			Plugins.UnloadPlugins();
			Sounds.Deinitialize();

			Interface.SaveOptions();
		}
	}
}
