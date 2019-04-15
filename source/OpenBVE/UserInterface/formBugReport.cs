using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
			catch { }
		}

		private void buttonViewLog_Click(object sender, System.EventArgs e)
		{
			try
			{
				var file = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "log.txt");
				if(File.Exists(file))
				{
					Process.Start(file);
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
				Process.Start(file.FullName);
			}
			catch
			{
				MessageBox.Show("No crash logs were found.");
			}
		}

		private void buttonReportProblem_Click(object sender, System.EventArgs e)
		{
			try
			{
				using (var ProblemReport = File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) , "openBVE Bug Report" + DateTime.Now.ToString("dd_MM_yyyy") + ".zip")))
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
						var packageDatabase = new DirectoryInfo(formMain.currentDatabaseFolder);
						FileInfo[] databaseFiles = packageDatabase.GetFiles("*.xml", SearchOption.AllDirectories);
						foreach (var currentFile in databaseFiles)
						{
							if (currentFile.Name.ToLowerInvariant() == "packages.xml")
							{
								zipWriter.Write("PackageDatabase\\" + currentFile.Name, currentFile);
							}
							else
							{
								zipWriter.Write("PackageDatabase\\Installed\\" + currentFile.Name, currentFile);
							}
						}
						MessageBox.Show(ProblemReport.Name + Environment.NewLine + Environment.NewLine + "Created successfully.");
					}
				}
			}
			catch
			{
			}
			this.Close();
		}
	}
}
