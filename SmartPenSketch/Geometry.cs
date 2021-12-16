using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenSketch
{
    public class Geometry
    {
        /// <summary>
        /// Computes the Squared Euclidean Distance between two points in 2D
        /// </summary>
        public static float SqrEuclideanDistance(DollarPoint a, DollarPoint b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }

        /// <summary>
        /// Computes the Euclidean Distance between two points in 2D
        /// </summary>
        public static float EuclideanDistance(DollarPoint a, DollarPoint b)
        {
            return (float)Math.Sqrt(SqrEuclideanDistance(a, b));
        }
    }
}
