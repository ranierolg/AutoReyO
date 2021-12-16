using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPenSketch
{
    class VertexCoalesce
    {

        public enum DistanceType
        {
            COMPLETE,
            AVERAGE,
            SINGLE,
            CENTROID
        }

        private static double completeDistance(SketchNode sn1, SketchNode sn2)
        {
            double maxdistance = Double.MinValue;
            for (int i = 0; i < sn1.points.Count(); i++)
                for (int j = 0; j < sn2.points.Count(); j++)
                {
                    maxdistance = Math.Max(
                        SketchGraph.euclideanDistance(sn1.points[i].x, sn2.points[j].x, sn1.points[i].y, sn2.points[j].y),
                        maxdistance
                    );
                }

            return maxdistance;
        }

        private static double averageDistance(SketchNode sn1, SketchNode sn2)
        {
            double avgdistance = 0.0;
            for (int i = 0; i < sn1.points.Count(); i++)
                for (int j = 0; j < sn2.points.Count(); j++)
                {
                    avgdistance += SketchGraph.euclideanDistance(
                        sn1.points[i].x,
                        sn2.points[i].x,
                        sn1.points[i].y,
                        sn2.points[i].y);
                }
            return avgdistance / (sn1.points.Count() * sn2.points.Count());
        }

        private static double singleDistance(SketchNode sn1, SketchNode sn2)
        {
            double mindistance = Double.MaxValue;
            for (int i = 0; i < sn1.points.Count(); i++)
                for (int j = 0; j < sn2.points.Count(); j++)
                {
                    mindistance = Math.Min(
                         SketchGraph.euclideanDistance(sn1.points[i].x, sn2.points[j].x, sn1.points[i].y, sn2.points[j].y),
                         mindistance
                     );
                }
            return mindistance;
        }

        private static double centroidDistance(SketchNode sn1, SketchNode sn2)
        {
            double cendistance = SketchGraph.euclideanDistance(
                sn1.avgx,
                sn2.avgx,
                sn1.avgy,
                sn2.avgy);
            return cendistance;
        }

        // Compares the farthest points of nodes to one another when joining
        public static SketchGraph clusterCoalesce(SketchGraph graph, double distance, DistanceType distype = DistanceType.COMPLETE)
        {
            // nodes is the array form of nodelist containing only the keys
            KeyValuePair<int, SketchAdjacencyList>[] nodelist = graph.getNodeDictArray();
            List<int> nodes = nodelist.AsEnumerable().Select(
                i => i.Key
            )
            .ToList();

            HashSet<int> bad_indices = new HashSet<int>();
            for (int i = 0; i < nodes.Count; i++)
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    int id1 = nodes[i];
                    int id2 = nodes[j];
                    if (bad_indices.Contains(id1) || bad_indices.Contains(id2)) continue;

                    double joindist = Double.MaxValue;
                    switch (distype) {
                        case DistanceType.COMPLETE:
                            joindist = completeDistance(nodelist[i].Value.node, nodelist[j].Value.node);
                            break;
                        case DistanceType.AVERAGE:
                            joindist = averageDistance(nodelist[i].Value.node, nodelist[j].Value.node);
                            break;
                        case DistanceType.SINGLE:
                            joindist = singleDistance(nodelist[i].Value.node, nodelist[j].Value.node);
                            break;
                        case DistanceType.CENTROID:
                            joindist = centroidDistance(nodelist[i].Value.node, nodelist[j].Value.node);
                            break;
                    }

                    if (joindist <= distance)
                    {
                        // Add to bad indices since this node will not exist anymore after the mergeNodes
                        bad_indices.Add(id2);   
                        graph.mergeNodes(id1, id2);
                    }
                }
        
            return graph;
        }

        public static SketchGraph linkNearPoints(SketchGraph graph, double distance, DistanceType distype = DistanceType.COMPLETE)
        {
            // nodes is the array form of nodelist containing only the keys
            KeyValuePair<int, SketchAdjacencyList>[] nodelist = graph.getNodeDictArray();
            List<int> nodes = nodelist.AsEnumerable().Select(
                i => i.Key
            )
            .ToList();

            for (int i = 0; i < nodes.Count; i++)
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    int id1 = nodes[i];
                    int id2 = nodes[j];
                    if (
                        !graph.getSubstrokes().ContainsKey(Tuple.Create<int, int>(id1, id2)) &&
                        !graph.getSubstrokes().ContainsKey(Tuple.Create<int, int>(id2, id1)))
                    {
                        double joindist = Double.MaxValue;
                        switch (distype)
                        {
                            case DistanceType.COMPLETE:
                                joindist = completeDistance(nodelist[i].Value.node, nodelist[j].Value.node);
                                break;
                            case DistanceType.AVERAGE:
                                joindist = averageDistance(nodelist[i].Value.node, nodelist[j].Value.node);
                                break;
                            case DistanceType.SINGLE:
                                joindist = singleDistance(nodelist[i].Value.node, nodelist[j].Value.node);
                                break;
                            case DistanceType.CENTROID:
                                joindist = centroidDistance(nodelist[i].Value.node, nodelist[j].Value.node);
                                break;
                        }
                        if (joindist <= distance)
                        {
                            graph.getSubstrokes()[Tuple.Create<int, int>(id1, id2)] = new SketchSubstroke();
                            graph.getNodes()[id1].adjacent.Add(id2);
                            graph.getNodes()[id2].adjacent.Add(id1);
                        }
                    }
                }
            return graph;
        }
    }
}
