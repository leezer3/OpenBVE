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

namespace DenshaDeGoInput
{
	partial class Help
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Help));
			this.labelController1 = new System.Windows.Forms.Label();
			this.textBoxController1 = new System.Windows.Forms.TextBox();
			this.labelController2 = new System.Windows.Forms.Label();
			this.textBoxController2 = new System.Windows.Forms.TextBox();
			this.labelWindows = new System.Windows.Forms.Label();
			this.labelLinux = new System.Windows.Forms.Label();
			this.textBoxWindows = new System.Windows.Forms.TextBox();
			this.textBoxLinux = new System.Windows.Forms.TextBox();
			this.buttonZadig = new System.Windows.Forms.Button();
			this.buttonWindows = new System.Windows.Forms.Button();
			this.buttonLinux = new System.Windows.Forms.Button();
			this.buttonOk = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelController1
			// 
			this.labelController1.AutoSize = true;
			this.labelController1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelController1.Location = new System.Drawing.Point(12, 9);
			this.labelController1.Name = "labelController1";
			this.labelController1.Size = new System.Drawing.Size(400, 25);
			this.labelController1.TabIndex = 0;
			this.labelController1.Text = "My controller is not working correctly";
			// 
			// textBoxController1
			// 
			this.textBoxController1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBoxController1.Location = new System.Drawing.Point(17, 37);
			this.textBoxController1.Multiline = true;
			this.textBoxController1.Name = "textBoxController1";
			this.textBoxController1.ReadOnly = true;
			this.textBoxController1.Size = new System.Drawing.Size(605, 71);
			this.textBoxController1.TabIndex = 1;
			this.textBoxController1.TabStop = false;
			this.textBoxController1.Text = resources.GetString("textBoxController1.Text");
			// 
			// labelController2
			// 
			this.labelController2.AutoSize = true;
			this.labelController2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelController2.Location = new System.Drawing.Point(12, 111);
			this.labelController2.Name = "labelController2";
			this.labelController2.Size = new System.Drawing.Size(362, 25);
			this.labelController2.TabIndex = 2;
			this.labelController2.Text = "My PS2 controller is not detected";
			// 
			// textBoxController2
			// 
			this.textBoxController2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBoxController2.Location = new System.Drawing.Point(17, 139);
			this.textBoxController2.Multiline = true;
			this.textBoxController2.Name = "textBoxController2";
			this.textBoxController2.ReadOnly = true;
			this.textBoxController2.Size = new System.Drawing.Size(605, 19);
			this.textBoxController2.TabIndex = 3;
			this.textBoxController2.TabStop = false;
			this.textBoxController2.Text = "If your PS2 controller does not appear in the list, follow these steps:";
			// 
			// labelWindows
			// 
			this.labelWindows.AutoSize = true;
			this.labelWindows.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelWindows.Location = new System.Drawing.Point(13, 161);
			this.labelWindows.Name = "labelWindows";
			this.labelWindows.Size = new System.Drawing.Size(80, 20);
			this.labelWindows.TabIndex = 4;
			this.labelWindows.Text = "Windows";
			// 
			// labelLinux
			// 
			this.labelLinux.AutoSize = true;
			this.labelLinux.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelLinux.Location = new System.Drawing.Point(13, 250);
			this.labelLinux.Name = "labelLinux";
			this.labelLinux.Size = new System.Drawing.Size(51, 20);
			this.labelLinux.TabIndex = 6;
			this.labelLinux.Text = "Linux";
			// 
			// textBoxWindows
			// 
			this.textBoxWindows.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBoxWindows.Location = new System.Drawing.Point(17, 184);
			this.textBoxWindows.Multiline = true;
			this.textBoxWindows.Name = "textBoxWindows";
			this.textBoxWindows.ReadOnly = true;
			this.textBoxWindows.Size = new System.Drawing.Size(605, 63);
			this.textBoxWindows.TabIndex = 5;
			this.textBoxWindows.TabStop = false;
			this.textBoxWindows.Text = resources.GetString("textBoxWindows.Text");
			// 
			// textBoxLinux
			// 
			this.textBoxLinux.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBoxLinux.Location = new System.Drawing.Point(17, 273);
			this.textBoxLinux.Multiline = true;
			this.textBoxLinux.Name = "textBoxLinux";
			this.textBoxLinux.ReadOnly = true;
			this.textBoxLinux.Size = new System.Drawing.Size(605, 150);
			this.textBoxLinux.TabIndex = 7;
			this.textBoxLinux.TabStop = false;
			this.textBoxLinux.Text = "1. Extract the Linux udev file.\r\n2. Copy the file to \"/etc/udev/rules.d/\" (root p" +
    "ermissions required).\r\n3. Reboot or reload udev rules manually.";
			// 
			// buttonZadig
			// 
			this.buttonZadig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonZadig.Location = new System.Drawing.Point(12, 429);
			this.buttonZadig.Name = "buttonZadig";
			this.buttonZadig.Size = new System.Drawing.Size(148, 23);
			this.buttonZadig.TabIndex = 8;
			this.buttonZadig.Text = "Get Zadig";
			this.buttonZadig.UseVisualStyleBackColor = true;
			this.buttonZadig.Click += new System.EventHandler(this.buttonZadig_Click);
			// 
			// buttonWindows
			// 
			this.buttonWindows.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonWindows.Location = new System.Drawing.Point(166, 429);
			this.buttonWindows.Name = "buttonWindows";
			this.buttonWindows.Size = new System.Drawing.Size(148, 23);
			this.buttonWindows.TabIndex = 9;
			this.buttonWindows.Text = "Extract files (Windows)";
			this.buttonWindows.UseVisualStyleBackColor = true;
			this.buttonWindows.Click += new System.EventHandler(this.buttonWindows_Click);
			// 
			// buttonLinux
			// 
			this.buttonLinux.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonLinux.Location = new System.Drawing.Point(320, 429);
			this.buttonLinux.Name = "buttonLinux";
			this.buttonLinux.Size = new System.Drawing.Size(148, 23);
			this.buttonLinux.TabIndex = 10;
			this.buttonLinux.Text = "Extract files (Linux)";
			this.buttonLinux.UseVisualStyleBackColor = true;
			this.buttonLinux.Click += new System.EventHandler(this.buttonLinux_Click);
			// 
			// buttonOk
			// 
			this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonOk.Location = new System.Drawing.Point(526, 429);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(96, 23);
			this.buttonOk.TabIndex = 11;
			this.buttonOk.Text = "OK";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// Help
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(634, 464);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.buttonLinux);
			this.Controls.Add(this.buttonWindows);
			this.Controls.Add(this.buttonZadig);
			this.Controls.Add(this.textBoxLinux);
			this.Controls.Add(this.textBoxWindows);
			this.Controls.Add(this.labelLinux);
			this.Controls.Add(this.labelWindows);
			this.Controls.Add(this.textBoxController2);
			this.Controls.Add(this.labelController2);
			this.Controls.Add(this.textBoxController1);
			this.Controls.Add(this.labelController1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "Help";
			this.ShowInTaskbar = false;
			this.Text = "Help";
			this.Shown += new System.EventHandler(this.Help_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelController1;
		private System.Windows.Forms.TextBox textBoxController1;
		private System.Windows.Forms.Label labelController2;
		private System.Windows.Forms.TextBox textBoxController2;
		private System.Windows.Forms.Label labelWindows;
		private System.Windows.Forms.Label labelLinux;
		private System.Windows.Forms.TextBox textBoxWindows;
		private System.Windows.Forms.TextBox textBoxLinux;
		private System.Windows.Forms.Button buttonZadig;
		private System.Windows.Forms.Button buttonWindows;
		private System.Windows.Forms.Button buttonLinux;
		private System.Windows.Forms.Button buttonOk;
	}
}
