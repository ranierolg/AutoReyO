using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;

namespace SmartPenSketch
{

    #region Data Structures

    public class SketchNode
    {
        public int id;
        public List<SketchPoint> points;
        public double avgx;
        public double avgy;

        public SketchNode(SketchPoint sp, int id)
        {
            this.points = new List<SketchPoint>();
            this.id = id;
            this.points.Add(sp);
            this.avgx = sp.x;
            this.avgy = sp.y;
        }

        public void insertPoint(SketchPoint sp)
        {
            //Inserts point and recalculates node center
            this.points.Add(sp);
            this.avgx = this.points.Aggregate(0.0, (sum, point) => sum + point.x) /  Convert.ToDouble(this.points.Count);
            this.avgy = this.points.Aggregate(0.0, (sum, point) => sum + point.y) / Convert.ToDouble(this.points.Count);
        } 
    }


    public class SketchAdjacencyList
    {
        public SketchNode node;
        public List<int> adjacent;

        public SketchAdjacencyList(SketchPoint sp, int id)
        {
            this.node = new SketchNode(sp, id);
            this.adjacent = new List<int>();
        }

        //Silently ignores duplicate inserts
        public void addAdjacent(int newid)
        {
            if (newid == this.node.id) // Cannot be the same ID
                return;
            foreach (int id in this.adjacent) // Cannot already be in the adjacency list
                if (newid == id)
                    return;

            this.adjacent.Add(newid);
        }

        public void combineWith(SketchAdjacencyList other)
        {
            // Merge the adjacency lists
            adjacent = adjacent.AsEnumerable().Union(other.adjacent.AsEnumerable()).ToList();

            // Recalculate locations of sampled x.y points
            node.points.AddRange(other.node.points);
            node.avgx = node.points.Aggregate(0.0, (sum, point) => sum + point.x) / Convert.ToDouble(node.points.Count);
            node.avgy = node.points.Aggregate(0.0, (sum, point) => sum + point.y) / Convert.ToDouble(node.points.Count);

            // Ensure its not keeping itself or the old node id as an adjacent element
            adjacent.Remove(node.id);
            adjacent.Remove(other.node.id);
        }

    }

    /*
    public void addAdjacentNode(SketchNode newNode)
    {
        //Ensure node is not equal to itself
        if (newNode.id == this.id) return;

        //Ensure node is not already within the adjacency list
        foreach (int key in this.adjacent.Keys)
            if (key == newNode.id) return;

        this.adjacent.Add(newNode.id, newNode);
        return;
    }

    //Override the equals operator for future graphs
    public bool sameNode(SketchNode sn, double thresh)
    {
        return Math.Abs(this.y - sn.y) <= thresh && Math.Abs(this.x - sn.x) <= thresh;
    }

    public void addAdjacent(int nodeid)
    {
        if (this.id == nodeid) return; //Return if the current node is the node
        foreach (int key in this.adjacent.Keys)
        {
            if (key == nodeid) //Return if the current node is already adjacent
                return;
            this.adjacent.Add();
        }

    } 
    */


    public class SketchGraph
    {
        private Dictionary<int, SketchAdjacencyList> nodes;
        private Dictionary<Tuple<int, int>, SketchSubstroke> substrokes;
        //To the tuple, am i adding a node? (dictionary of index + adjacencylist), or just the adjacencylist?
        //a "node" consists of its index, plus its adjacency list. So it should be
        //Tuple<int, SketchAdjacencyList, ellipse>
        //and for substrokes it should be
        //Tuple<Tuple<int, int>, SketchSubstroke, Line>
        Tuple<int, SketchAdjacencyList, Ellipse> nodeLinks;
        Tuple<Tuple<int, int>, SketchSubstroke, Line> lineLinks;
        List<Tuple<int, SketchAdjacencyList, Ellipse>> linkNodeList = new List<Tuple<int, SketchAdjacencyList, Ellipse>>();
        List<Tuple<Tuple<int, int>, SketchSubstroke, Line>> linkLineList = new List<Tuple<Tuple<int, int>, SketchSubstroke, Line>>();

        private double threshold;
        private int id_counter;

        public SketchGraph()
        {
            this.nodes = new Dictionary<int, SketchAdjacencyList>();
            this.substrokes = new Dictionary<Tuple<int, int>, SketchSubstroke>();
            this.threshold = 20;
            this.id_counter = 0;
        }

        public SketchGraph(double threshold)
        {
            this.nodes = new Dictionary<int, SketchAdjacencyList>();
            this.substrokes = new Dictionary<Tuple<int, int>, SketchSubstroke>();
            this.threshold = threshold;
            this.id_counter = 0;
        }

