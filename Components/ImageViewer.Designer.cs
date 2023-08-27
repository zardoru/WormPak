namespace WormPak.Components
{
    partial class ImageViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ZoomCheckbox = new System.Windows.Forms.CheckBox();
            this.WalPictureBox = new System.Windows.Forms.PictureBox();
            this.PropertyGrid = new System.Windows.Forms.PropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WalPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ZoomCheckbox);
            this.splitContainer1.Panel1.Controls.Add(this.WalPictureBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.PropertyGrid);
            this.splitContainer1.Size = new System.Drawing.Size(596, 404);
            this.splitContainer1.SplitterDistance = 420;
            this.splitContainer1.TabIndex = 0;
            // 
            // ZoomCheckbox
            // 
            this.ZoomCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ZoomCheckbox.AutoSize = true;
            this.ZoomCheckbox.Location = new System.Drawing.Point(3, 382);
            this.ZoomCheckbox.Name = "ZoomCheckbox";
            this.ZoomCheckbox.Size = new System.Drawing.Size(58, 19);
            this.ZoomCheckbox.TabIndex = 1;
            this.ZoomCheckbox.Text = "Zoom";
            this.ZoomCheckbox.UseVisualStyleBackColor = true;
            this.ZoomCheckbox.CheckedChanged += new System.EventHandler(this.ZoomCheckbox_CheckedChanged);
            // 
            // WalPictureBox
            // 
            this.WalPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WalPictureBox.Location = new System.Drawing.Point(0, 0);
            this.WalPictureBox.Name = "WalPictureBox";
            this.WalPictureBox.Size = new System.Drawing.Size(420, 376);
            this.WalPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.WalPictureBox.TabIndex = 0;
            this.WalPictureBox.TabStop = false;
            // 
            // PropertyGrid
            // 
            this.PropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.PropertyGrid.Name = "PropertyGrid";
            this.PropertyGrid.Size = new System.Drawing.Size(172, 404);
            this.PropertyGrid.TabIndex = 0;
            // 
            // ImageViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ImageViewer";
            this.Size = new System.Drawing.Size(596, 404);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.WalPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private PictureBox WalPictureBox;
        private PropertyGrid PropertyGrid;
        private CheckBox ZoomCheckbox;
    }
}
