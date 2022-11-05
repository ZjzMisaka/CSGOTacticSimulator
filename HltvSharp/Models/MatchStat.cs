using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class MatchStat
    {
        public string PlayerName { get; set; }
        public int PlayerID { get; set; }
        public string KD { get; set; }
        public decimal ADR { get; set; }
        public int plusminus { get; set; }
        public decimal KastProcentage { get; set; }
        public decimal Rating { get; set; }
    }
}
