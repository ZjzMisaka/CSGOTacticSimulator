using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class TeamSearchItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string flagUrl { get; set; }
        public string teamLogoDarkUrl { get; set; }
        public string teamLogoLightUrl { get; set; }
        public string webLocation { get; set; }

        public List<TeamSearchPlayerItem> PlayerList { get; set; }
    }
}
