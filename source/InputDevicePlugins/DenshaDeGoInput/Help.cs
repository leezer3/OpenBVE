//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020-2021, Marc Riera, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace DenshaDeGoInput
{
	public partial class Help : Form
	{
		public Help()
		{
			InitializeComponent();
			switch (DenshaDeGoInput.CurrentHost.Platform)
			{
				case HostPlatform.MicrosoftWindows:
					buttonZadig.Enabled = true;
					buttonWindows.Enabled = true;
					buttonLinux.Enabled = false;
					break;
				case HostPlatform.GNULinux:
					buttonZadig.Enabled = false;
					buttonWindows.Enabled = false;
					buttonLinux.Enabled = true;
					break;
				case HostPlatform.AppleOSX:
					buttonZadig.Enabled = false;
					buttonWindows.Enabled = false;
					buttonLinux.Enabled = false;
					break;
			}
		}

		/// <summary>
		/// Retranslates the configuration interface.
		/// </summary>
		private void UpdateTranslation()
		{
			Text = Translations.GetInterfaceString("denshadego_help_title");
			labelController1.Text = Translations.GetInterfaceString("denshadego_help_controller1_label");
			labelController2.Text = Translations.GetInterfaceString("denshadego_help_controller2_label");
			labelWindows.Text = Translations.GetInterfaceString("denshadego_help_windows_label");
			labelLinux.Text = Translations.GetInterfaceString("denshadego_help_linux_label");
			textBoxController1.Text = Translations.GetInterfaceString("denshadego_help_controller1_textbox");
			textBoxController2.Text = Translations.GetInterfaceString("denshadego_help_controller2_textbox");
			textBoxWindows.Text = Translations.GetInterfaceString("denshadego_help_windows_textbox");
			if (DenshaDeGoInput.LibUsbIssue)
			{
				textBoxLinux.Text = Translations.GetInterfaceString("denshadego_help_libusb_symlink");
			}
			else
			{
				textBoxLinux.Text = Translations.GetInterfaceString("denshadego_help_linux_textbox");	
			}
			buttonZadig.Text = Translations.GetInterfaceString("denshadego_help_zadig_button");
			buttonWindows.Text = Translations.GetInterfaceString("denshadego_help_windows_button");
			buttonLinux.Text = Translations.GetInterfaceString("denshadego_help_linux_button");
			buttonOk.Text = Translations.GetInterfaceString("denshadego_help_ok_button");
		}

		private void Help_Shown(object sender, EventArgs e)
		{
			// Translate the interface to the current language
			UpdateTranslation();
		}

		private void buttonZadig_Click(object sender, EventArgs e)
		{
			try
			{
				// Open the Zadig homepage
				System.Diagnostics.Process.Start("https://zadig.akeo.ie/");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void buttonWindows_Click(object sender, EventArgs e)
		{
			// Extract Zadig config files for Windows
			var saveFolderDialog = new FolderBrowserDialog();
			DialogResult result = saveFolderDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				string folderName = saveFolderDialog.SelectedPath;
				var resource = new MemoryStream();
				Assembly.GetExecutingAssembly().GetManifestResourceStream("type2_driver").CopyTo(resource);
				File.WriteAllBytes(Path.Combine(folderName, "Type2.cfg"), resource.ToArray());
				resource.SetLength(0);
				Assembly.GetExecutingAssembly().GetManifestResourceStream("shinkansen_driver").CopyTo(resource);
				File.WriteAllBytes(Path.Combine(folderName, "Shinkansen.cfg"), resource.ToArray());
				resource.SetLength(0);
				Assembly.GetExecutingAssembly().GetManifestResourceStream("ryojouhen_driver").CopyTo(resource);
				File.WriteAllBytes(Path.Combine(folderName, "Ryojouhen.cfg"), resource.ToArray());
			}
		}

		private void buttonLinux_Click(object sender, EventArgs e)
		{
			// Extract Linux udev rules
			var saveFolderDialog = new FolderBrowserDialog();
			DialogResult result = saveFolderDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				string folderName = saveFolderDialog.SelectedPath;
				var resource = new MemoryStream();
				Assembly.GetExecutingAssembly().GetManifestResourceStream("udev_rules").CopyTo(resource);
				File.WriteAllBytes(Path.Combine(folderName, "99-ps2-train-controllers.rules"), resource.ToArray());
			}
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
