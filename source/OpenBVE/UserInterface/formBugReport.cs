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
				Icon = new Icon(File);
			}
			catch
			{
				// Ignored- Just an icon
			}
			ApplyLanguage();
		}

		private void ApplyLanguage() {
			Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","title"});
			textBoxReportLabel.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "bug_report", "description"});
			labelViewLog.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "bug_report", "view_log"});
			labelViewCrash.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "bug_report", "view_crash_log"});
			label1.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "bug_report", "enter_description"});
			buttonReportProblem.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "bug_report", "save"});
			buttonViewLog.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "bug_report", "view_log_button"});
			buttonViewCrashLog.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "bug_report", "view_log_button"});
		}

		private void buttonViewLog_Click(object sender, EventArgs e)
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
						
						FormViewLog log = new FormViewLog(File.ReadAllText(file));
						log.ShowDialog();
					}
				} else {
					MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","no_log"}), Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","title"}), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch
			{
				// Actually failed to load, but same difference
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "bug_report", "no_log"}), Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","title"}), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void buttonViewCrashLog_Click(object sender, EventArgs e)
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
					FormViewLog log = new FormViewLog(File.ReadAllText(file.FullName));
					log.ShowDialog();
				}
			}
			catch
			{
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","no_crash_log"}), Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","title"}), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void buttonReportProblem_Click(object sender, EventArgs e)
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
							// unable to find / load crash log- Access issues?
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
						MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","saved"}).Replace("[filename]", fileName), Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","title"}), MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
			catch
			{
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","save_failed"}), Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"bug_report","title"}), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			Close();
		}
	}
}
