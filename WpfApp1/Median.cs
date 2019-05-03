using System.Collections.Generic;
using System.Drawing;

namespace WpfApp1
{
    public static class Median
    {
        public static Bitmap MakeMedian(int size, Bitmap bitmap)
        {
            var tmp = new Bitmap(bitmap);

            for (int i = size / 2; i < bitmap.Width - size / 2; i++)
            {
                var reds = new List<int>();
                var greens = new List<int>();
                var blues = new List<int>();
                for (int j = size / 2; j < bitmap.Height - size / 2; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        for (int l = 0; l < size; l++)
                        {
                            var x = i + k - size / 2;
                            var y = j + l - size / 2;
                            var p = bitmap.GetPixel(x, y);
                            reds.Add(p.R);
                            greens.Add(p.G);
                            blues.Add(p.B);
                        }
                    }
                    blues.Sort();
                    reds.Sort();
                    greens.Sort();
                    var sizesq = (size*size) / 2;
                    bitmap.SetPixel(i, j, Color.FromArgb(255, reds[sizesq], greens[sizesq], blues[sizesq]));
                }
            }
            return tmp;
        }
    }
}
