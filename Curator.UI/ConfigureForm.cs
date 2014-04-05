using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Curator.UI
{
    public partial class ConfigureForm : Form
    {
        private Curator.Utils.IConfigManager _configManager;

        public ConfigureForm(Curator.Utils.IConfigManager configManager)
        {
            InitializeComponent();
            this._configManager = configManager;
        }

        protected virtual void OnApplyChanges(EventArgs e)
        {
            int interval = Convert.ToInt32(timeIntervalInput.Text);
            int scaleFactor;

            int index = selectedTimeUnits.SelectedIndex;
            switch (index)
            {
                case 0:
                    scaleFactor = 1000;
                    break;
                case 1:
                    scaleFactor = 60 * 1000;
                    break;
                case 2:
                    scaleFactor = 60 * 60 * 1000;
                    break;
                case 3:
                    scaleFactor = 24 * 60 * 60 * 1000;
                    break;
                default:
                    scaleFactor = 1000;
                    break;
            }

            _configManager.Interval = interval * scaleFactor;

            index = styleComboBox.SelectedIndex;
            Curator.Utils.StretchStyles style;
            switch (index)
            {
                case 0:
                    style = Curator.Utils.StretchStyles.Fill;
                    break;
                case 1:
                    style = Curator.Utils.StretchStyles.Fit;
                    break;
                case 2:
                    style = Curator.Utils.StretchStyles.Stretch;
                    break;
                case 3:
                    style = Curator.Utils.StretchStyles.Center;
                    break;
                case 4:
                    style = Curator.Utils.StretchStyles.CenterFit;
                    break;
                case 5:
                    style = Curator.Utils.StretchStyles.Tile;
                    break;
                default:
                    style = Curator.Utils.StretchStyles.Fill;
                    break;
            }

            _configManager.StretchStyle = style;

        }

        private void PopulateImageSetTree()
        {
            foreach (string source in this._configManager.WallpaperLocations)
            {
                bool alreadyListed = false;

                foreach (TreeNode node in this.sourcesTreeView.Nodes)
                {
                    if (node.Text == source)
                    {
                        alreadyListed = true;
                    }
                }

                if (!alreadyListed)
                {
                    this.sourcesTreeView.Nodes.Add(source);
                }
            }
        }

        private void ConfigureForm_Load(object sender, EventArgs e)
        {
            int interval = _configManager.Interval;

            styleComboBox.SelectedIndex = (int)_configManager.StretchStyle;
            selectedTimeUnits.SelectedIndex = 0;
            timeIntervalInput.Text = Convert.ToString(interval / 1000);

            PopulateImageSetTree();

            applyButton.Enabled = false;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            OnApplyChanges(e);
            applyButton.Enabled = false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            OnApplyChanges(e);
            this.DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutDesktopCuratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm.GetInstance.TopMost = false;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!_configManager.WallpaperLocations.Contains(folderBrowserDialog.SelectedPath))
                {
                    _configManager.WallpaperLocations.Add(folderBrowserDialog.SelectedPath);
                    Curator.Utils.WallpaperChanger.GetInstance.SelectedWallpaperLocations.Add(folderBrowserDialog.SelectedPath);

                    var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
                    foreach (var filter in filters)
                    {
                        foreach (var path in Curator.Utils.WallpaperChanger.GetInstance.SelectedWallpaperLocations)
                        {
                            Curator.Utils.WallpaperChanger.GetInstance.WallpaperImagePaths.AddRange(System.IO.Directory.GetFiles(path, String.Format("*.{0}", filter), System.IO.SearchOption.AllDirectories));
                        }
                    }

                    Curator.Utils.WallpaperChanger.GetInstance.ShuffleWallpaperImages();
                }
            }

            PopulateImageSetTree();

            applyButton.Enabled = true;
        }

        private void selectedTimeUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void timeIntervalInput_TextChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void styleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void browseButton_Click_1(object sender, EventArgs e)
        {
            browseButton_Click(sender, e);
        }

        private void selectedTimeUnits_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            selectedTimeUnits_SelectedIndexChanged(sender, e);
        }

        private void timeIntervalInput_TextChanged_1(object sender, EventArgs e)
        {
            timeIntervalInput_TextChanged(sender, e);
        }
    }
}
