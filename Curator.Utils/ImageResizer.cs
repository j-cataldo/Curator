using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace Curator.Utils
{
    public enum StretchStyles { Fill, Fit, Stretch, Center, CenterFit, Tile };

    public static class ImageResizer
    {
        public static void Fit(ref Bitmap original, float height, float width)
        {
            Bitmap bmp = new Bitmap((int)width, (int)height);

            var graph = Graphics.FromImage(bmp);
            graph.InterpolationMode = InterpolationMode.High;
            graph.CompositingQuality = CompositingQuality.HighQuality;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            float scale = Math.Min(width / original.Width, height / original.Height);
            var scaleWidth = (int)(original.Width * scale);
            var scaleHeight = (int)(original.Height * scale);

            var brush = new SolidBrush(Color.Black);
            graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
            graph.DrawImage(original, new Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));

            graph.Dispose();
            brush.Dispose();
            original = bmp;
        }

        public static void Fill(ref Bitmap original, float height, float width)
        {
            Bitmap bmp = new Bitmap((int)width, (int)height);

            var graph = Graphics.FromImage(bmp);
            graph.InterpolationMode = InterpolationMode.High;
            graph.CompositingQuality = CompositingQuality.HighQuality;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            float scale = Math.Max(width / original.Width, height / original.Height);
            var scaleWidth = (int)(original.Width * scale);
            var scaleHeight = (int)(original.Height * scale);

            var brush = new SolidBrush(Color.Black);
            graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
            graph.DrawImage(original, new Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));

            graph.Dispose();
            brush.Dispose();
            original = bmp;
        }

        public static void Stretch(ref Bitmap original, float height, float width)
        {
            Bitmap bmp = new Bitmap((int)width, (int)height);

            var graph = Graphics.FromImage(bmp);
            graph.InterpolationMode = InterpolationMode.High;
            graph.CompositingQuality = CompositingQuality.HighQuality;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            var brush = new SolidBrush(Color.Black);
            graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
            graph.DrawImage(original, new Rectangle(0, 0, (int)width, (int)height));

            graph.Dispose();
            brush.Dispose();
            original = bmp;
        }

        public static void Center(ref Bitmap original, float height, float width)
        {
            Bitmap bmp = new Bitmap((int)width, (int)height);

            var graph = Graphics.FromImage(bmp);
            graph.InterpolationMode = InterpolationMode.High;
            graph.CompositingQuality = CompositingQuality.HighQuality;
            graph.SmoothingMode = SmoothingMode.AntiAlias;

            int x = ((int)width / 2) - (original.Width / 2);
            int y = ((int)height / 2) - (original.Height / 2);

            var brush = new SolidBrush(Color.Black);
            graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
            graph.DrawImage(original, new Rectangle(x, y, original.Width, original.Height));

            graph.Dispose();
            brush.Dispose();
            original = bmp;
        }

        public static void CenterFit(ref Bitmap original, float height, float width)
        {
            if (original.Height <= height && original.Width <= width)
                Center(ref original, height, width);
            else
                Fit(ref original, height, width);
        }

        public static void Tile(ref Bitmap original, float height, float width)
        { }
    }
}
