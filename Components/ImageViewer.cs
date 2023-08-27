using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WormPak.Formats;

namespace WormPak.Components
{
    public partial class ImageViewer : UserControl
    {
        public ImageViewer(Image bitmap, object propObject)
        {
            InitializeComponent();
            WalPictureBox.Image = bitmap;
            PropertyGrid.SelectedObject = propObject;
        }

        private void ZoomCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is not CheckBox cb) return;
            WalPictureBox.SizeMode = cb.Checked ? 
                PictureBoxSizeMode.Zoom : 
                PictureBoxSizeMode.CenterImage;
        }
    }
}
