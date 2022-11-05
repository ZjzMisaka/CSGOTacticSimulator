using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class RankedTeam
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int? change { get; set; }

    }
}
