using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Curator.Core
{
    public class ConfigManager
    {
        /// <summary>
        /// Another singleton class. Manages configuration settings, in memory for now.
        /// </summary>

        private static ConfigManager _configManagerInstance;
        private static readonly object _configManagerInstanceSync = new object(); // In case we want to multithread later

        private int _interval;
        private List<string> _wallpaperLocations;
        private Curator.Utils.StretchStyles _stretchStyle;

        private ConfigManager()
        {
            this._interval = 10000;
            this._wallpaperLocations = new List<string>() { System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) };
            this._stretchStyle = Curator.Utils.StretchStyles.Stretch;
        }

        public static ConfigManager GetInstance
        {
            get
            {
                if (_configManagerInstance == null)
                {
                    lock (_configManagerInstanceSync)
                    {
                        if (_configManagerInstance == null)
                        {
                            _configManagerInstance = new ConfigManager();
                        }
                    }
                }

                return _configManagerInstance;
            }
        }

        public int Interval
        {
            get
            {
                return this._interval;
            }
            set
            {
                this._interval = value;
            }
        }

        public List<string> WallpaperLocations
        {
            get
            {
                return this._wallpaperLocations;
            }
            set
            {
                this._wallpaperLocations = value;
            }
        }

        public Curator.Utils.StretchStyles StretchStyle
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

    }
}
