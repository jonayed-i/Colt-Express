using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataHolders {
    [System.Serializable]
    public class GameParameters {
        public string location;
        public int maxSessionPlayers;
        public int minSessionPlayers;
        public string name;
        public bool webSupport;
    }
}
