using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SmartPenSketch
{
    class IStraw
    {
        public static List<List<int>> RunIStraw(List<List<SketchPoint>> resampledSketch)
        {
            List<List<int>> sketchCorners = new List<List<int>>();
            foreach(List<SketchPoint> stroke in resampledSketch)
            {
                List<int> strokeCorners = IStrawCorners(stroke);
                sketchCorners.Add(strokeCorners);
            }
            return sketchCorners;
        }
        public static List<int> IStrawCorners(List<SketchPoint> points) //called "getCorners in JS code
        {
            //initialize array of corner indices
            List<int> corners = new List<int>();

            //add 0th index to corner indices
            corners.Add(0);

            ///handle singleton-stroke case
            if (points.Count == 1)
                return corners;

            //window length
            int W = 3;

            //initialize straws and fill with 0s
            double[] straws = new double[points.Count-1];
            for (int i = 0; i < straws.Length; i++)
                straws[i] = 0;

            //set the straw distances for the points OUTSIDE the window
            straws[1] = distance(points[0], points[1 + W]) * ((2 * W) / (1 + W));
            straws[2] = distance(points[0], points[2 + W]) * ((2 * W) / (2 + W));
            straws[points.Count - 2] = ((2 * W) / (1 + W)) * distance(points[points.Count - 1], points[points.Count - 2 - W]);
            straws[points.Count - 3] = ((2 * W) / (2 + W)) * distance(points[points.Count - 1], points[points.Count - 3 - W]);

            //set the straw distances for the points INSIDE the window
            for(int i = W; i<points.Count - W; i++)
            {
                straws[i] = distance(points[i - W], points[i + W]);
            }

            corners = initCorners(points, corners, straws, W);
            corners = polylineProc(points, corners, straws);
            corners = curveProcessPass1(points, corners);
            corners = curveProcessPass2(points, corners);

            return corners;
        }

        #region Four Main Processing Functions
        public static List<int> initCorners(List<SketchPoint> points, List<int> corners, double[] straws, int W)
        {
            List<double> strawSlices = new List<double>();
            //double[] strawSlices = new double[straws.Length];
            //Array.Copy(straws, 1, strawSlices, 0, straws.Length);
            foreach (double str in straws)
                strawSlices.Add(str);
            strawSlices.RemoveAt(0);
            double pseudoMean = mean(strawSlices) * 0.95;

            for(int i = W; i < points.Count - W - 1; i++)
            {
                if(straws[i] < pseudoMean)
                {
                    double localMin = straws[i];
                    int localMinIndex = i;
                    while(i<points.Count - W && straws[i] < pseudoMean)
                    {
                        if (straws[i] < localMin)
                        {
                            localMin = straws[i];
                            localMinIndex = i;
                        }
                        i++;
                    }
                    corners.Add(localMinIndex);
                }
            }

            corners.Add(points.Count - 1);

            //get the array of point times
            List<long> times = new List<long>();
            foreach(SketchPoint skPt in points)
            {
                times.Add(skPt.getTime());
            }

            //long = int64
            long meanTime = Convert.ToInt64(times.Average());

            for(int i =1; i < corners.Count - 1; i++)
            {
                int c1 = corners[i - 1];
                int c2 = corners[i];

                if(c2 - c1 >= 6)
                {
                    int localMaxIndex = c1 + 3;
                    long localMax = points[localMaxIndex].getTime();

                    for (int j = c1 +3; j <= c2 - 3; j++)
                    {
                        if(localMax < points[j].getTime())
                        {
                            localMax = points[j].getTime();
                            localMaxIndex = j;
                        }
                        if(localMax > 2 * meanTime)
                        {
                            corners.Insert(i, localMaxIndex);
                        }
                    }
                }
            }

            //Did not insert debug stuff into here

            return corners;
        }

        public static List<int> curveProcessPass1(List<SketchPoint> points, List<int> corners)
        {
            //Debug garbo not included

            int preCorner = corners[0];

            for(int i = 1; i < corners.Count - 1; i++)
            {
                double[] angles = compAngles1(points, corners, i);
                preCorner = corners[i];
                bool notCorner = notCorner1(angles, corners, i);
                if (notCorner)
                {
                    corners.RemoveAt(i);
                    i--;
                }

            }

            //More debug garbo

            return corners;
        }

        public static List<int> curveProcessPass2(List<SketchPoint> points, List<int> corners)
        {
            for(int i = 1; i < corners.Count - 1; i++)
            {
                double[] angles = compAngles2(points, corners, i);
                bool notCorner = false;

                if ((angles[2] > 26.1 + 0.93 * angles[1] && ((angles[3] > 31 + angles[1] && angles[3] > 100) || angles[3] > 161))
                    || ((angles[0] == 0) && angles[2] - angles[1] > 15 && angles[3] > 20)) //angles[0] == 0 originally is a === comparison in the JS code. In this specific context I don't think it makes a difference.
                {
                    notCorner = true;
                    if(notCorner || angles[0]> 0)
                    {
                        corners.RemoveAt(i);
                        i--;
                    }
                }
            }

            //More debug stuff that I'm skipping

            return corners;
        }

        public static List<int> polylineProc(List<SketchPoint> points, List<int> corners, double[] straws)
        {
            bool loop = true;
            while (loop)
            {
                loop = false;
                for (int i = 1; i < corners.Count; i++)
                {
                    //Get consecutive corner pairs

                    int c1 = corners[i - 1];
                    int c2 = corners[i];

                    //check if proper line exists between consecutive corner pairs
                    bool lineBool = isLine(points, c1, c2, 0.975);

                    //If the path between consecutive corner pair does not form a line,
                    //then insert a corner in-between the pair in question
                    if (!lineBool)
                    {
                        //create the new halfway-corner
                        int newC = halfwayCorners(straws, c1, c2);

                        corners.Insert(i, newC);        //Syntax in original code seems like they misused slice. If code doesn't work, check here.

                        //With a new corner added, the halfway corner search has to be restarted
                        //when this for-loop ends, restart the for-loop again
                        loop = true;
                        //code comments out a "leave" flag that doesn't exist. I believe they intended to make "loop" = true instead. 
                        //They already misused slice above, so this snippet in practice probably doesn't actually work as intended in the original JS code
                        //Check entire polylineProc code if the code doesn't work in C#
                    }
                }
            }

            //debug garbo
            corners = adjustCorners(points, corners);
            corners = tripletCollinearTest(points, corners);
            corners = shapeNoiseProcess(points, corners, straws);

            return corners;
        }
        #endregion


        #region Math Helper Functions
        public static double distance(SketchPoint skP0, SketchPoint skP1)
        {
            double deltaX = skP1.x - skP0.x;
            double deltaY = skP1.y - skP0.y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
        public static double mean(List<double> values)
        {
            double sum = values.Sum();
            return sum / values.Count();
        }
        public static long meanLong(List<long> values)
        {
            long sum=0;
            foreach (long val in values)
                sum += val;
            return sum / values.Count();
        }
        public static double getAngle(SketchPoint center, SketchPoint start, SketchPoint end)
        {
            double returnAngle, theAngle;
            //get the two vectors of the angle
            Point direction1 = new Point(start.x - center.x, start.y - center.y);
            direction1 = normalize(direction1);
            Point direction2 = new Point(end.x - center.x, end.y - center.y);
            direction2 = normalize(direction2);

            //Compute the angle between the two vectors
            theAngle = Math.Acos(direction1.X * direction2.X + direction1.Y * direction2.Y);
            returnAngle = theAngle * 180 / Math.PI;

            return returnAngle;
        }
        public static Point normalize(Point pt)
        {
            //THIS USES WINDOWS.FONDATION.POINT, NOT SKETCHPOINT, BECAUSE WE DON'T NEED TIME DATA FOR THIS
            double a = pt.X, b = pt.Y;

            double v = Math.Sqrt((a*a)+(b*b));
            Point u = new Point((a / v), (b / v));
            return u;
        }
        public static double Distance(SketchPoint p0, SketchPoint p1)
        {
            double deltaX = p1.x - p0.x;
            double deltaY = p1.y - p0.y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
        public static double pathDistance(List<SketchPoint> points, int a, int b)
        {
            double d = 0;
            for (int i = a; i < b; i++)
                d += Distance(points[i], points[i + 1]);
            return d;
        }
        #endregion

        #region Processing Helper Functions
        public static bool diffDir(List<SketchPoint> points, int o, int a, int b, int c, int d)
        {
            Point d0 = new Point(points[a].x - points[o].x, points[a].y - points[o].y);
            Point d1 = new Point(points[o].x - points[b].x, points[o].y - points[b].y);
            Point d2 = new Point(points[c].x - points[o].x, points[c].y - points[o].y);
            Point d3 = new Point(points[o].x - points[d].x, points[o].y - points[d].y);
            double cross0 = (d0.X * d1.Y) - (d0.Y * d1.X);
            double cross1 = (d2.X * d3.Y) - (d2.Y * d3.X);
            double result = cross0 * cross1;
            return result > 0;
        }
        public static bool isLine(List<SketchPoint> points, int a, int b, double threshold)
        {
            double distance = Distance(points[a], points[b]);
            double pDistance = pathDistance(points, a, b);
            return (distance / pDistance) > threshold;
        }
        public static bool notCorner1(double[] angles, List<int> corners, int i)
        {
            if (angles[3] > 161)
                return true;
            if ((angles[2] > 36 + 0.85 * angles[1]) && (angles[1] > 20) && (angles[3] > 80 + 0.55 * angles[1]))
                return true;
            if ((corners[i] - corners[i - 1] < 3 || corners[i + 1] - corners[i] < 3) && (angles[2] > 130))
                return true;

            return false;
        }

        public static double[] compAngles1(List<SketchPoint> points, List<int> corners, int ind) {
            int c = corners[ind];
            SketchPoint pos = points[c];
            int s = c - 12;

            if (s < corners[ind - 1])
                s = corners[ind - 1];

            int e = c + 12;
            if (e > corners[ind + 1])
                e = corners[ind + 1];

            double a1 = getAngle(pos, points[s], points[e]);
            s = corners[ind] - Convert.ToInt32(Math.Ceiling(Convert.ToDouble((c - s) / 3)));
            e = corners[ind] - Convert.ToInt32(Math.Ceiling(Convert.ToDouble((c - e) / 3)));

            double a2 = getAngle(pos, points[s], points[e]);
            double a3 = getAngle(pos, points[c - 1], points[c + 1]);
            if((c - corners[ind-1] > 6))
                a3 = getAngle(pos, points[c - 2], points[c + 1]);
            if ((corners[ind + 1] - c) > 6)
                a3 = getAngle(pos, points[c - 1], points[c + 2]);

            double[] a = new double[4] { 0, a1, a2, a3 };
            return a;
        }
        public static double[] compAngles2(List<SketchPoint> points, List<int> corners, int ind)
        {
            double a0 = 0, a1 = 0, a2 = 0, a3 = 0;

            int c = corners[ind];
            SketchPoint pos = points[c];
            int s0 = c - 12;
            if(s0 < corners[ind - 1])
                s0 = corners[ind - 1];

            int e0 = c + 12;
            if (e0 > corners[ind + 1])
                e0 = corners[ind + 1];

            int s1 = c - Convert.ToInt32(Math.Ceiling(Convert.ToDouble((corners[ind] - s0) / 3)));
            int e1 = c - Convert.ToInt32(Math.Ceiling(Convert.ToDouble((corners[ind] - e0) / 3)));
            a3 = getAngle(pos, points[c - 1], points[c + 1]);
            if(diffDir(points, c, s0, e0, s1, e1))
            {
                s0 = c - 4;
                e0 = c + 4;
                if (s0 < corners[ind - 1])
                    s0 = corners[ind - 1];

                if (e0 > corners[ind + 1])
                    e0 = corners[ind + 1];

                s1 = c - 1;
                e1 = c + 1;
                if(diffDir(points, c, s0, e0, s1, e1))
                {
                    a0 = -1;
                    double[] retArray = new double[4] { a0, a1, a2, a3 };
                    return retArray;
                }
                a0 = 0;
            }
            else if(!isLine(points, c, corners[ind - 1], 0.975)
                && isLine(points, c, corners[ind + 1], 0.975)){
                if(diffDir(points, c, s0, s1, e1, e0) && a3 > 135)
                {
                    a0 = 1;
                }
            }
            a1 = getAngle(pos, points[s0], points[e0]);
            a2 = getAngle(pos, points[s1], points[e1]);

            double[] returnArray = new double[4] { a0, a1, a2, a3 };
            return returnArray;

        }

        public static int halfwayCorners(double[] straws, int a, int b)
        {
            int quarter = Convert.ToInt32(Math.Floor(Convert.ToDouble((b - a) / 4)));
            int minValue = Int32.MaxValue;
            int minIndex = a + quarter;
            for (int i = a + quarter; i<b-quarter; i++)
            {
                if (straws[i] < minValue)
                {
                    minValue = Convert.ToInt32(straws[i]);
                    minIndex = i;
                }
            }
            return minIndex;
        }
        public static List<int> adjustCorners(List<SketchPoint> points, List<int> corners)
        {
            //debug stuff

            for(int i = 1; i< corners.Count - 2; i++)
            {
                int index = corners[i];
                if(index > 2 && index < points.Count - 3)
                {
                    List<SketchPoint> pos = new List<SketchPoint>();
                    List<double> angle = new List<double>();
                    for(int j = 0; j <= 6; j++)
                        pos.Add(points[index - 3 + j]);

                    for (int j = 0; j <= 4; j++)
                        angle.Add(getAngle(pos[j + 1], pos[j], pos[j + 2]));

                    if(angle[1] < angle[3])
                    {
                        if (angle[0] < angle[1] && angle[0] < angle[2])
                            index = index - 2;
                        else if (angle[1] < angle[2])
                            index = index - 1;
                    }
                    else
                    {
                        if (angle[4] < angle[3] && angle[4] < angle[2])
                            index = index + 2;
                        else if (angle[3] < angle[2])
                            index++;
                    }
                    corners[i] = index;

                }
            }

            return corners;
        }
        public static List<int> tripletCollinearTest(List<SketchPoint> points, List<int> corners)
        {
            //debug stuff

            for (int i = 1; i < corners.Count - 1; i++)
            {
                var c1 = corners[i - 1];
                var c2 = corners[i + 1];
                if (isLine(points, c1, c2, 0.988))
                {
                    //console.log("triplet collinear test: !!!");
                    corners.RemoveAt(i);
                    i = i - 1;
                }
            }

            List<long> times = new List<long>();
            foreach (SketchPoint point in points)
                times.Add(point.getTime());

            var meanTime = meanLong(times);
            for (int i = 1; i < corners.Count - 1; i++)
            {
                var c = corners[i];
                var c1 = corners[i - 1];
                var c2 = corners[i + 1];
                var threshold = 0.9747;
                if (c2 - c1 > 10)
                {
                    threshold = threshold + 0.0053;
                }
                if ((points[c].getTime() > 2 * meanTime) || (points[c - 1].getTime() > 2 * meanTime) || (points[c + 1].getTime() > 2 * meanTime))
                {
                    threshold = threshold + 0.0066;
                }
                if (isLine(points, c1, c2, threshold))
                {
                    corners.RemoveAt(i);
                    i = i - 1;
                }
            }

            // debug
            /*if (this.isDebug)
            {
                console.log("    after (" + corners.length + "): " + this.debugCorners(corners));
                console.log("----- end tripletCollinearTest -----");
            }*/
            // end debug

            return corners;
        }
        public static List<int> shapeNoiseProcess(List<SketchPoint> points, List<int> corners, double[] straws)
        {
            /*if (this.isDebug)
            {
                console.log("----- 5. shapeNoiseProcess -----");
                console.log("   before (" + corners.length + "): " + this.debugCorners(corners));
            }*/

            for (var i = 1; i < corners.Count; i++)
            {
                var c1 = corners[i - 1];
                var c2 = corners[i];
                if ((c2 - c1 <= 1) || ((c2 - c1 <= 2) && (i == 0 && i == corners.Count - 2))) // last two comparative statements used === originally
                {
                    // console.log("c1: " + c1);
                    // console.log("c2: " + c2);
                    if (straws[c1] < straws[c2])
                    {
                        corners.RemoveAt(c2);
                    }
                    else
                    {
                        corners.RemoveAt(c1);
                    }
                    i--;
                }
            }

            /*if (this.isDebug)
            {
                console.log("    after (" + corners.length + "): " + this.debugCorners(corners));
                console.log("----- end shapeNoiseProcess -----");
            }*/

            return corners;
        }


        #endregion

    }
}
