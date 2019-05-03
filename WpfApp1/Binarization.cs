using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using System.Linq;
namespace WpfApp1
{
    public static class Binarization
    {
        public static Bitmap niblack(Bitmap bm, int window_size, float k_value)
        {
            var tmp = new Bitmap(bm);

            for (int i = window_size / 2; i < tmp.Width - window_size / 2; i++)
            {
                for (int j = window_size / 2; j < tmp.Height - window_size / 2; j++)
                {

                    var pixelsInWindow = new List<int>();
                    for (int x = -window_size / 2; x <= window_size / 2; x++)
                    {
                        for (int y = -window_size / 2; y <= window_size / 2; y++)
                        {
                            pixelsInWindow.Add(tmp.GetPixel(i + x, j + y).G);
                        }
                    }

                    // <image src="https://i.imgur.com/ho2vBNc.png"/>
                    double avg = pixelsInWindow.Sum() / pixelsInWindow.Count;
                    double variance = pixelsInWindow.Select(p => Math.Pow(p - avg, 2)).Sum() / pixelsInWindow.Count - 1;
                    double stdev = Math.Sqrt(variance);
                    var threshold = (avg + k_value * stdev);
                    var px = tmp.GetPixel(i, j);
                    var color = px.getAverage() < threshold ? Color.Black : Color.White;
                    tmp.SetPixel(i, j, color);
                }
            }
            return tmp;
        }

        public static Bitmap binarization(Bitmap bm, int threshold)
        {
            var tmp = new Bitmap(bm);

            for (int i = 0; i < tmp.Width; i++)
            {
                for (int j = 0; j < tmp.Height; j++)
                {
                    var p = tmp.GetPixel(i, j);
                    var avg = (p.R + p.G + p.B) / 3;
                    var color = avg < threshold ? Color.Black : Color.White;
                    tmp.SetPixel(i, j, color);
                }
            }
            return tmp;
        }

        public static Bitmap otsu(Bitmap bm)
        {
            //http://www.labbookpages.co.uk/software/imgProc/otsuThreshold.html
            var tmp = bm;
            var histogramGrayscale = new int[256];
            for (int i = 0; i < 256; i++)
            {
                histogramGrayscale[i] = 0;
            }
            for (int i = 0; i < bm.Width; i++)
            {
                for (int j = 0; j < bm.Height; j++)
                {
                    var p = bm.GetPixel(i, j).getAverage();
                    histogramGrayscale[p]++;
                }
            }

            var totalPixels = bm.Width * bm.Height;
            double withinClassVariance = 1000000.0;
            var optimal_threshold = 0;
            for (int current_threshold = 0; current_threshold < 256; current_threshold++)
            {
                long sumOfPixelCountsBackground = 0;
                long countTimesPixelSumBackground = 0;



                for (int j = 0; j < current_threshold; j++)
                {
                    var pixelCount = histogramGrayscale[j];
                    sumOfPixelCountsBackground += pixelCount;
                    countTimesPixelSumBackground += j * pixelCount;
                }
                if (sumOfPixelCountsBackground == 0) continue;
                float weightBackground = (float)sumOfPixelCountsBackground / (float)totalPixels;
                float meanBackground = (float)countTimesPixelSumBackground / (float) sumOfPixelCountsBackground;

                double varianceBackgroundTop = 0;
                for(int j = 0; j < current_threshold; j++)
                {
                    varianceBackgroundTop += Math.Pow(j-meanBackground,2) * histogramGrayscale[j];
                }
                double varianceBackground = varianceBackgroundTop / sumOfPixelCountsBackground;


                //FOREGROUND

                long sumOfPixelCountsForeground = 0;
                long countTimesPixelSumForeground = 0;
                for (int j = current_threshold; j < 256; j++)
                {
                    var pixelCount = histogramGrayscale[j];
                    sumOfPixelCountsForeground += pixelCount;
                    countTimesPixelSumForeground += j * pixelCount;
                }
                if (sumOfPixelCountsForeground == 0) continue;
                float meanForeground = countTimesPixelSumForeground / (float)sumOfPixelCountsForeground;

                double varianceForegroundTop = 0;
                for (int j = current_threshold; j < 256; j++)
                {
                    varianceForegroundTop += Math.Pow(j - meanForeground, 2) * histogramGrayscale[j];
                }
                double varianceForeground = varianceForegroundTop / sumOfPixelCountsForeground;

                var localWithinClassVariance = weightBackground * varianceBackground + (1 - weightBackground) * varianceForeground;

                if(localWithinClassVariance<withinClassVariance)
                {
                    withinClassVariance = localWithinClassVariance;
                    optimal_threshold = current_threshold;
                }
            }
            return binarization(bm, optimal_threshold);
        }

     


        public static Bitmap bernsen(Bitmap bm, int window, int userinput_contrast_threshold)
        {
            //1. wczytaj obraz
            //2. zdefiniuj obszar sasiedztwa (window) i wartość progową kontr (userinput_contrast_threshold
            var tmp = bm;
            int center_offset = window / 2;
            for (int i = center_offset; i < bm.Height - center_offset; i++)
            {
                for (int j = center_offset; j < bm.Width - center_offset; j++)
                {
                    var window_values = new List<int>();
                    //przelec po wszystkich pikselach w oknie
                    for (int x = -1; x < window - 1; x++)
                    {
                        for (int y = -1; y < window - 1; y++)
                        {
                            var p = tmp.GetPixel(j + y, i + x);
                            window_values.Add(p.G);
                        }
                    }

                    var mingray = window_values.Min(); //3. znajdz najwieksza wartosc poz szarosci
                    var maxgray = window_values.Max(); //4. znajdz najmniejsza wartosc poz szarosci

                    //5. wyznacz wartość progu na podstawie wzoru
                    var mid_gray = (window_values.Min() + window_values.Max()) / 2;
                    //6. wyznacz wartość kontrastu na podstawie wzoru
                    var local_contrast = (window_values.Max() - window_values.Min()) / 2;

                    //7. oznacz piksel jako przynalezacy do tla lub obiektu
                    //8. powtorz dla kazdego piksela na obrazie
                    for (int x = -1; x < window - 1; x++)
                    {
                        for (int y = -1; y < window - 1; y++)
                        {
                            var p = tmp.GetPixel(j + y, i + x);
                            Color color;
                            if (local_contrast < userinput_contrast_threshold)
                            {
                                //zakladamy ze sasiedztwo jest z jednej klasy
                                color = (mid_gray >= 255 / 2) ? Color.White : Color.Black;
                            }
                            else
                            {
                                color = (p.getAverage() >= mid_gray) ? Color.White : Color.Black;
                            }

                            tmp.SetPixel(j + y, i + x, color);
                        }

                    }
                }
            }
            return tmp;
        }
    }
}
