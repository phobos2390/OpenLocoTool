﻿namespace OpenLocoToolGui
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			treeView1 = new TreeView();
			lbLogs = new ListBox();
			btnSaveChanges = new Button();
			pgObject = new PropertyGrid();
			btnSetDirectory = new Button();
			folderBrowserDialog1 = new FolderBrowserDialog();
			tbFileFilter = new TextBox();
			lblFilenameRegex = new Label();
			saveFileDialog1 = new SaveFileDialog();
			SuspendLayout();
			// 
			// treeView1
			// 
			treeView1.Location = new Point(12, 70);
			treeView1.Name = "treeView1";
			treeView1.Size = new Size(231, 649);
			treeView1.TabIndex = 1;
			treeView1.AfterSelect += treeView1_AfterSelect;
			// 
			// lbLogs
			// 
			lbLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			lbLogs.FormattingEnabled = true;
			lbLogs.HorizontalScrollbar = true;
			lbLogs.ItemHeight = 15;
			lbLogs.Location = new Point(12, 725);
			lbLogs.Name = "lbLogs";
			lbLogs.SelectionMode = SelectionMode.None;
			lbLogs.Size = new Size(680, 124);
			lbLogs.TabIndex = 17;
			// 
			// btnSaveChanges
			// 
			btnSaveChanges.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnSaveChanges.Location = new Point(249, 12);
			btnSaveChanges.Name = "btnSaveChanges";
			btnSaveChanges.Size = new Size(443, 52);
			btnSaveChanges.TabIndex = 18;
			btnSaveChanges.Text = "Save Changes";
			btnSaveChanges.UseVisualStyleBackColor = true;
			btnSaveChanges.Click += btnSaveChanges_Click;
			// 
			// pgObject
			// 
			pgObject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			pgObject.HelpVisible = false;
			pgObject.Location = new Point(249, 70);
			pgObject.Name = "pgObject";
			pgObject.Size = new Size(443, 649);
			pgObject.TabIndex = 22;
			pgObject.ToolbarVisible = false;
			// 
			// btnSetDirectory
			// 
			btnSetDirectory.Location = new Point(12, 12);
			btnSetDirectory.Name = "btnSetDirectory";
			btnSetDirectory.Size = new Size(231, 23);
			btnSetDirectory.TabIndex = 23;
			btnSetDirectory.Text = "Set ObjData Directory";
			btnSetDirectory.UseVisualStyleBackColor = true;
			btnSetDirectory.Click += btnSetDirectory_Click;
			// 
			// tbFileFilter
			// 
			tbFileFilter.BorderStyle = BorderStyle.FixedSingle;
			tbFileFilter.Location = new Point(80, 41);
			tbFileFilter.Name = "tbFileFilter";
			tbFileFilter.Size = new Size(163, 23);
			tbFileFilter.TabIndex = 24;
			tbFileFilter.TextChanged += tbFileFilter_TextChanged;
			// 
			// lblFilenameRegex
			// 
			lblFilenameRegex.BorderStyle = BorderStyle.FixedSingle;
			lblFilenameRegex.Location = new Point(12, 41);
			lblFilenameRegex.Name = "lblFilenameRegex";
			lblFilenameRegex.Size = new Size(62, 23);
			lblFilenameRegex.TabIndex = 25;
			lblFilenameRegex.Text = "Filter";
			lblFilenameRegex.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(704, 861);
			Controls.Add(lblFilenameRegex);
			Controls.Add(tbFileFilter);
			Controls.Add(btnSetDirectory);
			Controls.Add(pgObject);
			Controls.Add(btnSaveChanges);
			Controls.Add(lbLogs);
			Controls.Add(treeView1);
			Name = "MainForm";
			Text = "OpenLocoTool";
			Load += MainForm_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private TreeView treeView1;
		private ListBox lbLogs;
		private Button btnSaveChanges;
		private PropertyGrid pgObject;
		private Button btnSetDirectory;
		private FolderBrowserDialog folderBrowserDialog1;
		private TextBox tbFileFilter;
		private Label lblFilenameRegex;
		private SaveFileDialog saveFileDialog1;
	}
}