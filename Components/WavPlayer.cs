using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WormPak.Components
{
    public partial class WavPlayer : UserControl
    {
        public SoundPlayer player = new();

        public WavPlayer(PakFile pak, PakFile.Entry entry)
        {
            InitializeComponent();
            player.Stream = pak.GetEntryStream(entry);
            lblFilename.Text = entry.Name;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            player.Stop();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            player.Play();
        }
    }
}
