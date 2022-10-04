using DemoInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGOTacticSimulator.Model
{
    internal class CurrentInfo
    {
        public CurrentInfo(int tScore, int ctScore, int currentTick, float currentTime, string map, float tickTime, IEnumerable<Player> participants)
        {
            TScore = tScore;
            CtScore = ctScore;
            CurrentTick = currentTick;
            CurrentTime = currentTime;
            Map = map;
            TickTime = tickTime;
            Participants = participants;
        }

        public int TScore { get; set; }
        public int CtScore { get; set; }
        public int CurrentTick { get; set; }
        public float CurrentTime { get; set; }
        public string Map { get; set; }
        public float TickTime { get; set; }
        public IEnumerable<Player> Participants { get; set; }
    }
}
