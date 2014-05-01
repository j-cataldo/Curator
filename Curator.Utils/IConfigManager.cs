using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Curator.Utils
{
    /// <summary>
    /// Interface for abstracting ConfigManager from the UI wiring
    /// </summary>

    public interface IConfigManager
    {
        int Interval { get; set;}
        List<string> WallpaperLocations { get; set; }
        string subreddits { get; set; }
        Curator.Utils.StretchStyles StretchStyle { get; set; }
        System.Windows.Forms.PictureBox ImagePreview { get; }
     
        void showConfigureForm();
    }
}
