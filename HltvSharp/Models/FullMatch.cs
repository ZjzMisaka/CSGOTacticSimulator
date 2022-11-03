using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class FullMatch
    {
        public int Id { get; set; }
        public Team Team1 { get; set; }
        public Team Team2 { get; set; }
        public Team WinningTeam { get; set; }
        public DateTime Date { get; set; }
        public string Format { get; set; }
        public string AdditionalInfo { get; set; }
        public Veto[] Vetos { get; set; }
        public Event Event { get; set; }
        public MapResult[] Maps { get; set; }
        public Demo[] Demos { get; set; }
        public Player[] Team1Players { get; set; }
        public Player[] Team2Players { get; set; }
        public List<MatchStat> Team1PlayerStats { get; set; }
        public List<MatchStat> Team2PlayerStats { get; set; }

    }
}
