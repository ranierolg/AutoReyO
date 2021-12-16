using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenSketch
{
    public class PointCloudRecognizerPlus
    {
        /// <summary>
        /// Main function of the $P+ recognizer.
        /// Classifies a candidate gesture against a set of training samples.
        /// Returns a float of the distance of the closest neighbor in the template set.
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="trainingSet"></param>
        /// <returns></returns>
        public static float Classify(Gesture candidate, Gesture[] trainingSet, out string gestureOut)
        {
            float minDistance = float.MaxValue;
            string gestureClass = "";
            foreach (Gesture template in trainingSet)
            {
                if (candidate.Name == template.Name)
                {
                    //Gesture temp = template;
                    float dist = GreedyCloudMatch(candidate.Points, template.Points);
                    /*if (candidate.Name == "det15")
                    {
                        Debug.WriteLine("Detail 15 compared against: " + template.Name + " " + dist);
                    }*/
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        gestureClass = template.Name;

                    }
                }
            }
            gestureOut = gestureClass;
            Debug.WriteLine(candidate.Name + " Predicted as: " + gestureClass + " with a distance of: " + minDistance);
            return minDistance;
        }

        /// <summary>
        /// Implements greedy search for a minimum-distance matching between two point clouds
        /// using local shape descriptors (theta turning angles).
        /// </summary>
        private static float GreedyCloudMatch(DollarPoint[] points1, DollarPoint[] points2)
        {
            float[] theta1 = ComputeLocalShapeDescriptors(points1);    // should be pre-processed in the Gesture class
            float[] theta2 = ComputeLocalShapeDescriptors(points2);    // should be pre-processed in the Gesture class
            return Math.Min(
                CloudDistance(points1, theta1, points2, theta2),
                CloudDistance(points2, theta2, points1, theta1)
            );
        }

        /// <summary>
        /// Computes the distance between two point clouds 
        /// using local shape descriptors (theta turning angles).
        /// </summary>
        private static float CloudDistance(DollarPoint[] points1, float[] theta1, DollarPoint[] points2, float[] theta2)
        {
            bool[] matched = new bool[points2.Length];
            Array.Clear(matched, 0, points2.Length);

            float sum = 0; // computes the cost of the cloud alignment
            int index;

            // match points1 to points2
            for (int i = 0; i < points1.Length; i++)
            {
                sum += GetClosestPointFromCloud(points1[i], theta1[i], points2, theta2, out index);
                matched[index] = true;
            }

            // match points2 to points1
            for (int i = 0; i < points2.Length; i++)
                if (!matched[i])
                    sum += GetClosestPointFromCloud(points2[i], theta2[i], points1, theta1, out index);

            return sum;
        }


        /// <summary>
        /// Searches for the point from point-cloud cloud that is closest to point p.
        /// </summary>
        private static float GetClosestPointFromCloud(DollarPoint p, float theta, DollarPoint[] cloud, float[] thetaCloud, out int indexMin)
        {
            float min = float.MaxValue;
            indexMin = -1;
            for (int i = 0; i < cloud.Length; i++)
            {
                float dist = (float)Math.Sqrt(Geometry.SqrEuclideanDistance(p, cloud[i]) + (theta - thetaCloud[i]) * (theta - thetaCloud[i]));
                if (dist < min)
                {
                    min = dist;
                    indexMin = i;
                }
            }
            return min;
        }

        /// <summary>
        /// Computes local shape descriptors (theta turning angles) at each point on the gesture path.
        /// </summary>
        public static float[] ComputeLocalShapeDescriptors(DollarPoint[] points)
        {
            int n = points.Length;
            float[] theta = new float[n];

            theta[0] = theta[n - 1] = 0;
            for (int i = 1; i < n - 1; i++)
                theta[i] = (float)(ShortAngle(points[i - 1], points[i], points[i + 1]) / Math.PI);
            return theta;
        }

        /// <summary>
        /// Computes the smallest turning angle between vectors (a,b) and (b,c) in radians in the interval [0..PI].
		/// </summary>
        public static float ShortAngle(DollarPoint a, DollarPoint b, DollarPoint c)
        {
            // compute path lengths for vectors (a,b) and (b,c)
            float length_ab = Geometry.EuclideanDistance(a, b);
            float length_bc = Geometry.EuclideanDistance(b, c);
            if (Math.Abs(length_ab * length_bc) <= float.Epsilon)
                return 0;

            // compute cosine of the angle between vectors (a,b) and (b,c)
            double cos_angle = ((b.X - a.X) * (c.X - b.X) + (b.Y - a.Y) * (c.Y - b.Y)) / (length_ab * length_bc);

            // deal with special cases near limits of the [-1,1] interval
            if (cos_angle <= -1.0) return (float)Math.PI;
            if (cos_angle >= 1.0) return 0.0f;

            // return the angle between vectors (a,b) and (b,c) in the interval [0,PI]
            return (float)Math.Acos(cos_angle);
        }
    }
}
