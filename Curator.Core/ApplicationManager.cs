using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Curator.Core
{
    /// <summary>
    /// Main application context.
    /// Manages other managers and UI elements.
    /// Run by the main program runner in Program.cs
    /// </summary>

    public class ApplicationManager : ApplicationContext
    {
        private readonly Curator.UI.TrayIcon _trayIcon;
        private HotKeyManager HotKeyManager = new HotKeyManager();

        public ApplicationManager()
        {
            _trayIcon = new Curator.UI.TrayIcon(ConfigManager.GetInstance);

            Curator.Utils.SlideshowTimer.GetInstance.Interval = ConfigManager.GetInstance.Interval;
            Curator.Utils.WallpaperChanger.GetInstance.SelectedWallpaperLocations = ConfigManager.GetInstance.WallpaperLocations;
            Curator.Utils.WallpaperChanger.GetInstance.StretchStyle = ConfigManager.GetInstance.StretchStyle;
            Curator.Utils.WallpaperChanger.GetInstance.ConfigManager = ConfigManager.GetInstance;

            //RegisterHotKey (Handle, Hotkey Identifier, Modifiers, Key)
            Curator.Utils.WinAPI.RegisterHotKey(HotKeyManager.Handle, 0, Constants.ALT + Constants.CTRL, (int)Keys.PageUp);
            Curator.Utils.WinAPI.RegisterHotKey(HotKeyManager.Handle, 1, Constants.ALT + Constants.CTRL, (int)Keys.PageDown);
            Curator.Utils.WinAPI.RegisterHotKey(HotKeyManager.Handle, 2, Constants.ALT + Constants.CTRL, (int)Keys.End);
            Curator.Utils.WinAPI.RegisterHotKey(HotKeyManager.Handle, 3, Constants.ALT + Constants.CTRL, (int)Keys.Home);

            Curator.UI.ConfigureForm configureForm = new Curator.UI.ConfigureForm(ConfigManager.GetInstance);
            ConfigManager.GetInstance.ConfigureForm = configureForm;
            configureForm.ShowDialog();

            Application.ApplicationExit += this.ApplicationExitHandler;
        }

        private void ApplicationExitHandler(object sender, EventArgs e)
        {
            // Make sure we hide the tray icon automatically when the program exits
            // Otherwise, the user will have to mouse over the icon before it disappears

            if (_trayIcon != null)
                _trayIcon.Dispose();
        }
    }
}