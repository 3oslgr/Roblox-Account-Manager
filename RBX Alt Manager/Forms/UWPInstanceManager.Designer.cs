namespace RBX_Alt_Manager.Forms
{
    partial class UWPInstanceManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UWPInstanceManager));
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.PoweredByLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.InfoLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.InfoLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ImportButton = new System.Windows.Forms.ToolStripButton();
            this.RefreshInstancesButton = new System.Windows.Forms.ToolStripButton();
            this.UninstallSelectedButton = new System.Windows.Forms.ToolStripSplitButton();
            this.UninstallAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AttemptLoginButton = new System.Windows.Forms.ToolStripButton();
            this.InstanceListBox = new BrightIdeasSoftware.ObjectListView();
            this.UserColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.PackageColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.UwpExeDialog = new System.Windows.Forms.OpenFileDialog();
            this.StatusStrip.SuspendLayout();
            this.ToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InstanceListBox)).BeginInit();
            this.SuspendLayout();
            // 
            // StatusStrip
            // 
            this.StatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel,
            this.PoweredByLabel,
            this.InfoLabel3});
            this.StatusStrip.Location = new System.Drawing.Point(0, 289);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(624, 22);
            this.StatusStrip.TabIndex = 0;
            this.StatusStrip.Text = "StatusStrip";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // PoweredByLabel
            // 
            this.PoweredByLabel.IsLink = true;
            this.PoweredByLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline;
            this.PoweredByLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.PoweredByLabel.Name = "PoweredByLabel";
            this.PoweredByLabel.Size = new System.Drawing.Size(133, 17);
            this.PoweredByLabel.Text = "Inspired by Babyhamsta";
            this.PoweredByLabel.Click += new System.EventHandler(this.PoweredByLabel_Click);
            this.PoweredByLabel.MouseEnter += new System.EventHandler(this.PoweredByLabel_MouseEnter);
            this.PoweredByLabel.MouseLeave += new System.EventHandler(this.PoweredByLabel_MouseLeave);
            // 
            // InfoLabel3
            // 
            this.InfoLabel3.Name = "InfoLabel3";
            this.InfoLabel3.Size = new System.Drawing.Size(239, 17);
            this.InfoLabel3.Text = "Not guaranteed to work on every computer!";
            // 
            // InfoLabel
            // 
            this.InfoLabel.AutoSize = true;
            this.InfoLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.InfoLabel.Location = new System.Drawing.Point(0, 270);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Padding = new System.Windows.Forms.Padding(3);
            this.InfoLabel.Size = new System.Drawing.Size(542, 19);
            this.InfoLabel.TabIndex = 1;
            this.InfoLabel.Text = "Account Manager can not log you in automatically, you will have to manually log i" +
    "n once your instance is created";
            // 
            // ToolStrip
            // 
            this.ToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InfoLabel2,
            this.toolStripSeparator1,
            this.ImportButton,
            this.RefreshInstancesButton,
            this.UninstallSelectedButton,
            this.AttemptLoginButton});
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Size = new System.Drawing.Size(624, 25);
            this.ToolStrip.TabIndex = 3;
            this.ToolStrip.Text = "ToolStrip";
            // 
            // InfoLabel2
            // 
            this.InfoLabel2.Name = "InfoLabel2";
            this.InfoLabel2.Size = new System.Drawing.Size(172, 22);
            this.InfoLabel2.Text = "Drag and Drop Accounts Below";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // ImportButton
            // 
            this.ImportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ImportButton.Image = ((System.Drawing.Image)(resources.GetObject("ImportButton.Image")));
            this.ImportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(90, 22);
            this.ImportButton.Text = "Manual Import";
            this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // RefreshInstancesButton
            // 
            this.RefreshInstancesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.RefreshInstancesButton.Image = ((System.Drawing.Image)(resources.GetObject("RefreshInstancesButton.Image")));
            this.RefreshInstancesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RefreshInstancesButton.Name = "RefreshInstancesButton";
            this.RefreshInstancesButton.Size = new System.Drawing.Size(102, 22);
            this.RefreshInstancesButton.Text = "Refresh Instances";
            this.RefreshInstancesButton.Click += new System.EventHandler(this.RefreshInstancesButton_Click);
            // 
            // UninstallSelectedButton
            // 
            this.UninstallSelectedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.UninstallSelectedButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UninstallAllButton});
            this.UninstallSelectedButton.Image = ((System.Drawing.Image)(resources.GetObject("UninstallSelectedButton.Image")));
            this.UninstallSelectedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.UninstallSelectedButton.Name = "UninstallSelectedButton";
            this.UninstallSelectedButton.Size = new System.Drawing.Size(116, 22);
            this.UninstallSelectedButton.Text = "Uninstall Selected";
            this.UninstallSelectedButton.ButtonClick += new System.EventHandler(this.UninstallSelectedButton_ButtonClick);
            // 
            // UninstallAllButton
            // 
            this.UninstallAllButton.Name = "UninstallAllButton";
            this.UninstallAllButton.Size = new System.Drawing.Size(189, 22);
            this.UninstallAllButton.Text = "Uninstall All Instances";
            this.UninstallAllButton.Click += new System.EventHandler(this.UninstallAllButton_Click);
            // 
            // AttemptLoginButton
            // 
            this.AttemptLoginButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.AttemptLoginButton.Image = ((System.Drawing.Image)(resources.GetObject("AttemptLoginButton.Image")));
            this.AttemptLoginButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AttemptLoginButton.Name = "AttemptLoginButton";
            this.AttemptLoginButton.Size = new System.Drawing.Size(88, 22);
            this.AttemptLoginButton.Text = "Attempt Login";
            this.AttemptLoginButton.Click += new System.EventHandler(this.AttemptLoginButton_Click);
            // 
            // InstanceListBox
            // 
            this.InstanceListBox.AllColumns.Add(this.UserColumn);
            this.InstanceListBox.AllColumns.Add(this.PackageColumn);
            this.InstanceListBox.AllowDrop = true;
            this.InstanceListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InstanceListBox.CellEditUseWholeCell = false;
            this.InstanceListBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.UserColumn,
            this.PackageColumn});
            this.InstanceListBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.InstanceListBox.FullRowSelect = true;
            this.InstanceListBox.HideSelection = false;
            this.InstanceListBox.Location = new System.Drawing.Point(0, 28);
            this.InstanceListBox.Name = "InstanceListBox";
            this.InstanceListBox.ShowGroups = false;
            this.InstanceListBox.Size = new System.Drawing.Size(624, 239);
            this.InstanceListBox.TabIndex = 5;
            this.InstanceListBox.UseCompatibleStateImageBehavior = false;
            this.InstanceListBox.View = System.Windows.Forms.View.Details;
            this.InstanceListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.InstanceListBox_DragDrop);
            this.InstanceListBox.DragOver += new System.Windows.Forms.DragEventHandler(this.InstanceListBox_DragOver);
            // 
            // UserColumn
            // 
            this.UserColumn.AspectName = "Username";
            this.UserColumn.Text = "Username";
            this.UserColumn.Width = 133;
            // 
            // PackageColumn
            // 
            this.PackageColumn.AspectName = "PackageID";
            this.PackageColumn.Text = "Package Name";
            this.PackageColumn.Width = 431;
            // 
            // UwpExeDialog
            // 
            this.UwpExeDialog.FileName = "Windows10Universal.exe";
            this.UwpExeDialog.Filter = "UWP Roblox|Windows10Universal.exe|All Files|*.*";
            this.UwpExeDialog.InitialDirectory = "C:\\Program Files\\WindowsApps";
            // 
            // UWPInstanceManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 311);
            this.Controls.Add(this.InstanceListBox);
            this.Controls.Add(this.ToolStrip);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.StatusStrip);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 300);
            this.Name = "UWPInstanceManager";
            this.ShowIcon = false;
            this.Text = "Mutli UWP Instance Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UWPInstanceManager_FormClosing);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InstanceListBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel PoweredByLabel;
        private System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.ToolStrip ToolStrip;
        private System.Windows.Forms.ToolStripLabel InfoLabel2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton ImportButton;
        private System.Windows.Forms.ToolStripStatusLabel InfoLabel3;
        private System.Windows.Forms.ToolStripButton RefreshInstancesButton;
        private System.Windows.Forms.ToolStripSplitButton UninstallSelectedButton;
        private System.Windows.Forms.ToolStripMenuItem UninstallAllButton;
        private BrightIdeasSoftware.ObjectListView InstanceListBox;
        private BrightIdeasSoftware.OLVColumn UserColumn;
        private BrightIdeasSoftware.OLVColumn PackageColumn;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.OpenFileDialog UwpExeDialog;
        private System.Windows.Forms.ToolStripButton AttemptLoginButton;
    }
}