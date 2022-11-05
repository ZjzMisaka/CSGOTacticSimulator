using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class AllSearchItem
    {
        public List<TeamSearchItem> Teams { get; set; }
        public List<PlayerSearchItem> Players { get; set; }
        public List<EventsSearchItem> Events { get; set; }
    }
}
