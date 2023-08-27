#nullable enable
using System.Collections.Specialized;
using System.Diagnostics;
using WormPak.Components;
using WormPak.Formats;

namespace WormPak;

public partial class PakViewer : Form
{
    private PakFile? _pak;
    public PakFile? Pak
    {
        get => _pak;
        set
        {
            _pak?.Dispose();

            _pak = value;
            _palette = null; // force palette to reload
            RebuildPakTree();
        }
    }

    private Color[]? _palette;
    public Color[] Palette
    {
        get
        {
            if (_palette != null)
                return _palette;

            var pal = LoadPalette();
            if (pal == null)
                return new Color[256];

            _palette = pal;
            return _palette;
        }
    }

    private Color[]? LoadPalette()
    {
        Color[]? pal = null;

        // Read from current pak
        if (Pak != null)
            pal = GetPaletteFromPak(Pak);

        if (pal != null)
            return pal;

        // Select pak or PCX file to load from
        while (true)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "pak files (*.pak)|*.pak|pcx files (*.pcx)|*.pcx",
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pal = Path.GetExtension(dialog.FileName).ToLower() switch
                    {
                        ".pcx" => PcxFile.FromStream(File.Open(dialog.FileName, FileMode.Open))?.Color8To24Table,
                        ".pak" => GetPaletteFromPak(PakFile.FromFile(dialog.FileName)),
                        _ => pal
                    };
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        "failed to load the provided file. invalid format, more likely than not.",
                        "error",
                            MessageBoxButtons.OK
                    );
                    continue;
                }


                if (pal != null) return pal;

                MessageBox.Show(
                    "not a pak that contains colormap.pcx or otherwise is a bad pcx file",
                    "error",
                    MessageBoxButtons.OK
                );
            }
            else
            {
                // user cancelled the thing so ok
                return null;
            }
        }
    }

    private static Color[]? GetPaletteFromPak(PakFile pak)
    {
        return (from entry in pak.Entries 
            where Path.GetFileName(entry.Name).ToLower() == "colormap.pcx" 
            select pak.GetPcxPalette(entry)).FirstOrDefault();
    }

    private class TreeListEntry
    {
        public IEnumerable<string> PathParts = new List<string>();
        public PakFile.Entry Entry = new();
    }

    private void RebuildPakTree()
    {
        if (Pak == null)
            return;

        TabbedItemView.TabPages.Clear();

        PakTree.BeginUpdate();
        PakTree.Nodes.Clear();
        var entries = Pak.Entries.Select(e => new TreeListEntry
        {
            PathParts = e.Name.Split(@"/", StringSplitOptions.RemoveEmptyEntries).ToArray(),
            Entry = e
        });
        BuildTreeLevel(PakTree.Nodes, entries);
        PakTree.EndUpdate();
    }

    private void BuildTreeLevel(TreeNodeCollection nodes, IEnumerable<TreeListEntry> entries)
    {
        var nonempty = entries.Where(it => it.PathParts.Any());
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
            Pak = PakFile.FromFile(dialog.FileName);
        }
    }

    private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node.Tag is not PakFile.Entry entry) /* it's a folder */
            return;

        /* if it exists, focus it. */
        foreach (TabPage tab in TabbedItemView.TabPages)
        {
            if (tab.Tag != entry) continue;

            TabbedItemView.SelectedTab = tab;
            return;
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

        // create the viewer for the file entry, make it into a tab.
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
        if (Pak == null)
            return null;

        var extension = Path.GetExtension(entry.Name);
        return extension.ToLower() switch
        {
            ".cfg" or ".txt" or ".lst" or ".json" => new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Text = Pak.GetStringContents(entry),
                ScrollBars = ScrollBars.Both,
            },
            ".tga" => new PictureBox
            {
                Image = Pak.GetTgaContents(entry),
                SizeMode = PictureBoxSizeMode.Zoom,
            },
            ".pcx" => GetImageViewerPcx(entry),
            ".wal" => GetImageViewerWal(entry),
            ".png" or ".jpg" => new PictureBox
            {
                Image = Pak.GetImageContents(entry),
                SizeMode = PictureBoxSizeMode.Zoom,
            },
            ".wav" => new WavPlayer(Pak, entry)
            {
                Name = "WavPlayer", /* for removal event at TabbedItemView_ControlRemoved */
            },
            _ => null,
        };
    }

    private Control GetImageViewerWal(PakFile.Entry entry)
    {
        var wal = Pak?.GetWalFile(entry, Palette);
        if (wal?.Bitmap == null)
            return new Label { Text = "Couldn't load file." };

        return new ImageViewer(wal.Bitmap, wal.Header);
    }

    private Control GetImageViewerPcx(PakFile.Entry entry)
    {
        var pcx = Pak?.GetPcxFile(entry);
        if (pcx?.Bitmap == null)
            return new Label { Text = "Couldn't load file." };

        return new ImageViewer(pcx.Bitmap, pcx.Header);
    }

    private void TabbedItemView_MouseDown(object sender, MouseEventArgs e)
    {
        // middle mouse closes, right click opens a context menu.
        if (e.Button == MouseButtons.Middle)
        {
            for (var i = 0; i < TabbedItemView.TabCount; i++)
            {
                if (!TabbedItemView.GetTabRect(i).Contains(e.Location)) continue;
                TabbedItemView.TabPages.RemoveAt(i);
                return;
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            var tabIndex = -1;
            for (var i = 0; i < TabbedItemView.TabCount; i++)
            {
                if (!TabbedItemView.GetTabRect(i).Contains(e.Location)) continue;
                tabIndex = i;
                break;
            }

            if (tabIndex == -1) return;

            var tab = TabbedItemView.TabPages[tabIndex];
            var loc = TabbedItemView.PointToClient(TabbedItemView.PointToScreen(e.Location));

            tabItemMenu.Tag = tab;
            tabItemMenu.Show(tab, loc);
        }
    }

    private void exitToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
    {
        Application.Exit();
    }

    private void TabbedItemView_ControlRemoved(object sender, ControlEventArgs e)
    {
        foreach (var control in e.Control.Controls.Find("WavPlayer", false))
        {
            var player = (WavPlayer)control;
            player.player.Stop();
        }

        /* our last control is being removed, so collapse the view. */
        if (TabbedItemView.Controls.Count == 0)
            splitContainer1.Panel2Collapsed = true;
    }

    private void PakTree_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Link;
    }

    private void PakTree_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files.Length > 0)
            Pak = PakFile.FromFile(files[0]);
    }

    private void extractToToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (treeItemMenu.Tag is not TreeNode node) return;

        using var fbd = new FolderBrowserDialog();
        if (fbd.ShowDialog() != DialogResult.OK) return;

        var path = fbd.SelectedPath;
        ExtractTree(path, node);
    }

    private void ExtractTree(string path, TreeNode node)
    {
        if (node.Nodes.Count > 0)
        {
            // folder
            foreach (TreeNode childItem in node.Nodes)
            {
                var newPath = path;
                if (childItem.Nodes.Count > 0)
                    newPath = Path.Combine(path, childItem.Text);

                ExtractTree(newPath, childItem);
            };
        }
        else
        {
            // file 
            if (node.Tag is not PakFile.Entry entry) return;
            var newPath = Path.Combine(path, node.Text);
            WritePakEntryToFile(entry, newPath);
        }
    }

    private void WritePakEntryToFile(PakFile.Entry entry, string newPath)
    {
        var stream = Pak?.GetEntryStream(entry);
        if (stream == null) return;
        using var outputFile = File.Create(newPath);
        stream.CopyToAsync(outputFile);
    }

    private void PakTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (sender is not TreeView tree) return;

        if (e.Button == MouseButtons.Right)
        {
            tree.SelectedNode = e.Node;
            treeItemMenu.Tag = e.Node;

            var loc = tree.PointToClient(tree.PointToScreen(e.Location));
            treeItemMenu.Show(tree, loc);
        }
        // todo: extract to temp path then move?
        // else if (e.Button == MouseButtons.Left)
        //{
        //    var path = MakeFileListFromNode(e.Node);
        //    var data = new DataObject();
        //    data.SetFileDropList(path);
        //    tree.DoDragDrop(data, DragDropEffects.All);
        //}
    }

    //private StringCollection MakeFileListFromNode(TreeNode startNode)
    //{
    //    var ret = new StringCollection();

    //    // leaf/file
    //    if (startNode.Nodes.Count == 0)
    //    {
    //        ret.Add(startNode.Text);
    //        return ret;
    //    }

    //    // folder
    //    foreach (TreeNode node in startNode.Nodes)
    //    {
    //        if (node.Nodes.Count == 0)
    //        {
    //            ret.Add(Path.Combine(startNode.Text, node.Text));
    //        }
    //        else
    //        {
    //            var children = MakeFileListFromNode(node);
    //            foreach (var child in children)
    //            {
    //                if (child == null) continue; // this shouldn't happen, but just in case.
    //                ret.Add(Path.Combine(startNode.Text, child));
    //            }
    //        }
    //    }

    //    return ret;
    //}

    private void extractToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (tabItemMenu.Tag is not TabPage { Tag: PakFile.Entry entry }) return;

        using var fbd = new FolderBrowserDialog();
        if (fbd.ShowDialog() != DialogResult.OK) return;

        var path = fbd.SelectedPath;
        var filename = Path.GetFileName(entry.Name);
        var outPath = Path.Combine(path, filename);
        WritePakEntryToFile(entry, outPath);
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (tabItemMenu.Tag is not TabPage page) return;
        TabbedItemView.TabPages.Remove(page);
    }

    private void TabbedItemView_ControlAdded(object sender, ControlEventArgs e)
    {
        splitContainer1.Panel2Collapsed = false;
    }
}