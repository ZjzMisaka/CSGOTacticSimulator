using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class EventsSearchItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public string flagUrl { get; set; }
        public string eventLogoUrl { get; set; }
        public string webLocation { get; set; }
        public string physicalLocation { get; set; }
        public string prizePool { get; set; }
        public string eventType { get; set; }

    }
}
