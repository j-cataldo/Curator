using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Curator.Utils
{
    public interface IConfigManager
    {
        int Interval { get; set;}
        List<string> WallpaperLocations { get; set; }
        string subreddits { get; set; }
        Curator.Utils.StretchStyles StretchStyle { get; set; }
     
        void showConfigureForm();
    }
}
