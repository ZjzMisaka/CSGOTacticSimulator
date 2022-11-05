using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class Team
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int Id { get; set; }
        public int WorldRank { get; set; }
        public double AveragePlayerAge { get; set; }
        public double winRateProcentage { get; set; }
        public TimePeriod timePeriod { get; set; }
        public Coach Coach { get; set; }
        public List<Player> Players { get; set; }
        public List<Match> RecentMatches { get; set; }
        public List<Match> UpcomingMatches { get; set; }
    }
}