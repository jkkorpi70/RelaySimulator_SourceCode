using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaySim
{
    class ComponentInfo
    {
        public string ComponentID { get; set; } // K1, K2 etc.
        public string ComponentType { get; set; } // norm, ondelay, offdelay for relays. g(green), y(yellow) or r(red) for lights
        public int IsOnBoard { get; set; } // 0 = not exists, 1 = is set on boards
        public int X { get; set; } // x position in table
        public int Y { get; set; } // y position in ttable
        public int DelayTime { get; set; } // Time for timed relays. Set 0 for normal relays
        public double RunningTime { get; set; } // For counting down the time. DelayTime holds original set time for reuse.
        public int TimeActivated { get; set; } // 1 active, 0 deactive 
        public int TimeReseted { get; set; } // If 1 relay is reset and can be activated again (default = 1)
        public int CountdownFinished { get; set; } // 1 if finished
        public DateTime StartTime { get; set; } // Time when time relay started

        public ComponentInfo(string ID)
        {
            ComponentID = ID;
            ComponentType = "norm";
            SetDefaults();
        }

        public ComponentInfo(string ID, string Type)
        {
            ComponentID = ID;
            ComponentType = Type;
            SetDefaults();
        }

        private void SetDefaults()
        {
            IsOnBoard = 0;
            X = 0;
            Y = 0;
            TimeReseted = 1;
            TimeActivated = 0;
            RunningTime = 0;
            CountdownFinished = 0;
        }

        ~ComponentInfo() { }
    }
}
