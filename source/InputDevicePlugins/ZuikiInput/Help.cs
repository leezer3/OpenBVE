//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Marc Riera, The OpenBVE Project
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

namespace ZuikiInput
{
	public partial class Help : Form
	{
		public Help()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Retranslates the configuration interface.
		/// </summary>
		private void UpdateTranslation()
		{
			Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","help_title"});
			textBox_help.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","help_textbox"});
			button_extract.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","help_extract_button"});
			button_ok.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","help_ok_button"});
		}

		private void Help_Shown(object sender, EventArgs e)
		{
			// Translate the interface to the current language
			UpdateTranslation();
		}

		private void button_extract_Click(object sender, EventArgs e)
		{
			// Extract udev rules
			var saveFolderDialog = new FolderBrowserDialog();
			DialogResult result = saveFolderDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				string folderName = saveFolderDialog.SelectedPath;
				var resource = new MemoryStream();
				Assembly.GetExecutingAssembly().GetManifestResourceStream("udev_rules").CopyTo(resource);
				File.WriteAllBytes(Path.Combine(folderName, "71-zuiki-mascon-pro.rules"), resource.ToArray());
			}
		}

		private void button_ok_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
