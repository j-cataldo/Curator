using System.Windows.Forms;
using System;

namespace Curator.Core
{
    //This class is not required but makes managing the modifiers easier.
    public static class Constants
    {
        public const int NOMOD = 0x0000;
        public const int ALT = 0x0001;
        public const int CTRL = 0x0002;
        public const int SHIFT = 0x0004;
        public const int WIN = 0x0008;

        public const int WM_HOTKEY_MSG_ID = 0x0312;
    }


    /// <summary>
    /// Manages the hotkey bindings for the Application Manager
    /// </summary>
    public sealed class HotKeyManager : NativeWindow, IDisposable
    {
        public HotKeyManager()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Constants.WM_HOTKEY_MSG_ID)
            {
                int id = m.WParam.ToInt32();

                // Bind "CTRL + ALT + PAGE UP" to Next
                if (id == 0)
                {
                    Curator.Utils.WallpaperChanger.GetInstance.SetNextWallpaper();
                }

                // Bind "CTRL + ALT + PAGE DOWN" to Previous
                else if (id == 1)
                {
                    Curator.Utils.WallpaperChanger.GetInstance.SetPreviousWallpaper();
                }

                // Bind "CTRL + ALT + END" to Pause/Resume
                else if (id == 2)
                {
                    if (Curator.Utils.SlideshowTimer.GetInstance.Enabled)
                    {
                        Curator.Utils.SlideshowTimer.GetInstance.Stop();
                    }
                    else
                    {
                        Curator.Utils.SlideshowTimer.GetInstance.Start();
                    }
                }

                // Bind "CTRL + ALT + HOME" to Re-shuffle
                else if (id == 3)
                {
                    Curator.Utils.WallpaperChanger.GetInstance.ShuffleWallpaperImages();
                    Curator.Utils.WallpaperChanger.GetInstance.SetNextWallpaper();
                }
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}