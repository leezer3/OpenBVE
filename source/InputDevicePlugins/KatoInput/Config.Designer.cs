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

namespace KatoInput
{
	partial class Config
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
            this.deviceInputBox = new System.Windows.Forms.GroupBox();
            this.label_reverser = new System.Windows.Forms.Label();
            this.label_power = new System.Windows.Forms.Label();
            this.label_brake = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.handleMappingBox = new System.Windows.Forms.GroupBox();
            this.holdbrakeCheck = new System.Windows.Forms.CheckBox();
            this.minmaxCheck = new System.Windows.Forms.CheckBox();
            this.convertnotchesCheck = new System.Windows.Forms.CheckBox();
            this.deviceBox = new System.Windows.Forms.ComboBox();
            this.label_device = new System.Windows.Forms.Label();
            this.deviceInputBox.SuspendLayout();
            this.handleMappingBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // deviceInputBox
            // 
            this.deviceInputBox.Controls.Add(this.label_reverser);
            this.deviceInputBox.Controls.Add(this.label_power);
            this.deviceInputBox.Controls.Add(this.label_brake);
            this.deviceInputBox.Location = new System.Drawing.Point(12, 37);
            this.deviceInputBox.Name = "deviceInputBox";
            this.deviceInputBox.Size = new System.Drawing.Size(298, 95);
            this.deviceInputBox.TabIndex = 1;
            this.deviceInputBox.TabStop = false;
            this.deviceInputBox.Text = "Device input";
            // 
            // label_reverser
            // 
            this.label_reverser.AutoSize = true;
            this.label_reverser.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_reverser.Location = new System.Drawing.Point(6, 65);
            this.label_reverser.Margin = new System.Windows.Forms.Padding(3);
            this.label_reverser.MaximumSize = new System.Drawing.Size(200, 17);
            this.label_reverser.Name = "label_reverser";
            this.label_reverser.Size = new System.Drawing.Size(134, 17);
            this.label_reverser.TabIndex = 33;
            this.label_reverser.Text = "Reverser: [notch]";
            // 
            // label_power
            // 
            this.label_power.AutoSize = true;
            this.label_power.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_power.Location = new System.Drawing.Point(6, 42);
            this.label_power.Margin = new System.Windows.Forms.Padding(3);
            this.label_power.MaximumSize = new System.Drawing.Size(200, 17);
            this.label_power.Name = "label_power";
            this.label_power.Size = new System.Drawing.Size(112, 17);
            this.label_power.TabIndex = 28;
            this.label_power.Text = "Power: [notch]";
            // 
            // label_brake
            // 
            this.label_brake.AutoSize = true;
            this.label_brake.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake.Location = new System.Drawing.Point(6, 19);
            this.label_brake.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake.MaximumSize = new System.Drawing.Size(200, 17);
            this.label_brake.Name = "label_brake";
            this.label_brake.Size = new System.Drawing.Size(110, 17);
            this.label_brake.TabIndex = 27;
            this.label_brake.Text = "Brake: [notch]";
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(116, 240);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(94, 23);
            this.buttonSave.TabIndex = 3;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(216, 240);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(94, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // handleMappingBox
            // 
            this.handleMappingBox.Controls.Add(this.holdbrakeCheck);
            this.handleMappingBox.Controls.Add(this.minmaxCheck);
            this.handleMappingBox.Controls.Add(this.convertnotchesCheck);
            this.handleMappingBox.Location = new System.Drawing.Point(12, 138);
            this.handleMappingBox.Name = "handleMappingBox";
            this.handleMappingBox.Size = new System.Drawing.Size(298, 95);
            this.handleMappingBox.TabIndex = 5;
            this.handleMappingBox.TabStop = false;
            this.handleMappingBox.Text = "Handle mapping";
            // 
            // holdbrakeCheck
            // 
            this.holdbrakeCheck.Location = new System.Drawing.Point(6, 69);
            this.holdbrakeCheck.Name = "holdbrakeCheck";
            this.holdbrakeCheck.Size = new System.Drawing.Size(229, 19);
            this.holdbrakeCheck.TabIndex = 2;
            this.holdbrakeCheck.Text = "Map hold brake to B1";
            this.holdbrakeCheck.UseVisualStyleBackColor = true;
            this.holdbrakeCheck.CheckedChanged += new System.EventHandler(this.holdbrakeCheck_CheckedChanged);
            // 
            // minmaxCheck
            // 
            this.minmaxCheck.Location = new System.Drawing.Point(6, 44);
            this.minmaxCheck.Name = "minmaxCheck";
            this.minmaxCheck.Size = new System.Drawing.Size(229, 19);
            this.minmaxCheck.TabIndex = 1;
            this.minmaxCheck.Text = "Keep minimum and maximum";
            this.minmaxCheck.UseVisualStyleBackColor = true;
            this.minmaxCheck.CheckedChanged += new System.EventHandler(this.minmaxCheck_CheckedChanged);
            // 
            // convertnotchesCheck
            // 
            this.convertnotchesCheck.Location = new System.Drawing.Point(6, 19);
            this.convertnotchesCheck.Name = "convertnotchesCheck";
            this.convertnotchesCheck.Size = new System.Drawing.Size(229, 19);
            this.convertnotchesCheck.TabIndex = 0;
            this.convertnotchesCheck.Text = "Convert notches";
            this.convertnotchesCheck.UseVisualStyleBackColor = true;
            this.convertnotchesCheck.CheckedChanged += new System.EventHandler(this.convertnotchesCheck_CheckedChanged);
            // 
            // deviceBox
            // 
            this.deviceBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceBox.FormattingEnabled = true;
            this.deviceBox.Location = new System.Drawing.Point(116, 10);
            this.deviceBox.Name = "deviceBox";
            this.deviceBox.Size = new System.Drawing.Size(194, 21);
            this.deviceBox.TabIndex = 6;
            this.deviceBox.SelectedIndexChanged += new System.EventHandler(this.deviceBox_SelectedIndexChanged);
            // 
            // label_device
            // 
            this.label_device.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_device.Location = new System.Drawing.Point(28, 13);
            this.label_device.Name = "label_device";
            this.label_device.Size = new System.Drawing.Size(82, 18);
            this.label_device.TabIndex = 7;
            this.label_device.Text = "Device";
            this.label_device.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 275);
            this.Controls.Add(this.label_device);
            this.Controls.Add(this.deviceBox);
            this.Controls.Add(this.handleMappingBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.deviceInputBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Config";
            this.Text = "Densha de GO! controller configuration";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Config_FormClosed);
            this.Shown += new System.EventHandler(this.Config_Shown);
            this.deviceInputBox.ResumeLayout(false);
            this.deviceInputBox.PerformLayout();
            this.handleMappingBox.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.GroupBox deviceInputBox;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.GroupBox handleMappingBox;
		private System.Windows.Forms.ComboBox deviceBox;
		private System.Windows.Forms.CheckBox holdbrakeCheck;
		private System.Windows.Forms.CheckBox minmaxCheck;
		private System.Windows.Forms.CheckBox convertnotchesCheck;
		private System.Windows.Forms.Label label_device;
		private System.Windows.Forms.Label label_power;
		private System.Windows.Forms.Label label_brake;
		private System.Windows.Forms.Label label_reverser;
	}
}
