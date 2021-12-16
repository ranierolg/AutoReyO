using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SmartPenSketch
{
    class FeatureDetection
    {
        const int N_RUBINE = 13;

        public struct RubineBasic
        {
            public List<SketchPoint> stroke;
            public int strokeIndex;
            public double f1;
            public double f2;
            public double f3;
            public double f4;
            public double f5;
            public double f6;
            public double f7;
            public double f8;
            public double f9;
            public double f10;
            public double f11;
            public double f12;
            public double f13;

            RubineBasic (
                List<SketchPoint> stroke,
                int strokeIndex,
                double f1 = 0,
                double f2 = 0,
                double f3 = 0,
                double f4 = 0,
                double f5 = 0,
                double f6 = 0,
                double f7 = 0,
                double f8 = 0,
                double f9 = 0,
                double f10 = 0,
                double f11 = 0,
                double f12 = 0,
                double f13 = 0
            ) {
                this.stroke = stroke;
                this.strokeIndex = strokeIndex;
                this.f1 = f1;
                this.f2 = f2;
                this.f3 = f3;
                this.f4 = f4;
                this.f5 = f5;
                this.f6 = f6;
                this.f7 = f7;
                this.f8 = f8;
                this.f9 = f9;
                this.f10 = f10;
                this.f11 = f11;
                this.f12 = f12;
                this.f13 = f13;
            }
        }

        public static RubineBasic rubine_F(List<SketchPoint> points, int index)
        {
            // Structs are always pre zeroed
            RubineBasic F;
            if (points.Count < 3) return new RubineBasic();

            int lasti = points.Count - 1;

            double xmin = Math.Min(points[1].x, points[0].x);
            double ymin = Math.Min(points[1].y, points[0].y);
            double xmax = Math.Max(points[1].x, points[0].x);
            double ymax = Math.Max(points[1].y, points[0].y);

            double dxp = points[1].x - points[0].x;
            double dyp = points[1].y - points[0].y;
            double dxpm1 = 0;
            double dypm1 = 0;
            double thetap = 0;
            double timep = points[1].milliseconds - points[0].milliseconds;
            F.stroke = points;
            F.strokeIndex = index;
            F.f1 = (points[2].x - points[0].x) / Math.Sqrt(Math.Pow(points[2].x - points[0].x, 2) + Math.Pow(points[2].y - points[0].y, 2));
            F.f2 = (points[2].y - points[0].y) / Math.Sqrt(Math.Pow(points[2].x - points[0].x, 2) + Math.Pow(points[2].y - points[0].y, 2));
            F.f5 = Math.Sqrt(Math.Pow(points[lasti].x - points[0].x, 2) + Math.Pow(points[lasti].y - points[0].y, 2));
            F.f6 = (points[lasti].x - points[0].x) / F.f5;
            F.f7 = (points[lasti].y - points[0].y) / F.f5;
            F.f8 = Math.Sqrt(Math.Pow(dxp, 2) + Math.Pow(dyp, 2));
            F.f9 = 0;
            F.f10 = 0;
            F.f11 = 0;
            F.f12 = (Math.Pow(dxp, 2) - Math.Pow(dyp, 2)) / Math.Pow(timep, 2);
            F.f13 = points[lasti].milliseconds - points[0].milliseconds;

            for (int i = 1; i < points.Count - 1; i++)
            {
                dxpm1 = dxp;
                dypm1 = dyp;
                dxp = points[i + 1].x - points[i].x;
                dyp = points[i + 1].y - points[i].y;
                thetap = Math.Atan((dxp * dypm1 - dxpm1 * dyp) / (dxp * dxpm1 - dyp * dypm1));
                timep = Math.Max(points[i + 1].milliseconds - points[i].milliseconds, 1);

                xmin = Math.Min(xmin, points[i].x);
                ymin = Math.Min(ymin, points[i].y);
                xmax = Math.Max(xmax, points[i].x);
                ymax = Math.Max(ymax, points[i].y);

                F.f8 += Math.Sqrt(Math.Pow(dxp, 2) + Math.Pow(dyp, 2));
                F.f9 += thetap;
                F.f10 += Math.Abs(thetap);
                F.f11 += Math.Pow(thetap, 2);
                F.f12 = Math.Max(F.f12, (Math.Pow(dxp, 2) - Math.Pow(dyp, 2)) / Math.Pow(timep, 2));
            }

            F.f3 = Math.Sqrt(Math.Pow(xmax - xmin, 2) + Math.Pow(ymax - ymin, 2));
            F.f4 = Math.Atan((ymax - ymin) / (xmax - xmin));

            return F;
        }

        public static List<RubineBasic> rubineFStrokes(List<List<SketchPoint>> strokes)
        {
            return strokes.AsEnumerable().Select((stroke, index) => rubine_F(stroke, index)).ToList();
        }

        public static RubineBasic singleRubineStroke(List<List<SketchPoint>> strokes)
        {
            //Flatten all strokes, purely for comparison purposes
            List<SketchPoint> flattened_strokes = new List<SketchPoint>(strokes.SelectMany(i => i));
            return rubine_F(flattened_strokes, 0);
        }

        public static List<RubineBasic> labelTrueCurves(List<List<SketchPoint>> strokes, int maxcount)
        {
            List<FeatureDetection.RubineBasic> rubineScores = FeatureDetection.rubineFStrokes(strokes);
            List<FeatureDetection.RubineBasic> firstStage = rubineScores.OrderByDescending(
                score => score.f11 / score.f3).ToList();
            List<FeatureDetection.RubineBasic> secondStage = firstStage.GetRange(0, firstStage.Count / 5).OrderByDescending(
                score => score.f8 / score.f3).ToList();
            return secondStage.GetRange(0, Math.Min(secondStage.Count, maxcount));
        } 
    }
}
