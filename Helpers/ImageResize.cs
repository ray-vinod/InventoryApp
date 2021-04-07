using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace InventoryApp.Helpers
{
    public static class ImageResize
    {
        public static Stream Resize(Stream inStream, int newWidth, int newHeight)
        {
            var img = Image.FromStream(inStream);
            if (newWidth != img.Width || newHeight != img.Height)
            {
                var ratioX = (double)newWidth / img.Width;
                var ratioY = (double)newHeight / img.Height;
                var ratio = Math.Max(ratioX, ratioY);
                var width = (int)(img.Width * ratio);
                var height = (int)(img.Height * ratio);

                var newImage = new Bitmap(width, height);
                Graphics.FromImage(newImage).DrawImage(img, 0, 0, width, height);
                img = newImage;

                if (img.Width != newWidth || img.Height != newHeight)
                {
                    var startX = (Math.Max(img.Width, newWidth) - Math.Min(img.Width, newWidth)) / 2;
                    var startY = (Math.Max(img.Height, newHeight) - Math.Min(img.Height, newHeight)) / 2;
                    img = Crop(img, newWidth, newHeight, startX, startY);
                }
            }

            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            return ms;
        }

        private static Image Crop(Image image, int newWidth, int newHeight, int startX = 0, int startY = 0)
        {
            if (image.Height < newHeight)
                newHeight = image.Height;

            if (image.Width < newWidth)
                newWidth = image.Width;

            using (var bmp = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb))
            {
                bmp.SetResolution(72, 72);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight), startX, startY, newWidth, newHeight, GraphicsUnit.Pixel);

                    var ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Jpeg);
                    image.Dispose();
                    var outimage = Image.FromStream(ms);
                    return outimage;
                }
            }
        }


    }
}
