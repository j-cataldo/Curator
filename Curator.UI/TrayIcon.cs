using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace Curator.UI
{
    public class TrayIcon
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;

        private Curator.Utils.IConfigManager _configManager;

        public TrayIcon(Curator.Utils.IConfigManager configManager)
        {
            this._configManager = configManager;

            _contextMenu = new ContextMenuStrip();
            this._contextMenu.Items.Add("&Configure", null, this.ConfigureContextMenuClickHandler).Font = new Font(this._contextMenu.Font, FontStyle.Bold);
            this._contextMenu.Items.Add("-");
            this._contextMenu.Items.Add("&Next wallpaper", null, this.NextContextMenuClickHandler);
            this._contextMenu.Items.Add("&Previous wallpaper", null, this.PreviousContextMenuClickHandler);
            this._contextMenu.Items.Add("Pause slideshow", null, this.PauseContextMenuClickHandler);
            this._contextMenu.Items.Add("Resume slideshow", null, this.ResumeContextMenuClickHandler);
            this._contextMenu.Items.Add("-");
            this._contextMenu.Items.Add("Force new shuffling", null, this.ShuffleContextMenuClickHandler);
            this._contextMenu.Items.Add("-");
            this._contextMenu.Items.Add("&About", null, this.AboutContextMenuClickHandler);
            this._contextMenu.Items.Add("-");
            this._contextMenu.Items.Add("E&xit", null, this.ExitContextMenuClickHandler);

            this._contextMenu.Items[5].Enabled = false; // Application starts with slideshow timer enabled

            _notifyIcon = new NotifyIcon()
            {
                Text = "Desktop Curator Beta",
                Icon = Properties.Resources.TrayIcon,
                ContextMenuStrip = _contextMenu,
                Visible = true,
            };
            this._notifyIcon.MouseClick += this.TrayIconClickHandler;
            this._notifyIcon.MouseDoubleClick += this.TrayIconDoubleClickHandler;
        }

        public void Dispose()
        {
            if (this._contextMenu != null)
            {
                this._contextMenu.Dispose();
            }

            if (this._notifyIcon != null)
            {
                this._notifyIcon.Visible = false;
                this._notifyIcon.Dispose();
            }
        }

        private void TrayIconClickHandler(object sender, MouseEventArgs e)
        {
            
        }

        private void TrayIconDoubleClickHandler(object sender, MouseEventArgs e)
        {
            _configManager.showConfigureForm();
        }

        private void ConfigureContextMenuClickHandler(object sender, EventArgs eventArgs)
        {
            _configManager.showConfigureForm();
        }

        private void NextContextMenuClickHandler(object sender, EventArgs eventArgs)
        {
            Curator.Utils.WallpaperChanger.GetInstance.SetNextWallpaper();
        }

        private void PreviousContextMenuClickHandler(object sender, EventArgs eventArgs)
        {
            Curator.Utils.WallpaperChanger.GetInstance.SetPreviousWallpaper();
        }

        private void PauseContextMenuClickHandler(object sender, EventArgs eventArgs)
        {
            this._contextMenu.Items[4].Enabled = false;
            this._contextMenu.Items[5].Enabled = true;
            Curator.Utils.SlideshowTimer.GetInstance.Stop();
        }

        private void ResumeContextMenuClickHandler(object sender, EventArgs eventArgs)
        {
            this._contextMenu.Items[4].Enabled = true;
            this._contextMenu.Items[5].Enabled = false;
            Curator.Utils.SlideshowTimer.GetInstance.Start();
        }

        private void ShuffleContextMenuClickHandler(object sender, EventArgs eventArgs)
        {
            Curator.Utils.WallpaperChanger.GetInstance.ShuffleWallpaperImages();
            Curator.Utils.WallpaperChanger.GetInstance.SetNextWallpaper();
        }

        private void AboutContextMenuClickHandler(object sender, EventArgs eventArgs)
        {
            AboutForm.GetInstance.TopMost = false;
        }

        private void ExitContextMenuClickHandler(object sender, EventArgs eventArgs)
        {
            Application.Exit();
        }

    }
}