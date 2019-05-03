using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    public static class ColorExtension
    {
        public static int getAverage(this Color p)
        {
            return (p.R + p.G + p.B) / 3;
        }
        //public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        //{
        //    BitmapImage bitmapImage = new BitmapImage();
        //    using (MemoryStream memory = new MemoryStream())
        //    {
        //        bitmap.Save(memory, ImageFormat.Png);
        //        memory.Position = 0;
        //        bitmapImage.BeginInit();
        //        bitmapImage.StreamSource = memory;
        //        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmapImage.EndInit();
        //    }
        //    return bitmapImage;
        //}
    }
}
