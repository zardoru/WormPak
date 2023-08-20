namespace WormPak;

public partial class PakViewer : Form
{
    PakFile? _pak;
    public PakFile? pak
    {
        get => _pak;
        set
        {
            if (_pak != null)
            {
                _pak.Dispose();
            }

            _pak = value;
            RebuildPakTree();
        }
    }

    class TreeListEntry
    {
        public IEnumerable<string> PathParts = new List<string>();
        public PakFile.Entry Entry = new();
    }

    private void RebuildPakTree()
    {
        if (pak == null) 
            return;

        TabbedItemView.TabPages.Clear();

        PakTree.BeginUpdate();
        PakTree.Nodes.Clear();
        var entries = pak.Entries.Select(e => new TreeListEntry{
            PathParts = e.Name.Split(@"/", StringSplitOptions.RemoveEmptyEntries).ToArray(),
            Entry = e
        });
        BuildTreeLevel(PakTree.Nodes, entries);
        PakTree.EndUpdate();
    }

    private void BuildTreeLevel(TreeNodeCollection nodes, IEnumerable<TreeListEntry> entries)
    {
        var nonempty = entries.Where(it => it.PathParts.Count() > 0);
        var entryByFolder = nonempty.GroupBy(it => it.PathParts.First());
        foreach (var dirEntry in entryByFolder)
        {
            var node = new TreeNode(dirEntry.Key);

            // No children, it's a leaf, or a file.
            if (dirEntry.Count() == 1)
            {
                node.Tag = dirEntry.Single().Entry;
                nodes.Add(node);
                continue;
            }

            // It's a folder.
            BuildTreeLevel(node.Nodes, dirEntry.Select(v => new TreeListEntry
            {
                PathParts = v.PathParts.Skip(1),
                Entry = v.Entry
            }));
            nodes.Add(node);
        }
    }

    public PakViewer()
    {
        InitializeComponent();
    }


    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "pak files (*.pak)|*.pak",
            RestoreDirectory = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            pak = PakFile.FromFile(dialog.FileName);
        }
    }

    private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node.Tag is not PakFile.Entry entry) /* it's a folder */
            return;

        /* if it exists, focus it. */
        foreach (TabPage tab in TabbedItemView.TabPages)
        {
            if (tab.Tag == entry)
            {
                TabbedItemView.SelectedTab = tab;
                return;
            }
        }

        // Tab for this file does not exist, create it.
        MakeTabForEntry(entry);
    }

    private void MakeTabForEntry(PakFile.Entry entry)
    {
        var newTab = new TabPage
        {
            Tag = entry,
            Text = entry.Name
        };

        var control = MakeEntryViewer(entry);
        if (control != null)
        {
            control.Dock = DockStyle.Fill;
            newTab.Controls.Add(control);
            TabbedItemView.TabPages.Add(newTab);
            TabbedItemView.SelectedTab = newTab;
        }
        else
        {
            newTab.Dispose();
        }
    }

    private Control? MakeEntryViewer(PakFile.Entry entry)
    {
        if (pak == null) 
            return null;

        var extension = Path.GetExtension(entry.Name);
        return extension.ToLower() switch
        {
            ".cfg" or ".txt" or ".lst" or ".json" => new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Text = pak.GetStringContents(entry),
                ScrollBars = ScrollBars.Both,
            },
            ".tga" => new PictureBox
            {
                Image = pak.GetTgaContents(entry),
                SizeMode = PictureBoxSizeMode.Zoom,
            },
            ".pcx" => new PictureBox
            {
                Image = pak.GetPcxContents(entry),
                SizeMode = PictureBoxSizeMode.Zoom,
            },
            ".png" or ".jpg" => new PictureBox
            {
                Image = pak.GetImageContents(entry),
                SizeMode = PictureBoxSizeMode.Zoom,
            },
            ".wav" => new WavPlayer(pak, entry)
            {
                Name = "WavPlayer", /* for removal event at TabbedItemView_ControlRemoved */
            },
            _ => null,
        };
    }

    private void TabbedItemView_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Middle) return;

        for (var i = 0; i < TabbedItemView.TabCount; i++) {
            if (TabbedItemView.GetTabRect(i).Contains(e.Location)) {
                TabbedItemView.TabPages.RemoveAt(i);
                return;
            }
        }
    }

    private void exitToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
    {
        Application.Exit();
    }

    private void TabbedItemView_ControlRemoved(object sender, ControlEventArgs e)
    {
        foreach(WavPlayer player in e.Control.Controls.Find("WavPlayer", false))
        {
            player.player.Stop();
        }
    }
}