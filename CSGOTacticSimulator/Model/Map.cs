using CSGOTacticSimulator.Global;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGOTacticSimulator.Model
{
    public class Map
    {
        [JsonConstructor]
        public Map(string mapName)
        {
            this.mapName = mapName;

            if (!GlobalDictionary.mapDic.ContainsKey(this.mapName))
            {
                GlobalDictionary.mapDic.Add(this.mapName, this);
            }
            else
            {
                GlobalDictionary.mapDic[mapName] = this;
            }
        }
        public Map(string mapName, List<MapNode> mapNodes)
        {
            this.mapName = mapName;
            this.mapNodes = mapNodes;
            if (!GlobalDictionary.mapDic.ContainsKey(this.mapName))
            {
                GlobalDictionary.mapDic.Add(this.mapName, this);
            }
        }

        public string mapName;

        public List<MapNode> mapNodes = new List<MapNode>();
    }
}
