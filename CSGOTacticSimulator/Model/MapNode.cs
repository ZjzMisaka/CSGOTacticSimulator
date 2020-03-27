using CSGOTacticSimulator.Global;
using CSGOTacticSimulator.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSGOTacticSimulator.Model
{
    public enum DirectionMode
    {
        OneWay,
        TwoWay,
        ReversedOneWay
    }
    
    public struct WayInfo
    {
        public WayInfo(ActionLimit actionLimit, double distance)
        {
            this.actionLimit = actionLimit;
            this.distance = distance;
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionLimit actionLimit;
        public double distance;
    }
    public class MapNode
    {
        public MapNode()
        {

        }
        public MapNode(string mapName, Point point, int layerNumber = 0, double distance = 0, ActionLimit moveMode = ActionLimit.AllAllowed, DirectionMode directionMode = DirectionMode.TwoWay, params MapNode[] neighbourMapNodes)
        {
            nodePoint = point;
            neighbourNodes = new Dictionary<int, WayInfo>();
            this.layoutNumber = layerNumber;
            bool isAlreadyInDic = false;
            for (int i = 0; i < GlobalDictionary.mapDic[mapName].mapNodes.Count; ++i)
            {
                MapNode mapNode = GlobalDictionary.mapDic[mapName].mapNodes[i];
                if (mapNode.nodePoint == this.nodePoint && mapNode.layoutNumber == this.layoutNumber)
                {
                    this.index = mapNode.index;
                    this.neighbourNodes = mapNode.neighbourNodes;
                    isAlreadyInDic = true;
                }
            }
            WayInfo wayInfo = new WayInfo();
            if (!(directionMode == DirectionMode.ReversedOneWay))
            {
                foreach (MapNode neighbourNode in neighbourMapNodes)
                {
                    wayInfo = new WayInfo(moveMode, point == neighbourNode.nodePoint ? distance : VectorHelper.GetDistance(point, neighbourNode.nodePoint));
                    if (!neighbourNodes.ContainsKey(GlobalDictionary.mapDic[mapName].mapNodes.IndexOf(neighbourNode)))
                    { 
                        neighbourNodes.Add(GlobalDictionary.mapDic[mapName].mapNodes.IndexOf(neighbourNode), wayInfo); 
                    }
                }
            }
            if (!isAlreadyInDic)
            {
                this.index = GlobalDictionary.mapDic[mapName].mapNodes.Count;
                GlobalDictionary.mapDic[mapName].mapNodes.Add(this);
            }
            else
            {
                GlobalDictionary.mapDic[mapName].mapNodes[index] = this;
            }

            if (directionMode == DirectionMode.TwoWay || directionMode == DirectionMode.ReversedOneWay)
            {
                foreach (MapNode neighbourNode in neighbourMapNodes)
                {
                    for (int i = 0; i < GlobalDictionary.mapDic[mapName].mapNodes.Count; ++i)
                    {
                        MapNode mapNode = GlobalDictionary.mapDic[mapName].mapNodes[i];
                        if (mapNode.nodePoint == neighbourNode.nodePoint && mapNode.layoutNumber == neighbourNode.layoutNumber)
                        {
                            wayInfo = new WayInfo(moveMode, point == neighbourNode.nodePoint ? distance : VectorHelper.GetDistance(point, neighbourNode.nodePoint));
                            if (!GlobalDictionary.mapDic[mapName].mapNodes[i].neighbourNodes.ContainsKey(GlobalDictionary.mapDic[mapName].mapNodes.IndexOf(this)))
                            {
                                GlobalDictionary.mapDic[mapName].mapNodes[i].neighbourNodes.Add(GlobalDictionary.mapDic[mapName].mapNodes.IndexOf(this), wayInfo);
                            }
                        }
                    }
                }
            }
        }

        public void PushNewIntoList(ref List<MapNode> mapNodes)
        {
            for(int i = 0; i < mapNodes.Count; ++i)
            {
                if(this.nodePoint == mapNodes[i].nodePoint && this.layoutNumber == mapNodes[i].layoutNumber)
                {
                    mapNodes[i] = this;
                    return;
                }
            }
            mapNodes.Add(this);
        }

        public int index;

        public Point nodePoint;

        public int layoutNumber;

        public Dictionary<int, WayInfo> neighbourNodes;

        [JsonIgnore]
        public MapNode parent;
        [JsonIgnore]
        public double F { get => f; set => f = value; }
        private double f = -1;
        [JsonIgnore]
        public double G { get => g; 
            set
            {
                g = value;
                f = g + h;
            }
        }
        private double g = -1;
        [JsonIgnore]
        public double H { get => h;
            set
            {
                h = value;
                f = g + h;
            }
        }
        private double h = -1;
    }
}
