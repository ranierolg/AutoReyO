using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SmartPenSketch.MainPage;
using static SmartPenSketch.SketchRecTools;
using static SmartPenSketch.Crawler;
using static SmartPenSketch.InsidePolygon;

namespace SmartPenSketch
{
    class DetailDetector
    {
        //Detail 1 - Cross on Upper Left
        static List<Tuple<int, int>> det1Cross = new List<Tuple<int, int>>();
        static Detail2Box det2Box = new Detail2Box();
        static bool Detail1Found;

        //Detail 2 - Large rectangle
        static List<int> nodesRectangleWest = new List<int>();
        static List<int> nodesRectangleNorth = new List<int>();
        static List<int> nodesRectangleEast = new List<int>();
        static List<int> nodesRectangleSouth = new List<int>();
        static bool Detail2Found;

        //Detail 3 - Diagonal Cross
        static List<int> shortestPathListLU_DR = new List<int>();
        static List<int> shortestPathListLD_UR = new List<int>();
        static bool Detail3Found;

        //Detail 4 - Horizontal Midline of 2
        static List<int> horMidline = new List<int>();
        static bool Detail4Found;

        //Detail 5 - Vertical Midline of 2
        static List<int> verMidline = new List<int>();
        static bool Detail5Found;

        //Detail 6 - Rectangle w/ cross on left within Detail 2
        static List<Tuple<int, int>> det6Rect = new List<Tuple<int, int>>();
        static List<Tuple<int, int>> det6RectAlt = new List<Tuple<int, int>>();
        static bool Detail6Found;

        //Detail 7 - Horizontal line above Detail 6
        static List<Tuple<int, int>> det7Line = new List<Tuple<int, int>>();
        static bool Detail7Found;

        //Detail 8 - Five parallel lines in upper left quadrant
        static List<Tuple<int, int>> det8Parallels = new List<Tuple<int, int>>();
        static bool Detail8Found;

        //Detail 9 - Triangle on top
        static List<Tuple<int, int>> det9Triangle = new List<Tuple<int, int>>();
        static bool Detail9Found;

        //Detail 10 - Small line just below detail 9
        static List<Tuple<int, int>> det10DownLine = new List<Tuple<int, int>>();
        static bool Detail10Found;

        //Detail 12 - Five parallel lines on bottom right region
        static List<Tuple<int, int>> det12Parallels = new List<Tuple<int, int>>();
        static bool Detail12Found;

        //Detail 13 - Triangle to the right of 2
        static List<Tuple<int, int>> det13Triangle = new List<Tuple<int, int>>();
        static List<int> det13LowerTriangle = new List<int>();
        static bool Detail13Found;

        //Detail 15 - Vertical midline inside Detail 13
        static List<Tuple<int, int>> det15Line = new List<Tuple<int, int>>();
        static bool Detail15Found;

        //Detail 16 - Horizontal midline inside Detail 13
        static List<Tuple<int, int>> det16Line = new List<Tuple<int, int>>();
        static bool Detail16Found;

        //Detail 17 - Cross attached to low center
        static List<Tuple<int, int>> det17Cross = new List<Tuple<int, int>>();
        static bool Detail17Found;

        //Detail 18 - Rectangle below Det 2
        static List<Tuple<int, int>> det18Rect = new List<Tuple<int, int>>();
        static List<int> subDet18East = new List<int>();
        static bool Detail18Found;

        //Exemption list - we use this when we want to exclude certain nodes form our comparison
        //Should not be used as a NECESSARY component of recognition, but rather something to help it
        static List<int> exemptNodes = new List<int>();

        public static void initDetailData()
        {
            det1Cross = new List<Tuple<int, int>>();
            det2Box = new Detail2Box();
            nodesRectangleWest = new List<int>();
            nodesRectangleNorth = new List<int>();
            nodesRectangleEast = new List<int>();
            nodesRectangleSouth = new List<int>();
            shortestPathListLU_DR = new List<int>();
            shortestPathListLD_UR = new List<int>();
            horMidline = new List<int>();
            verMidline = new List<int>();
            det6Rect = new List<Tuple<int, int>>();
            det6RectAlt = new List<Tuple<int, int>>();
            det7Line = new List<Tuple<int, int>>();
            det8Parallels = new List<Tuple<int, int>>();
            det9Triangle = new List<Tuple<int, int>>();
            det10DownLine = new List<Tuple<int, int>>();
            det12Parallels = new List<Tuple<int, int>>();
            det13Triangle = new List<Tuple<int, int>>();
            det13LowerTriangle = new List<int>();
            det15Line = new List<Tuple<int, int>>();
            det16Line = new List<Tuple<int, int>>();
            det17Cross = new List<Tuple<int, int>>();
            det18Rect = new List<Tuple<int, int>>();
            subDet18East = new List<int>();

            Detail1Found = false;
            Detail2Found = false;
            Detail3Found = false;
            Detail4Found = false;
            Detail5Found = false;
            Detail6Found = false;
            Detail7Found = false;
            Detail8Found = false;
            Detail9Found = false;
            Detail10Found = false;
            //Detail11Found = false;
            Detail12Found = false;
            Detail13Found = false;
            //Detail14Found = false;
            Detail15Found = false;
            Detail16Found = false;
            Detail17Found = false;
            Detail18Found = false;
        }

