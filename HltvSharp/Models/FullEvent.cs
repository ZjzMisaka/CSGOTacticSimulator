using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class FullEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string PrizePool { get; set; }
        public Team[] Teams { get; set; }
        public Country Location { get; set; }
        public Event[] RelatedEvents { get; set; }
        public bool IsOnline { get; set; }

    }
}
