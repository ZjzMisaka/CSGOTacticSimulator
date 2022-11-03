using System;
using System.Collections.Generic;
using System.Text;

namespace HltvSharp.Models
{
    public class Veto
    {
        public Team Team { get; set; }
        public string Map { get; set; }
        public string Action { get; set; }
    }
}
