using System;

namespace Terraria.ModLoader.Setup
{
    partial class MainForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.labelStatus = new System.Windows.Forms.Label();
			this.buttonSetup = new System.Windows.Forms.Button();
			this.buttonDecompile = new System.Windows.Forms.Button();
			this.buttonDiffTerraria = new System.Windows.Forms.Button();
			this.buttonPatchTerraria = new System.Windows.Forms.Button();
			this.buttonPatchModLoader = new System.Windows.Forms.Button();
			this.buttonDiffModLoader = new System.Windows.Forms.Button();
			this.toolTipButtons = new System.Windows.Forms.ToolTip(this.components);
			this.buttonRegenSource = new System.Windows.Forms.Button();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.menuItemOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemTerraria = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemTmlPath = new System.Windows.Forms.ToolStripMenuItem();
			this.resetTimeStampOptimizationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.formatDecompiledOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.decompileServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.formatCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.hookGenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.simplifierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exactToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.offsetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fuzzyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonSetupDebugging = new System.Windows.Forms.Button();
			this.mainMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Enabled = false;
			this.buttonCancel.Location = new System.Drawing.Point(158, 388);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(96, 27);
			this.buttonCancel.TabIndex = 6;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(14, 354);
			this.progressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(379, 27);
			this.progressBar.TabIndex = 1;
			// 
			// labelStatus
			// 
			this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.labelStatus.Location = new System.Drawing.Point(14, 212);
			this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(377, 138);
			this.labelStatus.TabIndex = 3;
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// buttonSetup
			// 
			this.buttonSetup.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonSetup.Location = new System.Drawing.Point(52, 48);
			this.buttonSetup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonSetup.Name = "buttonSetup";
			this.buttonSetup.Size = new System.Drawing.Size(150, 27);
			this.buttonSetup.TabIndex = 0;
			this.buttonSetup.Text = "Setup";
			this.toolTipButtons.SetToolTip(this.buttonSetup, "Complete environment setup for working on tModLoader source\r\nEquivalent to Decomp" +
        "ile+Patch+SetupDebug\r\nEdit the source in src/tModLoader then run Diff tModLoader" +
        " and commit the /patches folder");
			this.buttonSetup.UseVisualStyleBackColor = true;
			this.buttonSetup.Click += new System.EventHandler(this.buttonTask_Click);
			// 
			// buttonDecompile
			// 
			this.buttonDecompile.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonDecompile.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonDecompile.Location = new System.Drawing.Point(210, 48);
			this.buttonDecompile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonDecompile.Name = "buttonDecompile";
			this.buttonDecompile.Size = new System.Drawing.Size(150, 27);
			this.buttonDecompile.TabIndex = 1;
			this.buttonDecompile.Text = "Decompile";
			this.toolTipButtons.SetToolTip(this.buttonDecompile, "Uses ILSpy to decompile Terraria\r\nAlso decompiles server classes not included in " +
        "the client binary\r\nOutputs to src/decompiled");
			this.buttonDecompile.UseVisualStyleBackColor = true;
			this.buttonDecompile.Click += new System.EventHandler(this.buttonTask_Click);
			// 
			// buttonDiffTerraria
			// 
			this.buttonDiffTerraria.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonDiffTerraria.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonDiffTerraria.Location = new System.Drawing.Point(52, 82);
			this.buttonDiffTerraria.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonDiffTerraria.Name = "buttonDiffTerraria";
			this.buttonDiffTerraria.Size = new System.Drawing.Size(150, 27);
			this.buttonDiffTerraria.TabIndex = 4;
			this.buttonDiffTerraria.Text = "Diff Terraria";
			this.toolTipButtons.SetToolTip(this.buttonDiffTerraria, "Recalculates the Terraria patches\r\nDiffs the src/Terraria directory\r\nUsed for fix" +
        "ing decompilation errors\r\n");
			this.buttonDiffTerraria.UseVisualStyleBackColor = true;
			this.buttonDiffTerraria.Click += new System.EventHandler(this.buttonTask_Click);
			// 
			// buttonPatchTerraria
			// 
			this.buttonPatchTerraria.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonPatchTerraria.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonPatchTerraria.Location = new System.Drawing.Point(210, 82);
			this.buttonPatchTerraria.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonPatchTerraria.Name = "buttonPatchTerraria";
			this.buttonPatchTerraria.Size = new System.Drawing.Size(150, 27);
			this.buttonPatchTerraria.TabIndex = 2;
			this.buttonPatchTerraria.Text = "Patch Terraria";
			this.toolTipButtons.SetToolTip(this.buttonPatchTerraria, "Applies patches to fix decompile errors\r\nLeaves functionality unchanged\r\nPatched " +
        "source is located in src/Terraria");
			this.buttonPatchTerraria.UseVisualStyleBackColor = true;
			this.buttonPatchTerraria.Click += new System.EventHandler(this.buttonTask_Click);
			// 
			// buttonPatchModLoader
			// 
			this.buttonPatchModLoader.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonPatchModLoader.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonPatchModLoader.Location = new System.Drawing.Point(210, 115);
			this.buttonPatchModLoader.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonPatchModLoader.Name = "buttonPatchModLoader";
			this.buttonPatchModLoader.Size = new System.Drawing.Size(150, 27);
			this.buttonPatchModLoader.TabIndex = 3;
			this.buttonPatchModLoader.Text = "Patch tModLoader";
			this.toolTipButtons.SetToolTip(this.buttonPatchModLoader, "Applies tModLoader patches to Terraria\r\nEdit the source code in src/tModLoader af" +
        "ter this phase\r\nInternally formats the Terraria sources before patching");
			this.buttonPatchModLoader.UseVisualStyleBackColor = true;
			this.buttonPatchModLoader.Click += new System.EventHandler(this.buttonTask_Click);
			// 
			// buttonDiffModLoader
			// 
			this.buttonDiffModLoader.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonDiffModLoader.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonDiffModLoader.Location = new System.Drawing.Point(52, 115);
			this.buttonDiffModLoader.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonDiffModLoader.Name = "buttonDiffModLoader";
			this.buttonDiffModLoader.Size = new System.Drawing.Size(150, 27);
			this.buttonDiffModLoader.TabIndex = 5;
			this.buttonDiffModLoader.Text = "Diff tModLoader";
			this.toolTipButtons.SetToolTip(this.buttonDiffModLoader, resources.GetString("buttonDiffModLoader.ToolTip"));
			this.buttonDiffModLoader.UseVisualStyleBackColor = true;
			this.buttonDiffModLoader.Click += new System.EventHandler(this.buttonTask_Click);
			// 
			// toolTipButtons
			// 
			this.toolTipButtons.AutomaticDelay = 200;
			this.toolTipButtons.AutoPopDelay = 0;
			this.toolTipButtons.InitialDelay = 200;
			this.toolTipButtons.ReshowDelay = 40;
			this.toolTipButtons.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTipButtons_Popup);
			// 
			// buttonRegenSource
			// 
			this.buttonRegenSource.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonRegenSource.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonRegenSource.Location = new System.Drawing.Point(210, 149);
			this.buttonRegenSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonRegenSource.Name = "buttonRegenSource";
			this.buttonRegenSource.Size = new System.Drawing.Size(150, 27);
			this.buttonRegenSource.TabIndex = 3;
			this.buttonRegenSource.Text = "Regenerate Source";
			this.toolTipButtons.SetToolTip(this.buttonRegenSource, "Regenerates all the source files\r\nUse this after pulling from the repo\r\nEquivalen" +
        "t to Setup without Decompile");
			this.buttonRegenSource.UseVisualStyleBackColor = true;
			this.buttonRegenSource.Click += new System.EventHandler(this.buttonTask_Click);
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemOptions,
            this.toolsToolStripMenuItem,
            this.patchToolStripMenuItem});
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
			this.mainMenuStrip.Size = new System.Drawing.Size(407, 24);
			this.mainMenuStrip.TabIndex = 9;
			this.mainMenuStrip.Text = "menuStrip1";
			this.mainMenuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.mainMenuStrip_ItemClicked);
			// 
			// menuItemOptions
			// 
			this.menuItemOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemTerraria,
            this.menuItemTmlPath,
            this.resetTimeStampOptimizationsToolStripMenuItem,
            this.formatDecompiledOutputToolStripMenuItem});
			this.menuItemOptions.Name = "menuItemOptions";
			this.menuItemOptions.Size = new System.Drawing.Size(61, 20);
			this.menuItemOptions.Text = "Options";
			// 
			// menuItemTerraria
			// 
			this.menuItemTerraria.Name = "menuItemTerraria";
			this.menuItemTerraria.Size = new System.Drawing.Size(268, 22);
			this.menuItemTerraria.Text = "Select Terraria";
			this.menuItemTerraria.Click += new System.EventHandler(this.menuItemTerraria_Click);
			// 
			// menuItemTmlPath
			// 
			this.menuItemTmlPath.Name = "menuItemTmlPath";
			this.menuItemTmlPath.Size = new System.Drawing.Size(268, 22);
			this.menuItemTmlPath.Text = "Select Custom TML Output Directory";
			this.menuItemTmlPath.Click += new System.EventHandler(this.menuItemTmlPath_Click);
			// 
			// resetTimeStampOptimizationsToolStripMenuItem
			// 
			this.resetTimeStampOptimizationsToolStripMenuItem.Name = "resetTimeStampOptimizationsToolStripMenuItem";
			this.resetTimeStampOptimizationsToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
			this.resetTimeStampOptimizationsToolStripMenuItem.Text = "Reset TimeStamp Optimizations";
			this.resetTimeStampOptimizationsToolStripMenuItem.Click += new System.EventHandler(this.menuItemResetTimeStampOptmizations_Click);
			// 
			// formatDecompiledOutputToolStripMenuItem
			// 
			this.formatDecompiledOutputToolStripMenuItem.Name = "formatDecompiledOutputToolStripMenuItem";
			this.formatDecompiledOutputToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
			this.formatDecompiledOutputToolStripMenuItem.Text = "Format Decompiled Output";
			this.formatDecompiledOutputToolStripMenuItem.Click += new System.EventHandler(this.formatDecompiledOutputToolStripMenuItem_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.decompileServerToolStripMenuItem,
            this.formatCodeToolStripMenuItem,
            this.hookGenToolStripMenuItem,
            this.simplifierToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
			this.toolsToolStripMenuItem.Text = "Tools";
			// 
			// decompileServerToolStripMenuItem
			// 
			this.decompileServerToolStripMenuItem.Name = "decompileServerToolStripMenuItem";
			this.decompileServerToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
			this.decompileServerToolStripMenuItem.Text = "Decompile Server";
			this.decompileServerToolStripMenuItem.Click += new System.EventHandler(this.menuItemDecompileServer_Click);
			// 
			// formatCodeToolStripMenuItem
			// 
			this.formatCodeToolStripMenuItem.Name = "formatCodeToolStripMenuItem";
			this.formatCodeToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
			this.formatCodeToolStripMenuItem.Text = "Formatter";
			this.formatCodeToolStripMenuItem.Click += new System.EventHandler(this.menuItemFormatCode_Click);
			// 
			// hookGenToolStripMenuItem
			// 
			this.hookGenToolStripMenuItem.Name = "hookGenToolStripMenuItem";
			this.hookGenToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
			this.hookGenToolStripMenuItem.Text = "HookGen";
			this.hookGenToolStripMenuItem.Click += new System.EventHandler(this.menuItemHookGen_Click);
			// 
			// simplifierToolStripMenuItem
			// 
			this.simplifierToolStripMenuItem.Name = "simplifierToolStripMenuItem";
			this.simplifierToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
			this.simplifierToolStripMenuItem.Text = "Simplifier";
			this.simplifierToolStripMenuItem.Click += new System.EventHandler(this.simplifierToolStripMenuItem_Click);
			// 
			// patchToolStripMenuItem
			// 
			this.patchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exactToolStripMenuItem,
            this.offsetToolStripMenuItem,
            this.fuzzyToolStripMenuItem});
			this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
			this.patchToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
			this.patchToolStripMenuItem.Text = "Patch";
			// 
			// exactToolStripMenuItem
			// 
			this.exactToolStripMenuItem.Name = "exactToolStripMenuItem";
			this.exactToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
			this.exactToolStripMenuItem.Text = "Exact";
			this.exactToolStripMenuItem.Click += new System.EventHandler(this.exactToolStripMenuItem_Click);
			// 
			// offsetToolStripMenuItem
			// 
			this.offsetToolStripMenuItem.Name = "offsetToolStripMenuItem";
			this.offsetToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
			this.offsetToolStripMenuItem.Text = "Offset";
			this.offsetToolStripMenuItem.Click += new System.EventHandler(this.offsetToolStripMenuItem_Click);
			// 
			// fuzzyToolStripMenuItem
			// 
			this.fuzzyToolStripMenuItem.Name = "fuzzyToolStripMenuItem";
			this.fuzzyToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
			this.fuzzyToolStripMenuItem.Text = "Fuzzy";
			this.fuzzyToolStripMenuItem.Click += new System.EventHandler(this.fuzzyToolStripMenuItem_Click);
			// 
			// buttonSetupDebugging
			// 
			this.buttonSetupDebugging.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonSetupDebugging.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonSetupDebugging.Location = new System.Drawing.Point(52, 149);
			this.buttonSetupDebugging.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.buttonSetupDebugging.Name = "buttonSetupDebugging";
			this.buttonSetupDebugging.Size = new System.Drawing.Size(150, 27);
			this.buttonSetupDebugging.TabIndex = 3;
			this.buttonSetupDebugging.Text = "Setup Debugging";
			this.buttonSetupDebugging.UseVisualStyleBackColor = true;
			this.buttonSetupDebugging.Click += new System.EventHandler(this.buttonTask_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(407, 428);
			this.Controls.Add(this.buttonDiffModLoader);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.buttonDiffTerraria);
			this.Controls.Add(this.buttonSetupDebugging);
			this.Controls.Add(this.buttonRegenSource);
			this.Controls.Add(this.buttonPatchModLoader);
			this.Controls.Add(this.buttonPatchTerraria);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonDecompile);
			this.Controls.Add(this.buttonSetup);
			this.Controls.Add(this.mainMenuStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.mainMenuStrip;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "MainForm";
			this.Text = "tModLoader Dev Setup";
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonSetup;
        private System.Windows.Forms.Button buttonDecompile;
        private System.Windows.Forms.Button buttonDiffTerraria;
        private System.Windows.Forms.Button buttonPatchTerraria;
        private System.Windows.Forms.Button buttonPatchModLoader;
        private System.Windows.Forms.Button buttonDiffModLoader;
        private System.Windows.Forms.ToolTip toolTipButtons;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuItemOptions;
        private System.Windows.Forms.ToolStripMenuItem menuItemTerraria;
		private System.Windows.Forms.ToolStripMenuItem resetTimeStampOptimizationsToolStripMenuItem;
        private System.Windows.Forms.Button buttonRegenSource;
        private System.Windows.Forms.Button buttonSetupDebugging;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem decompileServerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem formatCodeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem hookGenToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem patchToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exactToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem offsetToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fuzzyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem simplifierToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem formatDecompiledOutputToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem menuItemTmlPath;
	}
}

