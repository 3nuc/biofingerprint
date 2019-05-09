using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class Minutia
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Type { get; set; }

        public bool IsInRange(Minutia other, double range)
        {
            double dist = Math.Sqrt(Math.Pow((X - other.X), 2) + Math.Pow((Y - other.Y), 2));

            return dist < range;
        }
    }
}
