using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenBve {
	public partial class formMessages : Form {
		public formMessages() {
			InitializeComponent();
		}

		// show messages
		internal static DialogResult ShowMessages() {
			formMessages Dialog = new formMessages();
			Dialog.listviewMessages.SmallImageList = new ImageList();
			string Folder = Program.FileSystem.GetDataFolder("Menu");
			try {
				Dialog.listviewMessages.SmallImageList.Images.Add("information", Image.FromFile(OpenBveApi.Path.CombineFile(Folder, "icon_information.png")));
			} catch { }
			try {
				Dialog.listviewMessages.SmallImageList.Images.Add("warning", Image.FromFile(OpenBveApi.Path.CombineFile(Folder, "icon_warning.png")));
			} catch { }
			try {
				Dialog.listviewMessages.SmallImageList.Images.Add("error", Image.FromFile(OpenBveApi.Path.CombineFile(Folder, "icon_error.png")));
			} catch { }
			try {
				Dialog.listviewMessages.SmallImageList.Images.Add("critical", Image.FromFile(OpenBveApi.Path.CombineFile(Folder, "icon_critical.png")));
			} catch { }
			for (int i = 0; i < Interface.MessageCount; i++) {
				string t = "Unknown";
				string g = "information";
				switch (Interface.Messages[i].Type) {
					case Interface.MessageType.Information:
						t = "Information";
						g = "information";
						break;
					case Interface.MessageType.Warning:
						t = "Warning";
						g = "warning";
						break;
					case Interface.MessageType.Error:
						t = "Error";
						g = "error";
						break;
					case Interface.MessageType.Critical:
						t = "Critical";
						g = "critical";
						break;
				}
				ListViewItem a = Dialog.listviewMessages.Items.Add(t, g);
				a.SubItems.Add(Interface.Messages[i].Text);
			}
			Dialog.listviewMessages.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			DialogResult Result = Dialog.ShowDialog();
			Dialog.Dispose();
			return Result;
		}

		// shown
		private void formMessages_Shown(object sender, EventArgs e) {
			buttonClose.Focus();
		}

		// ignore
		private void buttonIgnore_Click(object sender, EventArgs e) {
			this.DialogResult = DialogResult.Ignore;
		}

		// cancel
		private void buttonCancel_Click(object sender, EventArgs e) {
			this.DialogResult = DialogResult.Cancel;
		}

		// save
		private void ButtonSaveClick(object sender, EventArgs e) {
			// prepare
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			for (int i = 0; i < Interface.MessageCount; i++) {
				Builder.AppendLine(Interface.Messages[i].Text);
			}
			// save
			SaveFileDialog Dialog = new SaveFileDialog();
			Dialog.Filter = "Text files|*.txt|All files|*";
			if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					System.IO.File.WriteAllText(Dialog.FileName, Builder.ToString(), System.Text.Encoding.UTF8);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
		
	}
}