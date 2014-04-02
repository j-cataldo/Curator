using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Curator.Utils
{
    public class WallpaperChanger
    {
        /// <summary>
        /// Another singleton class. Does most of the heavy lifting for slideshow transitioning.
        /// </summary>

        private static WallpaperChanger _wallpaperChangerInstance;
        private static readonly object _wallpaperChangerInstanceSync = new object(); 

        private StretchStyles _stretchStyle;
        private List<string> _selectedWallpaperLocations;
        private List<string> _wallpaperImagePaths;

        private int _currentWallpaperIndex;
        private int uncaughtErrors;

        private WallpaperChanger()
        {
            _currentWallpaperIndex = 0;
            uncaughtErrors = 0;
        }

        public static WallpaperChanger GetInstance
        {
            get
            {
                if (_wallpaperChangerInstance == null)
                {
                    lock (_wallpaperChangerInstanceSync)
                    {
                        if (_wallpaperChangerInstance == null)
                        {
                            _wallpaperChangerInstance = new WallpaperChanger();
                        }
                    }
                }

                return _wallpaperChangerInstance;
            }
        }

        public StretchStyles StretchStyle
        {
            get
            {
                return this._stretchStyle;
            }
            set
            {
                this._stretchStyle = value;
            }
        }

        public List<string> SelectedWallpaperLocations
        {
            set
            {
                _selectedWallpaperLocations = value;

                _wallpaperImagePaths = new List<string>();
                var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
                foreach (var filter in filters)
                {
                    foreach (var path in _selectedWallpaperLocations)
                    {
                        _wallpaperImagePaths.AddRange(System.IO.Directory.GetFiles(path, String.Format("*.{0}", filter), System.IO.SearchOption.AllDirectories));
                    }
                }

                ShuffleWallpaperImages();
            }
        }

        public void SetNextWallpaper()
        {
            if (++_currentWallpaperIndex >= _wallpaperImagePaths.Count)
            {
                _currentWallpaperIndex = 0;
            }

            ResizeAndSetWallpaperWithRetry(_wallpaperImagePaths[_currentWallpaperIndex]);

            if (SlideshowTimer.GetInstance.Enabled)
            {
                SlideshowTimer.GetInstance.Stop();
                SlideshowTimer.GetInstance.Start();
            }
        }

        public void SetPreviousWallpaper()
        {
            if (--_currentWallpaperIndex < 0)
            {
                _currentWallpaperIndex = _wallpaperImagePaths.Count - 1;
            }

            ResizeAndSetWallpaperWithRetry(_wallpaperImagePaths[_currentWallpaperIndex]);

            if (SlideshowTimer.GetInstance.Enabled)
            {
                SlideshowTimer.GetInstance.Stop();
                SlideshowTimer.GetInstance.Start();
            }
        }

        private void ResizeAndSetWallpaperWithRetry(string path)
        {
            try
            {
                ResizeAndSaveTempWallpaper(path, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp"));
                SetWallpaperUsingActiveDesktop(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp\temp.bmp"));
            }
            catch (IOException)
            {
                Console.WriteLine("DEBUG::ResizeAndSetWallpaperWithRetry failed attempt 1...");
                System.Threading.Thread.Sleep(1000);
                try
                {
                    ResizeAndSaveTempWallpaper(path, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp"));
                    SetWallpaperUsingActiveDesktop(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp\temp.bmp"));
                }
                catch (IOException)
                {
                    Console.WriteLine("DEBUG::ResizeAndSetWallpaperWithRetry failed attempt 2...");
                    System.Threading.Thread.Sleep(2000);
                    try
                    {
                        ResizeAndSaveTempWallpaper(path, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp"));
                        SetWallpaperUsingActiveDesktop(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp\temp.bmp"));
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("DEBUG::ResizeAndSetWallpaperWithRetry failed attempt 3. Giving up...");
                        Console.WriteLine("DEBUG::Uncaught Error Count: " + ++uncaughtErrors);
                    }
                }
            }
        }

        private void ResizeAndSaveTempWallpaper(string sourcePath, string outputFolder)
        {
            Bitmap image = new Bitmap(sourcePath);

            switch (this.StretchStyle)
            {
                case StretchStyles.Fill:
                    ImageResizer.Fill(ref image, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width);
                    break;
                case StretchStyles.Fit:
                    ImageResizer.Fit(ref image, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width);
                    break;
                case StretchStyles.Stretch:
                    ImageResizer.Stretch(ref image, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width);
                    break;
                case StretchStyles.Center:
                    ImageResizer.Center(ref image, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width);
                    break;
                case StretchStyles.CenterFit:
                    ImageResizer.CenterFit(ref image, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width);
                    break;
                case StretchStyles.Tile:
                    ImageResizer.Tile(ref image, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width);
                    break;
                default:
                    ImageResizer.Fill(ref image, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width);
                    break;

            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream file = new FileStream(Path.Combine(outputFolder, @"temp.bmp"), FileMode.Create, FileAccess.ReadWrite))
                {
                    image.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    byte[] bytes = memory.ToArray();
                    file.Write(bytes, 0, bytes.Length);
                    file.Flush();
                }
            }

            image.Dispose();
        }

        private void SetWallpaperUsingActiveDesktop(string path)
        {
            EnableActiveDesktop();

            ThreadStart threadStarter = () =>
            {
                WinAPI.IActiveDesktop _activeDesktop = WinAPI.ActiveDesktopWrapper.GetActiveDesktop();
                _activeDesktop.SetWallpaper(path, 0);
                _activeDesktop.ApplyChanges(WinAPI.AD_Apply.ALL | WinAPI.AD_Apply.FORCE);

                Marshal.ReleaseComObject(_activeDesktop);
            };
            Thread thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA (REQUIRED!!!!)
            thread.Start();
            thread.Join(2000);

        }

        private void EnableActiveDesktop()
        {
            IntPtr result = IntPtr.Zero;
            WinAPI.SendMessageTimeout(WinAPI.FindWindow("Progman", null), 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 500, out result);
        }

        public void ShuffleWallpaperImages()
        {
            int n = _wallpaperImagePaths.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                string value = _wallpaperImagePaths[k];
                _wallpaperImagePaths[k] = _wallpaperImagePaths[n];
                _wallpaperImagePaths[n] = value;
            }

            _currentWallpaperIndex = 0;
        }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
}