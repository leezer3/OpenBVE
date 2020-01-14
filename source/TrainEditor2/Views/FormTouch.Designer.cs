namespace TrainEditor2.Views
{
	partial class FormTouch
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			disposable.Dispose();

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panelTouch = new System.Windows.Forms.Panel();
			this.groupBoxTouch = new System.Windows.Forms.GroupBox();
			this.groupBoxTouchSound = new System.Windows.Forms.GroupBox();
			this.numericUpDownTouchSoundIndex = new System.Windows.Forms.NumericUpDown();
			this.labelTouchSoundIndex = new System.Windows.Forms.Label();
			this.buttonTouchCopy = new System.Windows.Forms.Button();
			this.groupBoxTouchCommand = new System.Windows.Forms.GroupBox();
			this.comboBoxTouchCommandName = new System.Windows.Forms.ComboBox();
			this.numericUpDownTouchCommandOption = new System.Windows.Forms.NumericUpDown();
			this.labelTouchCommandOption = new System.Windows.Forms.Label();
			this.labelTouchCommandName = new System.Windows.Forms.Label();
			this.buttonTouchAdd = new System.Windows.Forms.Button();
			this.buttonTouchRemove = new System.Windows.Forms.Button();
			this.splitContainerTouch = new System.Windows.Forms.SplitContainer();
			this.treeViewTouch = new System.Windows.Forms.TreeView();
			this.listViewTouch = new System.Windows.Forms.ListView();
			this.panelTouchNavi = new System.Windows.Forms.Panel();
			this.buttonOK = new System.Windows.Forms.Button();
			this.panelTouch.SuspendLayout();
			this.groupBoxTouch.SuspendLayout();
			this.groupBoxTouchSound.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchSoundIndex)).BeginInit();
			this.groupBoxTouchCommand.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchCommandOption)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTouch)).BeginInit();
			this.splitContainerTouch.Panel1.SuspendLayout();
			this.splitContainerTouch.Panel2.SuspendLayout();
			this.splitContainerTouch.SuspendLayout();
			this.panelTouchNavi.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelTouch
			// 
			this.panelTouch.Controls.Add(this.groupBoxTouch);
			this.panelTouch.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelTouch.Location = new System.Drawing.Point(496, 0);
			this.panelTouch.Name = "panelTouch";
			this.panelTouch.Size = new System.Drawing.Size(216, 408);
			this.panelTouch.TabIndex = 0;
			// 
			// groupBoxTouch
			// 
			this.groupBoxTouch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxTouch.Controls.Add(this.groupBoxTouchSound);
			this.groupBoxTouch.Controls.Add(this.buttonTouchCopy);
			this.groupBoxTouch.Controls.Add(this.groupBoxTouchCommand);
			this.groupBoxTouch.Controls.Add(this.buttonTouchAdd);
			this.groupBoxTouch.Controls.Add(this.buttonTouchRemove);
			this.groupBoxTouch.Location = new System.Drawing.Point(8, 224);
			this.groupBoxTouch.Name = "groupBoxTouch";
			this.groupBoxTouch.Size = new System.Drawing.Size(200, 184);
			this.groupBoxTouch.TabIndex = 110;
			this.groupBoxTouch.TabStop = false;
			this.groupBoxTouch.Text = "Edit entry";
			// 
			// groupBoxTouchSound
			// 
			this.groupBoxTouchSound.Controls.Add(this.numericUpDownTouchSoundIndex);
			this.groupBoxTouchSound.Controls.Add(this.labelTouchSoundIndex);
			this.groupBoxTouchSound.Location = new System.Drawing.Point(8, 16);
			this.groupBoxTouchSound.Name = "groupBoxTouchSound";
			this.groupBoxTouchSound.Size = new System.Drawing.Size(184, 48);
			this.groupBoxTouchSound.TabIndex = 90;
			this.groupBoxTouchSound.TabStop = false;
			this.groupBoxTouchSound.Text = "Sound";
			// 
			// numericUpDownTouchSoundIndex
			// 
			this.numericUpDownTouchSoundIndex.Location = new System.Drawing.Point(72, 16);
			this.numericUpDownTouchSoundIndex.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.numericUpDownTouchSoundIndex.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericUpDownTouchSoundIndex.Name = "numericUpDownTouchSoundIndex";
			this.numericUpDownTouchSoundIndex.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownTouchSoundIndex.TabIndex = 108;
			// 
			// labelTouchSoundIndex
			// 
			this.labelTouchSoundIndex.Location = new System.Drawing.Point(8, 16);
			this.labelTouchSoundIndex.Name = "labelTouchSoundIndex";
			this.labelTouchSoundIndex.Size = new System.Drawing.Size(56, 16);
			this.labelTouchSoundIndex.TabIndex = 9;
			this.labelTouchSoundIndex.Text = "Index:";
			this.labelTouchSoundIndex.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonTouchCopy
			// 
			this.buttonTouchCopy.Location = new System.Drawing.Point(72, 152);
			this.buttonTouchCopy.Name = "buttonTouchCopy";
			this.buttonTouchCopy.Size = new System.Drawing.Size(56, 24);
			this.buttonTouchCopy.TabIndex = 93;
			this.buttonTouchCopy.Text = "Copy";
			this.buttonTouchCopy.UseVisualStyleBackColor = true;
			// 
			// groupBoxTouchCommand
			// 
			this.groupBoxTouchCommand.Controls.Add(this.comboBoxTouchCommandName);
			this.groupBoxTouchCommand.Controls.Add(this.numericUpDownTouchCommandOption);
			this.groupBoxTouchCommand.Controls.Add(this.labelTouchCommandOption);
			this.groupBoxTouchCommand.Controls.Add(this.labelTouchCommandName);
			this.groupBoxTouchCommand.Location = new System.Drawing.Point(8, 72);
			this.groupBoxTouchCommand.Name = "groupBoxTouchCommand";
			this.groupBoxTouchCommand.Size = new System.Drawing.Size(184, 72);
			this.groupBoxTouchCommand.TabIndex = 109;
			this.groupBoxTouchCommand.TabStop = false;
			this.groupBoxTouchCommand.Text = "Command";
			// 
			// comboBoxTouchCommandName
			// 
			this.comboBoxTouchCommandName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTouchCommandName.FormattingEnabled = true;
			this.comboBoxTouchCommandName.Location = new System.Drawing.Point(72, 16);
			this.comboBoxTouchCommandName.Name = "comboBoxTouchCommandName";
			this.comboBoxTouchCommandName.Size = new System.Drawing.Size(104, 20);
			this.comboBoxTouchCommandName.TabIndex = 112;
			// 
			// numericUpDownTouchCommandOption
			// 
			this.numericUpDownTouchCommandOption.Location = new System.Drawing.Point(72, 40);
			this.numericUpDownTouchCommandOption.Name = "numericUpDownTouchCommandOption";
			this.numericUpDownTouchCommandOption.Size = new System.Drawing.Size(48, 19);
			this.numericUpDownTouchCommandOption.TabIndex = 111;
			// 
			// labelTouchCommandOption
			// 
			this.labelTouchCommandOption.Location = new System.Drawing.Point(8, 40);
			this.labelTouchCommandOption.Name = "labelTouchCommandOption";
			this.labelTouchCommandOption.Size = new System.Drawing.Size(56, 16);
			this.labelTouchCommandOption.TabIndex = 109;
			this.labelTouchCommandOption.Text = "Option:";
			this.labelTouchCommandOption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelTouchCommandName
			// 
			this.labelTouchCommandName.Location = new System.Drawing.Point(8, 16);
			this.labelTouchCommandName.Name = "labelTouchCommandName";
			this.labelTouchCommandName.Size = new System.Drawing.Size(56, 16);
			this.labelTouchCommandName.TabIndex = 9;
			this.labelTouchCommandName.Text = "Name:";
			this.labelTouchCommandName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonTouchAdd
			// 
			this.buttonTouchAdd.Location = new System.Drawing.Point(8, 152);
			this.buttonTouchAdd.Name = "buttonTouchAdd";
			this.buttonTouchAdd.Size = new System.Drawing.Size(56, 24);
			this.buttonTouchAdd.TabIndex = 92;
			this.buttonTouchAdd.Text = "Add";
			this.buttonTouchAdd.UseVisualStyleBackColor = true;
			// 
			// buttonTouchRemove
			// 
			this.buttonTouchRemove.Location = new System.Drawing.Point(136, 152);
			this.buttonTouchRemove.Name = "buttonTouchRemove";
			this.buttonTouchRemove.Size = new System.Drawing.Size(56, 24);
			this.buttonTouchRemove.TabIndex = 91;
			this.buttonTouchRemove.Text = "Remove";
			this.buttonTouchRemove.UseVisualStyleBackColor = true;
			// 
			// splitContainerTouch
			// 
			this.splitContainerTouch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTouch.Location = new System.Drawing.Point(0, 0);
			this.splitContainerTouch.Name = "splitContainerTouch";
			// 
			// splitContainerTouch.Panel1
			// 
			this.splitContainerTouch.Panel1.Controls.Add(this.treeViewTouch);
			// 
			// splitContainerTouch.Panel2
			// 
			this.splitContainerTouch.Panel2.Controls.Add(this.listViewTouch);
			this.splitContainerTouch.Size = new System.Drawing.Size(496, 408);
			this.splitContainerTouch.SplitterDistance = 155;
			this.splitContainerTouch.TabIndex = 1;
			// 
			// treeViewTouch
			// 
			this.treeViewTouch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewTouch.HideSelection = false;
			this.treeViewTouch.Location = new System.Drawing.Point(0, 0);
			this.treeViewTouch.Name = "treeViewTouch";
			this.treeViewTouch.Size = new System.Drawing.Size(155, 408);
			this.treeViewTouch.TabIndex = 0;
			// 
			// listViewTouch
			// 
			this.listViewTouch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewTouch.FullRowSelect = true;
			this.listViewTouch.HideSelection = false;
			this.listViewTouch.Location = new System.Drawing.Point(0, 0);
			this.listViewTouch.MultiSelect = false;
			this.listViewTouch.Name = "listViewTouch";
			this.listViewTouch.Size = new System.Drawing.Size(337, 408);
			this.listViewTouch.TabIndex = 0;
			this.listViewTouch.UseCompatibleStateImageBehavior = false;
			this.listViewTouch.View = System.Windows.Forms.View.Details;
			// 
			// panelTouchNavi
			// 
			this.panelTouchNavi.Controls.Add(this.buttonOK);
			this.panelTouchNavi.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelTouchNavi.Location = new System.Drawing.Point(0, 408);
			this.panelTouchNavi.Name = "panelTouchNavi";
			this.panelTouchNavi.Size = new System.Drawing.Size(712, 42);
			this.panelTouchNavi.TabIndex = 2;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Location = new System.Drawing.Point(648, 8);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 24);
			this.buttonOK.TabIndex = 41;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// FormTouch
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(712, 450);
			this.Controls.Add(this.splitContainerTouch);
			this.Controls.Add(this.panelTouch);
			this.Controls.Add(this.panelTouchNavi);
			this.Name = "FormTouch";
			this.Text = "TouchElement";
			this.Load += new System.EventHandler(this.FormTouch_Load);
			this.panelTouch.ResumeLayout(false);
			this.groupBoxTouch.ResumeLayout(false);
			this.groupBoxTouchSound.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchSoundIndex)).EndInit();
			this.groupBoxTouchCommand.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownTouchCommandOption)).EndInit();
			this.splitContainerTouch.Panel1.ResumeLayout(false);
			this.splitContainerTouch.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTouch)).EndInit();
			this.splitContainerTouch.ResumeLayout(false);
			this.panelTouchNavi.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelTouch;
		private System.Windows.Forms.SplitContainer splitContainerTouch;
		private System.Windows.Forms.TreeView treeViewTouch;
		private System.Windows.Forms.ListView listViewTouch;
		private System.Windows.Forms.GroupBox groupBoxTouchSound;
		private System.Windows.Forms.Label labelTouchSoundIndex;
		private System.Windows.Forms.Button buttonTouchCopy;
		private System.Windows.Forms.Button buttonTouchAdd;
		private System.Windows.Forms.Button buttonTouchRemove;
		private System.Windows.Forms.GroupBox groupBoxTouchCommand;
		private System.Windows.Forms.Label labelTouchCommandOption;
		private System.Windows.Forms.Label labelTouchCommandName;
		private System.Windows.Forms.NumericUpDown numericUpDownTouchSoundIndex;
		private System.Windows.Forms.NumericUpDown numericUpDownTouchCommandOption;
		private System.Windows.Forms.ComboBox comboBoxTouchCommandName;
		private System.Windows.Forms.GroupBox groupBoxTouch;
		private System.Windows.Forms.Panel panelTouchNavi;
		private System.Windows.Forms.Button buttonOK;
	}
}