        public KeyValuePair<int, SketchAdjacencyList>[] getNodeDictArray()
        {
            return nodes.AsEnumerable().ToArray();
        }

        public void addSubstroke(SketchSubstroke ss)
        {
            //Get nodes and link them
            //Right now, we just use the closest node and say that that is the node we should merge
            //In the future this might be more complicated
            Tuple<int, double> nn1 = this.getNearestNode(ss.getFirst());
            Tuple<int, double> nn2 = this.getNearestNode(ss.getLast());

            int nodeid1 = nn1.Item1;
            int nodeid2 = nn2.Item1;

            //Resolve a similar node conflict
            if (nn1.Item1 == nn2.Item1 && nn1.Item1 != -1)
            {
                if (nn1.Item2 < nn2.Item2)
                {
                    this.nodes[nn1.Item1].node.insertPoint(ss.getFirst());
                    SketchAdjacencyList sal = new SketchAdjacencyList(ss.getLast(), this.id_counter);
                    nodeid2 = this.id_counter;
                    this.id_counter += 1;
                    this.nodes[sal.node.id] = sal;
                }
                else
                {
                    this.nodes[nn2.Item1].node.insertPoint(ss.getLast());
                    SketchAdjacencyList sal = new SketchAdjacencyList(ss.getFirst(), this.id_counter);
                    nodeid1 = this.id_counter;
                    this.id_counter += 1;
                    this.nodes[sal.node.id] = sal;
                }
            }

            else
            {
                if (nn1.Item1 == -1)
                {
                    SketchAdjacencyList sal = new SketchAdjacencyList(ss.getFirst(), this.id_counter);
                    nodeid1 = this.id_counter;
                    this.id_counter += 1;
                    this.nodes[sal.node.id] = sal;
                }
                else
                {
                    this.nodes[nn1.Item1].node.insertPoint(ss.getFirst());
                }

                if (nn2.Item1 == -1)
                {
                    SketchAdjacencyList sal = new SketchAdjacencyList(ss.getLast(), this.id_counter);
                    nodeid2 = this.id_counter;
                    this.id_counter += 1;
                    this.nodes[sal.node.id] = sal;
                }
                else
                {
                    this.nodes[nn2.Item1].node.insertPoint(ss.getLast());
                }

            }

            //May need to happen in the future
            this.linkNodeById(nodeid1, nodeid2);

            //ensure nodes are insered in order
            if (nodeid2 < nodeid1)
            {
                int temp = nodeid2;
                nodeid2 = nodeid1;
                nodeid1 = temp;
            }

            //Do not insert overlapping lines
            Tuple<int, int> tup = new Tuple<int, int>(nodeid1, nodeid2);
            if (!this.substrokes.ContainsKey(tup))
            {
                this.substrokes[tup] = ss;
            }

        }

        public void mergeNodes(int n1, int n2)
        {
            SketchAdjacencyList l1 = this.nodes[n1];
            SketchAdjacencyList l2 = this.nodes[n2];

            // remove n2 from adjacent nodes
            foreach (int i in l2.adjacent)
            {
                this.nodes[i].adjacent.Remove(n2);
                if (!this.nodes[i].adjacent.Contains(i))
                {
                    this.nodes[i].adjacent.Add(n1);
                } 
            }

            // Combine n1 and n2 SketchAdjacencyList
            l1.combineWith(l2);

            // Remove n1 from n2
            this.nodes[n1] = l1;
            this.nodes.Remove(n2);
            List<KeyValuePair<Tuple<int, int>, SketchSubstroke>> substroke_list = this.substrokes.ToList();

            // Coalesce edges
            foreach (KeyValuePair<Tuple<int, int>,SketchSubstroke> pair in substroke_list)
            {                
                Tuple<int, int> substroke = pair.Key;
                SketchSubstroke substroke_values = pair.Value;

                if (substroke.Item1 == n1 && substroke.Item2 == n2 ||
                    substroke.Item1 == n2 && substroke.Item2 == n1)
                {
                    foreach (SketchPoint p in substroke_values.get_points())
                    {
                        nodes[n1].node.insertPoint(p);
                    }
                    this.substrokes.Remove(substroke);
                } else if (substroke.Item1 == n2)
                {
                    this.substrokes.Remove(substroke);
                    Tuple<int, int> newKey = new Tuple<int, int>(n1, substroke.Item2);
                    this.substrokes[newKey] = substroke_values;
                } else if (substroke.Item2 == n2)
                {
                    this.substrokes.Remove(substroke);
                    Tuple<int, int> newKey = new Tuple<int, int>(substroke.Item1, n1);
                    this.substrokes[newKey] = substroke_values;
                }
            }

            id_counter--;
        }

