using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenSketch
{
    class ShortStraw
    {
        private const int W = 3;
        public static List<List<int>> RunShortStraw(List<List<SketchPoint>> resampledSketch)
        {
            List<List<int>> sketchCorners = new List<List<int>>();
            foreach (List<SketchPoint> stroke in resampledSketch)
            {
                List<int> strokeCorners = ShortStrawCorners(stroke);
                sketchCorners.Add(strokeCorners.GetRange(0,strokeCorners.Count));
                strokeCorners.Clear();
            }
            return sketchCorners;
        }

        public static List<int> ShortStrawCorners(List<SketchPoint> points)
        {

            List<int> corners = new List<int>();

            corners.Add(0);

            if (points.Count == 1)
                return corners;

            List<double> straws = new List<double>();
            //double[] straws = new double[points.Count - 1];

            for (int i = W; i < points.Count - W; i++)
            {
                SketchPoint straw0 = points[i - W];
                SketchPoint strawN = points[i + W];
                double strawDistance = SketchRecTools.calculateDistance(straw0.x, straw0.y, strawN.x, strawN.y);
                straws.Add(strawDistance);
            }
            ///////////////////////////////////////////////////////////////PATCH TO GET THE CODE WORKING///////////////////////////////////////////////////////////////////////////////////////
            ///This only occurs if the amount of samples on any given line is smaller or equal to 6, due to the window size being 3.
            ///This only happens when calculating bounding box by auto-interspace screws up and is undesired behavior.
            if (straws.Count == 0) {
                Debug.WriteLine("CORNER FINDING FAILED WITH STRAWS = 0");
                return new List<int>();
            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Calculate the pseudo-median

            double t = ShortStraw.calculateMedian(straws) * 0.95;

            // iterate through each straw distance
            for (int i = 0; i < straws.Count; i++)
            {

                // case: the current straw distance is less than the pseudo-median
                if (straws[i] < t)
                {

                    // initialize the local min and min index's starting values
                    double localMin = Double.MaxValue;
                    int localMinIndex = i;

                    // iterate through the local straw distance cluster
                    while (i < straws.Count && straws[i] < t)
                    {

                        // local min (i.e., corner index) candidate found
                        if (straws[i] < localMin)
                        {
                            localMin = straws[i];
                            localMinIndex = i;
                        }

                        // iterate through the local cluster
                        // note: need to iterate through i in inner loop to skip local cluster in outer loop
                        i = i + 1;
                    }

                    // add the corner index of local cluster to the corner indices array
                    // note: need to add W to offset between the straw indices and point indices
                    corners.Add(localMinIndex + W);

                }
            }
            // add the last point index
            corners.Add(points.Count - 1);

            corners = postProcessCorners(points, corners, straws);
            return corners;
        }

        public static double calculateMedian(List<double> straws)
        {
            List<double> temporary = new List<double>(straws);
            int count = temporary.Count;

            temporary.Sort();

            double medianValue = 0;
             if (count % 2 == 0)
            {
                // count is even, need to get the middle two elements, add them together, then divide by 2
                double middleElement1 = temporary[(count / 2) - 1];
                double middleElement2 = temporary[(count / 2)];
                medianValue = (middleElement1 + middleElement2) / 2;
            }
            else
            {
                // count is odd, simply get the middle element.
                medianValue = temporary[(count / 2)];
            }

            return medianValue;

        }

        public static List<int> postProcessCorners(List<SketchPoint> points, List<int> corners, List<double> straws)
        {
            // ----- start corner post-processing check #1 -----
            bool advance = false;
            while (!advance)
            {
                advance = true;

                // iterate through the corner indices
                for (int i = 1; i < corners.Count; i++)
                {
                    // get the previous and current corner indices
                    int c1 = corners[i - 1];
                    int c2 = corners[i];

                    // check if line is formed between previous and current corner indices
                    bool lineBool = isLine(points, c1, c2);
                    if (!isLine(points, c1, c2))
                    {

                        // get the candidate halfway corner
                        // offset it by W due to straw indices and points indices mis-match
                        int newCorner = halfwayCorner(straws, c1, c2);
                        newCorner = newCorner + W;

                        // skip adding new corner, since it already exists
                        // can happen during an overzealous halfway corner calculation
                        if (newCorner == c2)
                        {
                            continue;
                        }

                        corners.Insert(i, newCorner);
                        advance = false;
                    }
                }

                // emergency stop
                if (corners.Count > 15) { Debug.WriteLine("WARNING: Infinite Loop"); break; }
            }
            // ----- end corner post-processing check #1 -----

            // ----- start corner post-processing check #2 -----
            for (int i = 1; i < corners.Count - 1; i++)
            {
                int c1 = corners[i - 1];
                int c2 = corners[i + 1];

                bool lineBool = isLine(points, c1, c2);
                if (lineBool)
                {
                    corners.RemoveAt(i);
                    i = i - 1;
                }
            }

            // ----- end corner post-processing check #2 -----
            return corners;
        }

        public static bool isLine(List<SketchPoint> points, int a, int b)
        {
            //ArraySegment<SketchPoint> subset = new ArraySegment<SketchPoint>(points, a, b);
            ///////////////////////////////////////////////////////////////PATCH TO GET THE CODE WORKING///////////////////////////////////////////////////////////////////////////////////////
            if (a>=b)
            {
                Debug.WriteLine("SUBSET INITIALIZED WHERE a !< b, returning false statement");
                return true;
            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            int upperBound = b - a;
            if(a + upperBound > points.Count-1)
            {
                b = points.Count-1;
                upperBound = b - a;
            }
            List<SketchPoint> subset = points.GetRange(a, upperBound);
            //points.slice(a, b-a);

            double threshold = 0.95;
            SketchPoint startPoint = points[a];
            SketchPoint endPoint = points[b];

            double ax = startPoint.x;
            double ay = startPoint.y;
            double bx = endPoint.x;
            double by = endPoint.y;
            double distance = SketchRecTools.calculateDistance(ax, ay, bx, by);
            double pathDistance = SketchRecTools.calculatePathLength(subset);
            bool returnLine = (distance / pathDistance > threshold);
            return returnLine;
        }

        public static int halfwayCorner(List<double> straws, int a, int b)
        {

            int quarter = (b - a) / 4;
            double minValue = Double.MaxValue;
            var minIndex = a + quarter;

            for (var i = a + quarter; i < b - quarter; i++)
            {
                if (straws.Count > i && straws[i] < minValue)  //Check for OOR and exit if it is
                {
                    minValue = straws[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }
    }
}


