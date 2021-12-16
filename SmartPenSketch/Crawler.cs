using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SmartPenSketch.MainPage;
using static SmartPenSketch.SketchRecTools;

namespace SmartPenSketch
{
    class Crawler
    {
        #region Basic Path Crawling
        public static List<int> StraightCrawl(Dictionary<int, SketchAdjacencyList> g_adj, SketchGraph mainGraph, int startNode, Direction dir, List<Tuple<int, int>> crawledKeys)
        {
            /*Crawls in a single given direction until it can no longer go in said direction
            Returns the list of nodes that it traverses
            Also adds the corresponding edges to "cralwedKeys" list*/
            int currentNode = startNode;
            List<Tuple<int, int>> checkedKeys = new List<Tuple<int, int>>();
            List<int> nodePath = new List<int>();
            nodePath.Add(currentNode);
            bool continueCrawlDirection = true;

            while (continueCrawlDirection)
            {
                continueCrawlDirection = false;

                SketchAdjacencyList skAdj;
                g_adj.TryGetValue(currentNode, out skAdj);

                foreach (int key in skAdj.adjacent)
                {
                    double x1, x2, y1, y2;
                    bool isCorrect = false;   //initialize isCorrect
                    Tuple<int, int> compTuple = new Tuple<int, int>(currentNode, key);

                    switch (dir)
                    {
                        case Direction.Up:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Up, 2);
                            break;
                        case Direction.Down:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Down, 2);
                            break;
                        case Direction.Left:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Left, 0.2);
                            break;
                        case Direction.Right:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Right, 0.2);
                            break;
                    }

