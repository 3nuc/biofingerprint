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
        private static bool[,] visited;
        private static short xsize = 0;
        private static short ysize = 0;
        private static void initialize(Bitmap bm)
        {
            xsize = (short)bm.Width;
            ysize = (short)bm.Height;
            Console.WriteLine("Initialize(), width:" + xsize + " ysize: " + ysize);
            //all black pixels get marked as "1" in the array, white are "0"
            var kmmLevels = new short[bm.Width, bm.Height]; //filled with 0
            for (int i = 1; i < bm.Width - 1; i++)
            {
                for (int j = 1; j < bm.Height - 1; j++)
                {
                    var black = Color.FromArgb(255, 0, 0, 0);
                    if (bm.GetPixel(i, j) == black)
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
            if (kmmArray[pixelX, pixelY] == 1)
            {
                var bottomright = kmmArray[pixelX + 1, pixelY + 1];
                var topleft = kmmArray[pixelX - 1, pixelY - 1];

                var topright = kmmArray[pixelX + 1, pixelY - 1];
                var bottomleft = kmmArray[pixelX - 1, pixelY + 1];


                if (topleft == 0 || topright == 0 || bottomleft == 0 || bottomright == 0) return true;
            }
            return false;
        }

        private static bool isPixelEdgesTouchZero(int pixelX, int pixelY)
        {

            if (kmmArray[pixelX, pixelY] == 1)
            {
                var left = kmmArray[pixelX - 1, pixelY];
                var right = kmmArray[pixelX + 1, pixelY];

                var up = kmmArray[pixelX, pixelY - 1];
                var down = kmmArray[pixelX, pixelY + 1];

                if (left == 0 || right == 0 || up == 0 | down == 0) return true;
            }
            return false;
        }


        private static void runEdgeAndCornerCheck()
        {
            //Piksele 1, które posiadają sąsiadów o oznaczeniu 0 po bokach, u góry lub u dołu, oznaczamy jako 2.

            for (int i = 0; i < xsize; i++)
            {
                for (int j = 0; j < ysize; j++)
                {
                    if (isPixelEdgesTouchZero(i, j)) kmmArray[i, j] = 2;
                    if (kmmArray[i, j] == 1 && isPixelCornersTouchZero(i, j)) kmmArray[i, j] = 3;
                }
            }
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
                    short currentMaskPositionWeight = FingerprintConstants.checkerMask[k + 1, l + 1];
                    if (pixel > 0)
                        weight += currentMaskPositionWeight;
                }
            }
            return weight;
        }

        private static void addFours()
        {
            for (int i = 1; i < xsize - 1; i++)
            {
                for (int j = 1; j < ysize - 1; j++)
                {
                    //(1 of 2)Dla pikseli oznaczonych jako 2 ...
                    if (kmmArray[i, j] != 2) continue;
                    short weight = getWeight(i, j);
                    Console.Write(weight);
                    // Jeśli waga znajduje się na liście czwórki oznaczenie piksela zamieniamy z 2 na 4.
                    if (FingerprintConstants.fourths.Contains(weight))
                    {
                        kmmArray[i, j] = 4;
                        Console.Write("! ");
                    }
                }
                Console.WriteLine();
            }
        }

        private static void printArray()
        {
            for (int i = 0; i < xsize; i++)
            {
                for (int j = 0; j < ysize; j++)
                {
                    Console.Write(kmmArray[i, j]);
                }
                Console.WriteLine();
            }
        }

        public static void cutOut(short level)
        {
            for (int i = 1; i < xsize; i++)
            {
                for (int j = 1; j < ysize; j++)
                {
                    if (kmmArray[i, j] != level) continue;
                    var weight = getWeight(i, j);
                    if (FingerprintConstants.removals.Contains(weight))
                    {
                        kmmArray[i, j] = 0;
                    }
                    else
                    {
                        kmmArray[i, j] = 1;
                    }
                }
            }
        }

        private static bool arrayIdenticalToKmmArray(short[,] data)
        {
            for (int i = 0; i < xsize; i++)
            {
                for (int j = 0; j < ysize; j++)
                {
                    if (data[i, j] != kmmArray[i, j]) return false;
                }
            }
            return true;
        }

        private static Bitmap kmmArrayToBitmap()
        {
            var bm = new Bitmap(xsize, ysize);
            for (int i = 0; i < xsize; i++)
            {
                for (int j = 0; j < ysize; j++)
                {
                    Color color;
                    color = kmmArray[i, j] == 0 ? Color.White : Color.Black;
                    if (kmmArray[i, j] > 1) // wykryta minucja
                    {
                        // punkt
                        if (kmmArray[i, j] == 10)
                        {
                            color = Color.Gold;
                        }
                        // zakonczenie
                        if (kmmArray[i, j] == 11)
                        {
                            color = Color.Red;
                        }
                        // rozwidlenie
                        if (kmmArray[i, j] == 13)
                        {
                            color = Color.Green;
                        }
                        // skrzyzowanie
                        if (kmmArray[i, j] == 14)
                        {
                            color = Color.Blue;
                        }
                    }
                    bm.SetPixel(i, j, color);
                }
            }
            return bm;
        }

        public static Bitmap KMM(Bitmap bm)
        {
            var tmp = Binarization.binarization(new Bitmap(bm), 127);
            //Wszystkie czarne piksele oznaczamy jako 1, zaś białe jako 0.
            initialize(tmp); //contains 0 and 1

            int counter = 0;
            short[,] saveInitial;
            do
            {
                counter++;
                saveInitial = (short[,])kmmArray.Clone();
                //Piksele 1, które posiadają sąsiadów o oznaczeniu 0 po bokach, u góry lub u dołu, oznaczamy jako 2.
                //Pozostałe piksele 1, które posiadają sąsiadów o oznaczeniu 0 na rogach, oznaczamy jako 3.
                runEdgeAndCornerCheck(); //contins 0,1,2,3
#if VERBOSE
                Console.WriteLine("After runEdgeAndCornerCheck() (should contain 0123)");
                printArray();
#endif
                //Dla pikseli oznaczonych jako 2 za pomocą maski sprawdzarka obliczamy ich wagę. Jeśli waga znajduje się na liście czwórki oznaczenie piksela zamieniamy z 2 na 4.
                addFours(); //contains 0,1,2,3,4
#if VERBOSE
                Console.WriteLine("After addFours() (should contain 01234)");
                printArray();
#endif


                /*
                 Dla wszystkich pikseli oznaczonych jako 4 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                 Dla wszystkich pikseli oznaczonych jako 2 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                 Dla wszystkich pikseli oznaczonych jako 3 wyliczamy wagę za pomocą maski sprawdzarka. Jeśli waga znajduje się na liście wycięcia, zamieniamy piksel na 0, zaś w przeciwnym razie zamieniamy go na 1.
                */
                cutOut(4); //contains 0,1
                cutOut(2);
                cutOut(3);

#if VERBOSE
                Console.WriteLine("After cutOut() (should be only 0 and 1)");
                printArray();

                Console.WriteLine("Comparing current image to previous...");
                if (arrayIdenticalToKmmArray(saveInitial))
                {
                    Console.WriteLine("No change since last iteration - EXITING");
                }
                else
                {
                    Console.WriteLine("Current and last iteration are different - RUNNING NEW ITERATION");
                }
#endif
            } while (!arrayIdenticalToKmmArray(saveInitial)); //powtarzamy dopoki operacja wywoluje zmiany
            Console.WriteLine("KMM Finished after " + counter + " iterations");
            return kmmArrayToBitmap();
        }

        private static short[] GetCNBlock(int x, int y)
        {
            short[] block = new short[8];

            int index = 0;
            for (int i = -1; i < 2; i++)
            {
                if (x + i < 0 || x + i > xsize) continue;

                for (int j = -1; j < 2; j++)
                {
                    if (y + j < 0 || y + j > ysize) continue;
                    if (i == 0 && j == 0) continue;

                    block[index++] = (short)(kmmArray[x + i, y + j] != 0 ? 1 : 0);
                }
            }

            return block;
        }

        private static int CN(int x, int y)
        {
            short[] block = GetCNBlock(x, y);
            int sum = 0;

            for (int i = 0; i < 7; i++)
            {
                sum += Math.Abs(block[i] + block[i + 1]);
            }
            sum += Math.Abs(block[7] + block[0]);
            sum /= 2;

            return sum;
        }

        // CN: 0 - pojedynczy pkt, 1 - zakonczenie krawedzi, 2 - kontynuacja krawedzi, 3 - rozwidlenie, 4 - skrzyzowanie
        // odpowienio w kmmArray: 10, 11, 1(brak), 13, 14
        public static Bitmap FindMinutiae(Bitmap bm)
        {
            initialize(bm);

            int cn;
            List<Minutia> minutiae = new List<Minutia>();
            for (int x = 0; x < xsize; x++)
            {
                for (int y = 0; y < ysize; y++)
                {
                    if (kmmArray[x, y] == 0) continue;

                    cn = CN(x, y);

                    if (cn > 4) throw new Exception();

                    if (cn != 2)
                    {
                        minutiae.Add(new Minutia { X = x, Y = y, Type = cn });
                        //kmmArray[x, y] = (short)(cn + 10);
                    }
                }
            }

            DelShortRidges(minutiae);

            foreach (var item in minutiae)
            {
                kmmArray[item.X, item.Y] = (short)(item.Type + 10);
            }

            return kmmArrayToBitmap();
        }

        private static int IsConnected(int ax, int ay, int bx, int by, int range)
        {
            if (ax == bx && ay == by) return 0;
            if (range < 0) return Int32.MinValue;

            for (int i = -1; i < 2; i++)
            {
                if (ax + i < 0 || ax + i > xsize) continue;

                for (int j = -1; j < 2; j++)
                {
                    if (ay + j < 0 || ay + j > ysize) continue;
                    if (i == 0 && j == 0) continue;

                    if(kmmArray[ax + i, ay + j] != 0 && !visited[ax + i, ay + j])
                    {
                        visited[ax + i, ay + j] = true;
                        int dist = IsConnected(ax + i, ay + j, bx, by, range - 1) + 1;
                        if (dist > 0)
                        {
                            return dist;
                        }
                    }
                }
            }

            return Int32.MinValue;
        }

        // CN: 0 - pojedynczy pkt, 1 - zakonczenie krawedzi, 2 - kontynuacja krawedzi, 3 - rozwidlenie, 4 - skrzyzowanie
        private static void DelShortRidges(List<Minutia> m)
        {
            for (int i = m.Count - 1; i >= 0; i--)
            {
                if (m[i].Type != 1) continue;

                int currx = m[i].X, curry = m[i].Y;
                int index = -1;
                int dist = int.MaxValue;
                for (int j = 0; j < m.Count - 1; j++)
                {
                    if (i == j) continue;
                    if (m[j].IsInRange(m[i], 10))
                    {
                        visited = new bool[xsize, ysize];
                        int d = IsConnected(m[i].X, m[i].Y, m[j].X, m[j].Y, 5);
                        if (d > 0)
                        {
                            if (d < dist)
                            {
                                index = j;
                                dist = d;
                            }
                        }
                    }
                }
                if (index > -1)
                {
                    m.RemoveAt(index);
                    if (i > m.Count - 1)
                    {
                        i--;
                    }
                    m.RemoveAll(x => x.X == currx && x.Y == curry);
                }
            }
        }
    }
}