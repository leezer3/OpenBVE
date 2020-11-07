//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Marc Riera, The OpenBVE Project
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
            this.components = new System.ComponentModel.Container();
            this.deviceInputBox = new System.Windows.Forms.GroupBox();
            this.buttonCalibrate = new System.Windows.Forms.Button();
            this.label_select = new System.Windows.Forms.Label();
            this.label_brakeemg = new System.Windows.Forms.Label();
            this.label_braken = new System.Windows.Forms.Label();
            this.label_start = new System.Windows.Forms.Label();
            this.label_brake1 = new System.Windows.Forms.Label();
            this.label_brake2 = new System.Windows.Forms.Label();
            this.label_d = new System.Windows.Forms.Label();
            this.label_c = new System.Windows.Forms.Label();
            this.label_brake3 = new System.Windows.Forms.Label();
            this.label_b = new System.Windows.Forms.Label();
            this.label_powern = new System.Windows.Forms.Label();
            this.label_a = new System.Windows.Forms.Label();
            this.label_power1 = new System.Windows.Forms.Label();
            this.label_power5 = new System.Windows.Forms.Label();
            this.label_power3 = new System.Windows.Forms.Label();
            this.label_power4 = new System.Windows.Forms.Label();
            this.label_power2 = new System.Windows.Forms.Label();
            this.label_brake4 = new System.Windows.Forms.Label();
            this.label_brake5 = new System.Windows.Forms.Label();
            this.label_brake6 = new System.Windows.Forms.Label();
            this.label_brake7 = new System.Windows.Forms.Label();
            this.label_brake8 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonMappingBox = new System.Windows.Forms.GroupBox();
            this.buttonselectBox = new System.Windows.Forms.ComboBox();
            this.buttonstartBox = new System.Windows.Forms.ComboBox();
            this.buttoncBox = new System.Windows.Forms.ComboBox();
            this.buttonbBox = new System.Windows.Forms.ComboBox();
            this.buttonaBox = new System.Windows.Forms.ComboBox();
            this.label_buttonselect = new System.Windows.Forms.Label();
            this.label_buttonstart = new System.Windows.Forms.Label();
            this.label_buttonc = new System.Windows.Forms.Label();
            this.label_buttonb = new System.Windows.Forms.Label();
            this.label_buttona = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.handleMappingBox = new System.Windows.Forms.GroupBox();
            this.holdbrakeCheck = new System.Windows.Forms.CheckBox();
            this.minmaxCheck = new System.Windows.Forms.CheckBox();
            this.convertnotchesCheck = new System.Windows.Forms.CheckBox();
            this.deviceBox = new System.Windows.Forms.ComboBox();
            this.label_device = new System.Windows.Forms.Label();
            this.label_up = new System.Windows.Forms.Label();
            this.label_down = new System.Windows.Forms.Label();
            this.label_left = new System.Windows.Forms.Label();
            this.label_right = new System.Windows.Forms.Label();
            this.buttonupBox = new System.Windows.Forms.ComboBox();
            this.buttondownBox = new System.Windows.Forms.ComboBox();
            this.buttonpedalBox = new System.Windows.Forms.ComboBox();
            this.buttonrightBox = new System.Windows.Forms.ComboBox();
            this.buttonleftBox = new System.Windows.Forms.ComboBox();
            this.label_buttonup = new System.Windows.Forms.Label();
            this.label_buttondown = new System.Windows.Forms.Label();
            this.label_buttonpedal = new System.Windows.Forms.Label();
            this.label_buttonright = new System.Windows.Forms.Label();
            this.label_buttonleft = new System.Windows.Forms.Label();
            this.buttondBox = new System.Windows.Forms.ComboBox();
            this.label_buttond = new System.Windows.Forms.Label();
            this.label_pedal = new System.Windows.Forms.Label();
            this.deviceInputBox.SuspendLayout();
            this.buttonMappingBox.SuspendLayout();
            this.handleMappingBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // deviceInputBox
            // 
            this.deviceInputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.deviceInputBox.Controls.Add(this.label_pedal);
            this.deviceInputBox.Controls.Add(this.label_right);
            this.deviceInputBox.Controls.Add(this.label_left);
            this.deviceInputBox.Controls.Add(this.label_down);
            this.deviceInputBox.Controls.Add(this.label_up);
            this.deviceInputBox.Controls.Add(this.buttonCalibrate);
            this.deviceInputBox.Controls.Add(this.label_select);
            this.deviceInputBox.Controls.Add(this.label_brakeemg);
            this.deviceInputBox.Controls.Add(this.label_braken);
            this.deviceInputBox.Controls.Add(this.label_start);
            this.deviceInputBox.Controls.Add(this.label_brake1);
            this.deviceInputBox.Controls.Add(this.label_brake2);
            this.deviceInputBox.Controls.Add(this.label_d);
            this.deviceInputBox.Controls.Add(this.label_c);
            this.deviceInputBox.Controls.Add(this.label_brake3);
            this.deviceInputBox.Controls.Add(this.label_b);
            this.deviceInputBox.Controls.Add(this.label_powern);
            this.deviceInputBox.Controls.Add(this.label_a);
            this.deviceInputBox.Controls.Add(this.label_power1);
            this.deviceInputBox.Controls.Add(this.label_power5);
            this.deviceInputBox.Controls.Add(this.label_power3);
            this.deviceInputBox.Controls.Add(this.label_power4);
            this.deviceInputBox.Controls.Add(this.label_power2);
            this.deviceInputBox.Controls.Add(this.label_brake4);
            this.deviceInputBox.Controls.Add(this.label_brake5);
            this.deviceInputBox.Controls.Add(this.label_brake6);
            this.deviceInputBox.Controls.Add(this.label_brake7);
            this.deviceInputBox.Controls.Add(this.label_brake8);
            this.deviceInputBox.Location = new System.Drawing.Point(12, 12);
            this.deviceInputBox.Name = "deviceInputBox";
            this.deviceInputBox.Size = new System.Drawing.Size(236, 320);
            this.deviceInputBox.TabIndex = 1;
            this.deviceInputBox.TabStop = false;
            this.deviceInputBox.Text = "Device input";
            // 
            // buttonCalibrate
            // 
            this.buttonCalibrate.Location = new System.Drawing.Point(82, 286);
            this.buttonCalibrate.Name = "buttonCalibrate";
            this.buttonCalibrate.Size = new System.Drawing.Size(146, 24);
            this.buttonCalibrate.TabIndex = 21;
            this.buttonCalibrate.Text = "Start calibration";
            this.buttonCalibrate.UseVisualStyleBackColor = true;
            this.buttonCalibrate.Click += new System.EventHandler(this.buttonCalibrate_Click);
            // 
            // label_select
            // 
            this.label_select.BackColor = System.Drawing.Color.Plum;
            this.label_select.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_select.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_select.ForeColor = System.Drawing.Color.Black;
            this.label_select.Location = new System.Drawing.Point(158, 16);
            this.label_select.Margin = new System.Windows.Forms.Padding(3);
            this.label_select.Name = "label_select";
            this.label_select.Size = new System.Drawing.Size(70, 24);
            this.label_select.TabIndex = 19;
            this.label_select.Text = "SELECT";
            this.label_select.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brakeemg
            // 
            this.label_brakeemg.BackColor = System.Drawing.Color.LightCoral;
            this.label_brakeemg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brakeemg.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brakeemg.ForeColor = System.Drawing.Color.Black;
            this.label_brakeemg.Location = new System.Drawing.Point(6, 16);
            this.label_brakeemg.Margin = new System.Windows.Forms.Padding(3);
            this.label_brakeemg.Name = "label_brakeemg";
            this.label_brakeemg.Size = new System.Drawing.Size(70, 24);
            this.label_brakeemg.TabIndex = 15;
            this.label_brakeemg.Text = "EMG";
            this.label_brakeemg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_braken
            // 
            this.label_braken.BackColor = System.Drawing.Color.LightBlue;
            this.label_braken.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_braken.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_braken.ForeColor = System.Drawing.Color.Black;
            this.label_braken.Location = new System.Drawing.Point(6, 286);
            this.label_braken.Margin = new System.Windows.Forms.Padding(3);
            this.label_braken.Name = "label_braken";
            this.label_braken.Size = new System.Drawing.Size(70, 24);
            this.label_braken.TabIndex = 8;
            this.label_braken.Text = "N";
            this.label_braken.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_start
            // 
            this.label_start.BackColor = System.Drawing.Color.Plum;
            this.label_start.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_start.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_start.ForeColor = System.Drawing.Color.Black;
            this.label_start.Location = new System.Drawing.Point(158, 46);
            this.label_start.Margin = new System.Windows.Forms.Padding(3);
            this.label_start.Name = "label_start";
            this.label_start.Size = new System.Drawing.Size(70, 24);
            this.label_start.TabIndex = 20;
            this.label_start.Text = "START";
            this.label_start.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brake1
            // 
            this.label_brake1.BackColor = System.Drawing.Color.LightCoral;
            this.label_brake1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brake1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake1.ForeColor = System.Drawing.Color.Black;
            this.label_brake1.Location = new System.Drawing.Point(6, 256);
            this.label_brake1.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake1.Name = "label_brake1";
            this.label_brake1.Size = new System.Drawing.Size(70, 24);
            this.label_brake1.TabIndex = 7;
            this.label_brake1.Text = "B1";
            this.label_brake1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brake2
            // 
            this.label_brake2.BackColor = System.Drawing.Color.LightCoral;
            this.label_brake2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brake2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake2.ForeColor = System.Drawing.Color.Black;
            this.label_brake2.Location = new System.Drawing.Point(6, 226);
            this.label_brake2.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake2.Name = "label_brake2";
            this.label_brake2.Size = new System.Drawing.Size(70, 24);
            this.label_brake2.TabIndex = 6;
            this.label_brake2.Text = "B2";
            this.label_brake2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_d
            // 
            this.label_d.BackColor = System.Drawing.Color.Plum;
            this.label_d.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_d.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_d.ForeColor = System.Drawing.Color.Black;
            this.label_d.Location = new System.Drawing.Point(158, 166);
            this.label_d.Margin = new System.Windows.Forms.Padding(3);
            this.label_d.Name = "label_d";
            this.label_d.Size = new System.Drawing.Size(70, 24);
            this.label_d.TabIndex = 18;
            this.label_d.Text = "D";
            this.label_d.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_c
            // 
            this.label_c.BackColor = System.Drawing.Color.Plum;
            this.label_c.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_c.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_c.ForeColor = System.Drawing.Color.Black;
            this.label_c.Location = new System.Drawing.Point(158, 136);
            this.label_c.Margin = new System.Windows.Forms.Padding(3);
            this.label_c.Name = "label_c";
            this.label_c.Size = new System.Drawing.Size(70, 24);
            this.label_c.TabIndex = 18;
            this.label_c.Text = "C";
            this.label_c.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brake3
            // 
            this.label_brake3.BackColor = System.Drawing.Color.LightCoral;
            this.label_brake3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brake3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake3.ForeColor = System.Drawing.Color.Black;
            this.label_brake3.Location = new System.Drawing.Point(6, 196);
            this.label_brake3.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake3.Name = "label_brake3";
            this.label_brake3.Size = new System.Drawing.Size(70, 24);
            this.label_brake3.TabIndex = 5;
            this.label_brake3.Text = "B3";
            this.label_brake3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_b
            // 
            this.label_b.BackColor = System.Drawing.Color.Plum;
            this.label_b.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_b.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_b.ForeColor = System.Drawing.Color.Black;
            this.label_b.Location = new System.Drawing.Point(158, 106);
            this.label_b.Margin = new System.Windows.Forms.Padding(3);
            this.label_b.Name = "label_b";
            this.label_b.Size = new System.Drawing.Size(70, 24);
            this.label_b.TabIndex = 17;
            this.label_b.Text = "B";
            this.label_b.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_powern
            // 
            this.label_powern.BackColor = System.Drawing.Color.LightBlue;
            this.label_powern.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_powern.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_powern.ForeColor = System.Drawing.Color.Black;
            this.label_powern.Location = new System.Drawing.Point(82, 16);
            this.label_powern.Margin = new System.Windows.Forms.Padding(3);
            this.label_powern.Name = "label_powern";
            this.label_powern.Size = new System.Drawing.Size(70, 24);
            this.label_powern.TabIndex = 9;
            this.label_powern.Text = "N";
            this.label_powern.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_a
            // 
            this.label_a.BackColor = System.Drawing.Color.Plum;
            this.label_a.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_a.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_a.ForeColor = System.Drawing.Color.Black;
            this.label_a.Location = new System.Drawing.Point(158, 76);
            this.label_a.Margin = new System.Windows.Forms.Padding(3);
            this.label_a.Name = "label_a";
            this.label_a.Size = new System.Drawing.Size(70, 24);
            this.label_a.TabIndex = 16;
            this.label_a.Text = "A";
            this.label_a.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_power1
            // 
            this.label_power1.BackColor = System.Drawing.Color.LightGreen;
            this.label_power1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_power1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_power1.ForeColor = System.Drawing.Color.Black;
            this.label_power1.Location = new System.Drawing.Point(82, 46);
            this.label_power1.Margin = new System.Windows.Forms.Padding(3);
            this.label_power1.Name = "label_power1";
            this.label_power1.Size = new System.Drawing.Size(70, 24);
            this.label_power1.TabIndex = 10;
            this.label_power1.Text = "P1";
            this.label_power1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_power5
            // 
            this.label_power5.BackColor = System.Drawing.Color.LightGreen;
            this.label_power5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_power5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_power5.ForeColor = System.Drawing.Color.Black;
            this.label_power5.Location = new System.Drawing.Point(82, 166);
            this.label_power5.Margin = new System.Windows.Forms.Padding(3);
            this.label_power5.Name = "label_power5";
            this.label_power5.Size = new System.Drawing.Size(70, 24);
            this.label_power5.TabIndex = 14;
            this.label_power5.Text = "P5";
            this.label_power5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_power3
            // 
            this.label_power3.BackColor = System.Drawing.Color.LightGreen;
            this.label_power3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_power3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_power3.ForeColor = System.Drawing.Color.Black;
            this.label_power3.Location = new System.Drawing.Point(82, 106);
            this.label_power3.Margin = new System.Windows.Forms.Padding(3);
            this.label_power3.Name = "label_power3";
            this.label_power3.Size = new System.Drawing.Size(70, 24);
            this.label_power3.TabIndex = 12;
            this.label_power3.Text = "P3";
            this.label_power3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_power4
            // 
            this.label_power4.BackColor = System.Drawing.Color.LightGreen;
            this.label_power4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_power4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_power4.ForeColor = System.Drawing.Color.Black;
            this.label_power4.Location = new System.Drawing.Point(82, 136);
            this.label_power4.Margin = new System.Windows.Forms.Padding(3);
            this.label_power4.Name = "label_power4";
            this.label_power4.Size = new System.Drawing.Size(70, 24);
            this.label_power4.TabIndex = 13;
            this.label_power4.Text = "P4";
            this.label_power4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_power2
            // 
            this.label_power2.BackColor = System.Drawing.Color.LightGreen;
            this.label_power2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_power2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_power2.ForeColor = System.Drawing.Color.Black;
            this.label_power2.Location = new System.Drawing.Point(82, 76);
            this.label_power2.Margin = new System.Windows.Forms.Padding(3);
            this.label_power2.Name = "label_power2";
            this.label_power2.Size = new System.Drawing.Size(70, 24);
            this.label_power2.TabIndex = 11;
            this.label_power2.Text = "P2";
            this.label_power2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brake4
            // 
            this.label_brake4.BackColor = System.Drawing.Color.LightCoral;
            this.label_brake4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brake4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake4.ForeColor = System.Drawing.Color.Black;
            this.label_brake4.Location = new System.Drawing.Point(6, 166);
            this.label_brake4.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake4.Name = "label_brake4";
            this.label_brake4.Size = new System.Drawing.Size(70, 24);
            this.label_brake4.TabIndex = 4;
            this.label_brake4.Text = "B4";
            this.label_brake4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brake5
            // 
            this.label_brake5.BackColor = System.Drawing.Color.LightCoral;
            this.label_brake5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brake5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake5.ForeColor = System.Drawing.Color.Black;
            this.label_brake5.Location = new System.Drawing.Point(6, 136);
            this.label_brake5.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake5.Name = "label_brake5";
            this.label_brake5.Size = new System.Drawing.Size(70, 24);
            this.label_brake5.TabIndex = 3;
            this.label_brake5.Text = "B5";
            this.label_brake5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brake6
            // 
            this.label_brake6.BackColor = System.Drawing.Color.LightCoral;
            this.label_brake6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brake6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake6.ForeColor = System.Drawing.Color.Black;
            this.label_brake6.Location = new System.Drawing.Point(6, 106);
            this.label_brake6.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake6.Name = "label_brake6";
            this.label_brake6.Size = new System.Drawing.Size(70, 24);
            this.label_brake6.TabIndex = 2;
            this.label_brake6.Text = "B6";
            this.label_brake6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brake7
            // 
            this.label_brake7.BackColor = System.Drawing.Color.LightCoral;
            this.label_brake7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brake7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake7.ForeColor = System.Drawing.Color.Black;
            this.label_brake7.Location = new System.Drawing.Point(6, 76);
            this.label_brake7.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake7.Name = "label_brake7";
            this.label_brake7.Size = new System.Drawing.Size(70, 24);
            this.label_brake7.TabIndex = 1;
            this.label_brake7.Text = "B7";
            this.label_brake7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_brake8
            // 
            this.label_brake8.BackColor = System.Drawing.Color.LightCoral;
            this.label_brake8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_brake8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_brake8.ForeColor = System.Drawing.Color.Black;
            this.label_brake8.Location = new System.Drawing.Point(6, 46);
            this.label_brake8.Margin = new System.Windows.Forms.Padding(3);
            this.label_brake8.Name = "label_brake8";
            this.label_brake8.Size = new System.Drawing.Size(70, 24);
            this.label_brake8.TabIndex = 0;
            this.label_brake8.Text = "B8";
            this.label_brake8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // buttonMappingBox
            // 
            this.buttonMappingBox.Controls.Add(this.buttondBox);
            this.buttonMappingBox.Controls.Add(this.label_buttond);
            this.buttonMappingBox.Controls.Add(this.buttonupBox);
            this.buttonMappingBox.Controls.Add(this.buttondownBox);
            this.buttonMappingBox.Controls.Add(this.buttonpedalBox);
            this.buttonMappingBox.Controls.Add(this.buttonrightBox);
            this.buttonMappingBox.Controls.Add(this.buttonleftBox);
            this.buttonMappingBox.Controls.Add(this.label_buttonup);
            this.buttonMappingBox.Controls.Add(this.label_buttondown);
            this.buttonMappingBox.Controls.Add(this.label_buttonpedal);
            this.buttonMappingBox.Controls.Add(this.label_buttonright);
            this.buttonMappingBox.Controls.Add(this.label_buttonleft);
            this.buttonMappingBox.Controls.Add(this.buttonselectBox);
            this.buttonMappingBox.Controls.Add(this.buttonstartBox);
            this.buttonMappingBox.Controls.Add(this.buttoncBox);
            this.buttonMappingBox.Controls.Add(this.buttonbBox);
            this.buttonMappingBox.Controls.Add(this.buttonaBox);
            this.buttonMappingBox.Controls.Add(this.label_buttonselect);
            this.buttonMappingBox.Controls.Add(this.label_buttonstart);
            this.buttonMappingBox.Controls.Add(this.label_buttonc);
            this.buttonMappingBox.Controls.Add(this.label_buttonb);
            this.buttonMappingBox.Controls.Add(this.label_buttona);
            this.buttonMappingBox.Location = new System.Drawing.Point(254, 43);
            this.buttonMappingBox.Name = "buttonMappingBox";
            this.buttonMappingBox.Size = new System.Drawing.Size(500, 184);
            this.buttonMappingBox.TabIndex = 2;
            this.buttonMappingBox.TabStop = false;
            this.buttonMappingBox.Text = "Button mapping";
            // 
            // buttonselectBox
            // 
            this.buttonselectBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonselectBox.FormattingEnabled = true;
            this.buttonselectBox.Location = new System.Drawing.Point(62, 20);
            this.buttonselectBox.Name = "buttonselectBox";
            this.buttonselectBox.Size = new System.Drawing.Size(182, 21);
            this.buttonselectBox.TabIndex = 8;
            this.buttonselectBox.SelectedIndexChanged += new System.EventHandler(this.buttonselectBox_SelectedIndexChanged);
            // 
            // buttonstartBox
            // 
            this.buttonstartBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonstartBox.FormattingEnabled = true;
            this.buttonstartBox.Location = new System.Drawing.Point(62, 47);
            this.buttonstartBox.Name = "buttonstartBox";
            this.buttonstartBox.Size = new System.Drawing.Size(182, 21);
            this.buttonstartBox.TabIndex = 6;
            this.buttonstartBox.SelectedIndexChanged += new System.EventHandler(this.buttonstartBox_SelectedIndexChanged);
            // 
            // buttoncBox
            // 
            this.buttoncBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttoncBox.FormattingEnabled = true;
            this.buttoncBox.Location = new System.Drawing.Point(62, 128);
            this.buttoncBox.Name = "buttoncBox";
            this.buttoncBox.Size = new System.Drawing.Size(182, 21);
            this.buttoncBox.TabIndex = 7;
            this.buttoncBox.SelectedIndexChanged += new System.EventHandler(this.buttoncBox_SelectedIndexChanged);
            // 
            // buttonbBox
            // 
            this.buttonbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonbBox.FormattingEnabled = true;
            this.buttonbBox.Location = new System.Drawing.Point(62, 101);
            this.buttonbBox.Name = "buttonbBox";
            this.buttonbBox.Size = new System.Drawing.Size(182, 21);
            this.buttonbBox.TabIndex = 6;
            this.buttonbBox.SelectedIndexChanged += new System.EventHandler(this.buttonbBox_SelectedIndexChanged);
            // 
            // buttonaBox
            // 
            this.buttonaBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonaBox.FormattingEnabled = true;
            this.buttonaBox.Location = new System.Drawing.Point(62, 74);
            this.buttonaBox.Name = "buttonaBox";
            this.buttonaBox.Size = new System.Drawing.Size(182, 21);
            this.buttonaBox.TabIndex = 5;
            this.buttonaBox.SelectedIndexChanged += new System.EventHandler(this.buttonaBox_SelectedIndexChanged);
            // 
            // label_buttonselect
            // 
            this.label_buttonselect.Location = new System.Drawing.Point(6, 23);
            this.label_buttonselect.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttonselect.Name = "label_buttonselect";
            this.label_buttonselect.Size = new System.Drawing.Size(50, 15);
            this.label_buttonselect.TabIndex = 3;
            this.label_buttonselect.Text = "SELECT";
            // 
            // label_buttonstart
            // 
            this.label_buttonstart.Location = new System.Drawing.Point(6, 50);
            this.label_buttonstart.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttonstart.Name = "label_buttonstart";
            this.label_buttonstart.Size = new System.Drawing.Size(50, 15);
            this.label_buttonstart.TabIndex = 4;
            this.label_buttonstart.Text = "START";
            // 
            // label_buttonc
            // 
            this.label_buttonc.Location = new System.Drawing.Point(6, 131);
            this.label_buttonc.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttonc.Name = "label_buttonc";
            this.label_buttonc.Size = new System.Drawing.Size(50, 15);
            this.label_buttonc.TabIndex = 2;
            this.label_buttonc.Text = "C";
            // 
            // label_buttonb
            // 
            this.label_buttonb.Location = new System.Drawing.Point(6, 104);
            this.label_buttonb.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttonb.Name = "label_buttonb";
            this.label_buttonb.Size = new System.Drawing.Size(50, 15);
            this.label_buttonb.TabIndex = 1;
            this.label_buttonb.Text = "B";
            // 
            // label_buttona
            // 
            this.label_buttona.Location = new System.Drawing.Point(6, 77);
            this.label_buttona.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttona.Name = "label_buttona";
            this.label_buttona.Size = new System.Drawing.Size(50, 15);
            this.label_buttona.TabIndex = 0;
            this.label_buttona.Text = "A";
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(598, 309);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 3;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(679, 309);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
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
            this.handleMappingBox.Location = new System.Drawing.Point(254, 233);
            this.handleMappingBox.Name = "handleMappingBox";
            this.handleMappingBox.Size = new System.Drawing.Size(500, 70);
            this.handleMappingBox.TabIndex = 5;
            this.handleMappingBox.TabStop = false;
            this.handleMappingBox.Text = "Handle mapping";
            // 
            // holdbrakeCheck
            // 
            this.holdbrakeCheck.Location = new System.Drawing.Point(259, 19);
            this.holdbrakeCheck.Name = "holdbrakeCheck";
            this.holdbrakeCheck.Size = new System.Drawing.Size(235, 19);
            this.holdbrakeCheck.TabIndex = 2;
            this.holdbrakeCheck.Text = "Map hold brake to B1";
            this.holdbrakeCheck.UseVisualStyleBackColor = true;
            this.holdbrakeCheck.CheckedChanged += new System.EventHandler(this.holdbrakeCheck_CheckedChanged);
            // 
            // minmaxCheck
            // 
            this.minmaxCheck.Location = new System.Drawing.Point(6, 44);
            this.minmaxCheck.Name = "minmaxCheck";
            this.minmaxCheck.Size = new System.Drawing.Size(238, 19);
            this.minmaxCheck.TabIndex = 1;
            this.minmaxCheck.Text = "Keep minimum and maximum";
            this.minmaxCheck.UseVisualStyleBackColor = true;
            this.minmaxCheck.CheckedChanged += new System.EventHandler(this.minmaxCheck_CheckedChanged);
            // 
            // convertnotchesCheck
            // 
            this.convertnotchesCheck.Location = new System.Drawing.Point(6, 19);
            this.convertnotchesCheck.Name = "convertnotchesCheck";
            this.convertnotchesCheck.Size = new System.Drawing.Size(238, 19);
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
            this.deviceBox.Location = new System.Drawing.Point(566, 16);
            this.deviceBox.Name = "deviceBox";
            this.deviceBox.Size = new System.Drawing.Size(182, 21);
            this.deviceBox.TabIndex = 6;
            this.deviceBox.SelectedIndexChanged += new System.EventHandler(this.deviceBox_SelectedIndexChanged);
            // 
            // label_device
            // 
            this.label_device.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_device.Location = new System.Drawing.Point(478, 19);
            this.label_device.Name = "label_device";
            this.label_device.Size = new System.Drawing.Size(82, 18);
            this.label_device.TabIndex = 7;
            this.label_device.Text = "Device";
            this.label_device.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_up
            // 
            this.label_up.BackColor = System.Drawing.Color.Khaki;
            this.label_up.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_up.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_up.ForeColor = System.Drawing.Color.Black;
            this.label_up.Location = new System.Drawing.Point(82, 196);
            this.label_up.Margin = new System.Windows.Forms.Padding(3);
            this.label_up.Name = "label_up";
            this.label_up.Size = new System.Drawing.Size(70, 24);
            this.label_up.TabIndex = 22;
            this.label_up.Text = "UP";
            this.label_up.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_down
            // 
            this.label_down.BackColor = System.Drawing.Color.Khaki;
            this.label_down.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_down.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_down.ForeColor = System.Drawing.Color.Black;
            this.label_down.Location = new System.Drawing.Point(82, 226);
            this.label_down.Margin = new System.Windows.Forms.Padding(3);
            this.label_down.Name = "label_down";
            this.label_down.Size = new System.Drawing.Size(70, 24);
            this.label_down.TabIndex = 23;
            this.label_down.Text = "DOWN";
            this.label_down.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_left
            // 
            this.label_left.BackColor = System.Drawing.Color.Khaki;
            this.label_left.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_left.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_left.ForeColor = System.Drawing.Color.Black;
            this.label_left.Location = new System.Drawing.Point(158, 196);
            this.label_left.Margin = new System.Windows.Forms.Padding(3);
            this.label_left.Name = "label_left";
            this.label_left.Size = new System.Drawing.Size(70, 24);
            this.label_left.TabIndex = 24;
            this.label_left.Text = "LEFT";
            this.label_left.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_right
            // 
            this.label_right.BackColor = System.Drawing.Color.Khaki;
            this.label_right.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_right.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_right.ForeColor = System.Drawing.Color.Black;
            this.label_right.Location = new System.Drawing.Point(158, 226);
            this.label_right.Margin = new System.Windows.Forms.Padding(3);
            this.label_right.Name = "label_right";
            this.label_right.Size = new System.Drawing.Size(70, 24);
            this.label_right.TabIndex = 25;
            this.label_right.Text = "RIGHT";
            this.label_right.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonupBox
            // 
            this.buttonupBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonupBox.FormattingEnabled = true;
            this.buttonupBox.Location = new System.Drawing.Point(312, 20);
            this.buttonupBox.Name = "buttonupBox";
            this.buttonupBox.Size = new System.Drawing.Size(182, 21);
            this.buttonupBox.TabIndex = 18;
			this.buttonupBox.SelectedIndexChanged += new System.EventHandler(this.buttonupBox_SelectedIndexChanged);
			// 
			// buttondownBox
			// 
			this.buttondownBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttondownBox.FormattingEnabled = true;
            this.buttondownBox.Location = new System.Drawing.Point(312, 47);
            this.buttondownBox.Name = "buttondownBox";
            this.buttondownBox.Size = new System.Drawing.Size(182, 21);
            this.buttondownBox.TabIndex = 15;
			this.buttondownBox.SelectedIndexChanged += new System.EventHandler(this.buttondownBox_SelectedIndexChanged);
			// 
			// buttonpedalBox
			// 
			this.buttonpedalBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonpedalBox.FormattingEnabled = true;
            this.buttonpedalBox.Location = new System.Drawing.Point(312, 128);
            this.buttonpedalBox.Name = "buttonpedalBox";
            this.buttonpedalBox.Size = new System.Drawing.Size(182, 21);
            this.buttonpedalBox.TabIndex = 17;
			this.buttonpedalBox.SelectedIndexChanged += new System.EventHandler(this.buttonpedalBox_SelectedIndexChanged);
			// 
			// buttonrightBox
			// 
			this.buttonrightBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonrightBox.FormattingEnabled = true;
            this.buttonrightBox.Location = new System.Drawing.Point(312, 101);
            this.buttonrightBox.Name = "buttonrightBox";
            this.buttonrightBox.Size = new System.Drawing.Size(182, 21);
            this.buttonrightBox.TabIndex = 16;
			this.buttonrightBox.SelectedIndexChanged += new System.EventHandler(this.buttonrightBox_SelectedIndexChanged);
			// 
			// buttonleftBox
			// 
			this.buttonleftBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonleftBox.FormattingEnabled = true;
            this.buttonleftBox.Location = new System.Drawing.Point(312, 74);
            this.buttonleftBox.Name = "buttonleftBox";
            this.buttonleftBox.Size = new System.Drawing.Size(182, 21);
            this.buttonleftBox.TabIndex = 14;
			this.buttonleftBox.SelectedIndexChanged += new System.EventHandler(this.buttonleftBox_SelectedIndexChanged);
			// 
			// label_buttonup
			// 
			this.label_buttonup.Location = new System.Drawing.Point(256, 23);
            this.label_buttonup.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttonup.Name = "label_buttonup";
            this.label_buttonup.Size = new System.Drawing.Size(50, 15);
            this.label_buttonup.TabIndex = 12;
            this.label_buttonup.Text = "UP";
            // 
            // label_buttondown
            // 
            this.label_buttondown.Location = new System.Drawing.Point(256, 50);
            this.label_buttondown.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttondown.Name = "label_buttondown";
            this.label_buttondown.Size = new System.Drawing.Size(50, 15);
            this.label_buttondown.TabIndex = 13;
            this.label_buttondown.Text = "DOWN";
            // 
            // label_buttonpedal
            // 
            this.label_buttonpedal.Location = new System.Drawing.Point(256, 131);
            this.label_buttonpedal.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttonpedal.Name = "label_buttonpedal";
            this.label_buttonpedal.Size = new System.Drawing.Size(50, 15);
            this.label_buttonpedal.TabIndex = 11;
            this.label_buttonpedal.Text = "PEDAL";
            // 
            // label_buttonright
            // 
            this.label_buttonright.Location = new System.Drawing.Point(256, 104);
            this.label_buttonright.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttonright.Name = "label_buttonright";
            this.label_buttonright.Size = new System.Drawing.Size(50, 15);
            this.label_buttonright.TabIndex = 10;
            this.label_buttonright.Text = "RIGHT";
            // 
            // label_buttonleft
            // 
            this.label_buttonleft.Location = new System.Drawing.Point(256, 77);
            this.label_buttonleft.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttonleft.Name = "label_buttonleft";
            this.label_buttonleft.Size = new System.Drawing.Size(50, 15);
            this.label_buttonleft.TabIndex = 9;
            this.label_buttonleft.Text = "LEFT";
            // 
            // buttondBox
            // 
            this.buttondBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttondBox.FormattingEnabled = true;
            this.buttondBox.Location = new System.Drawing.Point(62, 155);
            this.buttondBox.Name = "buttondBox";
            this.buttondBox.Size = new System.Drawing.Size(182, 21);
            this.buttondBox.TabIndex = 20;
			this.buttondBox.SelectedIndexChanged += new System.EventHandler(this.buttondBox_SelectedIndexChanged);
			// 
			// label_buttond
			// 
			this.label_buttond.Location = new System.Drawing.Point(6, 158);
            this.label_buttond.Margin = new System.Windows.Forms.Padding(3);
            this.label_buttond.Name = "label_buttond";
            this.label_buttond.Size = new System.Drawing.Size(50, 15);
            this.label_buttond.TabIndex = 19;
            this.label_buttond.Text = "D";
            // 
            // label_pedal
            // 
            this.label_pedal.BackColor = System.Drawing.Color.Silver;
            this.label_pedal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_pedal.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_pedal.ForeColor = System.Drawing.Color.Black;
            this.label_pedal.Location = new System.Drawing.Point(82, 256);
            this.label_pedal.Margin = new System.Windows.Forms.Padding(3);
            this.label_pedal.Name = "label_pedal";
            this.label_pedal.Size = new System.Drawing.Size(145, 24);
            this.label_pedal.TabIndex = 26;
            this.label_pedal.Text = "PEDAL";
            this.label_pedal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 339);
            this.Controls.Add(this.label_device);
            this.Controls.Add(this.deviceBox);
            this.Controls.Add(this.handleMappingBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonMappingBox);
            this.Controls.Add(this.deviceInputBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Config";
            this.Text = "Densha de GO! controller configuration";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Config_FormClosed);
            this.Shown += new System.EventHandler(this.Config_Shown);
            this.deviceInputBox.ResumeLayout(false);
            this.buttonMappingBox.ResumeLayout(false);
            this.handleMappingBox.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.GroupBox deviceInputBox;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label_select;
		private System.Windows.Forms.Label label_start;
		private System.Windows.Forms.Label label_d;
		private System.Windows.Forms.Label label_c;
		private System.Windows.Forms.Label label_b;
		private System.Windows.Forms.Label label_a;
		private System.Windows.Forms.Label label_brakeemg;
		private System.Windows.Forms.Label label_power5;
		private System.Windows.Forms.Label label_power4;
		private System.Windows.Forms.Label label_power3;
		private System.Windows.Forms.Label label_power2;
		private System.Windows.Forms.Label label_power1;
		private System.Windows.Forms.Label label_powern;
		private System.Windows.Forms.Label label_braken;
		private System.Windows.Forms.Label label_brake1;
		private System.Windows.Forms.Label label_brake2;
		private System.Windows.Forms.Label label_brake3;
		private System.Windows.Forms.Label label_brake4;
		private System.Windows.Forms.Label label_brake5;
		private System.Windows.Forms.Label label_brake6;
		private System.Windows.Forms.Label label_brake7;
		private System.Windows.Forms.Label label_brake8;
		private System.Windows.Forms.GroupBox buttonMappingBox;
		private System.Windows.Forms.ComboBox buttonselectBox;
		private System.Windows.Forms.ComboBox buttonstartBox;
		private System.Windows.Forms.ComboBox buttoncBox;
		private System.Windows.Forms.ComboBox buttonbBox;
		private System.Windows.Forms.ComboBox buttonaBox;
		private System.Windows.Forms.Label label_buttonselect;
		private System.Windows.Forms.Label label_buttonstart;
		private System.Windows.Forms.Label label_buttonc;
		private System.Windows.Forms.Label label_buttonb;
		private System.Windows.Forms.Label label_buttona;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.GroupBox handleMappingBox;
		private System.Windows.Forms.ComboBox deviceBox;
		private System.Windows.Forms.Button buttonCalibrate;
		private System.Windows.Forms.CheckBox holdbrakeCheck;
		private System.Windows.Forms.CheckBox minmaxCheck;
		private System.Windows.Forms.CheckBox convertnotchesCheck;
		private System.Windows.Forms.Label label_device;
		private System.Windows.Forms.Label label_right;
		private System.Windows.Forms.Label label_left;
		private System.Windows.Forms.Label label_down;
		private System.Windows.Forms.Label label_up;
		private System.Windows.Forms.ComboBox buttondBox;
		private System.Windows.Forms.Label label_buttond;
		private System.Windows.Forms.ComboBox buttonupBox;
		private System.Windows.Forms.ComboBox buttondownBox;
		private System.Windows.Forms.ComboBox buttonpedalBox;
		private System.Windows.Forms.ComboBox buttonrightBox;
		private System.Windows.Forms.ComboBox buttonleftBox;
		private System.Windows.Forms.Label label_buttonup;
		private System.Windows.Forms.Label label_buttondown;
		private System.Windows.Forms.Label label_buttonpedal;
		private System.Windows.Forms.Label label_buttonright;
		private System.Windows.Forms.Label label_buttonleft;
		private System.Windows.Forms.Label label_pedal;
	}
}
