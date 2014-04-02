using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Curator.Core
{
    public class ApplicationManager : ApplicationContext
    {
        private readonly Curator.UI.TrayIcon _trayIcon;

        public ApplicationManager()
        {
            _trayIcon = new Curator.UI.TrayIcon();

            Curator.Utils.SlideshowTimer.GetInstance.Interval = ConfigManager.GetInstance.Interval;
            Curator.Utils.WallpaperChanger.GetInstance.SelectedWallpaperLocations = ConfigManager.GetInstance.WallpaperLocations;
            Curator.Utils.WallpaperChanger.GetInstance.StretchStyle = ConfigManager.GetInstance.StretchStyle;

            Curator.UI.ConfigureForm configureForm = new Curator.UI.ConfigureForm(ConfigManager.GetInstance);
            configureForm.ShowDialog();

            Application.ApplicationExit += this.ApplicationExitHandler;
        }

        private void ApplicationExitHandler(object sender, EventArgs e)
        {
            if (_trayIcon != null)
                _trayIcon.Dispose();
        }
    }
}