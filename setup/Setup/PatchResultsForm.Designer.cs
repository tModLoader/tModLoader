namespace Terraria.ModLoader.Setup
{
	partial class PatchResultsForm
	{
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.treeView = new System.Windows.Forms.TreeView();
			this.buttonOpenPatch = new System.Windows.Forms.Button();
			this.buttonOpenFile = new System.Windows.Forms.Button();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.patchDisplayPanel = new Terraria.ModLoader.Setup.DualPaneHTMLDisplay();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.appliedDisplayPanel = new Terraria.ModLoader.Setup.DualPaneHTMLDisplay();
			this.textBoxTitle = new System.Windows.Forms.TextBox();
			this.buttonAcceptPatch = new System.Windows.Forms.Button();
			this.buttonRepatchFile = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.buttonRepatchFile);
			this.splitContainer1.Panel2.Controls.Add(this.buttonAcceptPatch);
			this.splitContainer1.Panel2.Controls.Add(this.textBoxTitle);
			this.splitContainer1.Panel2.Controls.Add(this.buttonOpenPatch);
			this.splitContainer1.Panel2.Controls.Add(this.buttonOpenFile);
			this.splitContainer1.Panel2.Controls.Add(this.tabControl);
			this.splitContainer1.Size = new System.Drawing.Size(1004, 561);
			this.splitContainer1.SplitterDistance = 357;
			this.splitContainer1.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView.HideSelection = false;
			this.treeView.Location = new System.Drawing.Point(3, 3);
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(351, 555);
			this.treeView.TabIndex = 0;
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
			this.treeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseDoubleClick);
			// 
			// buttonOpenPatch
			// 
			this.buttonOpenPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonOpenPatch.Location = new System.Drawing.Point(93, 531);
			this.buttonOpenPatch.Name = "buttonOpenPatch";
			this.buttonOpenPatch.Size = new System.Drawing.Size(80, 23);
			this.buttonOpenPatch.TabIndex = 2;
			this.buttonOpenPatch.Text = "Open Patch";
			this.buttonOpenPatch.UseVisualStyleBackColor = true;
			this.buttonOpenPatch.Click += new System.EventHandler(this.buttonOpenPatch_Click);
			// 
			// buttonOpenFile
			// 
			this.buttonOpenFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonOpenFile.Location = new System.Drawing.Point(7, 531);
			this.buttonOpenFile.Name = "buttonOpenFile";
			this.buttonOpenFile.Size = new System.Drawing.Size(80, 23);
			this.buttonOpenFile.TabIndex = 1;
			this.buttonOpenFile.Text = "Open Files";
			this.buttonOpenFile.UseVisualStyleBackColor = true;
			this.buttonOpenFile.Click += new System.EventHandler(this.buttonOpenFile_Click);
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tabPage1);
			this.tabControl.Controls.Add(this.tabPage2);
			this.tabControl.Location = new System.Drawing.Point(3, 30);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(637, 495);
			this.tabControl.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.patchDisplayPanel);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(629, 496);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Patch";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// patchDisplayPanel
			// 
			this.patchDisplayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.patchDisplayPanel.Location = new System.Drawing.Point(3, 3);
			this.patchDisplayPanel.Name = "patchDisplayPanel";
			this.patchDisplayPanel.Size = new System.Drawing.Size(623, 490);
			this.patchDisplayPanel.TabIndex = 0;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.appliedDisplayPanel);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(629, 469);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Applied";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// appliedDisplayPanel
			// 
			this.appliedDisplayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.appliedDisplayPanel.Location = new System.Drawing.Point(3, 3);
			this.appliedDisplayPanel.Name = "appliedDisplayPanel";
			this.appliedDisplayPanel.Size = new System.Drawing.Size(623, 463);
			this.appliedDisplayPanel.TabIndex = 0;
			// 
			// textBoxTitle
			// 
			this.textBoxTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxTitle.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxTitle.Location = new System.Drawing.Point(3, 4);
			this.textBoxTitle.Name = "textBoxTitle";
			this.textBoxTitle.ReadOnly = true;
			this.textBoxTitle.Size = new System.Drawing.Size(633, 24);
			this.textBoxTitle.TabIndex = 3;
			this.textBoxTitle.Text = "Title";
			this.textBoxTitle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// buttonAcceptPatch
			// 
			this.buttonAcceptPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonAcceptPatch.Location = new System.Drawing.Point(179, 531);
			this.buttonAcceptPatch.Name = "buttonAcceptPatch";
			this.buttonAcceptPatch.Size = new System.Drawing.Size(80, 23);
			this.buttonAcceptPatch.TabIndex = 4;
			this.buttonAcceptPatch.Text = "Accept Patch";
			this.buttonAcceptPatch.UseVisualStyleBackColor = true;
			// 
			// buttonRepatchFile
			// 
			this.buttonRepatchFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonRepatchFile.Location = new System.Drawing.Point(265, 531);
			this.buttonRepatchFile.Name = "buttonRepatchFile";
			this.buttonRepatchFile.Size = new System.Drawing.Size(80, 23);
			this.buttonRepatchFile.TabIndex = 5;
			this.buttonRepatchFile.Text = "Repatch File";
			this.buttonRepatchFile.UseVisualStyleBackColor = true;
			// 
			// PatchResultsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1004, 561);
			this.Controls.Add(this.splitContainer1);
			this.Name = "PatchResultsForm";
			this.Text = "Patch Results";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Button buttonOpenPatch;
		private System.Windows.Forms.Button buttonOpenFile;
		private DualPaneHTMLDisplay patchDisplayPanel;
		private DualPaneHTMLDisplay appliedDisplayPanel;
		private System.Windows.Forms.TextBox textBoxTitle;
		private System.Windows.Forms.Button buttonRepatchFile;
		private System.Windows.Forms.Button buttonAcceptPatch;
	}
}