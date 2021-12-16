using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SmartPenSketch
{
    class SketchRecTools
    {
        #region Resampling Methods

        public static List<List<SketchPoint>> copySubstrokes(List<List<SketchPoint>> ss)
        {
            List<List<SketchPoint>> newls = new List<List<SketchPoint>>(ss.Count);
            for (int i = 0; i < ss.Count; i++)
            {
                newls.Add(new List<SketchPoint>(ss[i].Count));
                for (int j = 0; j < ss[i].Count; j++)
                    newls[i].Add(new SketchPoint(ss[i][j]));
            }

            return newls;
        }

        //Resample that automatically determines the interspacing distance
        public static List<List<SketchPoint>> resampleByDistanceAuto(List<List<SketchPoint>> sketch)
        {
            //List<List<SketchPoint>> resampledPoints = generalResample(sketch, interspaceS);
            List<List<SketchPoint>> resampledSketch = new List<List<SketchPoint>>();
            foreach (List<SketchPoint> stroke in sketch)
            {
                double interspaceS = calculateSStroke(stroke);

                //===Interspace S, change to constant if needing constant
                List<SketchPoint> resampledPoints = generalResampleStroke(stroke, Math.Min(4, interspaceS));
                //===

                if(resampledPoints.Count <= 6 && stroke.Count <=10)
                {
                    Debug.WriteLine("TOO FEW POINTS TO RESAMPLE, DEFAULTING TO RAW POINTS");
                    resampledPoints = stroke.GetRange(0,stroke.Count);
                }
                resampledSketch.Add(resampledPoints.GetRange(0, resampledPoints.Count));
            }
            return resampledSketch;
        }

        //Resample that takes in user-defined distance
        public static List<List<SketchPoint>> resampleByDistance(List<List<SketchPoint>> sketch, double S)
        {
            List<List<SketchPoint>> resampledPoints = generalResample(sketch, S);
            return resampledPoints;
        }

        //Resample where interspace distance is by user-defined count.

        //General resample function
        public static List<List<SketchPoint>> generalResample(List<List<SketchPoint>> sketch, double S)
        {
            double stepDistance = 0.0;
            double calculatedDist = 0.0;
            List<SketchPoint> newSketchPoints = new List<SketchPoint>();
            List<List<SketchPoint>> newStrokes = new List<List<SketchPoint>>();
            SketchPoint prevPoint = new SketchPoint(0.0, 0.0, 0, 0);
            double interspaceDist = S;
            double resampledX, resampledY;
            long resampledT;
            int localIndex = 0;

            foreach (List<SketchPoint> stroke in sketch)
            {
                foreach (SketchPoint skPt in stroke)
                {
                    if (localIndex!=0) // check that we're not on the very first point
                    {
                        calculatedDist = calculateDistance(prevPoint, skPt);

                        if (stepDistance + calculatedDist >= interspaceDist)
                        {
                            resampledX = prevPoint.x + ((interspaceDist - stepDistance) / calculatedDist) * (skPt.x - prevPoint.x);
                            resampledY = prevPoint.y + ((interspaceDist - stepDistance) / calculatedDist) * (skPt.y - prevPoint.y);
                            resampledT = skPt.milliseconds;
                            SketchPoint newPt = new SketchPoint(resampledX, resampledY, skPt.force, resampledT);
                            newSketchPoints.Add(newPt);

                            stepDistance = 0.0;
                            //calculated distance - step distance
                        }
                        else
                        {
                            stepDistance += calculatedDist;
                            //Debug.WriteLine(stepDistance);
                        }
                    }
                    if (localIndex==0) //add the first point automatically to the resampled list
                    {
                        SketchPoint newPt = skPt;
                        newSketchPoints.Add(newPt);
                    }

                    localIndex++;
                    prevPoint.setSketchPoint(skPt.x, skPt.y, skPt.force, skPt.milliseconds);
                }
                stepDistance = 0.0;
                localIndex = 0;
                newStrokes.Add(new List<SketchPoint>());
                foreach (SketchPoint skPt in newSketchPoints)
                    newStrokes[newStrokes.Count - 1].Add(skPt);


                newSketchPoints.Clear();
            }
            return newStrokes;
        }

        public static List<SketchPoint> generalResampleStroke(List<SketchPoint> stroke, double S)
        {
            double stepDistance = 0.0;
            double calculatedDist = 0.0;
            List<SketchPoint> newSketchPoints = new List<SketchPoint>();
            SketchPoint prevPoint = new SketchPoint(0.0, 0.0, 0, 0);
            double interspaceDist = S;
            double resampledX, resampledY;
            long resampledT;
            int localIndex = 0;

            foreach (SketchPoint skPt in stroke)
            {
                if (localIndex != 0) // check that we're not on the very first point
                {
                    calculatedDist = calculateDistance(prevPoint, skPt);

                    while (stepDistance + calculatedDist >= interspaceDist)
                    {
                        double ratio = ((interspaceDist - stepDistance) / calculatedDist);
                        resampledX = prevPoint.x +  ratio * (skPt.x - prevPoint.x);
                        resampledY = prevPoint.y + ratio * (skPt.y - prevPoint.y);

                        resampledT = skPt.milliseconds;
                        SketchPoint newPt = new SketchPoint(resampledX, resampledY, skPt.force, resampledT);
                        newSketchPoints.Add(newPt);

                        stepDistance = 0.0;
                        prevPoint.setSketchPoint(newPt.x, newPt.y, newPt.force, newPt.milliseconds);
                        calculatedDist = calculateDistance(prevPoint, skPt);

                    }

                    stepDistance += calculatedDist;
                   // Debug.WriteLine(stepDistance);
                }
                if (localIndex == 0) //add the first point automatically to the resampled list
                {
                    SketchPoint newPt = skPt;
                    newSketchPoints.Add(newPt);
                }

                localIndex++;
                prevPoint.setSketchPoint(skPt.x, skPt.y, skPt.force, skPt.milliseconds);
            }
            stepDistance = 0.0;
            localIndex = 0;

            return newSketchPoints;
        }

        //Function to automatically generate S
        public static double calculateS(List<List<SketchPoint>> sketch)
        {
            double S;
            SketchBox boundingBox = calculateBoundingBox(sketch);
            double diagonal = calculateDistance(boundingBox.minX, boundingBox.minY, boundingBox.maxX, boundingBox.maxY);
            S = diagonal / 40.0; //Hard-coded form original JS code, may need to check this.
            //Debug.WriteLine(S);
            return S;
        }

        private static double calculateSStroke(List<SketchPoint> stroke)
        {
            double S;
            SketchBox boundingBox = calculateBoundingBox(stroke);
            double diagonal = calculateDistance(boundingBox.minX, boundingBox.minY, boundingBox.maxX, boundingBox.maxY);
            S = diagonal / 40.0; //Hard-coded form original JS code, may need to check this.
            //Debug.WriteLine(S);
            return S;
        }
        #endregion

        #region Bounding Box Class, Functions
        public class SketchBox
        {
            public double minX = double.MaxValue, minY = double.MaxValue, maxX = 0, maxY = 0;
            public double centerX, centerY, boxWidth, boxHeight;
            public Point topLeft, topRight, bottomLeft, bottomRight, center;

            public SketchBox()
            {
                this.minX = double.MaxValue;
                this.minY = double.MaxValue;
                this.maxX = 0;
                this.maxY = 0;
                this.centerX = 0;
                this.centerY = 0;
                this.boxWidth = 0;
                this.boxHeight = 0;

                this.topLeft.X = 0; this.topLeft.Y = 0;
                this.topRight.X = 0; this.topRight.Y = 0;
                this.bottomLeft.X = 0; this.bottomLeft.Y = 0;
                this.bottomRight.X = 0; this.bottomRight.Y = 0;
                this.center.X = 0; this.center.Y = 0;
            }

            public SketchBox(double mnX, double mnY, double mxX, double mxY, double cX, double cY,
                double bxW, double bxH, Point tpL, Point tpR, Point btL, Point btR, Point ct)
            {
                this.minX = mnX;
                this.minY = mnY;
                this.maxX = mxX;
                this.maxY = mxY;
                this.centerX = cX;
                this.centerY = cY;
                this.boxWidth = bxW;
                this.boxHeight = bxH;

                this.topLeft = tpL;
                this.topRight = tpR;
                this.bottomLeft = btL;
                this.bottomRight = btR;
                this.center = ct;
            }
        }
        public static SketchBox calculateBoundingBox(List<List<SketchPoint>> sketch)
        {
            double minX = double.MaxValue, minY = double.MaxValue, maxX = 0, maxY = 0;
            double centerX, centerY, boxWidth, boxHeight;
            Point topLeft, topRight, bottomLeft, bottomRight, center;

            foreach (List<SketchPoint> strokes in sketch)
            {
                foreach (SketchPoint skPt in strokes)
                {
                    if (skPt.x < minX)
                        minX = skPt.x;
                    if (skPt.y < minY)
                        minY = skPt.y;
                    if (skPt.x > maxX)
                        maxX = skPt.x;
                    if (skPt.y > maxY)
                        maxY = skPt.y;
                }
            }
            centerX = minX + ((maxX - minX) / 2);
            centerY = minY + ((maxY - minY) / 2);

            topLeft.X = minX; topLeft.Y = minY;
            topRight.X = maxX; topRight.Y = minY;
            bottomLeft.X = minX; bottomLeft.Y = maxY;
            bottomRight.X = maxX; bottomRight.Y = maxY;

            center.X = centerX; center.Y = centerY;
            boxWidth = maxX - minX;
            boxHeight = maxY - minY;

            SketchBox boundBox = new SketchBox(minX, minY, maxX, maxY, centerX, centerY, boxWidth, boxHeight,
                topLeft, topRight, bottomLeft, bottomRight, center);
            return boundBox;
        }

        public static SketchBox calculateBoundingBox(List<SketchPoint> stroke)
        {
            double minX = double.MaxValue, minY = double.MaxValue, maxX = 0, maxY = 0;
            double centerX, centerY, boxWidth, boxHeight;
            Point topLeft, topRight, bottomLeft, bottomRight, center;

            foreach (SketchPoint skPt in stroke)
            {
                if (skPt.x < minX)
                    minX = skPt.x;
                if (skPt.y < minY)
                    minY = skPt.y;
                if (skPt.x > maxX)
                    maxX = skPt.x;
                if (skPt.y > maxY)
                    maxY = skPt.y;
            }
            centerX = minX + ((maxX - minX) / 2);
            centerY = minY + ((maxY - minY) / 2);

            topLeft.X = minX; topLeft.Y = minY;
            topRight.X = maxX; topRight.Y = minY;
            bottomLeft.X = minX; bottomLeft.Y = maxY;
            bottomRight.X = maxX; bottomRight.Y = maxY;

            center.X = centerX; center.Y = centerY;
            boxWidth = maxX - minX;
            boxHeight = maxY - minY;

            SketchBox boundBox = new SketchBox(minX, minY, maxX, maxY, centerX, centerY, boxWidth, boxHeight,
                topLeft, topRight, bottomLeft, bottomRight, center);
            return boundBox;
        }
        #endregion

        public static double calculateDistance(SketchPoint skPt0, SketchPoint skPt1)
        {
            return Math.Sqrt((skPt1.x - skPt0.x) * (skPt1.x - skPt0.x) + (skPt1.y - skPt0.y) * (skPt1.y - skPt0.y));
        }

        public static double calculateDistance(double x0, double y0, double x1, double y1)
        {
            return Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
        }

        public static List<List<SketchPoint>> ConvertToPixels(List<List<SketchPoint>> sketch, double canvasWidth, double canvasHeight)
        {
            foreach(List<SketchPoint> stroke in sketch)
            {
                foreach(SketchPoint skPt in stroke)
                {
                    skPt.x = canvasWidth * (skPt.x) / 210;
                    skPt.y = canvasHeight * (skPt.y) / 297;
                }
            }
            return sketch;
        }

        public static double calculatePathLength(List<SketchPoint> stroke)
        {

            double distances = 0.0;
            int j = 0;

            foreach (SketchPoint point in stroke)
            {
                SketchPoint p0 = point;
                if (j < stroke.Count - 1)
                {
                    SketchPoint p1 = stroke[j + 1];
                    distances += calculateDistance(p0.x, p0.y, p1.x, p1.y);
                }
                j++;
            }
            return distances;
        }
    }

}
