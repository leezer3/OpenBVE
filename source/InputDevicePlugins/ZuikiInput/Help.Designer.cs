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

namespace ZuikiInput
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
            this.textBox_help = new System.Windows.Forms.TextBox();
            this.button_extract = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_help
            // 
            this.textBox_help.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_help.Location = new System.Drawing.Point(12, 12);
            this.textBox_help.Multiline = true;
            this.textBox_help.Name = "textBox_help";
            this.textBox_help.ReadOnly = true;
            this.textBox_help.Size = new System.Drawing.Size(408, 101);
            this.textBox_help.TabIndex = 7;
            this.textBox_help.TabStop = false;
            this.textBox_help.Text = resources.GetString("textBox_help.Text");
            // 
            // button_extract
            // 
            this.button_extract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_extract.Location = new System.Drawing.Point(170, 131);
            this.button_extract.Name = "button_extract";
            this.button_extract.Size = new System.Drawing.Size(148, 23);
            this.button_extract.TabIndex = 10;
            this.button_extract.Text = "Extract files";
            this.button_extract.UseVisualStyleBackColor = true;
            this.button_extract.Click += new System.EventHandler(this.button_extract_Click);
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.Location = new System.Drawing.Point(324, 131);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(96, 23);
            this.button_ok.TabIndex = 11;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // Help
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 166);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_extract);
            this.Controls.Add(this.textBox_help);
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

		private System.Windows.Forms.TextBox textBox_help;
		private System.Windows.Forms.Button button_extract;
		private System.Windows.Forms.Button button_ok;
	}
}