                    if (isCorrect)  //greedily advance to the next correct node
                    {
                        checkedKeys.Add(compTuple);
                        currentNode = key;
                        nodePath.Add(currentNode);
                        continueCrawlDirection = true;
                        break;
                    }
                }

            }
            crawledKeys.AddRange(TupleListConstructor(mainGraph, nodePath));
            return nodePath;
        }

        public static List<int> StraightCrawlStack(Dictionary<int, SketchAdjacencyList> g_adj, SketchGraph main_graph, int startNode, Direction dir, out List<Tuple<int, int>> crawledKeys)
        {
            List<Tuple<int, int>> checkedKeys = new List<Tuple<int, int>>();
            List<Tuple<int, int>> outKeys = new List<Tuple<int, int>>();
            List<int> nodePath = new List<int>();
            int currentNode = startNode;

            nodePath.Add(currentNode);
            Stack<int> nodeStack = new Stack<int>();
            nodeStack.Push(currentNode);

            while (nodeStack.Count>0)
            {
                currentNode = nodeStack.Pop();

                SketchAdjacencyList skAdj;
                g_adj.TryGetValue(currentNode, out skAdj);

                bool foundNode = false;

                foreach (int key in skAdj.adjacent)
                {
                    bool isCorrect = false;
                    Tuple<int, int> compTuple = new Tuple<int, int>(currentNode, key);

                    switch (dir)
                    {
                        case Direction.Up:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Up, 2);
                            break;
                        case Direction.Down:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Down, 2);
                            break;
                        case Direction.Left:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Left, 0.2);
                            break;
                        case Direction.Right:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Right, 0.2);
                            break;
                    }

                    //also check if the distance between key and skAdj.adjacent is short enough
                    if(CalculateDistanceNodes(g_adj, currentNode, key) <= 15)
                    {
                        isCorrect = true;
                    }

                    if (isCorrect)  //greedily advance to the next correct node
                    {
                        foundNode = true;
                        checkedKeys.Add(compTuple);
                        nodeStack.Push(key);
                        nodePath.Add(currentNode);
                    }
                }

                if (!foundNode)
                    nodePath.Remove(nodePath.Count - 1);
            }
            outKeys.AddRange(TupleListConstructor(main_graph, nodePath));
            crawledKeys = outKeys;
            return nodePath;
        }
        
        public static List<int> PathToPathCrawl(Dictionary<int, SketchAdjacencyList> g_adj, Dictionary<Tuple<int, int>, SketchSubstroke> g_strokes, SketchGraph mainGraph, List<int> startNodePath, Direction dir, List<Tuple<int, int>> crawledKeys, out List<Tuple<int, int>> outKeys)
        {
            /*Crawls in a single given direction along a given path until it encounters a node along a previously traversed path (crawledKeys)
             * It essentially treats the given previously traversed path as a boundary
             * Used for recognizing sub-shapes within another recognized shape
             * Returns the list of nodes that it traverses
             * Also adds the corresponding edges to external traversed list outKeys*/
            int currentNode;
            int nodeCheckInd = 0;

            List<Tuple<int, int>> checkedKeys = new List<Tuple<int, int>>(crawledKeys);
            outKeys = new List<Tuple<int, int>>();
            List<int> boundaryNodes = new List<int>();


            List<int> nodePath = new List<int>();
            bool continueCrawlDirection = true;
            bool foundNewPath = false;
            /* This method has two modes, denoted by the "foundNewPath" bool.
             * The first mode crawls through given line, until it finds a new path
             * When it does, it switches to Straight Crawl that stops when it encounters the boundary */

            currentNode = startNodePath[nodeCheckInd];

            foreach (Tuple<int, int> key in crawledKeys)
            {
                if (!(boundaryNodes.Contains(key.Item1)))
                    boundaryNodes.Add(key.Item1);
                if (!(boundaryNodes.Contains(key.Item2)))
                    boundaryNodes.Add(key.Item2);
            }

            while (continueCrawlDirection)
            {
                if (nodeCheckInd >= startNodePath.Count && foundNewPath == false)
                    break;
                if (foundNewPath == false)
                    currentNode = startNodePath[nodeCheckInd];
                continueCrawlDirection = false;
                List<Tuple<int, int>> adjacentKeys = new List<Tuple<int, int>>();
                foreach (Tuple<int, int> key in mainGraph.getSubstrokes().Keys)
                {
                    if ((currentNode.Equals(key.Item1) || currentNode.Equals(key.Item2)) && !(checkedKeys.Contains(key)))
                    {
                        adjacentKeys.Add(key);
                    }
                }

                foreach (Tuple<int, int> key in adjacentKeys)
                {
                    double x1, x2, y1, y2;
                    bool backwards;
                    //Check if backwards
                    if (key.Item2.Equals(currentNode))
                    {
                        x1 = g_adj[key.Item2].node.avgx; x2 = g_adj[key.Item1].node.avgx; y1 = g_adj[key.Item2].node.avgy; y2 = g_adj[key.Item1].node.avgy;
                        backwards = true;
                    }
                    else //if not backwards
                    {
                        x1 = g_adj[key.Item1].node.avgx; x2 = g_adj[key.Item2].node.avgx; y1 = g_adj[key.Item1].node.avgy; y2 = g_adj[key.Item2].node.avgy;
                        backwards = false;

                    }
                    bool isCorrect = false;

                    switch (dir)
                    {
                        case Direction.Up:
                            isCorrect = IsUp(x1, x2, y1, y2);
                            break;
                        case Direction.Down:
                            isCorrect = IsDown(x1, x2, y1, y2);
                            break;
                        case Direction.Left:
                            isCorrect = IsLeft(x1, x2, y1, y2);
                            break;
                        case Direction.Right:
                            isCorrect = IsRight(x1, x2, y1, y2);
                            break;
                    }

                    if (isCorrect)
                    {
                        if (!foundNewPath)
                            nodePath.Add(currentNode);
                        checkedKeys.Add(key);
                        outKeys.Add(key);
                        foundNewPath = true;

                        if (backwards)
                            currentNode = key.Item1;
                        else
                            currentNode = key.Item2;

                        nodePath.Add(currentNode);

                        if (boundaryNodes.Contains(currentNode))
                            continueCrawlDirection = false;
                        else
                            continueCrawlDirection = true;
                        break;
                    }
                }
                if (!foundNewPath)
                {
                    nodeCheckInd++;
                    continueCrawlDirection = true;
                }
            }
            return nodePath;
        }
        #endregion

        #region Directional Check
        public static bool IsDown(double x1, double x2, double y1, double y2)
        {
            bool isDow = false;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            if (slope > 2 && (y2 - y1) > 0)
            {
                Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | VERTICAL");
                isDow = true;
            }

            return isDow;
        }

        public static bool IsDown(double x1, double x2, double y1, double y2, double threshold)
        {
            bool isDow = false;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            if (slope > threshold && (y2 - y1) > 0)
            {
                Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | VERTICAL");
                isDow = true;
            }

            return isDow;
        }

        public static bool IsUp(double x1, double x2, double y1, double y2)
        {
            bool isDow = false;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            if (slope > 2 && (y2 - y1) < 0)
            {
                Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | VERTICAL");
                isDow = true;
            }

            return isDow;
        }

        public static bool IsRight(double x1, double x2, double y1, double y2)
        {
            bool isHor = false;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            if (slope < 0.2 && (x2 - x1) > 0)
            {
                Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | HORIZONTAL");
                isHor = true;
            }

            return isHor;
        }

        public static bool IsRight(double x1, double x2, double y1, double y2, double threshold)
        {
            bool isHor = false;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            if (slope < threshold && (x2 - x1) > 0)
            {
                Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | HORIZONTAL");
                isHor = true;
            }

            return isHor;
        }

        public static bool IsLeft(double x1, double x2, double y1, double y2)
        {
            bool isHor = false;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            if (slope < 0.2 && (x2 - x1) < 0)
            {
                Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | HORIZONTAL");
                isHor = true;
            }

            return isHor;
        }

        public static bool IsDirection(Tuple<int, int> inputTuple, Dictionary<int, SketchAdjacencyList> g_adj, Direction dir, double threshold)
        {
            //A function that combines IsRight, IsLeft, IsUp, IsDown
            //It also detects general-direction slant in four possible directions
            //It takes in a tuple of two nodes to look at.
            //g_adj is needed to map the tuple ints with the nodes they belong to.

            bool isDirection = false;

            //THIS IS ASSUMING THE NODES ARE NOT "BACKWARDS".
            //You will need to make the "is backwards" check BEFORE calling this function,
                //so that when you pass inputTuple they're already in the order you want to check
            double x1 = g_adj[inputTuple.Item1].node.avgx;
            double x2 = g_adj[inputTuple.Item2].node.avgx;
            double y1 = g_adj[inputTuple.Item1].node.avgy;
            double y2 = g_adj[inputTuple.Item2].node.avgy;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            switch (dir)
            {
                case Direction.Up:
                    if (slope > threshold && (y2 - y1) < 0)
                    {
                        //Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | UP");
                        isDirection = true;
                    }
                    break;
                case Direction.Down:
                    if (slope > threshold && (y2 - y1) > 0)
                    {
                        //Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | DOWN");
                        isDirection = true;
                    }
                    break;
                case Direction.Left:
                    if (slope < threshold && (x2 - x1) < 0)
                    {
                        //Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | LEFT");
                        isDirection = true;
                    }
                    break;
                case Direction.Right:
                    if (slope < threshold && (x2 - x1) > 0)
                    {
                        //Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | RIGHT");
                        isDirection = true;
                    }
                    break;
                case Direction.SlantNE:
                    if ((slope > threshold && slope < 12) && (((x2 - x1) > 0) && ((y2 - y1) < 0)))
                    {
                        //Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | NORTHEAST");
                        isDirection = true;
                    }
                    break;
                case Direction.SlantNW:
                    if ((slope > threshold && slope < 15) && (((x2 - x1) < 0) && ((y2 - y1) < 0)))
                    {
                        //Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | NORTHWEST");
                        isDirection = true;
                    }
                    break;
                case Direction.SlantSE:
                    if ((slope > threshold && slope < 15) && (((x2 - x1) > 0) && ((y2 - y1) > 0)))
                    {
                        //Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | SOUTHEAST");
                        isDirection = true;
                    }
                    break;
                case Direction.SlantSW:
                    if ((slope > threshold && slope < 15) && (((x2 - x1) < 0) && ((y2 - y1) > 0)))
                    {
                        //Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | SOUTHWEST");
                        isDirection = true;
                    }
                    break;

            }

            return isDirection;
        }

        private bool IsVertical(double x1, double x2, double y1, double y2)
        {
            bool isVer = false;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            if (slope > 2)
            {
                Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | VERTICAL");
                isVer = true;
            }

            return isVer;
        }

        private bool IsHorizontal(double x1, double x2, double y1, double y2)
        {
            bool isHor = false;

            double slope = Math.Abs((y2 - y1) / (x2 - x1));

            if (slope < 0.2)
            {
                Debug.WriteLine("X Delta: " + (x2 - x1) + " | Y Delta: " + (y2 - y1) + " | Slope: " + (y2 - y1) / (x2 - x1) + " | HORIZONTAL");
                isHor = true;
            }

            return isHor;
        }

        #endregion

        #region Traversal Algorithms
        public static List<int> DijkstraBounded(Dictionary<int, SketchAdjacencyList> g_adj, SketchGraph mainGraph, int startNode, int endNode/*, out List<int> prev*/)
        {
            int u = startNode;
            //List<double> distances = new List<double>();
            Dictionary<int, double> distances = new Dictionary<int, double>();
            List<int> Q = new List<int>();      //list of every vertex in the graph
            //prev = new List<int>();
            Dictionary<int, int> prev = new Dictionary<int, int>();

            //Hack-y solution to the g_adj key-node mismatch: make a new g_adj where the node ID is changed to 

            foreach (int v in g_adj.Keys)
            {
                distances.Add(v, Double.MaxValue);
                prev.Add(v, -1);
                Q.Add(v);

            }
            distances[startNode] = 0;

            while (Q.Count > 0)
            {
                Dictionary<int, double> temporaryDistances = new Dictionary<int, double>(distances);
                while (true)
                {
                    u = GetMinimum(temporaryDistances).Key; //IN Q, not just any vertex!!
                    //u = temporaryDistances.Min().Key; 
                    if (!Q.Contains(u))
                        temporaryDistances[u] = double.MaxValue;
                    if (Q.Contains(u))
                        break;
                }


                Q.Remove(u);
                if (u == endNode)
                    break;


                //Get every neighbor 
                List<Tuple<int, int>> neighbors = new List<Tuple<int, int>>();
                foreach (Tuple<int, int> key in mainGraph.getSubstrokes().Keys)
                {
                    if ((u.Equals(key.Item1) || u.Equals(key.Item2)) && (Q.Contains(key.Item1) || Q.Contains(key.Item2)))         //Only for v that are still in Q
                    {
                        neighbors.Add(key);
                    }
                }

                //for each neighbor v of u
                double alt;
                foreach (Tuple<int, int> key in neighbors)
                {
                    int v;
                    double x1, x2, y1, y2, uvLength;
                    bool backwards;

                    //Check if backwards
                    if (key.Item2.Equals(u))
                    {
                        x1 = g_adj[key.Item2].node.avgx; x2 = g_adj[key.Item1].node.avgx; y1 = g_adj[key.Item2].node.avgy; y2 = g_adj[key.Item1].node.avgy;
                        v = key.Item1;
                        backwards = true;
                    }
                    else //if not backwards
                    {
                        x1 = g_adj[key.Item1].node.avgx; x2 = g_adj[key.Item2].node.avgx; y1 = g_adj[key.Item1].node.avgy; y2 = g_adj[key.Item2].node.avgy;
                        v = key.Item2;
                        backwards = false;
                    }

                    uvLength = CalculateDistanceSimple(x1, y1, x2, y2);

                    alt = distances[u] + uvLength;
                    if (alt < distances[v])
                    {
                        distances[v] = alt;
                        prev[v] = u;
                    }

                }
            }

            List<int> shortestPath = new List<int>();
            if ((prev[u] != -1) || (u == startNode))
                while (u != -1)
                {
                    shortestPath.Insert(0, u);
                    u = prev[u];
                }

            return shortestPath;
        }

        public static List<Tuple<int, int>> DFSBounded(Dictionary<int, SketchAdjacencyList> g_adj, SketchGraph mainGraph, List<Tuple<int, int>> preVisitedEdges, int startNode)
        {
            //A slightly modified version of Depth First Search, where we are given edges already part of other details as "pre-visited edges"
            //This will let us isolate sub-sketches inside quadrants we have already identified

            List<Tuple<int, int>> visitedEdges = new List<Tuple<int, int>>(preVisitedEdges);
            List<Tuple<int, int>> returnEdges = new List<Tuple<int, int>>();

            Stack<int> edgeStack = new Stack<int>();
            int currentNode;

            edgeStack.Push(startNode);
            while (edgeStack.Count > 0)
            {
                currentNode = edgeStack.Pop();
                SketchAdjacencyList skAdj;
                g_adj.TryGetValue(currentNode, out skAdj);

                foreach (int adjNode in skAdj.adjacent)
                {
                    Tuple<int, int> compTuple = SingleTupleConstructor(mainGraph, currentNode, adjNode);
                    if (!visitedEdges.Contains(compTuple))
                    {
                        edgeStack.Push(adjNode);
                        visitedEdges.Add(compTuple);
                        returnEdges.Add(compTuple);
                    }
                }
            }


            return returnEdges;
        }
        
        public static List<Tuple<int, int>> DFSDirectionBounded(Dictionary<int, SketchAdjacencyList> g_adj, SketchGraph mainGraph, List<Tuple<int, int>> preVisitedEdges, int startNode, int endNode, Direction dir, double threshold)
        {
            //A modified version of the DFSBounded algorithm. Attempts to avoid "leaking" in the DFS crawler beyond bounded edges by:
            //1) starting DFS only on nodes that match the given dir
            //2) completely stopping crawling as soon as the algorithm reaches any node part of preVisitedEdges.

            List<Tuple<int, int>> visitedEdges = new List<Tuple<int, int>>(preVisitedEdges);
            List<int> visitedNodes = new List<int>(NodeListConstructor(visitedEdges));
            List<Tuple<int, int>> returnEdges = new List<Tuple<int, int>>();

            Stack<int> edgeStack = new Stack<int>();
            int currentNode;
            bool firstNode = true;

            edgeStack.Push(startNode);
            while (edgeStack.Count > 0)
            {
                currentNode = edgeStack.Pop();
                SketchAdjacencyList skAdj;
                g_adj.TryGetValue(currentNode, out skAdj);

                foreach (int adjNode in skAdj.adjacent)
                {

                    Tuple<int, int> compTuple = SingleTupleConstructor(mainGraph, currentNode, adjNode);
                    Tuple<int, int> dirTuple = new Tuple<int, int>(currentNode, adjNode);   //We need this second one to preserve our directional check
                    if (firstNode)
                    {
                        if (!IsDirection(dirTuple, g_adj, dir, threshold))
                            continue;
                    }
                    if (!visitedNodes.Contains(adjNode))
                    {
                        edgeStack.Push(adjNode);
                        visitedEdges.Add(compTuple);
                        visitedNodes.Add(adjNode);
                        returnEdges.Add(compTuple);
                    }
                    else if (!visitedEdges.Contains(compTuple)) 
                    {
                        //We still want to include the edges that got us to the preVisitedEdges nodes
                        returnEdges.Add(compTuple);
                    }
                }
                firstNode = false;
            }

            return returnEdges;
        }

        public static List<List<int>> DFSOneDirection(Dictionary<int, SketchAdjacencyList> g_adj, SketchGraph mainGraph, int startNode, Direction dir, double threshold)
        {
            List<int> nodePath = new List<int>();

            List<Tuple<int, int>> returnEdges = new List<Tuple<int, int>>();

            Stack<int> edgeStack = new Stack<int>();
            List<int> edgeStackHistory = new List<int>();

            List<List<int>> nodePaths = new List<List<int>>();
            List<int> leaves = new List<int>();
            List<int> leafList = new List<int>();
            bool leafAdder = false;
            List<int> proximityNodes = new List<int>();

            int currentNode;            

            edgeStack.Push(startNode);
            nodePath.Add(startNode);
            //longestPath.Add(startNode);

            while (edgeStack.Count > 0)
            {
                edgeStackHistory.AddRange(edgeStack.Except(edgeStackHistory));
                currentNode = edgeStack.Pop();

                if(currentNode == 154)    //For debugging only, put a stop here 
                {
                    Debug.WriteLine("Stop");
                }

                SketchAdjacencyList skAdj;
                g_adj.TryGetValue(currentNode, out skAdj);

                foreach (int adjNode in skAdj.adjacent)
                {
                    Tuple<int, int> compTuple = new Tuple<int, int>(currentNode, adjNode);

                    bool isCorrect = false;
                    switch (dir)
                    {
                        case Direction.Up:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Up, threshold);
                            break;
                        case Direction.Down:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Down, threshold);
                            break;
                        case Direction.Left:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Left, threshold);
                            break;
                        case Direction.Right:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Right, threshold);
                            break;
                        case Direction.SlantNE:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.SlantNE, threshold);
                            break;
                        case Direction.SlantNW:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.SlantNW, threshold);
                            break;
                        case Direction.SlantSE:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.SlantSE, threshold);
                            break;
                        case Direction.SlantSW:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.SlantSW, threshold);
                            break;
                    }

                    if (CalculateDistanceNodes(g_adj, currentNode, adjNode) <= 26 && !proximityNodes.Contains(adjNode))
                    {
                        isCorrect = true;
                        proximityNodes.Add(currentNode);
                    }
                    if (proximityNodes.Contains(adjNode))
                        proximityNodes.Add(currentNode);

                    if (isCorrect && !edgeStackHistory.Contains(adjNode))
                    {
                        edgeStack.Push(adjNode);
                        returnEdges.Add(compTuple);
                        nodePath.Add(adjNode);
                        leaves.Add(adjNode);
                    }

                    if (!isCorrect)
                    {
                        //we've reached a dead end
                        leaves.Remove(adjNode);
                    }
                }
                leafList.AddRange(leaves.Except(leafList));
            }

            //foreach(int leaf in leaves)
            foreach(int leaf in leafList)
            {
                List<int> currentPath = DijkstraBounded(g_adj, mainGraph, startNode, leaf);
                nodePaths.Add(currentPath);
            }

            return nodePaths;
        }

        #endregion

        #region Math Helper Functions
        private static double CalculateDistanceSimple(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt( Math.Pow((x2-x1),2) + Math.Pow((y2 - y1), 2) );
        }

        public static double CalculateDistanceNodes(Dictionary<int, SketchAdjacencyList> g_adj, int node1, int node2)
        {
            //Pulls avgx and avgy coordinates directly from nodes instead of having to put them in from outside this function
            double x1 = g_adj[node1].node.avgx;
            double y1 = g_adj[node1].node.avgy;
            double x2 = g_adj[node2].node.avgx;
            double y2 = g_adj[node2].node.avgy;

            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        public static double CalculateDistanceNodes(Dictionary<int, SketchAdjacencyList> g_adj, List<int> inputNodes)
        {
            double dist=0;
            int prevNode = -1;
            foreach(int node in inputNodes)
            {
                if (prevNode == -1)
                {
                    prevNode = node;
                    continue;
                }
                else
                {
                    dist += CalculateDistanceNodes(g_adj, prevNode, node);
                }
            }
            return dist;

        }

        public static KeyValuePair<int, double> GetMinimum(Dictionary<int, double> inputDict)
        {
            KeyValuePair<int, double> min = new KeyValuePair<int, double>();
            min = new KeyValuePair<int, double>(-1, double.MaxValue);

            foreach (var item in inputDict)
            {
                if (item.Value < min.Value)
                {
                    min = new KeyValuePair<int, double>(item.Key, item.Value);
                }
            }

            return min;
        }

        public static double CalculateArea(Dictionary<int, SketchAdjacencyList> g_adj, List<int> nodePath)
        {
            double area;
            double minX = double.MaxValue; double minY = double.MaxValue; double maxX = 0; double maxY = 0;
            foreach (int node in nodePath)
            {
                if (g_adj[node].node.avgx < minX)
                    minX = g_adj[node].node.avgx;
                if (g_adj[node].node.avgy < minY)
                    minY = g_adj[node].node.avgy;
                if (g_adj[node].node.avgx > maxX)
                    maxX = g_adj[node].node.avgx;
                if (g_adj[node].node.avgy > maxY)
                    maxY = g_adj[node].node.avgy;
            }
            area = (maxX - minX) * (maxY - minY);

            return area;
        }

        public static double SlopeNodes(Dictionary<int, SketchAdjacencyList> g_adj, int node1, int node2)
        {
            return Math.Abs((g_adj[node2].node.avgy - g_adj[node1].node.avgy) / (g_adj[node2].node.avgx - g_adj[node1].node.avgx));
        }

        #endregion

        #region Tuples and Lists Constructors
        public static List<Tuple<int, int>> TupleListConstructor(SketchGraph main_graph, List<int> nodesList)
        {
            //Use this if you have a collection of loose ints and want to get a nicely organized list of the corresponding graph <int, int> Tuples.
            //Otherwise you will just have a loose list of ints without knowing what they connect to. This "connects" them according to main_graph.

            List<Tuple<int, int>> constructedKeys = new List<Tuple<int, int>>();

            if (nodesList != null)
            {
                foreach (Tuple<int, int> key in main_graph.getSubstrokes().Keys)
                {
                    if (nodesList.Contains(key.Item1) && nodesList.Contains(key.Item2))
                    {
                        constructedKeys.Add(key);
                    }
                }
            }

            return constructedKeys;
        }

        public static List<Tuple<int, int>> TupleListConstructorInclusive(SketchGraph main_graph, List<int> nodesList)
        {
            //Slight variant of the TupleListConstructor. Builds a List of tuples that correspond to the main_graph Tuple list,
            //but also includes tuples connected to the given int list (as in, any other node connected to the given ints, they don't all have to be in
            //the given input). Needed ONLY for Detail 12. Do not use in any other instance unless constructing Tuples from a "region"

            List<Tuple<int, int>> constructedKeys = new List<Tuple<int, int>>();

            foreach (Tuple<int, int> key in main_graph.getSubstrokes().Keys)
            {
                if (nodesList.Contains(key.Item1) || nodesList.Contains(key.Item2))
                {
                    constructedKeys.Add(key);
                }
            }


            return constructedKeys;
        }

        public static Tuple<int, int> SingleTupleConstructor(SketchGraph main_graph, int node1, int node2)
        {
            //A simpler version of TupleListConstructor. Receives two separate nodes and returns the corresponding Tuple from the Sketch Graph
            //Returns a tuple of <-1,-1> if there is no corresponding Tuple.
            foreach (Tuple<int, int> key in main_graph.getSubstrokes().Keys)
            {
                if (node1 == key.Item1 && node2 == key.Item2)
                {
                    Tuple<int, int> constructedTuple = new Tuple<int, int>(node1, node2);
                    return constructedTuple;
                }
                else if (node2 == key.Item1 && node1 == key.Item2)
                {
                    Tuple<int, int> constructedTuple = new Tuple<int, int>(node2, node1);
                    return constructedTuple;
                }
            }
            Tuple<int, int> nullTuple = new Tuple<int, int>(-1, -1);
            return nullTuple;
        }

        public static List<int> NodeListConstructor(List<Tuple<int, int>> inputTupList)
        {
            //Does the reverse of TupleListConstructor, takes an input Tuple List and returns the unique nodes

            List<int> singleNodes = new List<int>();

            foreach(Tuple<int, int> tup in inputTupList)
            {
                if (!singleNodes.Contains(tup.Item1))
                    singleNodes.Add(tup.Item1);
                if (!singleNodes.Contains(tup.Item2))
                    singleNodes.Add(tup.Item2);
            }

            return singleNodes; //all the single nodes, all the single nodes
        }

        public static List<int> TupleListIntersect(List<Tuple<int, int>> tupList1, List<Tuple<int, int>> tupList2)
        {
            List<int> intList1 = new List<int>(NodeListConstructor(tupList1));
            List<int> intList2 = new List<int>(NodeListConstructor(tupList2));

            List<int> intersectList = new List<int>(intList1.Intersect(intList2));
            return intersectList;
        }
        #endregion

        #region Misc
        public static int CheckOnce(Dictionary<int, SketchAdjacencyList> g_nodes, int inputNode, Direction dir, double threshold)
        {
            //Checks once in any given direction, and if successful returns the node so that we can crawl to it
            //Can also be used as a boolean; if we return -1, then the check failed
            int crawledNode = -1;

            SketchAdjacencyList skAdj;
            g_nodes.TryGetValue(inputNode, out skAdj);
            foreach (int adjNode in skAdj.adjacent)
            {
                Tuple<int, int> compTuple = new Tuple<int, int>(inputNode, adjNode);
                if (Crawler.IsDirection(compTuple, g_nodes, dir, threshold))
                {
                    crawledNode = adjNode;
                    break;
                }
            }

            return crawledNode;
        }

        public static List<int> CheckCross(Dictionary<int, SketchAdjacencyList> g_nodes, int inputNode, Direction dir, double thresholdHor, double thresholdVer)
        {
            //Crawls in given direction and checks for a "cross"
            //"cross" = a node where there is exactly one dead-end node coming out of the three remaining directions
            //If we do not find a cross, crawls in that direction and checks again
            //If successful, returns a List of all nodes, including dead-ends, involved in this process
            //If failed, returns a single -1 int (to check for failure)

            List<int> crossLines = new List<int>();
            int currentNode, upNode, downNode, leftNode, rightNode;
            bool crossFound = false;

            currentNode = inputNode;
            while(currentNode != -1)
            {
                crossLines.Add(currentNode);
                upNode = CheckOnce(g_nodes, currentNode, Direction.Up, thresholdVer);
                downNode = CheckOnce(g_nodes, currentNode, Direction.Down, thresholdVer);
                leftNode = CheckOnce(g_nodes, currentNode, Direction.Left, thresholdHor);
                rightNode = CheckOnce(g_nodes, currentNode, Direction.Right, thresholdHor);

                switch (dir)
                {
                    case Direction.Right:
                        if(upNode != -1 && rightNode != -1 && downNode != -1)
                        {
                            //want to make sure we're encountering dead-ends
                            SketchAdjacencyList skAdj;
                            g_nodes.TryGetValue(upNode, out skAdj);
                            if (skAdj.adjacent.Count > 1)
                                break;

                            g_nodes.TryGetValue(rightNode, out skAdj);
                            if (skAdj.adjacent.Count > 1)
                                break;

                            g_nodes.TryGetValue(downNode, out skAdj);
                            if (skAdj.adjacent.Count > 1)
                                break;

                            crossLines.Add(upNode);
                            crossLines.Add(rightNode);
                            crossLines.Add(downNode);
                            crossFound = true;
                        }
                        break;
                    case Direction.Up:
                        if(upNode != -1 && leftNode != -1 && rightNode != -1)
                        {
                            //want to make sure we're encountering dead-ends
                            SketchAdjacencyList skAdj;
                            g_nodes.TryGetValue(upNode, out skAdj);
                            if (skAdj.adjacent.Count > 1)
                                break;

                            g_nodes.TryGetValue(leftNode, out skAdj);
                            if (skAdj.adjacent.Count > 1)
                                break;

                            g_nodes.TryGetValue(rightNode, out skAdj);
                            if (skAdj.adjacent.Count > 1)
                                break;

                            crossLines.Add(upNode);
                            crossLines.Add(leftNode);
                            crossLines.Add(rightNode);
                            crossFound = true;
                        }
                        break;
                }

                if (crossFound)
                    return crossLines;

                currentNode = CheckOnce(g_nodes, currentNode, dir, thresholdHor);
            }

            crossLines.Clear();
            //crossLines.Add(-1);
            return crossLines;
        }

        public static bool IsOverlapping(List<int> newNodes, List<Tuple<int, int>> existingTuples, SketchGraph main_graph)
        {
            //Returns true if the input node path is at least partially in another Tuple list
            //Used to ensure new recognized tuples are not already part of some other recognized detail.

            List<Tuple<int, int>> newTuples = new List<Tuple<int, int>>(TupleListConstructor(main_graph, newNodes));

            foreach(Tuple<int, int> tup in newTuples)
            {
                if (existingTuples.Contains(tup))
                    return true;
            }
            return false;
        }

        /*public static void RemoveOverlap(Dictionary<int, SketchAdjacencyList> g_nodes, List<Tuple<int, int>> inputTuples, List<Tuple<int, int>> tupsToRemove)
        {

        }*/
        #endregion

        #region Detail-specific Functions

        public static int CheckNPop(List<List<int>> inputCandidates, Dictionary<int, SketchAdjacencyList> g_adj, Direction dir, double threshold, List<int> allNodes, out List<int> lineNodes)
        {
            //Specific to Detail 2
            //Goes through all input paths from the previous line, checking if they're attached to an edge that is of the NEXT direction
            //If the last node of the longest path is not connected to such an edge, that node is popped form the List
            //The process then repeats for the now longest path (might be the same or different path)
            //Returns the first node it sees that is the start of the new direction or -1 if it did not find one
            int startNext = -1;
            List<int> currentPath = new List<int>();
            bool startFound = false;

            while (!startFound)
            {
                int pathInd = -1;
                //int longestPathCount = 0;
                double longestPathDist = 0;
                for (int i = 0; i < inputCandidates.Count; i++)
                {
                    double pathDist = Crawler.CalculateDistanceNodes(g_adj, inputCandidates[i]);
                    if (pathDist > longestPathDist)
                    {
                        currentPath.Clear();
                        currentPath.AddRange(inputCandidates[i]);
                        longestPathDist = pathDist;
                        pathInd = i;
                    }
                }
                if (currentPath.Count == 0 || pathInd == -1)
                    break;

                bool isCorrect = false;
                SketchAdjacencyList skAdj;
                g_adj.TryGetValue(currentPath[currentPath.Count - 1], out skAdj);

                foreach (int adjNode in skAdj.adjacent)
                {
                    Tuple<int, int> compTuple = new Tuple<int, int>(currentPath[currentPath.Count - 1], adjNode);
                    switch (dir)
                    {
                        case Direction.Up:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Up, 2);
                            break;
                        case Direction.Down:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Down, 2);
                            break;
                        case Direction.Left:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Left, 0.2);
                            break;
                        case Direction.Right:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.Right, 0.2);
                            break;
                        case Direction.SlantNE:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.SlantNE, threshold);
                            break;
                        case Direction.SlantNW:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.SlantNW, threshold);
                            break;
                        case Direction.SlantSE:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.SlantSE, threshold);
                            break;
                        case Direction.SlantSW:
                            isCorrect = IsDirection(compTuple, g_adj, Direction.SlantSW, threshold);
                            break;
                    }

                    if (isCorrect)
                    {
                        //Do another check here, we are skipping this if adjNode is in other nodes that we've found already
                        //Otherwise we risk going "backwards" and picking the node that will go in the opposite direction
                        if (!allNodes.Contains(adjNode))
                        {
                            startNext = currentPath[currentPath.Count - 1];
                            lineNodes = currentPath;
                            return startNext;
                        }
                        else
                        {
                            isCorrect = false;
                        }
                    }
                }
                if (!isCorrect || pathInd == -1)
                    inputCandidates[pathInd].RemoveAt(currentPath.Count - 1);
            }

            lineNodes = new List<int>();
            return startNext;
        }

        #endregion
    }
}
