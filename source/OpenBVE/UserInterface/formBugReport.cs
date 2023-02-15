using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenBve.UserInterface;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace OpenBve
{
	internal partial class formBugReport : Form
	{
		public formBugReport()
		{
			InitializeComponent();
			try
			{
				string File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.ico");
				this.Icon = new Icon(File);
			}
			catch
			{
				// Ignored- Just an icon
			}
			ApplyLanguage();
		}

		private void ApplyLanguage() {
			this.Text = Translations.GetInterfaceString("bug_report_title");
			textBoxReportLabel.Text = Translations.GetInterfaceString("bug_report_description");
			labelViewLog.Text = Translations.GetInterfaceString("bug_report_view_log");
			labelViewCrash.Text = Translations.GetInterfaceString("bug_report_view_crash_log");
			label1.Text = Translations.GetInterfaceString("bug_report_enter_description");
			buttonReportProblem.Text = Translations.GetInterfaceString("bug_report_save");
			buttonViewLog.Text = Translations.GetInterfaceString("bug_report_view_log_button");
			buttonViewCrashLog.Text = Translations.GetInterfaceString("bug_report_view_log_button");
		}

		private void buttonViewLog_Click(object sender, System.EventArgs e)
		{
			try
			{
				var file = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "log.txt");
				if(File.Exists(file))
				{
					if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows)
					{
						Process.Start(file);
					}
					else
					{
						
						formViewLog log = new formViewLog(File.ReadAllText(file));
						log.ShowDialog();
					}
				} else {
					MessageBox.Show(Translations.GetInterfaceString("bug_report_no_log"), Translations.GetInterfaceString("bug_report_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch
			{
			}
		}

		private void buttonViewCrashLog_Click(object sender, System.EventArgs e)
		{
			try
			{
				var directory = new DirectoryInfo(Program.FileSystem.SettingsFolder);
				var file = directory.GetFiles("OpenBVE Crash*.log").OrderByDescending(f => f.LastWriteTime).First();
				if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows)
				{
					Process.Start(file.FullName);
				}
				else
				{
					formViewLog log = new formViewLog(File.ReadAllText(file.FullName));
					log.ShowDialog();
				}
			}
			catch
			{
				MessageBox.Show(Translations.GetInterfaceString("bug_report_no_crash_log"), Translations.GetInterfaceString("bug_report_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void buttonReportProblem_Click(object sender, System.EventArgs e)
		{
			string fileName = "openBVE Bug Report" + DateTime.Now.ToString("dd_MM_yyyy") + ".zip";
			try
			{
				using (var ProblemReport = File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName)))
				{
					using (var zipWriter = WriterFactory.Open(ProblemReport, ArchiveType.Zip, CompressionType.LZMA))
					{
						//Add log file to the archive
						var file = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "log.txt");
						if (File.Exists(file))
						{
							zipWriter.Write("log.txt", file);
						}
						//Find the latest crash log and add to the archive (Even if this isn't related to the current crash, we probably want to see it anyway....)
						FileInfo crashLog = null;
						try
						{
							var directory = new DirectoryInfo(Program.FileSystem.SettingsFolder);
							crashLog = directory.GetFiles("OpenBVE Crash*.log").OrderByDescending(f => f.LastWriteTime).First();
						}
						catch
						{
						}
						if (crashLog != null)
						{
							zipWriter.Write(crashLog.Name, crashLog);
						}
						//Save the problem description to the archive using a memory stream
						MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(textBoxProblemDescription.Text));
						zipWriter.Write("Problem Description.txt", ms);
						//Finally add the package database to the archive- Again, this isn't necessarily helpful, but we may well want to see it
						var packageDatabase = new DirectoryInfo(Program.FileSystem.PackageDatabaseFolder);
						if(packageDatabase.Exists) {
							FileInfo[] databaseFiles = packageDatabase.GetFiles("*.xml", SearchOption.AllDirectories);
							foreach (var currentFile in databaseFiles) {
								if (currentFile.Name.ToLowerInvariant() == "packages.xml") {
									zipWriter.Write("PackageDatabase\\" + currentFile.Name, currentFile);
								} else {
									zipWriter.Write("PackageDatabase\\Installed\\" + currentFile.Name, currentFile);
								}
							}
						}

						// Successful, would've thrown into the catch block otherwise.
						MessageBox.Show(Translations.GetInterfaceString("bug_report_saved").Replace("[filename]", fileName), Translations.GetInterfaceString("bug_report_title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
			catch
			{
				MessageBox.Show(Translations.GetInterfaceString("bug_report_save_failed"), Translations.GetInterfaceString("bug_report_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			this.Close();
		}
	}
}
