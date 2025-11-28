using System;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace TrainEditor {
	internal static class Program {

		// --- members ---

		/// <summary>Information about the file system organization.</summary>
		internal static FileSystem FileSystem = null;

		internal static HostInterface CurrentHost;
		// --- functions ---

		/// <summary>Is executed when the program starts.</summary>
		/// <param name="args">The command-line arguments.</param>
		[STAThread]
		private static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			CurrentHost = new Host();
			try {
				FileSystem = FileSystem.FromCommandLineArgs(args, CurrentHost);
				FileSystem.CreateFileSystem();
			} catch (Exception ex) {
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.TrainEditor,  new [] {"errors","filesystem_invalid"}) + Environment.NewLine + Environment.NewLine + ex.Message, @"TrainEditor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			Application.Run(new formEditor());
		}
		
	}
}
