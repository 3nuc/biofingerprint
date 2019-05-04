using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public static class Fingerprint
    {
        private static short[,] kmmArray = null;
        private static short xsize = 0;
        private static short ysize = 0;
        private static void initialize(Bitmap bm)
        {
            xsize = (short)bm.Width;
            ysize = (short)bm.Height;
            //all black pixels get marked as "1" in the array, white are "0"
            var kmmLevels = new short[bm.Width, bm.Height]; //filled with 0
            for (int i = 0; i < bm.Width; i++)
            {
                for (int j = 0; j < bm.Height; j++)
                {
                    //remove borders
                    if (i == 0 || i == bm.Width - 1 || j == 0 || j == bm.Height - 1)
                    {
                        kmmLevels[i, j] = 0;
                        continue;
                    }
                    if (bm.GetPixel(i, j) == Color.Black)
                    {
                        kmmLevels[i, j] = 1;
                    }
                }
            }
            kmmArray = kmmLevels;
        }

        private static bool isPixelCornersTouchZero(int pixelX, int pixelY)
        {
            //Pozostałe piksele 1, które posiadają sąsiadów o oznaczeniu 0 na rogach, oznaczamy jako 3.
            if (pixelX > 0 && pixelY > 0 && kmmArray[pixelX, pixelY] == 1)
            {
                var topleft = kmmArray[pixelX - 1, pixelY - 1];
                var bottomleft = kmmArray[pixelX - 1, pixelY + 1];

                var topright = kmmArray[pixelX + 1, pixelY - 1];
                var bottomright = kmmArray[pixelX + 1, pixelY + 1];

                if (topleft == 0 || topright == 0 || bottomleft == 0 || bottomright == 0) return true;
            }
            return false;
        }

        private static bool isPixelEdgesTouchZero(int pixelX, int pixelY)
        {

            if (pixelX > 0 && pixelY > 0 && kmmArray[pixelX, pixelY] == 1)
            {
                var left = kmmArray[pixelX - 1, pixelY];
                var right = kmmArray[pixelX + 1, pixelY];

                var up = kmmArray[pixelX, pixelY - 1];
                var down = kmmArray[pixelX, pixelY - 1];

                if (left == 0 || right == 0 || up == 0 | down == 0) return true;
            }
            return false;
        }


        private static void runEdgeAndCornerCheck()
        {
            //Piksele 1, które posiadają sąsiadów o oznaczeniu 0 po bokach, u góry lub u dołu, oznaczamy jako 2.
            var edgeChecked = kmmArray;

            for (int i = 0; i < xsize; i++)
            {
                for (int j = 0; j < ysize; j++)
                {
                    if (isPixelEdgesTouchZero(i, j)) edgeChecked[i, j] = 2;
                    if (edgeChecked[i, j] == 1 && isPixelCornersTouchZero(i, j)) edgeChecked[i, j] = 3;
                }
            }
            kmmArray = edgeChecked;
        }

        private static short getWeight(int pixelX, int pixelY)
        {
            short weight = 0;
            for (int k = -1; k < 2; k++)
            {
                for (int l = -1; l < 2; l++)
                {
                    //(2 of 2)... za pomocą maski sprawdzarka obliczamy ich wagę.
                    var pixel = kmmArray[pixelX + k, pixelY + l];
                    short currentMaskPositionWeight = FingerprintConstants.checkerMask[k, l];
                    if (pixel > 0)
                        weight += currentMaskPositionWeight;
                }
            }
            return weight;
        }

        private static short[,] addFours()
        {
            short[,] checkerMask = FingerprintConstants.checkerMask;
            short[,] result = kmmArray;

            for (int i = 1; i < xsize; i++)
            {
                for (int j = 1; j < ysize; j++)
                {
                    //(1 of 2)Dla pikseli oznaczonych jako 2 ...
                    if (kmmArray[i, j] != 2) continue;
                    short weight = getWeight(i, j);
                    // Jeśli waga znajduje się na liście czwórki oznaczenie piksela zamieniamy z 2 na 4.
                    if (FingerprintConstants.fourths.Contains(weight))
                        result[i, j] = 4;
                }
            }
            return result;
        }

        public static void cutOut()
        {
            for (int i = 1; i < xsize; i++)
            {
                for (int j = 1; j < ysize; j++)
                {
                    /*  
                        Dla wszystkich pikseli oznaczonych jako 4 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                        Dla wszystkich pikseli oznaczonych jako 2 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                        Dla wszystkich pikseli oznaczonych jako 3 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                    */
                    if (kmmArray[i, j] <= 1)
                        continue;
                    var weight = getWeight(i, j);
                    if (kmmArray[i, j] > 1 && FingerprintConstants.removals.Contains(weight))
                        kmmArray[i, j] = 0;
                    else kmmArray[i, j] = 1;
                }
            }
        }

        private static bool compareMultidimensionalArrays(short[,] data1, short[,] data2)
        {
            //i stole this from stackoverflow lmao
            return data1.Rank == data2.Rank &&
            Enumerable.Range(0, data1.Rank).All(dimension => data1.GetLength(dimension) == data2.GetLength(dimension)) &&
            data1.Cast<short>().SequenceEqual(data2.Cast<short>());
        }

        private static Bitmap kmmArrayToBitmap()
        {
            var bm = new Bitmap((int)xsize, (int)ysize);
            for (int i = 0; i < xsize; i++)
            {
                for (int j = 0; j < ysize; j++)
                {
                    if (kmmArray[i, j] > 1) throw new Exception(); //to jest zly exception ale nie chce mi sie nowego tworzyc
                    var color = kmmArray[i, j] == 0 ? Color.Black : Color.White;
                    bm.SetPixel(i, j, color);
                }
            }
            return bm;
        }

        public static Bitmap KMM(Bitmap bm)
        {
            var tmp = Binarization.binarization(new Bitmap(bm), 255 / 2);
            //Wszystkie czarne piksele oznaczamy jako 1, zaś białe jako 0.
            initialize(tmp); //contains 0 and 1

            short[,] saveInitial;
            do
            {
                saveInitial = kmmArray;
                //Piksele 1, które posiadają sąsiadów o oznaczeniu 0 po bokach, u góry lub u dołu, oznaczamy jako 2.
                //Pozostałe piksele 1, które posiadają sąsiadów o oznaczeniu 0 na rogach, oznaczamy jako 3.
                runEdgeAndCornerCheck(); //contins 0,1,2,3

                //Dla pikseli oznaczonych jako 2 za pomocą maski sprawdzarka obliczamy ich wagę. Jeśli waga znajduje się na liście czwórki oznaczenie piksela zamieniamy z 2 na 4.
                addFours(); //contains 0,1,2,3,4


                /*
                 Dla wszystkich pikseli oznaczonych jako 4 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                 Dla wszystkich pikseli oznaczonych jako 2 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                 Dla wszystkich pikseli oznaczonych jako 3 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                */
                cutOut(); //contains 0,1
            } while (!compareMultidimensionalArrays(kmmArray, saveInitial)); //powtarzamy dopoki operacja wywoluje zmiany
            return kmmArrayToBitmap();
        }
    }
}
