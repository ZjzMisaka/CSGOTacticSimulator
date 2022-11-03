using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class PlayerSearchItem
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string nickName { get; set; }
        public string flagUrl { get; set; }
        public string webLocation { get; set; }
        public int id { get; set; }
        public string pictureUrl { get; set; }
        public PlayerTeamItem team { get; set; }

    }
}
