namespace TrainEditor2.Views
{
	partial class FormDelay
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
			this.components = new System.ComponentModel.Container();
			this.buttonOK = new System.Windows.Forms.Button();
			this.listViewDelay = new System.Windows.Forms.ListView();
			this.columnHeaderNotch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderUp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderDown = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.textBoxUp = new System.Windows.Forms.TextBox();
			this.groupBoxEntry = new System.Windows.Forms.GroupBox();
			this.textBoxDown = new System.Windows.Forms.TextBox();
			this.labelDown = new System.Windows.Forms.Label();
			this.labelUp = new System.Windows.Forms.Label();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.groupBoxEntry.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(248, 192);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(56, 24);
			this.buttonOK.TabIndex = 40;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// listViewDelay
			// 
			this.listViewDelay.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNotch,
            this.columnHeaderUp,
            this.columnHeaderDown});
			this.listViewDelay.FullRowSelect = true;
			this.listViewDelay.HideSelection = false;
			this.listViewDelay.Location = new System.Drawing.Point(0, 0);
			this.listViewDelay.MultiSelect = false;
			this.listViewDelay.Name = "listViewDelay";
			this.listViewDelay.Size = new System.Drawing.Size(176, 184);
			this.listViewDelay.TabIndex = 41;
			this.listViewDelay.UseCompatibleStateImageBehavior = false;
			this.listViewDelay.View = System.Windows.Forms.View.Details;
			this.listViewDelay.SelectedIndexChanged += new System.EventHandler(this.ListViewDelay_SelectedIndexChanged);
			// 
			// columnHeaderNotch
			// 
			this.columnHeaderNotch.Text = "Notch";
			this.columnHeaderNotch.Width = 56;
			// 
			// columnHeaderUp
			// 
			this.columnHeaderUp.Text = "Up";
			this.columnHeaderUp.Width = 56;
			// 
			// columnHeaderDown
			// 
			this.columnHeaderDown.Text = "Down";
			// 
			// textBoxUp
			// 
			this.textBoxUp.Location = new System.Drawing.Point(56, 16);
			this.textBoxUp.Name = "textBoxUp";
			this.textBoxUp.Size = new System.Drawing.Size(56, 19);
			this.textBoxUp.TabIndex = 42;
			// 
			// groupBoxEntry
			// 
			this.groupBoxEntry.Controls.Add(this.textBoxDown);
			this.groupBoxEntry.Controls.Add(this.labelDown);
			this.groupBoxEntry.Controls.Add(this.labelUp);
			this.groupBoxEntry.Controls.Add(this.textBoxUp);
			this.groupBoxEntry.Location = new System.Drawing.Point(184, 112);
			this.groupBoxEntry.Name = "groupBoxEntry";
			this.groupBoxEntry.Size = new System.Drawing.Size(120, 72);
			this.groupBoxEntry.TabIndex = 44;
			this.groupBoxEntry.TabStop = false;
			this.groupBoxEntry.Text = "Edit entry";
			// 
			// textBoxDown
			// 
			this.textBoxDown.Location = new System.Drawing.Point(56, 40);
			this.textBoxDown.Name = "textBoxDown";
			this.textBoxDown.Size = new System.Drawing.Size(56, 19);
			this.textBoxDown.TabIndex = 45;
			// 
			// labelDown
			// 
			this.labelDown.Location = new System.Drawing.Point(8, 40);
			this.labelDown.Name = "labelDown";
			this.labelDown.Size = new System.Drawing.Size(40, 16);
			this.labelDown.TabIndex = 44;
			this.labelDown.Text = "Down:";
			this.labelDown.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelUp
			// 
			this.labelUp.Location = new System.Drawing.Point(8, 16);
			this.labelUp.Name = "labelUp";
			this.labelUp.Size = new System.Drawing.Size(40, 16);
			this.labelUp.TabIndex = 43;
			this.labelUp.Text = "Up:";
			this.labelUp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// FormDelay
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(314, 221);
			this.Controls.Add(this.groupBoxEntry);
			this.Controls.Add(this.listViewDelay);
			this.Controls.Add(this.buttonOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormDelay";
			this.Text = "Delay";
			this.Load += new System.EventHandler(this.FormDelay_Load);
			this.groupBoxEntry.ResumeLayout(false);
			this.groupBoxEntry.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.ListView listViewDelay;
		private System.Windows.Forms.ColumnHeader columnHeaderNotch;
		private System.Windows.Forms.ColumnHeader columnHeaderUp;
		private System.Windows.Forms.TextBox textBoxUp;
		private System.Windows.Forms.GroupBox groupBoxEntry;
		private System.Windows.Forms.ColumnHeader columnHeaderDown;
		private System.Windows.Forms.Label labelUp;
		private System.Windows.Forms.TextBox textBoxDown;
		private System.Windows.Forms.Label labelDown;
		private System.Windows.Forms.ErrorProvider errorProvider;
	}
}
