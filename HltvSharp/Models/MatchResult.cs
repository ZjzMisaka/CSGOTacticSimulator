using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class MatchResult
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Event Event { get; set; }
        public Team Team1 { get; set; }
        public Team Team2 { get; set; }
        public Team WinningTeam { get; set; }
        public int Stars { get; set; }
        public string Format { get; set; }
        public string Map { get; set; } = null;
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
    }
}
