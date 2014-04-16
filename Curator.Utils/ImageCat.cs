using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace Curator.Utils
{
    // Class to allow for the concatenation of an image with another image.
    // Primarily used for multi monitor support. To allow different wallpapers
    // to display on different monitors Windows combines 
    public static class ImageCat
    {
        // Designed to return a new image such that the original images are not 
        // modified in case they still need to be used. It is assumed that these
        // images already fit the size of the respective monitors. 
        public static Bitmap Cat(Bitmap img1, Bitmap img2)
        {
            int hght = Math.Max(img1.Height,img2.Height);
            int wdth = img1.Width+img2.Width;
            Bitmap result = new Bitmap(img1, wdth, hght);
            Graphics g = Graphics.FromImage(result);

            // draw img2 to right side of img1
            g.DrawImage(img1, 0, 0);
            g.DrawImage(img2, img1.Width, 0);

            // return image
            return result;
        }
    }
}
