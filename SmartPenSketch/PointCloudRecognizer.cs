using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenSketch
{
    public class PointCloudRecognizer
    {
        /// <summary>
        /// Main function of the $P recognizer.
        /// Classifies a candidate gesture against a set of training samples.
        /// Returns the class of the closest neighbor in the training set.
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="trainingSet"></param>
        /// <returns></returns>
        public static string Classify(Gesture candidate, Gesture[] trainingSet)
        {
            float minDistance = float.MaxValue;
            string gestureClass = "";
            foreach (Gesture template in trainingSet)
            {
                //Testing to see if we get any better results by classifying against our expected gesture
                //if (template.Name == candidate.Name)
                {
                    
                    float dist = GreedyCloudMatch(candidate.Points, template.Points);
                    if (candidate.Name == "det6")
                    {
                        //Debug.WriteLine("Detail 6 compared against: " + template.Name + " " + dist);
                    }
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        gestureClass = template.Name;
                    }
                }
            }
            Debug.WriteLine(candidate.Name + " Predicted as: " + gestureClass + " with a distance of: " + minDistance);
            return gestureClass;
        }

        /// <summary>
        /// Implements greedy search for a minimum-distance matching between two point clouds
        /// </summary>
        /// <param name="points1"></param>
        /// <param name="points2"></param>
        /// <returns></returns>
        private static float GreedyCloudMatch(DollarPoint[] points1, DollarPoint[] points2)
        {
            int n = points1.Length; // the two clouds should have the same number of points by now
            float eps = 0.5f;       // controls the number of greedy search trials (eps is in [0..1])
            int step = (int)Math.Floor(Math.Pow(n, 1.0f - eps));
            float minDistance = float.MaxValue;
            for (int i = 0; i < n; i += step)
            {
                float dist1 = CloudDistance(points1, points2, i);   // match points1 --> points2 starting with index point i
                float dist2 = CloudDistance(points2, points1, i);   // match points2 --> points1 starting with index point i
                minDistance = Math.Min(minDistance, Math.Min(dist1, dist2));
            }
            return minDistance;
        }

        /// <summary>
        /// Computes the distance between two point clouds by performing a minimum-distance greedy matching
        /// starting with point startIndex
        /// </summary>
        /// <param name="points1"></param>
        /// <param name="points2"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private static float CloudDistance(DollarPoint[] points1, DollarPoint[] points2, int startIndex)
        {
            int n = points1.Length;       // the two clouds should have the same number of points by now
            bool[] matched = new bool[n]; // matched[i] signals whether point i from the 2nd cloud has been already matched
            Array.Clear(matched, 0, n);   // no points are matched at the beginning

            float sum = 0;  // computes the sum of distances between matched points (i.e., the distance between the two clouds)
            int i = startIndex;
            do
            {
                int index = -1;
                float minDistance = float.MaxValue;
                for (int j = 0; j < n; j++)
                    if (!matched[j])
                    {
                        float dist = Geometry.SqrEuclideanDistance(points1[i], points2[j]);  // use squared Euclidean distance to save some processing time
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            index = j;
                        }
                    }
                matched[index] = true; // point index from the 2nd cloud is matched to point i from the 1st cloud
                float weight = 1.0f - ((i - startIndex + n) % n) / (1.0f * n);
                sum += weight * minDistance; // weight each distance with a confidence coefficient that decreases from 1 to 0
                i = (i + 1) % n;
            } while (i != startIndex);
            return sum;
        }
    }
}
