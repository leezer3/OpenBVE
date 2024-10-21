
namespace RouteViewer
{
	partial class formRailPaths
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
            this.dataGridViewPaths = new System.Windows.Forms.DataGridView();
            this.railIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.railDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.railColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.railVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnDisplay = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonClose = new System.Windows.Forms.Button();
            this.checkBoxRenderPaths = new System.Windows.Forms.CheckBox();
            this.buttonSelectNone = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPaths)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewPaths
            // 
            this.dataGridViewPaths.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPaths.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.railIndex,
            this.railDescription,
            this.railColor,
            this.railVisible,
            this.columnDisplay,
            this.key});
            this.dataGridViewPaths.Location = new System.Drawing.Point(13, 13);
            this.dataGridViewPaths.Name = "dataGridViewPaths";
            this.dataGridViewPaths.Size = new System.Drawing.Size(651, 396);
            this.dataGridViewPaths.TabIndex = 0;
            this.dataGridViewPaths.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewPaths_CellMouseUp);
            this.dataGridViewPaths.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewPaths_CellValidated);
            this.dataGridViewPaths.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewPaths_CurrentCellDirtyStateChanged);
            // 
            // railIndex
            // 
            this.railIndex.HeaderText = "Rail Index";
            this.railIndex.Name = "railIndex";
            this.railIndex.ReadOnly = true;
            this.railIndex.Width = 79;
            // 
            // railDescription
            // 
            this.railDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.railDescription.HeaderText = "Description";
            this.railDescription.Name = "railDescription";
            // 
            // railColor
            // 
            this.railColor.HeaderText = "Color";
            this.railColor.Name = "railColor";
            this.railColor.ReadOnly = true;
            this.railColor.Width = 56;
            // 
            // railVisible
            // 
            this.railVisible.HeaderText = "Currently Visible";
            this.railVisible.Name = "railVisible";
            this.railVisible.ReadOnly = true;
            this.railVisible.Width = 78;
            // 
            // columnDisplay
            // 
            this.columnDisplay.HeaderText = "Display";
            this.columnDisplay.Name = "columnDisplay";
            this.columnDisplay.Width = 47;
            // 
            // key
            // 
            this.key.HeaderText = "key";
            this.key.Name = "key";
            this.key.Visible = false;
            this.key.Width = 49;
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(589, 415);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // checkBoxRenderPaths
            // 
            this.checkBoxRenderPaths.AutoSize = true;
            this.checkBoxRenderPaths.Location = new System.Drawing.Point(13, 421);
            this.checkBoxRenderPaths.Name = "checkBoxRenderPaths";
            this.checkBoxRenderPaths.Size = new System.Drawing.Size(164, 17);
            this.checkBoxRenderPaths.TabIndex = 2;
            this.checkBoxRenderPaths.Text = "Enable Drawing of Rail Paths";
            this.checkBoxRenderPaths.UseVisualStyleBackColor = true;
            this.checkBoxRenderPaths.CheckedChanged += new System.EventHandler(this.checkBoxRenderPaths_CheckedChanged);
            // 
            // buttonSelectNone
            // 
            this.buttonSelectNone.Location = new System.Drawing.Point(508, 415);
            this.buttonSelectNone.Name = "buttonSelectNone";
            this.buttonSelectNone.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectNone.TabIndex = 3;
            this.buttonSelectNone.Text = "Select None";
            this.buttonSelectNone.UseVisualStyleBackColor = true;
            this.buttonSelectNone.Click += new System.EventHandler(this.buttonSelectNone_Click);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Location = new System.Drawing.Point(427, 415);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAll.TabIndex = 4;
            this.buttonSelectAll.Text = "Select All";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // formRailPaths
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 450);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.buttonSelectNone);
            this.Controls.Add(this.checkBoxRenderPaths);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.dataGridViewPaths);
            this.Name = "formRailPaths";
            this.Text = "Rail Paths";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPaths)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridViewPaths;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.DataGridViewTextBoxColumn railIndex;
		private System.Windows.Forms.DataGridViewTextBoxColumn railDescription;
		private System.Windows.Forms.DataGridViewTextBoxColumn railColor;
		private System.Windows.Forms.DataGridViewCheckBoxColumn railVisible;
		private System.Windows.Forms.DataGridViewCheckBoxColumn columnDisplay;
		private System.Windows.Forms.DataGridViewTextBoxColumn key;
		private System.Windows.Forms.CheckBox checkBoxRenderPaths;
		private System.Windows.Forms.Button buttonSelectNone;
		private System.Windows.Forms.Button buttonSelectAll;
	}
}