        Tuple<int, double> getNearestNode(SketchPoint target)
        {
            //Iterates through the list of points that make up a node, if close enough to one it is close enough to them all.
            //However, is still judged (the double that is returned) by its distance from the center
            //This allows new points to be introduced that might not be very close to a node, while also makging sure that we are focusing on the center as the main decider of distance
            //Still shows a bias towards order of node evalutation, but does not seem to be large enough to cause an issue

            //iterate through each AdjacencyList
            Tuple<int, double> closestNode = new Tuple<int, double>(-1, Double.MaxValue);
            foreach (SketchAdjacencyList sal in this.nodes.Values)
                foreach (SketchPoint sp in sal.node.points)
                {
                    double triggerDist = euclideanDistance(sp.x, target.x, sp.y, target.y);
                    
                    if (triggerDist < this.threshold)
                    {
                        double centerDist = euclideanDistance(sal.node.avgx, target.x, sal.node.avgy, target.y);
                        if (centerDist < closestNode.Item2)
                        {
                            closestNode = new Tuple<int, double>(sal.node.id, centerDist);
                        }
                    }                        
                }
            return closestNode;
        }

        public void linkNodeById(int id1, int id2)
        {
            //We consider this graph undirected (equal weight in both directions), so both edges are added to each other
            this.nodes[id1].addAdjacent(id2);
            this.nodes[id2].addAdjacent(id1);
        }

        public Dictionary<int, SketchAdjacencyList> getNodes ()
        {
            return this.nodes;
        }

        public Dictionary<Tuple<int, int>, SketchSubstroke> getSubstrokes()
        {
            return this.substrokes;
        }

        public static double euclideanDistance(double x1, double x2, double y1, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public void addNodeLink(Tuple<int, SketchAdjacencyList, Ellipse> tupN)
        {
            linkNodeList.Add(tupN);
        }

        public void addLineLink(Tuple<Tuple<int, int>, SketchSubstroke, Line> tupL)
        {
            linkLineList.Add(tupL);
        }

        /*
        public int findNode(SketchNode sn)
        {
            foreach (SketchNode node in this.nodes)
                if (node.sameNode(sn, this.threshold))
                    return node.id;
            return -1;
        }


        //This has not been completed yet, but it does not make sense in the OG javascript
        public void updateNodeCenter(int id, double x, double y)
        {
            SketchNode sn = this.nodes[id]; //Since ID is tied to the index of the list

        }

        public int addNode(SketchNode sn)
        {
            int foundNodeID = this.findNode(sn);
            if (foundNodeID == -1)
            {
                sn.id = this.nodes.Count; // Set id to the next available in the array.
                this.nodes.Add(sn); // Add node.
                foundNodeID = sn.id;
            }
            else
            {
                this.updateNodeCenter(foundNodeID, sn.x, sn.y);
            }
            return foundNodeID;
        }
        */

    }

    #endregion

    public class SegmentSplit
    {
        #region Processing function

        public static SketchGraph convertToGraph(List<SketchSubstroke> substrokes, double thresh)
        {
            SketchGraph sg = new SketchGraph(thresh);

            foreach (SketchSubstroke ss in substrokes)
                sg.addSubstroke(ss);

            return sg;
        }

        public SketchGraph convertToGraph(List<SketchSubstroke> substrokes) { return convertToGraph(substrokes, 20); }

        public static List<SketchSubstroke> splitByIntersection(List<SketchSubstroke> substroke)
        {

            for (int i = 0; i < substroke.Count - 1; i++)
                for (int j = i + 1; j < substroke.Count; j++)
                {
                    List<SketchSubstroke> temp = SketchSubstroke.intersect(substroke[i], substroke[j]);
                    if (temp.Count > 2) substroke.Add(temp[2]);
                    if (temp.Count > 3) substroke.Add(temp[3]);
                    substroke[i] = temp[0];
                    substroke[j] = temp[1];
                }



            return substroke; 
        }

        #endregion

        #region Math Helper Functions
        public double getAngle(SketchPoint n1, SketchPoint n2)
        {
            // Since y values increase going downwards in computer graphics, the y value difference must be negated to get the correct angle in a regular math coordinate system.
            return Math.Atan2(n1.y - n2.y, n2.x - n1.x);
        }

        //Normalizes an angle between -pi and pi
        public double normalizeAngle(double angle)
        {
            if (angle > Math.PI)
            {
                angle = angle - Math.Ceiling(angle / (2 * Math.PI)) * 2 * Math.PI;
            }
            if (angle < -1 * Math.PI)
            {
                angle = angle + Math.Ceiling(-1 * angle / (2 * Math.PI)) * 2 * Math.PI;
            }
      
            return angle;
        }
        #endregion
    }
}
