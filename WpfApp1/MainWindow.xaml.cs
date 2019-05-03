using System.Windows;
using System.Drawing;
using Microsoft.Win32;
using System.Collections.Generic;
using System;
using System.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bitmap bm;
        Bitmap originalImage;

        public MainWindow()
        {
            InitializeComponent();
            loadImageFromFile();
        }

        private void loadImageFromFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                bm = new Bitmap(fileDialog.FileName);
                originalImage = bm;
                refreshImage(bm);
                OriginalImage.Source = originalImage.ToBitmapImage();
            }
        }

        private void refreshImage(Bitmap bitmap)
        {
            MainImage.Source = bitmap.ToBitmapImage();
            bm = bitmap;
        }

        private void filtrKonwolucyjny()
        {
            var mih = bm.Height;
            var miw = bm.Width;
            var lattice = Helpers.toLatticeArray(top, mid, bot);
            Bitmap tmp = new Bitmap(bm);

            int sum = Helpers.getSumOfLattice(lattice, 3, 3);

            for (int x = 1; x < miw - 1; x++)
            {
                for (int y = 1; y < mih - 1; y++)
                {
                    int sumR = 0, sumG = 0, sumB = 0;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            var p = bm.GetPixel(x + i, y + j);
                            var lv = lattice[i + 1, j + 1];
                            sumR += p.R * lv;
                            sumG += p.G * lv;
                            sumB += p.B * lv;

                        }
                    }
                    if (sum != 0)
                    {
                        sumR /= sum;
                        sumG /= sum;
                        sumB /= sum;
                    }
                    sumR = Helpers.clamp0to255(sumR);
                    sumG = Helpers.clamp0to255(sumG);
                    sumB = Helpers.clamp0to255(sumB);
                    tmp.SetPixel(x, y, Color.FromArgb(255, sumR, sumG, sumB));
                }
            }
            refreshImage(tmp);
        }

        private void Konwolucyjny_Click(object sender, RoutedEventArgs e)
        {
            filtrKonwolucyjny();
        }

        private void Original_Click(object sender, RoutedEventArgs e)
        {
            refreshImage(originalImage);
        }

        private void Median3_Click(object sender, RoutedEventArgs e)
        {
            MakeMedian(3);
        }
        private void Median5_Click(object sender, RoutedEventArgs e)
        {
            MakeMedian(5);
        }
        private void Kuwaharaa_Click(object sender, RoutedEventArgs e)
        {
             var bitmap = Kuwahara2(bm);
            refreshImage(bitmap);
        }
        private void MakeMedian(int medianWindowSize)
        {
            var tmp = new Bitmap(bm);

            for (int i = medianWindowSize / 2; i < bm.Width - medianWindowSize / 2; i++)
            {
                for (int j = medianWindowSize / 2; j < bm.Height - medianWindowSize / 2; j++)
                {
                    var reds = new List<int>();
                    var greens = new List<int>();
                    var blues = new List<int>();
                    for (int k = 0; k < medianWindowSize; k++)
                    {
                        for (int l = 0; l < medianWindowSize; l++)
                        {
                            var x = i + k - medianWindowSize / 2;
                            var y = j + l - medianWindowSize / 2;
                            var p = bm.GetPixel(x, y);
                            reds.Add(p.R);
                            greens.Add(p.G);
                            blues.Add(p.B);
                        }
                    }
                    //posortuj wszystko
                    blues.Sort();
                    reds.Sort();
                    greens.Sort();
                    int sizesq = medianWindowSize * medianWindowSize / 2; //wez mediane
                    tmp.SetPixel(i, j, Color.FromArgb(255, reds[sizesq], greens[sizesq], blues[sizesq]));
                }
            }
            bm = new Bitmap(tmp);
            refreshImage(bm);
        }

        public Bitmap Kuwahara2(Bitmap bm) //kuwahara 3x3
        {
            var tmp = bm;
            for (int i = 1; i < tmp.Width - 1; i++)
            {
                for (int j = 1; j < tmp.Height - 1; j++)
                {
                    Color getAverage(List<Color> avg)
                    {
                        var len = avg.Count;
                        var avgR = avg.Sum(c => c.R) / len;
                        var avgG = avg.Sum(c => c.G) / len;
                        var avgB = avg.Sum(c => c.B) / len;
                        var color = Color.FromArgb(255, avgR, avgG, avgB);
                        return color;
                    }

                    double variance(Color avg, List<Color> region)
                    {
                        var rValues = region.Select(x => x.R);
                        var gValues = region.Select(x => x.G);
                        var bValues = region.Select(x => x.B);

                        var len = region.Count;

                        var rVariance = rValues.Select(x => Math.Pow(x - avg.R,2.0)).Sum()/len;
                        var gVariance = rValues.Select(x => Math.Pow(x - avg.G, 2.0)).Sum()/len;
                        var bVariance = rValues.Select(x => Math.Pow(x - avg.B, 2.0)).Sum()/len;

                        return rVariance + gVariance + bVariance;
                    }


                    var center = tmp.GetPixel(i, j);

                    //1 top left region
                    var topLeft1 = tmp.GetPixel(i - 1, j + 1);
                    var topRight1 = tmp.GetPixel(i, j + 1);
                    var bottomLeft1 = tmp.GetPixel(i - 1, j);

                    //2 top right region
                    var topLeft2 = tmp.GetPixel(i, j + 1);
                    var topRight2 = tmp.GetPixel(i + 1, j + 1);
                    var bottomRight2 = tmp.GetPixel(i + 1, j);

                    //3 bottom left region
                    var topLeft3 = tmp.GetPixel(i - 1, j);
                    var bottomLeft3 = tmp.GetPixel(i - 1, j - 1);
                    var bottomRight3 = tmp.GetPixel(i, j - 1);

                    //4 bottom right region
                    var topRight4 = tmp.GetPixel(i + 1, j);
                    var bottomLeft4 = tmp.GetPixel(i, j + 1);
                    var bottomRight4 = tmp.GetPixel(i + 1, j + 1);

                    var region1 = new List<Color>() { center, topLeft1, topRight2, bottomLeft1 };
                    var region2 = new List<Color>() { center, topLeft2, topRight2, bottomRight2 };
                    var region3 = new List<Color>() { center, topLeft3, bottomLeft3, bottomRight3 };
                    var region4 = new List<Color>() { center, topRight4, bottomLeft4, bottomRight4 };

                    var regions = new List<List<Color>>() { region1, region2, region3, region4 };
                    var averages = regions.Select(region => getAverage(region)).ToArray();
                    var variances = regions.Select((x, index) => variance(averages.ElementAt(index), x));
                    var minVariance = variances.Min();

                    var indexOfMinimal = Array.IndexOf(variances.ToArray(), minVariance);

                    tmp.SetPixel(i, j, averages[indexOfMinimal]);
                }
            }
            return tmp;
        }
        private void Manual_Click(object sender, RoutedEventArgs e)
        {
            var thresholdInt = int.Parse(Manual_threshold.Text);
            var bmp = Binarization.binarization(bm, thresholdInt);
            refreshImage(bmp);
        }

        private void Bernsen_Click(object sender, RoutedEventArgs e)
        {
            var contrast_threshold = int.Parse(Bernsen_contrast.Text);
            var temp = Binarization.bernsen(bm, 3, contrast_threshold);
            refreshImage(temp);
        }

        private void Otsu_Click(object sender, RoutedEventArgs e)
        {
            var temp = Binarization.otsu(bm);
            refreshImage(temp);
        }

        private void Niblack_Click(object sender, RoutedEventArgs e)
        {
            var windowSize = int.Parse(niblack_window.Text);
            var kValue = float.Parse(niblack_k.Text);
            var temp = Binarization.niblack(this.bm, windowSize, kValue);
            refreshImage(temp);
        }
    }
}
