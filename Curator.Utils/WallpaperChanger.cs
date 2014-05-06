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
using Microsoft.Win32;

namespace Curator.Utils
{
    /// <summary>
    /// Another singleton class. Does most of the heavy lifting for slideshow transitioning.
    /// </summary>
    
    public class WallpaperChanger
    {
        private static WallpaperChanger _wallpaperChangerInstance;
        private static readonly object _wallpaperChangerInstanceSync = new object();

        private IConfigManager _configManager;

        // Private settings variables. Use corresponding properties for direct access.
        private StretchStyles _stretchStyle;
        private List<string> _selectedWallpaperLocations;
        private List<string> _wallpaperImagePaths;

        private int _currentWallpaperIndex;
        private Bitmap _previewImage;

        private WallpaperChanger()
        {
            _currentWallpaperIndex = 0;
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

        public IConfigManager ConfigManager
        {
            get
            {
                return this._configManager;
            }
            set
            {
                this._configManager = value;
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
            get
            {
                return _selectedWallpaperLocations;
            }

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

        public List<string> WallpaperImagePaths
        {
            get
            {
                return this._wallpaperImagePaths;
            }
        }

        public void SetNextWallpaper()
        {
            if (++_currentWallpaperIndex >= _wallpaperImagePaths.Count)
            {
                _currentWallpaperIndex = 0;
            }
            
            // This if statement is to allow multi monitor support. What this means is if more than 1 screen
            // is detected then several images will be concatenated such that a different image appears
            // on each screen. This code should be refactored with the same code in SetPreviousWallpaper
            if (Screen.AllScreens.Length > 1)
            {
                // skips over a wallpaper so when two monitors are used both wallpapers are refreshed
                // in the future this should be modified to change depending on number of monitors
                // such that each monitor changes its wallpaper every time.
                if (++_currentWallpaperIndex >= _wallpaperImagePaths.Count)
                {
                    _currentWallpaperIndex = 0;
                }
                int num_monitors = Screen.AllScreens.Length;
                int index = _currentWallpaperIndex;
                Bitmap img_cat = new Bitmap(_wallpaperImagePaths[_currentWallpaperIndex]);

                // Multi-monitor support works only if the windows default style variable is set to tiled

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());

                for (int i = 0; i < num_monitors; i++)
                {
                    if (++index >= _wallpaperImagePaths.Count)
                    {
                        index = 0;
                    }

                    Bitmap img_tmp = new Bitmap(_wallpaperImagePaths[index]);

                    switch (this.StretchStyle)
                    {
                        case StretchStyles.Fill:
                            ImageResizer.Fill(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.Fit:
                            ImageResizer.Fit(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.Stretch:
                            ImageResizer.Stretch(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.Center:
                            ImageResizer.Center(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.CenterFit:
                            ImageResizer.CenterFit(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.Tile:
                            ImageResizer.Tile(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        default:
                            ImageResizer.Fill(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;

                    }

                    if(i==0)
                    {
                        img_cat = new Bitmap(img_tmp);
                    }
                    else
                    {
                        img_cat = ImageCat.Cat(img_cat, img_tmp);
                    }
                }
                string multi_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp\temp_multi.bmp");
                img_cat.Save(multi_path);
                SetWallpaperUsingActiveDesktop(multi_path);
            }
            else
            {
                // This is run when only 1 monitor is detected.
                ResizeAndSetWallpaperWithRetry(_wallpaperImagePaths[_currentWallpaperIndex]);
            }


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

            if (Screen.AllScreens.Length > 1)
            {
                // skips over a wallpaper so when two monitors are used both wallpapers are refreshed
                // in the future this should be modified to change depending on number of monitors
                // such that each monitor changes its wallpaper every time.
                if (--_currentWallpaperIndex < 0)
                {
                    _currentWallpaperIndex = _wallpaperImagePaths.Count - 1;
                }
                int num_monitors = Screen.AllScreens.Length;
                int index = _currentWallpaperIndex;
                Bitmap img_cat = new Bitmap(_wallpaperImagePaths[_currentWallpaperIndex]);


                // Multi-monitor support works only if the windows default style variable is set to tiled

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());

                for (int i = 0; i < num_monitors; i++)
                {
                    if (++index >= _wallpaperImagePaths.Count)
                    {
                        index = 0;
                    }

                    Bitmap img_tmp = new Bitmap(_wallpaperImagePaths[index]);

                    switch (this.StretchStyle)
                    {
                        case StretchStyles.Fill:
                            ImageResizer.Fill(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.Fit:
                            ImageResizer.Fit(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.Stretch:
                            ImageResizer.Stretch(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.Center:
                            ImageResizer.Center(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.CenterFit:
                            ImageResizer.CenterFit(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        case StretchStyles.Tile:
                            ImageResizer.Tile(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;
                        default:
                            ImageResizer.Fill(ref img_tmp, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].Bounds.Width);
                            break;

                    }

                    if (i == 0)
                    {
                        img_cat = new Bitmap(img_tmp);
                    }
                    else
                    {
                        img_cat = ImageCat.Cat(img_cat, img_tmp);
                    }
                }
                string multi_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp\temp_multi.bmp");
                img_cat.Save(multi_path);
                SetWallpaperUsingActiveDesktop(multi_path);
            }
            else
            {
                ResizeAndSetWallpaperWithRetry(_wallpaperImagePaths[_currentWallpaperIndex]);
            }

            if (SlideshowTimer.GetInstance.Enabled)
            {
                SlideshowTimer.GetInstance.Stop();
                SlideshowTimer.GetInstance.Start();
            }
        }

        private void ResizeAndSetWallpaperWithRetry(string path)
        {
            // Try to set the wallpaper 10 times over 1.5s
            // Both saving and setting may fail if hard drive is busy, so they are each tried at every attempt

            var attempts = 0;

            while (attempts < 10)
            {
                try
                {
                    ResizeAndSaveTempWallpaper(path, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp"));
                    SetWallpaperUsingActiveDesktop(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Curator\temp\temp.bmp"));
                    break;
                }

                catch (IOException)
                {
                    Console.WriteLine("DEBUG::ResizeAndSetWallpaperWithRetry failed attempt {0}...", ++attempts);
                    System.Threading.Thread.Sleep(150);
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

            if (this._previewImage != null)
            {
                this._previewImage.Dispose();
            }
            this.ConfigManager.ImagePreview.Image = null;

            // Safest way to save image to file without excessive resource use
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

        // This function actually sets the current desktop wallpaper to image at the given path
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

            Image temp = Image.FromFile(path);
            this._previewImage = new Bitmap(temp);
            Curator.Utils.ImageResizer.Stretch(ref this._previewImage, 180, 320);
            temp.Dispose();

            this.ConfigManager.ImagePreview.Image = this._previewImage;
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

    /// <summary>
    /// Thread-safe random implementation for reliably shuffling wallpaper list
    /// </summary>

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