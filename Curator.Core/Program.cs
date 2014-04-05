using System;
using System.Windows.Forms;

namespace Curator.Core
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var HotKeyManager = new HotKeyManager();
            //RegisterHotKey (Handle, Hotkey Identifier, Modifiers, Key)
            Curator.Utils.WinAPI.RegisterHotKey(HotKeyManager.Handle, 0, Constants.ALT + Constants.CTRL, (int)Keys.PageUp);
            Curator.Utils.WinAPI.RegisterHotKey(HotKeyManager.Handle, 1, Constants.ALT + Constants.CTRL, (int)Keys.PageDown);
            Curator.Utils.WinAPI.RegisterHotKey(HotKeyManager.Handle, 2, Constants.ALT + Constants.CTRL, (int)Keys.End);
            Curator.Utils.WinAPI.RegisterHotKey(HotKeyManager.Handle, 3, Constants.ALT + Constants.CTRL, (int)Keys.Home);

            Application.Run(new ApplicationManager());
        }
    }
}
