using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenSketch
{
    public class SketchSubstroke
    {

        const double MAX_TANGENT_SLOPE = 1.47; //Cutsoff around 10 size slope, when the floats start becoming innacurate
        const double DEFAULT_MARGIN_PCT = 0.15; //gives a margin based on percentage distance, VERY broken right now
        const double MIN_EXTENTION_LENGTH = 20.0; // ensures short lines produce at least somewhat of an extension length
        private List<SketchPoint> points;
        private SketchSubstroke parent { get; set; }
        //`private t` is also a varaible that was included in the original SRLSubstroke class, but I have not been able to determine its type
        //Once we have the SRLSubstroke Class we will know what this variable is and we will enable it

        public SketchSubstroke()
        {
            this.points = new List<SketchPoint>();
            this.parent = null;
        }

        public SketchSubstroke(List<SketchPoint> points, SketchSubstroke parent = null)
        {
            if (points.First().x > points.Last().x)
                points.Reverse();
            this.points = points;
            this.parent = parent;
        }
        public SketchPoint getFirst()
        {
            return this.points.First();
        }

        public SketchPoint getLast()
        {
            return this.points.Last();
        }
        
        public List<SketchPoint> get_points()
        {
            return points;
        }
        
        public static List<SketchSubstroke> createSubstrokesFromIndices(List<SketchPoint> points, List<int> indices)
        {
            int currentNode = 0;
            List<SketchSubstroke> substrokes = new List<SketchSubstroke>(indices.Count);
            for(int i = 1; i < indices.Count; i++)

            {
                substrokes.Add(
                    new SketchSubstroke(points.GetRange(currentNode, indices[i] - currentNode + 1))
                );

                currentNode = indices[i];
            }

            return substrokes;
        } 

        public bool inXRange(double x, SketchSubstroke ss, double margin = DEFAULT_MARGIN_PCT, double min_margin = MIN_EXTENTION_LENGTH) //Margin is a percent in this case, this is to either side 
        {
            if (ReferenceEquals(this.parent, ss.parent) && !ReferenceEquals(this.parent, null))
                return false;

            double x0 = this.getFirst().x;
            double y0 = this.getFirst().y;
            double x1 = this.getLast().x;
            double y1 = this.getLast().y;
            double a1 = (y1 - y0) / (x1 - x0);

            //Calculation terms
            double mlsq = (Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2)) * margin * margin;
            double xmargin = Math.Max(Math.Sqrt(mlsq / (1 + a1 * a1)), min_margin);

            // Lines should always be ordered in the X direction 
            return x > (this.getFirst().x - xmargin) && x < (this.getLast().x + xmargin);
        }

        public bool inYRange(double y, SketchSubstroke ss, double margin = DEFAULT_MARGIN_PCT, double min_margin = MIN_EXTENTION_LENGTH)
        {
            if (ReferenceEquals(this.parent, ss.parent) && !ReferenceEquals(this.parent, null))
                return false;

            double x0 = this.getFirst().x;
            double y0 = this.getFirst().y;
            double x1 = this.getLast().x;
            double y1 = this.getLast().y;
            double a1 = (x1 - x0) / (y1 - y0);
            //double a0 = x0 - a1 * y0;
            //Calculation terms
            double mlsq = (Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2)) * margin * margin;
            double ymargin = Math.Max(Math.Sqrt(mlsq / (1 + a1 * a1)), min_margin);

            if (this.getFirst().y < this.getLast().y)
                return y > (this.getFirst().y - ymargin) && y < (this.getLast().y + ymargin);
            else
                return y < (this.getFirst().y + ymargin) && y > (this.getLast().y - ymargin);
        }

        public List<SketchSubstroke> splitAtNodeIndex(int index, double x, double y)
        {
            //Copy first half
            List<SketchSubstroke> lss = new List<SketchSubstroke>();
            SketchSubstroke front = new SketchSubstroke(this.points.GetRange(0, index+1), this.parent ?? (this));
            //Place on closest end
            /*
            if (
                (Math.Pow(front.points[0].x - x, 2) + Math.Pow(front.points[0].y - y, 2)) <
                (Math.Pow(front.points[front.points.Count - 1].x - x, 2) + Math.Pow(front.points[front.points.Count - 1].y - y, 2))
            )
            {
                front.points[0].x = x;
                front.points[0].y = y;
            }
            else
            {
                front.points[front.points.Count - 1].x = x;
                front.points[front.points.Count - 1].y = y;
            }
            */
            
            
            lss.Add(front);

            //Do the same only if the next half exists
            SketchSubstroke back;
            if ((this.points.Count - (index + 1)) != 0)
            {
                back = new SketchSubstroke(this.points.GetRange(index + 1, this.points.Count - (index + 1)), this.parent ?? (this));
                //back.points.Insert(0, new SketchPoint(back.getFirst().x, back.getFirst().y, 0, 0));
                
                /*
                if (
                    (Math.Pow(back.points[0].x - x, 2) + Math.Pow(back.points[0].y - y, 2)) <
                    (Math.Pow(back.points[back.points.Count - 1].x - x, 2) + Math.Pow(back.points[back.points.Count - 1].y - y, 2))
                )
                {
                    back.points[0].x = x;
                    back.points[0].y = y;
                }
                else
                {
                    back.points[back.points.Count - 1].x = x;
                    back.points[back.points.Count - 1].y = y;
                }
                */
                
                
                lss.Add(back);
            }

            return lss;
        }
        
        //Finds a minimum intersect between two vectors
        //Currently gets the two points of minumum distance and generates the ideal intercept from there
        //for example, if for vector i and j returns 4,12, this means that the closest point exits between point 4&5 in i and point 12&13 in j
        //This can definitely be sped up, but this is a proof of concept
        public static Tuple<int, int> findMinimumDistVec(SketchSubstroke i_sub, SketchSubstroke j_sub)
        {
            //Setting up the basic variable names
            List<SketchPoint> i_vec = i_sub.points;
            List<SketchPoint> j_vec = j_sub.points;
            int minloci = 0;
            int minlocj = 0;
            double minval = Double.MaxValue;
            double tempval = 0;
            int finalloci = 0;
            int finallocj = 0;

            //Find minimum points
            for (int i = 0; i < i_vec.Count; i++) for (int j = 0; j < j_vec.Count; j++) //Matrix for loop
                if ((tempval = Math.Pow(i_vec[i].x - j_vec[j].x, 2) + Math.Pow(i_vec[i].y - j_vec[j].y, 2)) < minval)
                {
                    minloci = i;
                    minlocj = j;
                    minval = tempval;
                }


            //Does not intersect if the minimum is towards the end
            //Performed for i vector
            if (i_vec.Count == 2) finalloci = 0;
            else if (minloci == i_vec.Count - 1) finalloci = minloci;
            else if (minloci == 0) finalloci = minloci;
            else
            {
                //Enumerate the points we need to test (directions refer to directionality in a matrix)
                double left = Math.Pow(i_vec[minloci - 1].x - j_vec[minlocj].x, 2) + Math.Pow(i_vec[minloci - 1].y - j_vec[minlocj].y, 2);
                double right = Math.Pow(i_vec[minloci + 1].x - j_vec[minlocj].x, 2) + Math.Pow(i_vec[minloci + 1].y - j_vec[minlocj].y, 2);

                if (left - minval < right - minval) finalloci = minloci - 1;
                else finalloci = minloci;
            }
            
            //performed for j vector
            if (j_vec.Count == 2) finallocj = 0;
            else if(minlocj == j_vec.Count - 1) finallocj = minlocj;
            else if (minlocj == 0) finallocj = minlocj;
            else
            {
                //Enumerate the points we need to test (directions refer to directionality in a matrix)
                double up = Math.Pow(i_vec[minloci].x - j_vec[minlocj - 1].x, 2) + Math.Pow(i_vec[minloci].y - j_vec[minlocj - 1].y, 2);
                double down = Math.Pow(i_vec[minloci].x - j_vec[minlocj + 1].x, 2) + Math.Pow(i_vec[minloci].y - j_vec[minlocj + 1].y, 2);
                if (up - minval < down - minval) finallocj = minlocj - 1;
                else finallocj = minlocj;
            }

            return new Tuple<int, int>(finalloci, finallocj);
        }

        //Inserts a dummy point into the segment to account for line segment point intradistance
        //Do not allow singleton strokes
        public static List<SketchSubstroke> minDiv(SketchSubstroke a, SketchSubstroke b, double x, double y)
        {
            List<SketchSubstroke> lss = new List<SketchSubstroke>();
            Tuple<int, int> indices = findMinimumDistVec(a, b);

            //Though it would not break anything, we still do not allow intersections to be only one line
            if (!(indices.Item1 == 0 || indices.Item1 == a.points.Count - 1))
                lss.AddRange(a.splitAtNodeIndex(indices.Item1, x, y));
            else lss.Add(a);

            if (!(indices.Item2 == 0 || indices.Item2 == b.points.Count - 1))
                lss.AddRange(b.splitAtNodeIndex(indices.Item2, x, y));
            else lss.Add(b);

            if (lss.Count > 3)
                Debug.WriteLine("{0} {1} {2} {3}", lss[0].points.Count, lss[1].points.Count, lss[2].points.Count, lss[3].points.Count);
            return lss;
        }

        public static List<SketchSubstroke> intersect(SketchSubstroke a, SketchSubstroke b)
        {
            List<SketchSubstroke> lss = new List<SketchSubstroke>();
            double x0 = 0;
            double y0 = 0;
            double x1 = 0;
            double y1 = 0;
            double a1 = 0;
            double a0 = 0;
            double b1 = 0;
            double b0 = 0;
            double xint = 0;
            double yint = 0; 

            //Caculate first line
            x0 = a.getFirst().x;
            y0 = a.getFirst().y;
            x1 = a.getLast().x;
            y1 = a.getLast().y;
            a1 = (y1 - y0) / (x1 - x0);
            a0 = y0 - a1 * x0;
               
            //Calulate second line
            x0 = b.getFirst().x;
            y0 = b.getFirst().y;
            x1 = b.getLast().x;
            y1 = b.getLast().y;
            b1 = (y1 - y0) / (x1 - x0);
            b0 = y0 - b1 * x0;

            //Variables to save math results
            bool isInfA1 = Math.Atan(a1) > MAX_TANGENT_SLOPE || Math.Atan(a1) < -MAX_TANGENT_SLOPE || Double.IsNaN(a1);
            bool isInfB1 = Math.Atan(b1) > MAX_TANGENT_SLOPE || Math.Atan(b1) < -MAX_TANGENT_SLOPE || Double.IsNaN(b1);

            //This gets the intercept of the two lines, will not fail if NaN or whatever
            xint = (b0 - a0) / (a1 - b1);

            //Debug.WriteLine("{0} {1}", b1, Math.Atan(b1));

            if (isInfA1 && isInfB1)
            {
                //Debug.WriteLine("BOTH SLOPES TOO HIGH: THIS IS NOT BEING PROPERLY HANDLED YET");
                //lss.Add(a);
                //lss.Add(b);
                double ax1 = (a.getFirst().x - a.getLast().x) / (a.getFirst().y - a.getLast().y);
                double bx1 = (b.getFirst().x - b.getLast().x) / (b.getFirst().y - b.getLast().y);
                double ax0 = a.getFirst().x - a.getFirst().y * ax1;
                double bx0 = b.getFirst().x - b.getFirst().y * bx1;
                yint = (bx0 - ax0) / (ax1 - bx1);
                if (a.inYRange(yint, b, 0.25) && b.inYRange(yint, a, 0.25))
                {
                    lss.AddRange(minDiv(a, b, yint * ax1 + ax0, yint));
                }
                else
                {
                    lss.Add(a);
                    lss.Add(b);
                }

            }

            else if (isInfA1)
            {
                double xavg = a.points.Aggregate(0.0, (sum, next) => sum + next.x) / a.points.Count;
                double yval = xavg * b1 + b0;
                if (b.inXRange(xavg, a) && a.inYRange(yval, b))
                {
                    lss.AddRange(minDiv(a, b, xavg, yval));
                }
                else 
                {
                    lss.Add(a);
                    lss.Add(b);
                }
            }

            else if (isInfB1)
            {
                double xavg = b.points.Aggregate(0.0, (sum, next) => sum + next.x) / b.points.Count;
                double yval = xavg * a1 + a0;
                if (a.inXRange(xavg, b) && b.inYRange(yval, a))
                {
                    lss.AddRange(minDiv(a, b, xavg, yval));
                }
                else
                {
                    lss.Add(a);
                    lss.Add(b);
                }
            }

            //Both are just regular lines
            //Getting all four strokes
            else if (a.inXRange(xint, b) && b.inXRange(xint, a))
            {
                lss.AddRange(minDiv(a, b, xint, xint * a1 + a0));
            }
            else
            {
                lss.Add(a);
                lss.Add(b);
            }
           

            return lss;
        }
    }
}
