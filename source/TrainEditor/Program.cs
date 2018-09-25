using System;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;

namespace TrainEditor {
	internal static class Program {

		// --- members ---

		/// <summary>Information about the file system organization.</summary>
		internal static FileSystem FileSystem = null;

		// --- functions ---

		/// <summary>Is executed when the program starts.</summary>
		/// <param name="args">The command-line arguments.</param>
		[STAThread]
		private static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			try {
				FileSystem = FileSystem.FromCommandLineArgs(args);
				FileSystem.CreateFileSystem();
			} catch (Exception ex) {
				MessageBox.Show(Translations.GetInterfaceString("errors_filesystem_invalid") + Environment.NewLine + Environment.NewLine + ex.Message, "TrainEditor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			Application.Run(new formEditor());
		}
		
	}
}