        public static List<Tuple<int, int>> Detail1(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<Tuple<int, int>> candidateEdges = new List<Tuple<int, int>>();
            List<Tuple<int, int>> preVisitedEdges = new List<Tuple<int, int>>();

            if (Detail2Found)
            {
                preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleWest));
                preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleNorth));
                preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleSouth));
                preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleEast));
            }

            foreach(int currentNode in nodesRectangleWest)
            {
                int nodeCheck = Crawler.CheckOnce(g_nodes, currentNode, Direction.Left, 0.75);
                if (nodeCheck != -1 && !nodesRectangleWest.Contains(nodeCheck))
                {
                    //candidateEdges.AddRange(Crawler.DFSBounded(g_nodes, main_graph, preVisitedEdges, nodeCheck));
                    candidateEdges.AddRange(Crawler.DFSDirectionBounded(g_nodes, main_graph, preVisitedEdges, currentNode, -1, Direction.Left, 0.75));
                }
            }

            det1Cross.AddRange(candidateEdges);
            Detail1Found = true;
            return det1Cross;

        }

        public static List<Tuple<int, int>> Detail2(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            KeyValuePair<Tuple<int, int>, SketchSubstroke> firstElement = g_temp.FirstOrDefault();

            //This checks for verticals and crawls into the next.
            List<Tuple<int, int>> crawledKeys = new List<Tuple<int, int>>();

            //Daisy-chain Straight-Crawl routine, where the starting node is the last added node of the previous straight path
            //nodesRectangleWest = Crawler.StraightCrawl(graph_nodes, graph_temp, mainGraph, firstElement.Key.Item1, Direction.Up, crawledKeys);
            nodesRectangleWest = Crawler.StraightCrawl(g_nodes, main_graph, 4, Direction.Up, crawledKeys);
            nodesRectangleNorth = Crawler.StraightCrawl(g_nodes, main_graph, nodesRectangleWest[nodesRectangleWest.Count - 1], Direction.Right, crawledKeys);
            nodesRectangleEast = Crawler.StraightCrawl(g_nodes, main_graph, nodesRectangleNorth[nodesRectangleNorth.Count - 1], Direction.Down, crawledKeys);
            nodesRectangleSouth = Crawler.StraightCrawl(g_nodes, main_graph, nodesRectangleEast[nodesRectangleEast.Count - 1], Direction.Left, crawledKeys);

            Detail2Found = true; //turns on automatically, will need to make a check later.

            return crawledKeys;
            //DrawKeyPath(graph_nodes, crawledKeys, Colors.Red);
        }

        public static List<Tuple<int, int>> Detail2Alt(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph){
            //This checks for verticals and crawls into the next.
            List<Tuple<int, int>> crawledKeys = new List<Tuple<int, int>>();
            List<Tuple<int, int>> candidateEdges = new List<Tuple<int, int>>();
            List<int> rectNodes = new List<int>();

            //Daisy-chain Straight-Crawl routine, where the starting node is the last added node of the previous straight path
            //nodesRectangleWest = Crawler.StraightCrawl(graph_nodes, graph_temp, mainGraph, firstElement.Key.Item1, Direction.Up, crawledKeys);

            foreach (int node in g_nodes.Keys)
            {
                crawledKeys.Clear();
                nodesRectangleNorth = Crawler.StraightCrawl(g_nodes, main_graph, node, Direction.Right, crawledKeys);

                if (nodesRectangleNorth.Count >= 2)
                {
                    while (Crawler.CheckOnce(g_nodes, nodesRectangleNorth[nodesRectangleNorth.Count - 1], Direction.Down, 2) == -1)
                    {
                        nodesRectangleNorth.RemoveAt(nodesRectangleNorth.Count - 1);
                        if (nodesRectangleNorth.Count == 0)
                            break;
                    }
                }

                if (nodesRectangleNorth.Count == 0)
                    continue;
                nodesRectangleEast = Crawler.StraightCrawl(g_nodes, main_graph, nodesRectangleNorth[nodesRectangleNorth.Count - 1], Direction.Down, crawledKeys);

                if (nodesRectangleEast.Count >= 2)
                {
                    while (Crawler.CheckOnce(g_nodes, nodesRectangleEast[nodesRectangleEast.Count - 1], Direction.Left, 0.2) == -1)
                    {
                        nodesRectangleEast.RemoveAt(nodesRectangleEast.Count - 1);
                        if (nodesRectangleEast.Count == 0)
                            break;
                    }
                }

                if (nodesRectangleEast.Count == 0)
                    continue;
                nodesRectangleSouth = Crawler.StraightCrawl(g_nodes, main_graph, nodesRectangleEast[nodesRectangleEast.Count - 1], Direction.Left, crawledKeys);

                if (nodesRectangleSouth.Count >= 2)
                {
                    while (Crawler.CheckOnce(g_nodes, nodesRectangleSouth[nodesRectangleSouth.Count - 1], Direction.Up, 2) == -1)
                    {
                        nodesRectangleSouth.RemoveAt(nodesRectangleSouth.Count - 1);
                        if (nodesRectangleSouth.Count == 0)
                            break;
                    }
                }

                if (nodesRectangleSouth.Count == 0)
                    continue;
                nodesRectangleWest = Crawler.StraightCrawl(g_nodes, main_graph, nodesRectangleSouth[nodesRectangleSouth.Count - 1], Direction.Up, crawledKeys);


                if (nodesRectangleWest[nodesRectangleWest.Count - 1] != node || nodesRectangleNorth.Count < 2 || nodesRectangleSouth.Count < 2 || nodesRectangleEast.Count < 2 || nodesRectangleWest.Count < 2)
                {
                    nodesRectangleEast.Clear();
                    nodesRectangleNorth.Clear();
                    nodesRectangleSouth.Clear();
                    nodesRectangleWest.Clear();
                }
                else
                {
                    rectNodes.AddRange(nodesRectangleEast);
                    rectNodes.AddRange(nodesRectangleNorth);
                    rectNodes.AddRange(nodesRectangleSouth);
                    rectNodes.AddRange(nodesRectangleWest);

                    candidateEdges.AddRange(Crawler.TupleListConstructor(main_graph, rectNodes));
                    //break;
                }
            }

            /*rectNodes.AddRange(nodesRectangleEast);
            rectNodes.AddRange(nodesRectangleNorth);
            rectNodes.AddRange(nodesRectangleSouth);
            rectNodes.AddRange(nodesRectangleWest);

            candidateEdges.AddRange(Crawler.TupleListConstructor(main_graph, rectNodes));*/

            Detail2Found = true; //turns on automatically, will need to make a check later.

            return candidateEdges;
        }

        public static List<Tuple<int, int>> Detail2Alt_2(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph, out Detail2Box det2Bx)
        {
            List<Tuple<int, int>> outputKeys = new List<Tuple<int, int>>();
            List<List<int>> inputCandidates = new List<List<int>>();
            List<Detail2Box> boxes = new List<Detail2Box>();

            foreach (int node in g_nodes.Keys)
            {
                if(node == 4)
                {
                    Debug.WriteLine("hit");
                }
                List<int> allNodes = new List<int>();
                int cornerNW = -1; int cornerNE = -1; int cornerSW = -1; int cornerSE=-1;

                inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, node, Direction.Right, 0.3);
                cornerNE = Crawler.CheckNPop(inputCandidates, g_nodes, Direction.Down, 2, allNodes, out nodesRectangleNorth);
                allNodes.AddRange(nodesRectangleNorth);

                if (cornerNE != -1)
                {
                    inputCandidates.Clear();
                    inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, cornerNE, Direction.Down, 2);
                    cornerSE = Crawler.CheckNPop(inputCandidates, g_nodes, Direction.Left, 0.3, allNodes, out nodesRectangleEast);
                    allNodes.AddRange(nodesRectangleEast);
                }

                if(cornerSE != -1)
                {
                    inputCandidates.Clear();
                    inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, cornerSE, Direction.Left, 0.3);
                    cornerSW = Crawler.CheckNPop(inputCandidates, g_nodes, Direction.Up, 2, allNodes, out nodesRectangleSouth);
                    allNodes.AddRange(nodesRectangleSouth);
                }

                if (cornerSW != -1)
                {
                    bool fastCornerFound = false;
                    inputCandidates.Clear();
                    foreach (int nd in nodesRectangleNorth)
                        allNodes.Remove(nd);
                    inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, cornerSW, Direction.Up, 2);

                    foreach(List<int> candidate in inputCandidates)
                    {
                        if(candidate[candidate.Count-1] == nodesRectangleNorth[0])
                        {
                            nodesRectangleWest = candidate;
                            cornerNW = candidate[candidate.Count - 1];
                            fastCornerFound = true;
                        }
                    }
                    if(!fastCornerFound)
                        cornerNW = Crawler.CheckNPop(inputCandidates, g_nodes, Direction.Right, 0.3, allNodes, out nodesRectangleWest);
                    allNodes.AddRange(nodesRectangleWest);

                    //Debug.WriteLine(Crawler.SlopeNodes(g_nodes, 72, 33).ToString());
                }

                if (nodesRectangleNorth.Count > 0)
                {
                    if (nodesRectangleNorth[0] == cornerNW)
                    {
                        boxes.Add(new Detail2Box(nodesRectangleNorth, nodesRectangleSouth, nodesRectangleEast, nodesRectangleWest, g_nodes));
                    }
                }
            }

            double largestArea=0;
            Detail2Box largestBox = new Detail2Box();
            foreach(Detail2Box box in boxes)
            {
                if (box.area > largestArea)
                {
                    largestBox = box;
                    largestArea = box.area;
                    Detail2Found = true;
                }
            }

            outputKeys.AddRange(Crawler.TupleListConstructor(main_graph, largestBox.allNodes));
            if (Detail2Found == false)
            {
                det2Box = largestBox;
                det2Bx = det2Box;
                return outputKeys;
            }

            //Construct the Detail 2 Box object
            nodesRectangleEast = largestBox.nodesRectangleEast;
            nodesRectangleNorth = largestBox.nodesRectangleNorth;
            nodesRectangleSouth = largestBox.nodesRectangleSouth;
            nodesRectangleWest = largestBox.nodesRectangleWest;

            largestBox.cornerNE = nodesRectangleNorth.Intersect(nodesRectangleEast).FirstOrDefault();
            largestBox.cornerNW = nodesRectangleNorth.Intersect(nodesRectangleWest).FirstOrDefault();
            largestBox.cornerSE = nodesRectangleSouth.Intersect(nodesRectangleSouth).FirstOrDefault();
            largestBox.cornerSW = nodesRectangleSouth.Intersect(nodesRectangleWest).FirstOrDefault();

            det2Box = largestBox;
            det2Bx = det2Box;


            return outputKeys;
        }

        public static List<int> Detail3_Back(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            shortestPathListLU_DR = Crawler.DijkstraBounded(g_nodes, main_graph, det2Box.cornerNW, det2Box.cornerSE);
            return shortestPathListLU_DR;
        }

        public static List<int> Detail3_Forward(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            shortestPathListLD_UR = Crawler.DijkstraBounded(g_nodes, main_graph, det2Box.cornerSW, det2Box.cornerNE);
            Detail3Found = true;
            return shortestPathListLD_UR;
        }

        public static List<int> Detail4(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            List<Tuple<int, int>> boundPath = new List<Tuple<int, int>>();
            bool foundLine = false;

            List<int> nodesWestInner = new List<int>(det2Box.nodesRectangleWest);
            List<int> nodesEastInner = new List<int>(det2Box.nodesRectangleEast);

            nodesWestInner.RemoveAt(nodesWestInner.Count - 1);
            nodesWestInner.RemoveAt(0);

            nodesEastInner.RemoveAt(nodesEastInner.Count - 1);
            nodesEastInner.RemoveAt(0);

            //Debug.WriteLine(Crawler.SlopeNodes(g_nodes, 27, 74));
            int half = Convert.ToInt32(Math.Floor(Convert.ToDouble(nodesWestInner.Count / 2)));

            for (int i = 0; i <= half; i++)
            {
                int node;
                if (!(half - i < 0))
                {
                    node = nodesWestInner[half - i];
                    foreach (int node2 in nodesEastInner)
                    {
                        Tuple<int, int> inTuple = new Tuple<int, int>(node, node2);
                        if (Crawler.IsDirection(inTuple, g_nodes, Direction.Right, 0.06))
                        {
                            boundPath.Add(inTuple);
                            foundLine = true;
                            break;
                        }
                    }
                }
                if (!(half + i > nodesWestInner.Count - 1))
                {
                    node = nodesWestInner[half + i];
                    foreach (int node2 in nodesEastInner)
                    {
                        Tuple<int, int> inTuple = new Tuple<int, int>(node, node2);
                        if (Crawler.IsDirection(inTuple, g_nodes, Direction.Right, 0.06))
                        {
                            boundPath.Add(inTuple);
                            foundLine = true;
                            break;
                        }
                    }
                }

                if (foundLine)
                {
                    Detail4Found = true;
                    break;
                }

            }

            //List<int> crossHorPathList = new List<int>(); ;
            foreach (Tuple<int, int> candidate in boundPath)
            {
                horMidline = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, candidate.Item1, candidate.Item2));
            }
            Detail5Found = true;    //Always returns true for now, but we will need to verify eventually
            return horMidline;
        }

        public static List<int> Detail5(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            /*List<Tuple<int, int>> boundPath = new List<Tuple<int, int>>();
            bool foundLine = false;

            List<int> nodesNorthInner = new List<int>(nodesRectangleNorth);
            List<int> nodesSouthInner = new List<int>(nodesRectangleSouth);

            nodesNorthInner.RemoveAt(nodesNorthInner.Count - 1);
            nodesNorthInner.RemoveAt(0);

            nodesSouthInner.RemoveAt(nodesSouthInner.Count - 1);
            nodesSouthInner.RemoveAt(0);

            foreach (int node in nodesNorthInner)
            {
                foreach (int node2 in nodesSouthInner)
                {
                    Tuple<int, int> inTuple = new Tuple<int, int>(node, node2);
                    if(Crawler.IsDirection(inTuple, g_nodes, Direction.Down, 7))
                    {
                        boundPath.Add(inTuple);
                        foundLine = true;
                        break;
                    }
                }
                if (foundLine)
                    break;
            }*/

            List<Tuple<int, int>> boundPath = new List<Tuple<int, int>>();
            bool foundLine = false;

            List<int> nodesNorthInner = new List<int>(det2Box.nodesRectangleNorth);
            List<int> nodesSouthInner = new List<int>(det2Box.nodesRectangleSouth);

            nodesNorthInner.RemoveAt(nodesNorthInner.Count - 1);
            nodesNorthInner.RemoveAt(0);

            nodesSouthInner.RemoveAt(nodesSouthInner.Count - 1);
            nodesSouthInner.RemoveAt(0);

            //Debug.WriteLine(Crawler.SlopeNodes(g_nodes, 27, 74));
            int half = Convert.ToInt32(Math.Floor(Convert.ToDouble(nodesNorthInner.Count / 2)));

            for (int i = 0; i <= half; i++)
            {
                int node;
                if (!(half - i < 0))
                {
                    node = nodesNorthInner[half - i];
                    foreach (int node2 in nodesSouthInner)
                    {
                        Tuple<int, int> inTuple = new Tuple<int, int>(node, node2);
                        if (Crawler.IsDirection(inTuple, g_nodes, Direction.Down, 7))
                        {
                            boundPath.Add(inTuple);
                            foundLine = true;
                            break;
                        }
                    }
                }
                if (!(half + i > nodesNorthInner.Count - 1))
                {
                    node = nodesNorthInner[half + i];
                    foreach (int node2 in nodesSouthInner)
                    {
                        Tuple<int, int> inTuple = new Tuple<int, int>(node, node2);
                        if (Crawler.IsDirection(inTuple, g_nodes, Direction.Down, 7))
                        {
                            boundPath.Add(inTuple);
                            foundLine = true;
                            break;
                        }
                    }
                }

                if (foundLine)
                {
                    Detail5Found = true;
                    break;
                }
            }

            //List<int> crossHorPathList = new List<int>(); ;
            foreach (Tuple<int, int> candidate in boundPath)
            {
                verMidline = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, candidate.Item1, candidate.Item2));
            }
            Detail5Found = true;    //Always returns true for now, but we will need to verify eventually
            return verMidline;
        }

        public static List<int> Detail4_Alt(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int,int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            List<int> nodesWestInner = new List<int>(det2Box.nodesRectangleWest);
            List<int> nodesEastInner = new List<int>(det2Box.nodesRectangleEast);

            nodesWestInner.RemoveAt(nodesWestInner.Count - 1);
            nodesWestInner.RemoveAt(0);

            nodesEastInner.RemoveAt(nodesEastInner.Count - 1);
            nodesEastInner.RemoveAt(0);

            if (nodesWestInner.Count == 0 || nodesEastInner.Count == 0)
                return horMidline;

            int half = Convert.ToInt32(Math.Floor(Convert.ToDouble(nodesWestInner.Count / 2)));

            List<List<int>> inputCandidates = new List<List<int>>();

            for (int i = 0; i <= half; i++)
            {
                int node=-1;
                if (!(half - i < 0))
                {
                    node = nodesWestInner[half - i];
                    inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, node, Direction.Right, 0.2);
                    foreach (List<int> nodeList in inputCandidates)
                    {
                        int p = 1;
                        while (p <= nodeList.Count)
                        {
                            if (nodesEastInner.Contains(nodeList[nodeList.Count - p]))
                            {
                                horMidline = nodeList.GetRange(0, nodeList.Count - p + 1);
                                //horMidline = nodeList;
                                Detail4Found = true;
                                return horMidline;
                            }
                            p++;
                        }
                    }
                }
                if (!(half + i > nodesWestInner.Count - 1))
                {
                    node = nodesWestInner[half + i];
                    inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, node, Direction.Right, 0.2);
                    foreach (List<int> nodeList in inputCandidates)
                    {
                        int p = 1;
                        while (p <= nodeList.Count)
                        {
                            if (nodesEastInner.Contains(nodeList[nodeList.Count - p]))
                            {
                                horMidline = nodeList.GetRange(0, nodeList.Count - p + 1);
                                //horMidline = nodeList;
                                Detail4Found = true;
                                return horMidline;
                            }
                            p++;
                        }
                    }
                }

                
            }
            return horMidline;  //horMidline will be 0 at this point but we're still returning it so that we do not return "null", but rather an empty List
        }

        public static List<int> Detail5_Alt(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            List<int> nodesNorthInner = new List<int>(det2Box.nodesRectangleNorth);
            List<int> nodesSouthInner = new List<int>(det2Box.nodesRectangleSouth);

            nodesNorthInner.RemoveAt(nodesNorthInner.Count - 1);
            nodesNorthInner.RemoveAt(0);

            nodesSouthInner.RemoveAt(nodesSouthInner.Count - 1);
            nodesSouthInner.RemoveAt(0);

            int half = Convert.ToInt32(Math.Floor(Convert.ToDouble(nodesNorthInner.Count / 2)));

            List<List<int>> inputCandidates = new List<List<int>>();

            for (int i = 0; i <= half; i++)
            {
                int node = -1;
                if (!(half - i < 0) && nodesNorthInner.Count>0)
                {
                    node = nodesNorthInner[half - i];
                    inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, node, Direction.Down, 2);
                    foreach (List<int> nodeList in inputCandidates)
                    {
                        int p = 1;
                        while (p <= nodeList.Count)
                        {
                            if (nodesSouthInner.Contains(nodeList[nodeList.Count - p]))
                            {
                                verMidline = nodeList.GetRange(0, nodeList.Count - p + 1);
                                //horMidline = nodeList;
                                Detail5Found = true;
                                return verMidline;
                            }
                            p++;
                        }
                    }
                }
                if (!(half + i > nodesNorthInner.Count - 1))
                {
                    node = nodesNorthInner[half + i];
                    inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, node, Direction.Down, 2);
                    foreach (List<int> nodeList in inputCandidates)
                    {
                        int p = 1;
                        while (p <= nodeList.Count)
                        {
                            if (nodesSouthInner.Contains(nodeList[nodeList.Count - p]))
                            {
                                verMidline = nodeList.GetRange(0, nodeList.Count - p + 1);
                                //horMidline = nodeList;
                                Detail5Found = true;
                                return verMidline;
                            }
                            p++;
                        }
                    }
                }


            }
            return verMidline;
        }

        public static List<Tuple<int, int>> Detail6(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            List<Tuple<int, int>> boundPath = new List<Tuple<int, int>>();
            List<Tuple<int, int>> allKeys = new List<Tuple<int, int>>();

            List<int> nodesWestInner = new List<int>(nodesRectangleWest);
            nodesWestInner.RemoveAt(nodesWestInner.Count - 1);
            nodesWestInner.RemoveAt(0);

            int cornerNW= new int(); int cornerNE = new int(); int cornerSW = new int(); int cornerSE = new int();
            int candidateNE = new int();
            int candidateSE = new int();
            bool startPhase2 = false;
            int startPhase3 = 0;

            //We're going to need booleans to flag when we have identified corners
            //A corner in Detail 6 is characterized by a right angle and a slant, both originating from the same point
            //Example:
            //  |  /               \  |
            //  | /        OR       \ |
            //  |/_____         _____\|
            //If we can detect four of them, in the appripriate directions, we consider the shape drawn

            List<Tuple<int, int>> adjacentKeys = new List<Tuple<int, int>>();
            

            foreach (int node in nodesWestInner)
            {
                adjacentKeys.Clear();
                foreach (Tuple<int, int> key in main_graph.getSubstrokes().Keys)
                {
                    if (node.Equals(key.Item1) || node.Equals(key.Item2)/* && !(nodesWestInner.Contains(key.Item1)*/)
                    {
                        adjacentKeys.Add(key);
                    }
                }

                int? IsRight = null;
                int? IsNE = null;
                int? IsUp = null;
                int? IsDown = null;
                int? IsSE = null;

                //check the lower left and upper left corners
                foreach (Tuple<int, int> key in adjacentKeys)
                {
                    Tuple<int, int> keyToUse = key;     //Need to use this to reassign keyToUse if the keys are backwards

                    //For every edge attached to this node, check if the edge is to the right
                    if (key.Item2.Equals(node))
                    {
                        //We are backwards!
                        Tuple<int, int> switchedKey = new Tuple<int, int>(key.Item2, key.Item1);
                        keyToUse = new Tuple<int, int>(switchedKey.Item1, switchedKey.Item2);
                        //Debug.WriteLine("NODES: " + switchedKey.Item1 + ", " + switchedKey.Item2 + " | IS RIGHT? " + IsRight + " | IS NE? " + IsNE + " | IS UP? " + IsUp);
                    }

                    if (Crawler.IsDirection(keyToUse, g_nodes, Direction.Right, 0.2))
                        IsRight = keyToUse.Item2;
                    if (Crawler.IsDirection(keyToUse, g_nodes, Direction.SlantNE, 0.8))
                        IsNE = keyToUse.Item2;
                    if (Crawler.IsDirection(keyToUse, g_nodes, Direction.Up, 20))
                        IsUp = keyToUse.Item2;
                    if (Crawler.IsDirection(keyToUse, g_nodes, Direction.Down, 20))
                        IsDown = keyToUse.Item2;
                    if (Crawler.IsDirection(keyToUse, g_nodes, Direction.SlantSE, 0.8))
                        IsSE = keyToUse.Item2;
                    Debug.WriteLine("NODES: " + key.Item1 + ", " + key.Item2 + " | IS RIGHT? " + IsRight + " | IS NE? " + IsNE + " | IS UP? " + IsUp);
                }

                //At the end of checking for one node, we count up if we have encountered
                //the three edges that comprise our Detail 6 corners (lower left and upper left only)
                if(IsRight!=null && IsNE!= null && IsUp!= null)
                {
                    cornerSW = node;
                    startPhase2 = true;
                    candidateSE = IsRight ?? default(int);
                    Debug.WriteLine("LOWER LEFT CORNER! Node: " + node);
                }
                if(IsRight!=null && IsSE!= null && IsDown!= null)
                {
                    cornerNW = node;
                    startPhase2 = true;
                    candidateNE = IsRight ?? default(int);
                    Debug.WriteLine("UPPER LEFT CORNER! Node: " + node);
                }
            }

            //==PHASE 1 DONE==
            //We have detected the two left corners, there has likely been an attempt to draw Detail 6
            //Phase 2 detects right corners

            if (startPhase2)
            {
                List<int> candidates = new List<int>();
                candidates.Add(candidateNE);
                candidates.Add(candidateSE);

                foreach (int node in candidates)
                {
                    adjacentKeys.Clear();
                    foreach (Tuple<int, int> key in main_graph.getSubstrokes().Keys)
                    {
                        if (node.Equals(key.Item1) || node.Equals(key.Item2)/* && !(nodesWestInner.Contains(key.Item1)*/)
                        {
                            adjacentKeys.Add(key);
                        }
                    }

                    int? IsLeft = null;
                    int? IsNW = null;
                    int? IsUp = null;
                    int? IsDown = null;
                    int? IsSW = null;

                    foreach (Tuple<int, int> key in adjacentKeys)
                    {
                        Tuple<int, int> keyToUse = key;     //Need to use this to reassign keyToUse if the keys are backwards

                        //For every edge attached to this node, check if the edge is to the right
                        if (key.Item2.Equals(node))
                        {
                            //We are backwards!
                            Tuple<int, int> switchedKey = new Tuple<int, int>(key.Item2, key.Item1);
                            keyToUse = new Tuple<int, int>(switchedKey.Item1, switchedKey.Item2);
                            //Debug.WriteLine("NODES: " + switchedKey.Item1 + ", " + switchedKey.Item2 + " | IS RIGHT? " + IsRight + " | IS NE? " + IsNE + " | IS UP? " + IsUp);
                        }

                        if (Crawler.IsDirection(keyToUse, g_nodes, Direction.Left, 0.2))
                            IsLeft = keyToUse.Item2;
                        if (Crawler.IsDirection(keyToUse, g_nodes, Direction.SlantNW, 0.8))
                            IsNW = keyToUse.Item2;
                        if (Crawler.IsDirection(keyToUse, g_nodes, Direction.Up, 20))
                            IsUp = keyToUse.Item2;
                        if (Crawler.IsDirection(keyToUse, g_nodes, Direction.Down, 20))
                            IsDown = keyToUse.Item2;
                        if (Crawler.IsDirection(keyToUse, g_nodes, Direction.SlantSW, 0.8))
                            IsSW = keyToUse.Item2;
                        Debug.WriteLine("NODES: " + key.Item1 + ", " + key.Item2 + " | IS LEFT? " + IsLeft + " | IS NW? " + IsNW + " | IS UP? " + IsUp);
                    }

                    if (IsLeft != null && IsNW != null && IsUp != null)
                    {
                        cornerSE = node;
                        startPhase3++;
                        candidateSE = IsLeft ?? default(int);
                        Debug.WriteLine("LOWER RIGHT CORNER! Node: " + node);
                    }
                    if (IsLeft != null && IsSW != null && IsDown != null)
                    {
                        cornerNE = node;
                        startPhase3++;
                        candidateNE = IsLeft ?? default(int);
                        Debug.WriteLine("UPPER RIGHT CORNER! Node: " + node);
                    }
                }
                
            }

            //==PHASE 2 DONE==
            //We have detected all 4 corners, only start phase 3 if we have detected BOTH corners of phase 2
            //Phase 3 connects the diagonals, and constructs the list of keys that comprise the Detail
            if (startPhase3 == 2)
            {
                List<int> NWtoSE = new List<int>();
                List<int> SWtoNE = new List<int>();

                List<int> leftSide = new List<int>();
                List<int> rightSide = new List<int>();
                List<int> upSide = new List<int>();
                List<int> downSide = new List<int>();

                List<int> allNodes = new List<int>();

                List<Tuple<int, int>> diag1 = new List<Tuple<int, int>>();
                List<Tuple<int, int>> diag2 = new List<Tuple<int, int>>();

                NWtoSE = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, cornerNW, cornerSE));
                SWtoNE = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, cornerSW, cornerNE));

                leftSide = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, cornerSW, cornerNW));
                rightSide = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, cornerSE, cornerNE));
                upSide = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, cornerNW, cornerNE));
                downSide = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, cornerSW, cornerSE));

                //allNodes.AddRange(NWtoSE);
                //allNodes.AddRange(SWtoNE);
                allNodes.AddRange(leftSide);
                allNodes.AddRange(rightSide);
                allNodes.AddRange(upSide);
                allNodes.AddRange(downSide);

                diag1 = Crawler.TupleListConstructor(main_graph, NWtoSE);
                diag2 = Crawler.TupleListConstructor(main_graph, SWtoNE);
                allKeys = Crawler.TupleListConstructor(main_graph, allNodes);

                allKeys.AddRange(diag1);
                allKeys.AddRange(diag2);
            }

            det6Rect = allKeys;
            return allKeys;
        }

        public static List<Tuple<int, int>> Detail6Alt(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<Tuple<int, int>> preVisitedEdges = new List<Tuple<int, int>>();
            List<Tuple<int, int>> candidateTuples = new List<Tuple<int, int>>();

            List<int> westNodesInner = new List<int>(nodesRectangleWest);
            westNodesInner.RemoveAt(0);
            westNodesInner.RemoveAt(westNodesInner.Count - 1);

            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, nodesRectangleWest));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, nodesRectangleNorth));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, nodesRectangleSouth));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, shortestPathListLU_DR));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, shortestPathListLD_UR));
            //preVisitedEdges.AddRange(det7Line);
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, horMidline));
            preVisitedEdges.AddRange(det18Rect);
            preVisitedEdges.AddRange(det8Parallels);
            preVisitedEdges.AddRange(det1Cross);

            foreach (int currentNode in westNodesInner)
            {
                int nodeCheck = Crawler.CheckOnce(g_nodes, currentNode, Direction.Right, 0.2);
                if (nodeCheck != -1)
                {
                    candidateTuples.AddRange(Crawler.DFSBounded(g_nodes, main_graph, preVisitedEdges, currentNode));
                }
            }

            List<Tuple<int, int>> noDupesCandidates = candidateTuples.Distinct().ToList();
            det6RectAlt.AddRange(noDupesCandidates);

            //To connect the east edge 
            List<int> nodesToConnect = new List<int>();
            foreach(Tuple<int, int> tup in noDupesCandidates)
            {
                if(nodesRectangleWest.Contains(tup.Item1))
                    nodesToConnect.Add(tup.Item1);
                else if (nodesRectangleWest.Contains(tup.Item2))
                    nodesToConnect.Add(tup.Item2);
            }
            nodesToConnect = nodesToConnect.Distinct().ToList();
            List<Tuple<int, int>> nodeEdges = new List<Tuple<int, int>>();

            if (nodesToConnect.Count > 1)
            {
                nodeEdges = new List<Tuple<int, int>>(Crawler.TupleListConstructor(main_graph, Crawler.DijkstraBounded(g_nodes, main_graph, nodesToConnect[0], nodesToConnect[1])));
            }
            
            if (nodesToConnect.Count > 1)
                det6RectAlt.AddRange(nodeEdges);

            foreach (Tuple<int, int> det7Edge in det7Line)
            {
                if (det6RectAlt.Contains(det7Edge))
                    det6RectAlt.Remove(det7Edge);
            }


            return det6RectAlt;
        }

        public static List<Tuple<int, int>> Detail7(Dictionary<int, SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph, List<Tuple<int, int>> exception_tuples)
        {
            //We use a list of "exception tuples" as an easier way to recognize the line.
            //We don't NEED it (to avoid dependencies), but it reduces the chances of recognizing the wrong line as Detail 7

            Tuple<int, int> detail7Nodes;
            List<Tuple<int, int>> return7Nodes = new List<Tuple<int, int>>();

            List<int> nodesWestInner = new List<int>(nodesRectangleWest);
            List<Tuple<int, int>> candidates = new List<Tuple<int, int>>();
            List<Tuple<int, int>> ignoreThese = new List<Tuple<int, int>>(); //Eliminates false positives from other shapes

            bool lineFound = false;

            nodesWestInner.RemoveAt(nodesWestInner.Count - 1);
            nodesWestInner.RemoveAt(0);
            nodesWestInner.Reverse();

            List<int> exemptNodes = new List<int>();

            if (Detail4Found){
                exemptNodes = Crawler.NodeListConstructor(det6Rect);
            }

            foreach (int node in nodesWestInner)
            {
                SketchAdjacencyList skAdj;
                g_nodes.TryGetValue(node, out skAdj);

                foreach (int adjNode in skAdj.adjacent)
                {
                    Tuple<int, int> compTuple = new Tuple<int, int>(node, adjNode);
                    if (Crawler.IsDirection(compTuple, g_nodes, Direction.Right, 0.2))
                    {
                        if ((!exemptNodes.Contains(compTuple.Item1) && !exemptNodes.Contains(compTuple.Item2)) && !lineFound)
                        {
                            candidates.Add(compTuple);
                            lineFound = true;
                        }
                    }
                }
            }

            if (candidates.Count >= 1)
            {
                detail7Nodes = new Tuple<int, int>(candidates[0].Item1, candidates[0].Item2);
                det7Line.Add(detail7Nodes);
                return7Nodes.Add(detail7Nodes);

                if (Detail4Found && Crawler.TupleListConstructor(main_graph, horMidline).Contains(detail7Nodes))
                {
                    det7Line.Remove(detail7Nodes);
                    return7Nodes.Remove(detail7Nodes);
                    detail7Nodes = null;
                }
                if(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleNorth).Contains(detail7Nodes))
                {
                    det7Line.Remove(detail7Nodes);
                    return7Nodes.Remove(detail7Nodes);
                    detail7Nodes = null;
                }
                return return7Nodes;
            }
            else
                return det7Line;
            
        }

        public static List<Tuple<int, int>> Detail8(Dictionary<int,SketchAdjacencyList> g_nodes, Dictionary<Tuple<int, int>, SketchSubstroke> g_temp, SketchGraph main_graph)
        {
            List<Tuple<int, int>> parallelLines = new List<Tuple<int, int>>();
            int parallelCount = 0;  //How many parallel lines we found (should be 5)
            bool keepGoing = true;


            if (Detail5Found && Detail4Found)
            {
                List<int> verMidLineInner = new List<int>(verMidline);
                verMidLineInner.RemoveAt(0);

                IEnumerable<int> intersection = verMidLineInner.AsQueryable().Intersect(horMidline);     //this should only return 1 node
                if (intersection.Any())
                {
                    int centerNode = intersection.First();
                    int currentNode;
                    foreach (int node in verMidLineInner)
                    {
                        currentNode = node;
                        if (currentNode == centerNode)
                            break;
                        keepGoing = true;
                        while (keepGoing)
                        {
                            keepGoing = false;
                            if (currentNode == centerNode)
                                break;
                            if (Detail3Found && shortestPathListLU_DR.Contains(currentNode))
                                break;

                            SketchAdjacencyList skAdj;
                            g_nodes.TryGetValue(currentNode, out skAdj);

                            foreach (int adjNode in skAdj.adjacent)
                            {
                                Tuple<int, int> compTuple = new Tuple<int, int>(currentNode, adjNode);
                                if (Crawler.IsDirection(compTuple, g_nodes, Direction.Left, 0.4))
                                {
                                    parallelLines.Add(Crawler.SingleTupleConstructor(main_graph, compTuple.Item1, compTuple.Item2));
                                    parallelCount += 1;
                                    currentNode = adjNode;
                                    keepGoing = true;
                                }
                            }
                        }
                    }
                }
            }

            keepGoing = true;
            if (Detail3Found && Detail4Found)
            {
                List<int> diagPathInner = new List<int>(shortestPathListLU_DR);
                diagPathInner.RemoveAt(0);
                IEnumerable<int> intersection = horMidline.AsQueryable().Intersect(diagPathInner);     //this should only return 1 node
                if (intersection.Any())
                {
                    int centerNode = intersection.First();
                    int currentNode;
                    foreach (int node in diagPathInner)
                    {
                        currentNode = node;
                        if (currentNode == centerNode)
                            break;
                        keepGoing = true;
                        while (keepGoing)
                        {
                            keepGoing = false;
                            if (currentNode == centerNode)
                                break;
                            if (Detail5Found && verMidline.Contains(currentNode))
                                break;

                            SketchAdjacencyList skAdj;
                            g_nodes.TryGetValue(currentNode, out skAdj);

                            foreach (int adjNode in skAdj.adjacent)
                            {
                                Tuple<int, int> compTuple = new Tuple<int, int>(currentNode, adjNode);
                                if (Crawler.IsDirection(compTuple, g_nodes, Direction.Right, 0.4))
                                {
                                    parallelLines.Add(Crawler.SingleTupleConstructor(main_graph, compTuple.Item1, compTuple.Item2));
                                    parallelCount += 1;
                                    currentNode = adjNode;
                                    keepGoing = true;
                                }
                            }
                        }
                    }
                }
            }
            keepGoing = true;
            if (Detail3Found && Detail5Found)
            {
                List<int> verMidLineInner = new List<int>(verMidline);
                verMidLineInner.RemoveAt(0);

                IEnumerable<int> intersection = verMidLineInner.AsQueryable().Intersect(shortestPathListLU_DR);     //this should only return 1 node
                int centerNode = -1;
                if (intersection.Any())
                {
                    centerNode = intersection.First();
                    int currentNode;
                    foreach (int node in verMidLineInner)
                    {
                        currentNode = node;
                        if (currentNode == centerNode || centerNode == -1)
                            break;
                        keepGoing = true;
                        while (keepGoing)
                        {
                            keepGoing = false;
                            if (currentNode == centerNode || centerNode == -1)
                                break;
                            if (Detail3Found && shortestPathListLU_DR.Contains(currentNode))
                                break;

                            SketchAdjacencyList skAdj;
                            g_nodes.TryGetValue(currentNode, out skAdj);

                            foreach (int adjNode in skAdj.adjacent)
                            {
                                Tuple<int, int> compTuple = new Tuple<int, int>(currentNode, adjNode);
                                if (Crawler.IsDirection(compTuple, g_nodes, Direction.Left, 0.4))
                                {
                                    parallelLines.Add(Crawler.SingleTupleConstructor(main_graph, compTuple.Item1, compTuple.Item2));
                                    parallelCount += 1;
                                    currentNode = adjNode;
                                    keepGoing = true;
                                }
                            }
                        }
                    }
                }
            }

            parallelLines = parallelLines.Distinct().ToList();

            foreach (Tuple<int, int> tup in Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleNorth))
            {
                if (parallelLines.Contains(tup))
                    parallelLines.Remove(tup);
            }

            //if(parallelCount == 4)
            if (true)
            {
                Detail8Found = true;
                det8Parallels.AddRange(parallelLines);
            }

            return det8Parallels;
        }

        public static List<Tuple<int, int>> Detail9(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<int> bottomNodes = new List<int>();
            List<List<int>> leftNodes = new List<List<int>>();
            List<int> leftEdge = new List<int>();
            int upperCorner = -1;
            //List<Tupleint> diagonalNodes = new List<int>();

            bool edgeUpFound = false;
            bool edgeRightFound = false;

            int edgeUp = -1;
            int edgeRight = -1;
            int originNode = -1;

            List<int> nodesNorthInner = new List<int>(nodesRectangleNorth);
            nodesNorthInner.RemoveAt(nodesNorthInner.Count - 1);
            nodesNorthInner.RemoveAt(0);

            if (Detail2Found)
            {
                foreach(int node in nodesNorthInner)
                {
                    if (!edgeUpFound) {
                        SketchAdjacencyList skAdj;
                        g_nodes.TryGetValue(node, out skAdj);

                        foreach (int adjNode in skAdj.adjacent)
                        {
                            Tuple<int, int> compTuple = new Tuple<int, int>(node, adjNode);
                            if (Crawler.IsDirection(compTuple, g_nodes, Direction.Up, 2))
                            {
                                List<int> prevNodes = new List<int>();
                                prevNodes.Add(node);
                                leftNodes = Crawler.DFSOneDirection(g_nodes, main_graph, node, Direction.Up, 2);
                                upperCorner = Crawler.CheckNPop(leftNodes, g_nodes, Direction.SlantSE, 0.25, prevNodes, out leftEdge);
                                if(upperCorner != -1)
                                {
                                    foreach(List<int> stroke in leftNodes)
                                    {
                                        det9Triangle.AddRange(Crawler.TupleListConstructor(main_graph, stroke));
                                    }
                                }
                                edgeUp = adjNode;
                                originNode = node;
                                edgeUpFound = true;
                            }
                        }
                    }
                }

                if (edgeUpFound)
                {
                    bottomNodes = Crawler.StraightCrawl(g_nodes, main_graph, originNode, Direction.Right, det9Triangle);
                    edgeRight = bottomNodes[bottomNodes.Count - 1];
                    edgeRightFound = true;
                }
            }

            if(edgeUpFound && edgeRightFound && upperCorner != -1 && edgeRight != -1)
            {
                det9Triangle.AddRange(Crawler.TupleListConstructor(main_graph, Crawler.DijkstraBounded(g_nodes, main_graph, upperCorner, edgeRight)));

            }

            return det9Triangle;
        }

        public static List<Tuple<int, int>> Detail10(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<int> northNodesInner = new List<int>(nodesRectangleNorth);

            northNodesInner.RemoveAt(northNodesInner.Count - 1);
            northNodesInner.RemoveAt(0);
            
            List<int> nodesInnerModified = new List<int>(northNodesInner);

            if (Detail5Found)
            {
                IEnumerable<int> intersection = nodesRectangleNorth.AsQueryable().Intersect(verMidline);
                foreach (int node in northNodesInner)
                {
                    if (node == intersection.First())
                    {
                        nodesInnerModified.RemoveAt(0);
                        break;
                    }
                    else
                        nodesInnerModified.RemoveAt(0);
                }
            }

            foreach (int node in nodesInnerModified)
            {
                SketchAdjacencyList skAdj;
                g_nodes.TryGetValue(node, out skAdj);

                foreach (int adjNode in skAdj.adjacent)
                {
                    Tuple<int, int> compTuple = new Tuple<int, int>(node, adjNode);
                    if (Crawler.IsDirection(compTuple, g_nodes, Direction.Down, 2) && Crawler.CalculateDistanceNodes(g_nodes, compTuple.Item1, compTuple.Item2)>30)
                    {
                        //double dis = Crawler.CalculateDistanceNodes(g_nodes, compTuple.Item1, compTuple.Item2);
                        List<int> shortList = new List<int>();
                        shortList.Add(node);
                        shortList.Add(adjNode);
                        det10DownLine.AddRange(Crawler.TupleListConstructor(main_graph, shortList));
                        break;
                    }
                }
            }

            return det10DownLine;
        }

        public static List<Tuple<int, int>> Detail11(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<Tuple<int, int>> candidateTuples = new List<Tuple<int, int>>();
            List<int> candidateNodes = new List<int>();
            List<int> areaBoundary = new List<int>();
            //GraphPoint[] testPoly = CreatePolygon(g_nodes, det2Box.allNodes);
            //IsInside(testPoly, testPoly.Length, new GraphPoint(g_nodes[30].node.avgx, g_nodes[30].node.avgy));

            if(Detail4Found && Detail3Found)
            {
                List<int> intersectNodesBack = new List<int>(horMidline.Intersect(shortestPathListLU_DR));
                List<int> intersectNodesForward = new List<int>(horMidline.Intersect(shortestPathListLD_UR));
                List<int> lowerCorner = new List<int>(horMidline.Intersect(det2Box.nodesRectangleEast));
                List<int> horMidlineRev = new List<int>(horMidline);
                horMidlineRev.Reverse();

                if (intersectNodesBack.Count == 0 || intersectNodesForward.Count == 0)
                    return candidateTuples;

                if(g_nodes[intersectNodesBack[0]].node.avgx > g_nodes[intersectNodesForward[0]].node.avgx)
                {
                    List<int> crossIntersect = new List<int>(shortestPathListLD_UR.Intersect(shortestPathListLU_DR));
                    areaBoundary.Add(intersectNodesBack[0]);
                    areaBoundary.Add(crossIntersect[0]);
                    foreach(int node in shortestPathListLD_UR)
                    {
                        if (g_nodes[node].node.avgx >= g_nodes[crossIntersect[0]].node.avgx)
                            areaBoundary.Add(node);
                    }

                    if (lowerCorner.Count > 0)
                        foreach (int node in det2Box.nodesRectangleEast)
                            if (g_nodes[node].node.avgy <= g_nodes[lowerCorner[0]].node.avgy)
                                areaBoundary.Add(node);

                    foreach (int node in horMidlineRev)
                        if(g_nodes[node].node.avgx >= g_nodes[intersectNodesBack[0]].node.avgx)
                            areaBoundary.Add(node);
                }

                if (g_nodes[intersectNodesBack[0]].node.avgx < g_nodes[intersectNodesForward[0]].node.avgx)
                {
                    areaBoundary.Add(intersectNodesForward[0]);
                    foreach (int node in shortestPathListLD_UR)
                    {
                        if (g_nodes[node].node.avgx >= g_nodes[intersectNodesForward[0]].node.avgx)
                            areaBoundary.Add(node);
                    }

                    if (lowerCorner.Count > 0)
                        foreach (int node in det2Box.nodesRectangleEast)
                            if (g_nodes[node].node.avgy <= g_nodes[lowerCorner[0]].node.avgy)
                                areaBoundary.Add(node);

                    foreach (int node in horMidlineRev)
                        if (g_nodes[node].node.avgx >= g_nodes[intersectNodesForward[0]].node.avgx)
                            areaBoundary.Add(node);
                }

                if(g_nodes[intersectNodesBack[0]].node.avgx == g_nodes[intersectNodesForward[0]].node.avgx)
                {
                    foreach (int node in shortestPathListLD_UR)
                    {
                        if (g_nodes[node].node.avgx >= g_nodes[intersectNodesBack[0]].node.avgx)
                            areaBoundary.Add(node);
                    }

                    if (lowerCorner.Count > 0)
                        foreach (int node in det2Box.nodesRectangleEast)
                            if (g_nodes[node].node.avgy <= g_nodes[lowerCorner[0]].node.avgy)
                                areaBoundary.Add(node);


                    foreach (int node in horMidlineRev)
                        if (g_nodes[node].node.avgx >= g_nodes[intersectNodesBack[0]].node.avgx)
                            areaBoundary.Add(node);
                }

                
                
            }

            areaBoundary = areaBoundary.Distinct().ToList();
            GraphPoint[] testPoly = CreatePolygon(g_nodes, areaBoundary);

            foreach (int currentNode in g_nodes.Keys)
            {
                if(currentNode == 48)
                {
                    Debug.WriteLine("STOP");
                }
                if (testPoly == null)
                    return candidateTuples;
                if(IsInside(testPoly, testPoly.Length, new GraphPoint(g_nodes[currentNode].node.avgx, g_nodes[currentNode].node.avgy)) && g_nodes[currentNode].adjacent.Count>0)
                    candidateNodes.Add(currentNode);
            }

            foreach(int node in areaBoundary)
            {
                if (candidateNodes.Contains(node))
                    candidateNodes.Remove(node);
            }

            candidateTuples = Crawler.TupleListConstructorInclusive(main_graph, candidateNodes);

            return candidateTuples;
        }

        public static List<Tuple<int, int>> Detail12(Dictionary<int, SketchAdjacencyList> g_nodes)
        {
            // Reverse the order of the nodes so that when we iterate, we come from the bottom-right corner
            List<int> LU_DRreversed = new List<int>();
            foreach (int node in shortestPathListLU_DR)
                LU_DRreversed.Insert(0, node);

            LU_DRreversed.RemoveAt(0); //exclude corner

            int nodeLimit = -1;     //this will change to the intersection between LU_DR and LD_UR (Detail 3 intersection) if it exists
            if (Detail3Found)
            {
                IEnumerable<int> intersection = shortestPathListLD_UR.AsQueryable().Intersect(shortestPathListLU_DR);
                if(intersection.Count() == 1)
                    nodeLimit = intersection.First();
            }

            foreach (int currentNode in LU_DRreversed) {
                if (!(nodeLimit == -1) && (currentNode == nodeLimit))
                    break;

                SketchAdjacencyList skAdj;
                g_nodes.TryGetValue(currentNode, out skAdj);

                foreach (int adjNode in skAdj.adjacent)
                {
                    Tuple<int, int> compTuple = new Tuple<int, int>(currentNode, adjNode);
                    if (Crawler.IsDirection(compTuple, g_nodes, Direction.SlantNE, 0.8))
                        det12Parallels.Add(compTuple);
                    if (Crawler.IsDirection(compTuple, g_nodes, Direction.SlantSW, 0.8))
                        det12Parallels.Add(compTuple);
                }
            }

            return det12Parallels;
        }

        public static List<Tuple<int, int>> Detail12Alt(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            //Our bounds will be: Start - Intersection between lines LU_DR of Detail 3, and Detail 5
            //End - The last element in Detail 3

            List<Tuple<int, int>> preVisitedEdges= new List<Tuple<int, int>>();
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, horMidline));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, verMidline));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleEast));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleNorth));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleSouth));
            preVisitedEdges.AddRange(Crawler.TupleListConstructor(main_graph, det2Box.nodesRectangleWest));

            List<int> intersectNodes = new List<int>(verMidline.Intersect(shortestPathListLU_DR));
            List<Tuple<int, int>> candidateTuples = new List<Tuple<int, int>>();

            //There should only be one intersection point
            if (!(intersectNodes.Count > 1))
            {
                int startNode = intersectNodes[0];
                int endNode = shortestPathListLU_DR[shortestPathListLU_DR.Count - 1];
                candidateTuples.AddRange(Crawler.DFSDirectionBounded(g_nodes, main_graph, preVisitedEdges, startNode, endNode, Direction.SlantSE, 0.3));
            }

            det12Parallels.AddRange(candidateTuples);
            return det12Parallels;
        }

        public static List<Tuple<int, int>> Detail12Alt_2(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            //Second Alternate version of Detail 12
            //Region-based checker, returns all nodes within the lower-right region and makes a Tuple list out of it

            //Define the "inner" box. The max and min X and Y values that define our "region"
            //Due to imperfections of handrawn sketches, we will define the inner region by the innermost nodes on each side
            List<int> innerNodes = new List<int>();

            List<int> intersectNodes = new List<int>(verMidline.Intersect(horMidline));
            List<int> regionNorth = new List<int>();
            if (verMidline.Count == 0 || horMidline.Count == 0 || intersectNodes.Count == 0)
                return det12Parallels;
            for(int i =0; i<horMidline.Count; i++)
                if(horMidline[i] == intersectNodes[0])
                {
                    regionNorth.AddRange(horMidline.GetRange(i, horMidline.Count - i));
                    break;
                }

            List<int> regionWest = new List<int>();
            for(int i = 0; i<verMidline.Count; i++)
                if (verMidline[i] == intersectNodes[0])
                {
                    regionWest.AddRange(verMidline.GetRange(i, verMidline.Count - i));
                    break;
                }

            intersectNodes = new List<int>(horMidline.Intersect(det2Box.nodesRectangleEast));
            List<int> regionEast =new List<int>();
            for(int i=0; i<det2Box.nodesRectangleEast.Count; i++)
            {
                if(det2Box.nodesRectangleEast[i] == intersectNodes[0])
                {
                    regionEast.AddRange(det2Box.nodesRectangleEast.GetRange(i, det2Box.nodesRectangleEast.Count - i));
                    break;
                }
            }

            intersectNodes = new List<int>(verMidline.Intersect(det2Box.nodesRectangleSouth));
            List<int> regionSouth = new List<int>();
            for (int i = 0; i < det2Box.nodesRectangleSouth.Count; i++)
                if (det2Box.nodesRectangleSouth[i] == intersectNodes[0])
                {
                    regionSouth.AddRange(det2Box.nodesRectangleSouth.GetRange(0, i+1));
                    break;
                }

            double minX = 0; double minY = 0;
            double maxX = double.MaxValue; double maxY = double.MaxValue;

            foreach (int node in regionNorth)
                if (g_nodes[node].node.avgy > minY)
                    minY = g_nodes[node].node.avgy;

            foreach (int node in regionSouth)
                if (g_nodes[node].node.avgy < maxY)
                    maxY = g_nodes[node].node.avgy;

            foreach (int node in regionWest)
                if (g_nodes[node].node.avgx > minX)
                    minX = g_nodes[node].node.avgx;

            foreach (int node in regionEast)
                if (g_nodes[node].node.avgx < maxX)
                    maxX = g_nodes[node].node.avgx;

            foreach(int node in g_nodes.Keys)
            {
                if (g_nodes[node].node.avgx > minX
                    && g_nodes[node].node.avgx < maxX
                    && g_nodes[node].node.avgy > minY
                    && g_nodes[node].node.avgy < maxY)
                    innerNodes.Add(node);
            }

            det12Parallels.AddRange(Crawler.TupleListConstructorInclusive(main_graph, innerNodes));

            return det12Parallels;
        }

        public static List<Tuple<int, int>> Detail13(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            int currentNode = nodesRectangleEast[nodesRectangleEast.Count - 1];
            List<int> allNodes = new List<int>();
            List<int> upperTriangleSide = new List<int>();
            List<List<int>> inputCandidates = Crawler.DFSOneDirection(g_nodes, main_graph, currentNode, Direction.SlantNE, 0.4);
            int triangleTip = Crawler.CheckNPop(inputCandidates, g_nodes, Direction.SlantNW, 0.4, allNodes, out det13LowerTriangle);
            if (triangleTip != -1)
            {
                det13Triangle.AddRange(Crawler.TupleListConstructor(main_graph, Crawler.DijkstraBounded(g_nodes, main_graph, det2Box.cornerNE, triangleTip)));
                det13Triangle.AddRange(Crawler.TupleListConstructor(main_graph, det13LowerTriangle));
            }

            Detail13Found = true;
            return det13Triangle;
        }

        public static List<Tuple<int, int>> Detail14(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<Tuple<int, int>> candidateTuples = new List<Tuple<int, int>>();
            List<int> det13LowerHalf = new List<int>(det13LowerTriangle);

            //if (det13LowerHalf.Count <= 2)
                foreach (int node in det2Box.allNodes)
                    if (det13LowerHalf.Contains(node))
                        det13LowerHalf.Remove(node);

            List<Tuple<int, int>> boundEdges = new List<Tuple<int, int>>();
            boundEdges.AddRange(det13Triangle);
            boundEdges.AddRange(det15Line);
            boundEdges.AddRange(det16Line);
            int halfCalc= (det13LowerHalf.Count - 1) / 2;
            int countCalc = (det13LowerHalf.Count) - halfCalc;
            det13LowerHalf = det13LowerHalf.GetRange(halfCalc, countCalc);
            foreach(int currentNode in det13LowerHalf)
            {
                candidateTuples.AddRange(Crawler.DFSBounded(g_nodes, main_graph, boundEdges, currentNode));
            }

            return candidateTuples;
        }

        public static List<Tuple<int, int>> Detail15(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            bool continueDirection = true;
            bool lineFound = false;
            int lineCount = 0; //number of lines. Should be 1, but we want to keep track of more to label as distortion
            List<Tuple<int, int>> det15Candidates = new List<Tuple<int, int>>(); 

            if (Detail13Found)
            {
                List<int> det13Nodes = new List<int>(Crawler.NodeListConstructor(det13Triangle));
                foreach (int currentNode in det13Nodes)
                {
                    lineFound = false;
                    continueDirection = true;
                    int crawlNode = currentNode;
                    while (continueDirection)
                    {
                        continueDirection = false;
                        SketchAdjacencyList skAdj;
                        g_nodes.TryGetValue(crawlNode, out skAdj);
                        foreach (int adjNode in skAdj.adjacent)
                        {
                            Tuple<int, int> compTuple = new Tuple<int, int>(crawlNode, adjNode);
                            if (Crawler.IsDirection(compTuple, g_nodes, Direction.Down, 5))
                            {
                                if (det13Nodes.Contains(compTuple.Item1) && det13Nodes.Contains(compTuple.Item2))
                                    break;

                                det15Candidates.Add(compTuple);
                                crawlNode = adjNode;
                                continueDirection = true;
                                break;
                            }
                        }
                    }

                    List<int> candidateNodes = new List<int>(Crawler.NodeListConstructor(det15Candidates));
                    if (det15Candidates.Count > 0)
                    {
                        //if (!(det13Nodes.Contains(candidateNodes[0]) && det13Nodes.Contains(candidateNodes[candidateNodes.Count - 1])))
                            //det15Candidates.Clear();
                        int overlapCount = 0;
                        foreach(int chckNode in det2Box.nodesRectangleEast)
                        {
                            if (candidateNodes.Contains(chckNode))
                                overlapCount++;
                        }
                        if (overlapCount > 2)
                            det15Candidates.Clear();
                    }

                    if (!lineFound)
                    {
                        det15Line.AddRange(Crawler.TupleListConstructor(main_graph, Crawler.NodeListConstructor(det15Candidates)));
                        if(det15Candidates.Count>0)
                            lineFound = true;
                    }
                    if (lineFound)
                        lineCount++;
                    
                    det15Candidates.Clear();
                }
            }
            Detail15Found = true;
            return det15Line;
        }

        public static List<Tuple<int, int>> Detail16(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<Tuple<int, int>> boundPath = new List<Tuple<int, int>>();
            List<int> midlineNodes = new List<int>();
            double minSlope = double.MaxValue;

            List<int> nodesEastInner = new List<int>(nodesRectangleEast);
            nodesEastInner.RemoveAt(nodesEastInner.Count - 1);
            nodesEastInner.RemoveAt(0);

            foreach (int node in nodesEastInner)
            {
                foreach (int node2 in Crawler.NodeListConstructor(det13Triangle))
                {
                    if (SlopeNodes(g_nodes, node, node2) < minSlope)
                    {
                        Tuple<int, int> compTuple = new Tuple<int, int>(node, node2);
                        if (Crawler.IsDirection(compTuple, g_nodes, Direction.Right, 0.2))
                            boundPath.Add(new Tuple<int, int>(node, node2));
                        minSlope = SlopeNodes(g_nodes, node, node2);
                    }
                }
            }

            //List<int> crossHorPathList = new List<int>(); 
            foreach (Tuple<int, int> candidate in boundPath)
            {
                midlineNodes = new List<int>(Crawler.DijkstraBounded(g_nodes, main_graph, candidate.Item1, candidate.Item2));
            }
            det16Line = Crawler.TupleListConstructor(main_graph, midlineNodes);
            Detail16Found = true;    //Always returns true for now, but we will need to verify eventually

            return det16Line;
        }

        public static List<Tuple<int, int>> Detail17(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<Tuple<int, int>> candidateTuples = new List<Tuple<int, int>>();
            List<Tuple<int, int>> boundEdges = new List<Tuple<int, int>>();

            if(Detail18Found && Detail2Found)
            {
                if (Detail5Found)
                    boundEdges.AddRange(Crawler.TupleListConstructor(main_graph, verMidline));
                boundEdges.AddRange(det18Rect);
                boundEdges.AddRange(Crawler.TupleListConstructor(main_graph, nodesRectangleSouth));
                foreach(int currentNode in subDet18East)
                {
                    candidateTuples.AddRange(Crawler.DFSBounded(g_nodes, main_graph, boundEdges, currentNode));
                }
            }

            det17Cross.AddRange(candidateTuples);
            Detail17Found = true;
            return det17Cross;
        }

        public static List<Tuple<int, int>> Detail18(Dictionary<int, SketchAdjacencyList> g_nodes, SketchGraph main_graph)
        {
            List<Tuple<int, int>> candidateTuples = new List<Tuple<int, int>>();
            List<List<int>> candidateLines = new List<List<int>>();
            List<int> det18West = new List<int>();
            List<int> det18North = new List<int>();
            List<int> det18East = new List<int>();
            List<int> det18South = new List<int>();
            List<int> det18Diagonal = new List<int>();

            int det18CornerNW = -1;
            int det18CornerNE = -1;
            int det18CornerSE = -1;
            int det18CornerSW = -1;

            List<int> reversedLine = new List<int>(det2Box.nodesRectangleSouth);
            reversedLine.Reverse();

            foreach(int currentNode in reversedLine)
            {
                List<int> allNodes = new List<int>();

                candidateLines = Crawler.DFSOneDirection(g_nodes, main_graph, currentNode, Direction.Down, 2);
                det18CornerSW = Crawler.CheckNPop(candidateLines, g_nodes, Direction.Right, 0.2, allNodes, out det18West);
                allNodes.AddRange(det18West);

                if(det18CornerSW != -1)
                {
                    candidateLines.Clear();
                    candidateLines = Crawler.DFSOneDirection(g_nodes, main_graph, det18CornerSW, Direction.Right, 0.2);
                    det18CornerSE = Crawler.CheckNPop(candidateLines, g_nodes, Direction.Up, 2, allNodes, out det18South);
                    if (det2Box.nodesRectangleEast.Contains(det18CornerSE))
                        det18CornerSE = -1;
                    allNodes.AddRange(det18South);
                }

                if(det18CornerSE != -1)
                {
                    candidateLines.Clear();
                    foreach (int nd in det18West)
                        allNodes.Remove(nd);
                    candidateLines = Crawler.DFSOneDirection(g_nodes, main_graph, det18CornerSE, Direction.Up, 2);
                    det18CornerNE = Crawler.CheckNPop(candidateLines, g_nodes, Direction.Left, 0.2, allNodes, out det18East);
                    allNodes.AddRange(det18East);
                }

                bool fastCornerFound = false;
                candidateLines.Clear();

                if (det18CornerNE != -1)
                {
                    candidateLines = Crawler.DFSOneDirection(g_nodes, main_graph, det18CornerNE, Direction.Left, 0.2);
                }

                foreach (List<int> candidate in candidateLines)
                {
                    if (candidate[candidate.Count - 1] == reversedLine[0])
                    {
                        det18North = candidate;
                        det18CornerNW = candidate[candidate.Count - 1];
                        fastCornerFound = true;
                    }
                }
                if (!fastCornerFound)
                {
                    allNodes.Remove(det18CornerSW);
                    det18CornerNW = Crawler.CheckNPop(candidateLines, g_nodes, Direction.Down, 2, allNodes, out det18North);
                    if (det18CornerSW != -1)
                        fastCornerFound = true;
                }
                allNodes.AddRange(det18North);
                if (fastCornerFound)
                    break;
            }

            if (det18North.Count > 0)
            {
                if (det18North[det18North.Count - 1] == det18CornerNW)
                {
                    candidateTuples.AddRange(Crawler.TupleListConstructor(main_graph, det18West));
                    candidateTuples.AddRange(Crawler.TupleListConstructor(main_graph, det18South));
                    candidateTuples.AddRange(Crawler.TupleListConstructor(main_graph, det18East));
                    candidateTuples.AddRange(Crawler.TupleListConstructor(main_graph, det18North));
                    det18Rect.AddRange(candidateTuples);
                    det18Diagonal = Crawler.DijkstraBounded(g_nodes, main_graph, det18CornerNW, det18CornerSE);
                    det18Rect.AddRange(Crawler.TupleListConstructor(main_graph, det18Diagonal));
                    det18Rect = det18Rect.Distinct().ToList();
                    subDet18East = det18East;

                    Detail18Found = true;
                    return det18Rect;
                }
            }
            return det18Rect;
        }

        public class Detail2Box
        {
            public List<int> rawNodes;
            public List<int> allNodes;
            public List<int> nodesRectangleNorth;
            public List<int> nodesRectangleSouth;
            public List<int> nodesRectangleEast;
            public List<int> nodesRectangleWest;
            Dictionary<int, SketchAdjacencyList> g_nodes = new Dictionary<int, SketchAdjacencyList>();

            public double area { get; set; }
            public int cornerNW { get; set; }
            public int cornerNE { get; set; }
            public int cornerSW { get; set; }
            public int cornerSE { get; set; }

            public Detail2Box()
            {

            }

            public Detail2Box(List<int> north, List<int> south, List<int> east, List<int> west, Dictionary<int, SketchAdjacencyList> graph_nodes)
            {
                rawNodes = new List<int>();

                nodesRectangleNorth = north;
                nodesRectangleSouth = south;
                nodesRectangleEast = east;
                nodesRectangleWest = west;
                g_nodes = graph_nodes;

                rawNodes.AddRange(nodesRectangleNorth);
                rawNodes.AddRange(nodesRectangleEast);
                rawNodes.AddRange(nodesRectangleSouth);
                rawNodes.AddRange(nodesRectangleWest);

                allNodes = rawNodes.Distinct().ToList();
                area = Crawler.CalculateArea(g_nodes, allNodes);
            }

            public void RecalculateArea()
            {
                area = Crawler.CalculateArea(g_nodes, allNodes);
            }
        }
    }
}
