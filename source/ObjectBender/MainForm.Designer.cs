/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2010-07-17
 * Time: 1:30 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace ObjectBender
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.labelInput = new System.Windows.Forms.Label();
			this.textboxInput = new System.Windows.Forms.TextBox();
			this.buttonInput = new System.Windows.Forms.Button();
			this.buttonOutput = new System.Windows.Forms.Button();
			this.textboxOutput = new System.Windows.Forms.TextBox();
			this.labelOutput = new System.Windows.Forms.Label();
			this.textboxNumberOfSegments = new System.Windows.Forms.TextBox();
			this.labelNumberOfSegments = new System.Windows.Forms.Label();
			this.textboxSegmentLength = new System.Windows.Forms.TextBox();
			this.labelSegmentLength = new System.Windows.Forms.Label();
			this.textboxBlockLength = new System.Windows.Forms.TextBox();
			this.labelBlockLength = new System.Windows.Forms.Label();
			this.textboxRadius = new System.Windows.Forms.TextBox();
			this.labelRadius = new System.Windows.Forms.Label();
			this.labelInformation = new System.Windows.Forms.Label();
			this.buttonClose = new System.Windows.Forms.Button();
			this.buttonStart = new System.Windows.Forms.Button();
			this.textboxRailGauge = new System.Windows.Forms.TextBox();
			this.labelRailGauge = new System.Windows.Forms.Label();
			this.textboxInitialCant = new System.Windows.Forms.TextBox();
			this.labelInitialCant = new System.Windows.Forms.Label();
			this.textboxFinalCant = new System.Windows.Forms.TextBox();
			this.labelFinalCant = new System.Windows.Forms.Label();
			this.panelPane = new System.Windows.Forms.Panel();
			this.labelLineHorizontal = new System.Windows.Forms.Label();
			this.pictureboxLogo = new System.Windows.Forms.PictureBox();
			this.panelPane.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureboxLogo)).BeginInit();
			this.SuspendLayout();
			// 
			// labelInput
			// 
			this.labelInput.Location = new System.Drawing.Point(128, 8);
			this.labelInput.Name = "labelInput";
			this.labelInput.Size = new System.Drawing.Size(72, 20);
			this.labelInput.TabIndex = 1;
			this.labelInput.Text = "Source file:";
			this.labelInput.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textboxInput
			// 
			this.textboxInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.textboxInput.Location = new System.Drawing.Point(200, 8);
			this.textboxInput.Name = "textboxInput";
			this.textboxInput.ReadOnly = true;
			this.textboxInput.Size = new System.Drawing.Size(168, 20);
			this.textboxInput.TabIndex = 2;
			// 
			// buttonInput
			// 
			this.buttonInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonInput.Location = new System.Drawing.Point(376, 8);
			this.buttonInput.Name = "buttonInput";
			this.buttonInput.Size = new System.Drawing.Size(80, 24);
			this.buttonInput.TabIndex = 3;
			this.buttonInput.Text = "Browse...";
			this.buttonInput.UseVisualStyleBackColor = true;
			this.buttonInput.Click += new System.EventHandler(this.ButtonInputClick);
			// 
			// buttonOutput
			// 
			this.buttonOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOutput.Location = new System.Drawing.Point(376, 40);
			this.buttonOutput.Name = "buttonOutput";
			this.buttonOutput.Size = new System.Drawing.Size(80, 24);
			this.buttonOutput.TabIndex = 6;
			this.buttonOutput.Text = "Browse...";
			this.buttonOutput.UseVisualStyleBackColor = true;
			this.buttonOutput.Click += new System.EventHandler(this.ButtonOutputClick);
			// 
			// textboxOutput
			// 
			this.textboxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.textboxOutput.Location = new System.Drawing.Point(200, 40);
			this.textboxOutput.Name = "textboxOutput";
			this.textboxOutput.ReadOnly = true;
			this.textboxOutput.Size = new System.Drawing.Size(168, 20);
			this.textboxOutput.TabIndex = 5;
			// 
			// labelOutput
			// 
			this.labelOutput.Location = new System.Drawing.Point(128, 40);
			this.labelOutput.Name = "labelOutput";
			this.labelOutput.Size = new System.Drawing.Size(72, 20);
			this.labelOutput.TabIndex = 4;
			this.labelOutput.Text = "Target file:";
			this.labelOutput.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textboxNumberOfSegments
			// 
			this.textboxNumberOfSegments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textboxNumberOfSegments.Location = new System.Drawing.Point(336, 80);
			this.textboxNumberOfSegments.Name = "textboxNumberOfSegments";
			this.textboxNumberOfSegments.Size = new System.Drawing.Size(120, 20);
			this.textboxNumberOfSegments.TabIndex = 8;
			this.textboxNumberOfSegments.Enter += new System.EventHandler(this.TextboxNumberOfSegmentsEnter);
			// 
			// labelNumberOfSegments
			// 
			this.labelNumberOfSegments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelNumberOfSegments.Location = new System.Drawing.Point(128, 80);
			this.labelNumberOfSegments.Name = "labelNumberOfSegments";
			this.labelNumberOfSegments.Size = new System.Drawing.Size(208, 20);
			this.labelNumberOfSegments.TabIndex = 7;
			this.labelNumberOfSegments.Text = "Number of segments:";
			this.labelNumberOfSegments.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textboxSegmentLength
			// 
			this.textboxSegmentLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textboxSegmentLength.Location = new System.Drawing.Point(336, 104);
			this.textboxSegmentLength.Name = "textboxSegmentLength";
			this.textboxSegmentLength.Size = new System.Drawing.Size(120, 20);
			this.textboxSegmentLength.TabIndex = 10;
			this.textboxSegmentLength.Enter += new System.EventHandler(this.TextboxSegmentLengthEnter);
			// 
			// labelSegmentLength
			// 
			this.labelSegmentLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelSegmentLength.Location = new System.Drawing.Point(128, 104);
			this.labelSegmentLength.Name = "labelSegmentLength";
			this.labelSegmentLength.Size = new System.Drawing.Size(208, 20);
			this.labelSegmentLength.TabIndex = 9;
			this.labelSegmentLength.Text = "Segment length (m):";
			this.labelSegmentLength.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textboxBlockLength
			// 
			this.textboxBlockLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textboxBlockLength.Location = new System.Drawing.Point(336, 128);
			this.textboxBlockLength.Name = "textboxBlockLength";
			this.textboxBlockLength.Size = new System.Drawing.Size(120, 20);
			this.textboxBlockLength.TabIndex = 12;
			this.textboxBlockLength.Enter += new System.EventHandler(this.TextboxBlockLengthEnter);
			// 
			// labelBlockLength
			// 
			this.labelBlockLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelBlockLength.Location = new System.Drawing.Point(128, 128);
			this.labelBlockLength.Name = "labelBlockLength";
			this.labelBlockLength.Size = new System.Drawing.Size(208, 20);
			this.labelBlockLength.TabIndex = 11;
			this.labelBlockLength.Text = "Block length (m):";
			this.labelBlockLength.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textboxRadius
			// 
			this.textboxRadius.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textboxRadius.Location = new System.Drawing.Point(336, 152);
			this.textboxRadius.Name = "textboxRadius";
			this.textboxRadius.Size = new System.Drawing.Size(120, 20);
			this.textboxRadius.TabIndex = 14;
			this.textboxRadius.Enter += new System.EventHandler(this.TextboxRadiusEnter);
			// 
			// labelRadius
			// 
			this.labelRadius.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelRadius.Location = new System.Drawing.Point(128, 152);
			this.labelRadius.Name = "labelRadius";
			this.labelRadius.Size = new System.Drawing.Size(208, 20);
			this.labelRadius.TabIndex = 13;
			this.labelRadius.Text = "Radius (m):";
			this.labelRadius.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelInformation
			// 
			this.labelInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelInformation.Location = new System.Drawing.Point(128, 256);
			this.labelInformation.Name = "labelInformation";
			this.labelInformation.Size = new System.Drawing.Size(328, 40);
			this.labelInformation.TabIndex = 21;
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClose.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonClose.Location = new System.Drawing.Point(8, 304);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(104, 24);
			this.buttonClose.TabIndex = 2;
			this.buttonClose.Text = "Close";
			this.buttonClose.UseVisualStyleBackColor = false;
			this.buttonClose.Click += new System.EventHandler(this.ButtonCloseClick);
			// 
			// buttonStart
			// 
			this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonStart.Location = new System.Drawing.Point(352, 304);
			this.buttonStart.Name = "buttonStart";
			this.buttonStart.Size = new System.Drawing.Size(104, 24);
			this.buttonStart.TabIndex = 22;
			this.buttonStart.Text = "Start";
			this.buttonStart.UseVisualStyleBackColor = true;
			this.buttonStart.Click += new System.EventHandler(this.ButtonStartClick);
			// 
			// textboxRailGauge
			// 
			this.textboxRailGauge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textboxRailGauge.Location = new System.Drawing.Point(336, 176);
			this.textboxRailGauge.Name = "textboxRailGauge";
			this.textboxRailGauge.Size = new System.Drawing.Size(120, 20);
			this.textboxRailGauge.TabIndex = 16;
			this.textboxRailGauge.Enter += new System.EventHandler(this.TextboxRailGaugeEnter);
			// 
			// labelRailGauge
			// 
			this.labelRailGauge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelRailGauge.Location = new System.Drawing.Point(128, 176);
			this.labelRailGauge.Name = "labelRailGauge";
			this.labelRailGauge.Size = new System.Drawing.Size(208, 20);
			this.labelRailGauge.TabIndex = 15;
			this.labelRailGauge.Text = "Rail gauge (mm):";
			this.labelRailGauge.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textboxInitialCant
			// 
			this.textboxInitialCant.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textboxInitialCant.Location = new System.Drawing.Point(336, 200);
			this.textboxInitialCant.Name = "textboxInitialCant";
			this.textboxInitialCant.Size = new System.Drawing.Size(120, 20);
			this.textboxInitialCant.TabIndex = 18;
			this.textboxInitialCant.Enter += new System.EventHandler(this.TextboxInitialCantEnter);
			// 
			// labelInitialCant
			// 
			this.labelInitialCant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelInitialCant.Location = new System.Drawing.Point(128, 200);
			this.labelInitialCant.Name = "labelInitialCant";
			this.labelInitialCant.Size = new System.Drawing.Size(208, 20);
			this.labelInitialCant.TabIndex = 17;
			this.labelInitialCant.Text = "Initial cant (mm):";
			this.labelInitialCant.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textboxFinalCant
			// 
			this.textboxFinalCant.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textboxFinalCant.Location = new System.Drawing.Point(336, 224);
			this.textboxFinalCant.Name = "textboxFinalCant";
			this.textboxFinalCant.Size = new System.Drawing.Size(120, 20);
			this.textboxFinalCant.TabIndex = 20;
			this.textboxFinalCant.Enter += new System.EventHandler(this.TextboxFinalCantEnter);
			// 
			// labelFinalCant
			// 
			this.labelFinalCant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelFinalCant.Location = new System.Drawing.Point(128, 224);
			this.labelFinalCant.Name = "labelFinalCant";
			this.labelFinalCant.Size = new System.Drawing.Size(208, 20);
			this.labelFinalCant.TabIndex = 19;
			this.labelFinalCant.Text = "Final cant (mm):";
			this.labelFinalCant.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panelPane
			// 
			this.panelPane.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left)));
			this.panelPane.BackColor = System.Drawing.Color.Silver;
			this.panelPane.Controls.Add(this.labelLineHorizontal);
			this.panelPane.Controls.Add(this.pictureboxLogo);
			this.panelPane.Controls.Add(this.buttonClose);
			this.panelPane.Location = new System.Drawing.Point(0, 0);
			this.panelPane.Name = "panelPane";
			this.panelPane.Size = new System.Drawing.Size(120, 336);
			this.panelPane.TabIndex = 0;
			// 
			// labelLineHorizontal
			// 
			this.labelLineHorizontal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.labelLineHorizontal.BackColor = System.Drawing.Color.White;
			this.labelLineHorizontal.Location = new System.Drawing.Point(0, 75);
			this.labelLineHorizontal.Name = "labelLineHorizontal";
			this.labelLineHorizontal.Size = new System.Drawing.Size(120, 2);
			this.labelLineHorizontal.TabIndex = 0;
			// 
			// pictureboxLogo
			// 
			this.pictureboxLogo.BackColor = System.Drawing.Color.Silver;
			this.pictureboxLogo.Image = ((System.Drawing.Image)(resources.GetObject("pictureboxLogo.Image")));
			this.pictureboxLogo.Location = new System.Drawing.Point(0, 0);
			this.pictureboxLogo.Name = "pictureboxLogo";
			this.pictureboxLogo.Size = new System.Drawing.Size(120, 75);
			this.pictureboxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureboxLogo.TabIndex = 7;
			this.pictureboxLogo.TabStop = false;
			// 
			// MainForm
			// 
			this.AcceptButton = this.buttonStart;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonClose;
			this.ClientSize = new System.Drawing.Size(464, 336);
			this.Controls.Add(this.panelPane);
			this.Controls.Add(this.textboxFinalCant);
			this.Controls.Add(this.labelFinalCant);
			this.Controls.Add(this.textboxInitialCant);
			this.Controls.Add(this.labelInitialCant);
			this.Controls.Add(this.textboxRailGauge);
			this.Controls.Add(this.labelRailGauge);
			this.Controls.Add(this.buttonStart);
			this.Controls.Add(this.labelInformation);
			this.Controls.Add(this.textboxRadius);
			this.Controls.Add(this.labelRadius);
			this.Controls.Add(this.textboxBlockLength);
			this.Controls.Add(this.labelBlockLength);
			this.Controls.Add(this.textboxSegmentLength);
			this.Controls.Add(this.labelSegmentLength);
			this.Controls.Add(this.textboxNumberOfSegments);
			this.Controls.Add(this.labelNumberOfSegments);
			this.Controls.Add(this.buttonOutput);
			this.Controls.Add(this.textboxOutput);
			this.Controls.Add(this.labelOutput);
			this.Controls.Add(this.buttonInput);
			this.Controls.Add(this.textboxInput);
			this.Controls.Add(this.labelInput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ObjectBender";
			this.panelPane.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureboxLogo)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.PictureBox pictureboxLogo;
		private System.Windows.Forms.Label labelLineHorizontal;
		private System.Windows.Forms.Panel panelPane;
		private System.Windows.Forms.Label labelFinalCant;
		private System.Windows.Forms.TextBox textboxFinalCant;
		private System.Windows.Forms.Label labelInitialCant;
		private System.Windows.Forms.TextBox textboxInitialCant;
		private System.Windows.Forms.Label labelRailGauge;
		private System.Windows.Forms.TextBox textboxRailGauge;
		private System.Windows.Forms.Button buttonStart;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.Label labelInformation;
		private System.Windows.Forms.Label labelRadius;
		private System.Windows.Forms.TextBox textboxRadius;
		private System.Windows.Forms.Label labelBlockLength;
		private System.Windows.Forms.TextBox textboxBlockLength;
		private System.Windows.Forms.Label labelSegmentLength;
		private System.Windows.Forms.TextBox textboxSegmentLength;
		private System.Windows.Forms.Label labelNumberOfSegments;
		private System.Windows.Forms.TextBox textboxNumberOfSegments;
		private System.Windows.Forms.Label labelOutput;
		private System.Windows.Forms.TextBox textboxOutput;
		private System.Windows.Forms.Button buttonOutput;
		private System.Windows.Forms.Button buttonInput;
		private System.Windows.Forms.TextBox textboxInput;
		private System.Windows.Forms.Label labelInput;
	}
}
