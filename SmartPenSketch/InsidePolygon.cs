using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenSketch
{
    class InsidePolygon
    {
        static double INF = 999999;

        public class GraphPoint
        {
            public double x;
            public double y;

            public GraphPoint(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
        };

        // Given three colinear GraphPoints p, q, r,  
        // the function checks if GraphPoint q lies 
        // on line segment 'pr' 
        static bool OnSegment(GraphPoint p, GraphPoint q, GraphPoint r)
        {
            if (q.x <= Math.Max(p.x, r.x) &&
                q.x >= Math.Min(p.x, r.x) &&
                q.y <= Math.Max(p.y, r.y) &&
                q.y >= Math.Min(p.y, r.y))
            {
                return true;
            }
            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        static int Orientation(GraphPoint p, GraphPoint q, GraphPoint r)
        {
            double val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (val == 0)
            {
                return 0; // colinear 
            }
            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        // The function that returns true if  
        // line segment 'p1q1' and 'p2q2' intersect. 
        static bool DoIntersect(GraphPoint p1, GraphPoint q1,
                                GraphPoint p2, GraphPoint q2)
        {
            // Find the four orientations needed for  
            // general and special cases 
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            // Special Cases 
            // p1, q1 and p2 are colinear and 
            // p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(p1, p2, q1))
            {
                return true;
            }

            // p1, q1 and p2 are colinear and 
            // q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(p1, q2, q1))
            {
                return true;
            }

            // p2, q2 and p1 are colinear and 
            // p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(p2, p1, q2))
            {
                return true;
            }

            // p2, q2 and q1 are colinear and 
            // q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(p2, q1, q2))
            {
                return true;
            }

            // Doesn't fall in any of the above cases 
            return false;
        }

        // Returns true if the GraphPoint p lies  
        // inside the polygon[] with n vertices 
        public static bool IsInside(GraphPoint[] polygon, int n, GraphPoint p)
        {
            // There must be at least 3 vertices in polygon[] 
            if (n < 3)
            {
                return false;
            }

            // Create a GraphPoint for line segment from p to infinite 
            GraphPoint extreme = new GraphPoint(INF, p.y);

            // Count intersections of the above line  
            // with sides of polygon 
            int count = 0, i = 0;
            do
            {
                int next = (i + 1) % n;

                // Check if the line segment from 'p' to  
                // 'extreme' intersects with the line  
                // segment from 'polygon[i]' to 'polygon[next]' 
                if (DoIntersect(polygon[i],
                                polygon[next], p, extreme))
                {
                    // If the GraphPoint 'p' is colinear with line  
                    // segment 'i-next', then check if it lies  
                    // on segment. If it lies, return true, otherwise false 
                    if (Orientation(polygon[i], p, polygon[next]) == 0)
                    {
                        return OnSegment(polygon[i], p,
                                         polygon[next]);
                    }
                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise 
            return (count % 2 == 1); // Same as (count%2 == 1) 
        }

        // Returns an array of GraphPoints for use with IsInside
        public static GraphPoint[] CreatePolygon(Dictionary<int, SketchAdjacencyList> g_nodes, List<int> inputNodes)
        {
            if (inputNodes.Count == 0)
                return null;

            GraphPoint[] polyPoints = new GraphPoint[inputNodes.Count];
            int index = 0;
            foreach(int currentNode in inputNodes)
            {
                GraphPoint point = new GraphPoint(g_nodes[currentNode].node.avgx, g_nodes[currentNode].node.avgy);
                polyPoints[index] = point;
                index++;
            }
            return polyPoints;
        }
    }
}
