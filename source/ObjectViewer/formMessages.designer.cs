namespace OpenBve {
    partial class formMessages {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
        	this.buttonClose = new System.Windows.Forms.Button();
        	this.listviewMessages = new System.Windows.Forms.ListView();
        	this.columnheaderType = new System.Windows.Forms.ColumnHeader();
        	this.columnheaderDescription = new System.Windows.Forms.ColumnHeader();
        	this.buttonSave = new System.Windows.Forms.Button();
        	this.SuspendLayout();
        	// 
        	// buttonClose
        	// 
        	this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        	this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
        	this.buttonClose.Location = new System.Drawing.Point(488, 224);
        	this.buttonClose.Name = "buttonClose";
        	this.buttonClose.Size = new System.Drawing.Size(96, 24);
        	this.buttonClose.TabIndex = 2;
        	this.buttonClose.Text = "Close";
        	this.buttonClose.UseVisualStyleBackColor = true;
        	this.buttonClose.Click += new System.EventHandler(this.buttonCancel_Click);
        	// 
        	// listviewMessages
        	// 
        	this.listviewMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        	        	        	| System.Windows.Forms.AnchorStyles.Left) 
        	        	        	| System.Windows.Forms.AnchorStyles.Right)));
        	this.listviewMessages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        	        	        	this.columnheaderType,
        	        	        	this.columnheaderDescription});
        	this.listviewMessages.FullRowSelect = true;
        	this.listviewMessages.GridLines = true;
        	this.listviewMessages.Location = new System.Drawing.Point(8, 8);
        	this.listviewMessages.MultiSelect = false;
        	this.listviewMessages.Name = "listviewMessages";
        	this.listviewMessages.Size = new System.Drawing.Size(576, 208);
        	this.listviewMessages.TabIndex = 0;
        	this.listviewMessages.UseCompatibleStateImageBehavior = false;
        	this.listviewMessages.View = System.Windows.Forms.View.Details;
        	// 
        	// columnheaderType
        	// 
        	this.columnheaderType.Text = "Type";
        	// 
        	// columnheaderDescription
        	// 
        	this.columnheaderDescription.Text = "Description";
        	// 
        	// buttonSave
        	// 
        	this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        	this.buttonSave.BackColor = System.Drawing.SystemColors.ButtonFace;
        	this.buttonSave.Location = new System.Drawing.Point(8, 224);
        	this.buttonSave.Name = "buttonSave";
        	this.buttonSave.Size = new System.Drawing.Size(96, 24);
        	this.buttonSave.TabIndex = 1;
        	this.buttonSave.Text = "Save report...";
        	this.buttonSave.UseVisualStyleBackColor = true;
        	this.buttonSave.Click += new System.EventHandler(this.ButtonSaveClick);
        	// 
        	// formMessages
        	// 
        	this.AcceptButton = this.buttonClose;
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.CancelButton = this.buttonClose;
        	this.ClientSize = new System.Drawing.Size(592, 256);
        	this.Controls.Add(this.buttonSave);
        	this.Controls.Add(this.listviewMessages);
        	this.Controls.Add(this.buttonClose);
        	this.MinimizeBox = false;
        	this.Name = "formMessages";
        	this.ShowIcon = false;
        	this.ShowInTaskbar = false;
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "Messages";
        	this.Shown += new System.EventHandler(this.formMessages_Shown);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Button buttonSave;

        #endregion

        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.ListView listviewMessages;
        private System.Windows.Forms.ColumnHeader columnheaderType;
        private System.Windows.Forms.ColumnHeader columnheaderDescription;
    }
}