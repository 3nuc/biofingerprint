using System.Windows.Controls;

namespace WpfApp1
{
    public static class Helpers
    {
        public static int[,] toLatticeArray(TextBox top, TextBox mid, TextBox low)
        {
            var topa = top.Text.Split(' ');
            var mida = mid.Text.Split(' ');
            var lowa = low.Text.Split(' ');
            var lattice = new int[3, 3];
            lattice[0, 0] = int.Parse(topa[0]);
            lattice[1, 0] = int.Parse(topa[1]);
            lattice[2, 0] = int.Parse(topa[2]);
            lattice[0, 1] = int.Parse(mida[0]);
            lattice[1, 1] = int.Parse(mida[1]);
            lattice[2, 1] = int.Parse(mida[2]);
            lattice[0, 2] = int.Parse(lowa[0]);
            lattice[1, 2] = int.Parse(lowa[1]);
            lattice[2, 2] = int.Parse(lowa[2]);
            return lattice;
        }
        public static int getSumOfLattice(int[,] lattice, int iMax, int jMax)
        {
            int sum = 0;
            for (int i = 0; i < iMax; i++)
            {
                for (int j = 0; j < jMax; j++)
                {
                    sum += lattice[i, j];
                }
            }
            return sum;
        }

        public static int clamp0to255(int value)
        {
            if(value>255)
            {
                return 255;
            }
            else if(value<0)
            {
                return 0;
            }
            else
            {
                return value;
            }
        }
    }
}