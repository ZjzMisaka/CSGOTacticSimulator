using CSGOTacticSimulator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSGOTacticSimulator.Helper
{
    static public class PathfindingHelper
    {
        static public MapNode GetNearestNode(Point startMapPoint, int startLayer, Map mapFrame)
        {
            double minimumDistance = -1;
            MapNode startNode = null;
            foreach (MapNode mapNode in mapFrame.mapNodes)
            {
                if (startLayer != mapNode.layerNumber)
                {
                    continue;
                }
                double currentDistance = VectorHelper.GetDistance(startMapPoint, mapNode.nodePoint);
                if (minimumDistance == -1 || currentDistance < minimumDistance)
                {
                    minimumDistance = currentDistance;
                    startNode = mapNode;
                }
            }
            return startNode;
        }
        static public List<MapNode> GetMapPathNodes(MapNode startNode, MapNode endNode, Map mapFrame, VolumeLimit volumeLimit)
        {
            List<MapNode> resultMapPathNodes = new List<MapNode>();

            List<MapNode> mapNodes = mapFrame.mapNodes;

            List<int> openList = new List<int>();
            List<int> closeList = new List<int>();
            openList.Add(startNode.index);

            while (openList.Count != 0)
            {
                double minimumF = -1;
                double minimumH = -1;
                MapNode minFNode = null;
                foreach (int i in openList)
                {
                    if ((minimumF == -1 || mapNodes[i].F < minimumF) || (mapNodes[i].F == minimumF && mapNodes[i].H < minimumH))
                    {
                        if (mapNodes[i].G == -1)
                        {
                            mapNodes[i].G = 0;
                        }
                        if (mapNodes[i].H == -1)
                        {
                            mapNodes[i].H = VectorHelper.GetDistance(mapNodes[i].nodePoint, endNode.nodePoint);
                        }

                        minFNode = mapNodes[i];
                        minimumH = mapNodes[i].H;
                        minimumF = mapNodes[i].H + mapNodes[i].G;
                    }
                }
                openList.Remove(minFNode.index);
                closeList.Add(minFNode.index);

                foreach (int key in minFNode.neighbourNodes.Keys)
                {
                    if (volumeLimit == VolumeLimit.Quietly)
                    {
                        ActionLimit actionLimit = minFNode.neighbourNodes[key].actionLimit;
                        if (actionLimit == ActionLimit.RunClimbOrFall || actionLimit == ActionLimit.RunJumpOnly || actionLimit == ActionLimit.RunOnly)
                        {
                            continue;
                        }
                    }

                    if (!closeList.Contains(key))
                    {
                        if (!openList.Contains(key))
                        {
                            openList.Add(key);
                            mapNodes[key].H = VectorHelper.GetDistance(mapNodes[key].nodePoint, endNode.nodePoint);
                            // F暂且以二维平面距离计算, 与高度层数和移速和移动方式无关
                            if (minFNode.nodePoint == mapNodes[key].nodePoint && minFNode.layerNumber != mapNodes[key].layerNumber)
                            {
                                mapNodes[key].G = minFNode.G;
                            }
                            else
                            {
                                mapNodes[key].G = minFNode.G + VectorHelper.GetDistance(mapNodes[key].nodePoint, minFNode.nodePoint); ;
                            }
                            mapNodes[key].parent = minFNode;
                        }
                        if (openList.Contains(key))
                        {
                            // F暂且以二维平面距离计算, 与高度层数和移速和移动方式无关
                            if (minFNode.nodePoint == mapNodes[key].nodePoint && minFNode.layerNumber != mapNodes[key].layerNumber)
                            {
                                //DO NOTHING, mapNodes[key].g == minFNode.g;
                                mapNodes[key].parent = minFNode;
                            }
                            else
                            {
                                double currentG = minFNode.G + VectorHelper.GetDistance(mapNodes[key].nodePoint, minFNode.nodePoint);
                                if (mapNodes[key].G > currentG)
                                {
                                    mapNodes[key].G = currentG;
                                    mapNodes[key].parent = minFNode;
                                }
                            }
                        }
                    }
                }

                if (openList.Contains(endNode.index))
                {
                    MapNode nodeTemp = endNode;
                    while (nodeTemp != startNode)
                    {
                        resultMapPathNodes.Insert(0, nodeTemp);
                        nodeTemp = nodeTemp.parent;
                    }
                    resultMapPathNodes.Insert(0, startNode);
                }
            }

            foreach (MapNode mapNode in mapNodes)
            {
                mapNode.parent = null;
                mapNode.F = mapNode.G = mapNode.H = -1;
            }

            return resultMapPathNodes;
        }
    }
}
