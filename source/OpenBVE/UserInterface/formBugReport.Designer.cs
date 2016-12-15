namespace OpenBve
{
	partial class formBugReport
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formBugReport));
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxProblemDescription = new System.Windows.Forms.TextBox();
			this.labelViewLog = new System.Windows.Forms.Label();
			this.buttonViewLog = new System.Windows.Forms.Button();
			this.buttonViewCrashLog = new System.Windows.Forms.Button();
			this.labelViewCrash = new System.Windows.Forms.Label();
			this.textBoxReportLabel = new System.Windows.Forms.TextBox();
			this.buttonReportProblem = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 134);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(273, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please enter a brief description of the problem:";
			// 
			// textBoxProblemDescription
			// 
			this.textBoxProblemDescription.Location = new System.Drawing.Point(15, 150);
			this.textBoxProblemDescription.Multiline = true;
			this.textBoxProblemDescription.Name = "textBoxProblemDescription";
			this.textBoxProblemDescription.Size = new System.Drawing.Size(458, 183);
			this.textBoxProblemDescription.TabIndex = 1;
			// 
			// labelViewLog
			// 
			this.labelViewLog.AutoSize = true;
			this.labelViewLog.Location = new System.Drawing.Point(12, 86);
			this.labelViewLog.Name = "labelViewLog";
			this.labelViewLog.Size = new System.Drawing.Size(111, 13);
			this.labelViewLog.TabIndex = 2;
			this.labelViewLog.Text = "View the previous log:";
			// 
			// buttonViewLog
			// 
			this.buttonViewLog.Location = new System.Drawing.Point(360, 81);
			this.buttonViewLog.Name = "buttonViewLog";
			this.buttonViewLog.Size = new System.Drawing.Size(113, 23);
			this.buttonViewLog.TabIndex = 3;
			this.buttonViewLog.Text = "Click...";
			this.buttonViewLog.UseVisualStyleBackColor = true;
			this.buttonViewLog.Click += new System.EventHandler(this.buttonViewLog_Click);
			// 
			// buttonViewCrashLog
			// 
			this.buttonViewCrashLog.Location = new System.Drawing.Point(360, 108);
			this.buttonViewCrashLog.Name = "buttonViewCrashLog";
			this.buttonViewCrashLog.Size = new System.Drawing.Size(113, 23);
			this.buttonViewCrashLog.TabIndex = 5;
			this.buttonViewCrashLog.Text = "Click...";
			this.buttonViewCrashLog.UseVisualStyleBackColor = true;
			this.buttonViewCrashLog.Click += new System.EventHandler(this.buttonViewCrashLog_Click);
			// 
			// labelViewCrash
			// 
			this.labelViewCrash.AutoSize = true;
			this.labelViewCrash.Location = new System.Drawing.Point(12, 113);
			this.labelViewCrash.Name = "labelViewCrash";
			this.labelViewCrash.Size = new System.Drawing.Size(140, 13);
			this.labelViewCrash.TabIndex = 4;
			this.labelViewCrash.Text = "View the previous crash log:";
			// 
			// textBoxReportLabel
			// 
			this.textBoxReportLabel.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxReportLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBoxReportLabel.Location = new System.Drawing.Point(17, 6);
			this.textBoxReportLabel.Multiline = true;
			this.textBoxReportLabel.Name = "textBoxReportLabel";
			this.textBoxReportLabel.ReadOnly = true;
			this.textBoxReportLabel.Size = new System.Drawing.Size(455, 68);
			this.textBoxReportLabel.TabIndex = 6;
			this.textBoxReportLabel.Text = resources.GetString("textBoxReportLabel.Text");
			// 
			// buttonReportProblem
			// 
			this.buttonReportProblem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonReportProblem.Location = new System.Drawing.Point(330, 339);
			this.buttonReportProblem.Name = "buttonReportProblem";
			this.buttonReportProblem.Size = new System.Drawing.Size(142, 36);
			this.buttonReportProblem.TabIndex = 7;
			this.buttonReportProblem.Text = "Report Problem";
			this.buttonReportProblem.UseVisualStyleBackColor = true;
			this.buttonReportProblem.Click += new System.EventHandler(this.buttonReportProblem_Click);
			// 
			// formBugReport
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 387);
			this.Controls.Add(this.buttonReportProblem);
			this.Controls.Add(this.textBoxReportLabel);
			this.Controls.Add(this.buttonViewCrashLog);
			this.Controls.Add(this.labelViewCrash);
			this.Controls.Add(this.buttonViewLog);
			this.Controls.Add(this.labelViewLog);
			this.Controls.Add(this.textBoxProblemDescription);
			this.Controls.Add(this.label1);
			this.Name = "formBugReport";
			this.Text = "Report a Problem....";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxProblemDescription;
		private System.Windows.Forms.Label labelViewLog;
		private System.Windows.Forms.Button buttonViewLog;
		private System.Windows.Forms.Button buttonViewCrashLog;
		private System.Windows.Forms.Label labelViewCrash;
		private System.Windows.Forms.TextBox textBoxReportLabel;
		private System.Windows.Forms.Button buttonReportProblem;
	}
}