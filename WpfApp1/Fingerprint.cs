using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class Fingerprint
    {
        private short[,] getInitialKMMArray(Bitmap bm)
        {
            //all black pixels get marked as "1" in the array, white are "0"
            var kmmLevels = new short[bm.Width, bm.Height]; //filled with 0
            for (int i = 0; i < bm.Width; i++)
            {
                for (int j = 0; j < bm.Height; j++)
                {
                    if(bm.GetPixel(i,j) == Color.Black)
                    {
                        kmmLevels[i, j] = 1;
                    }
                }
            }
            return kmmLevels;
        }

        private short[,] getEdgeCheckedKmmArray(short[,] kmmArray, int x, int y)
        {
            var edgeChecked = kmmArray;

            bool pixelEdgesTouchZero(int pixelX, int pixelY) {
            
                if(pixelX>0 && pixelY>0 && kmmArray[pixelX,pixelY] == 1)
                {
                    var left = kmmArray[pixelX - 1, pixelY];
                    var right = kmmArray[pixelX + 1, pixelY];

                    var up = kmmArray[pixelX, pixelY - 1];
                    var down = kmmArray[pixelX, pixelY - 1];

                    if (left == 0 || right == 0 || up == 0 | down == 0) return true; 
                }
            return false;
            }

            bool pixelCornersTouchZero(int pixelX, int pixelY)
            {
                if (pixelX > 0 && pixelY > 0 && kmmArray[pixelX, pixelY] == 1)
                {
                    var topleft = kmmArray[pixelX - 1, pixelY - 1];
                    var bottomleft = kmmArray[pixelX - 1, pixelY + 1];

                    var topright = kmmArray[pixelX + 1, pixelY - 1];
                    var bottomright = kmmArray[pixelX + 1, pixelY + 1];

                    if (topleft == 0 || topright == 0 || bottomleft == 0 || bottomright == 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            for(int i =0; i < x;i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if(pixelEdgesTouchZero(i, j)) edgeChecked[i,j]=2;
                    if(pixelCornersTouchZero(i,j)) edgeChecked[i,j]=3;
                }
            }

        }
        public Bitmap KMM(Bitmap bm)
        {
            var tmp = Binarization.binarization(new Bitmap(bm), 255 / 2);


        }
    }
}
