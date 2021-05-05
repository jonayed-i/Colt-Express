using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataHolders {
    [System.Serializable]
    public class SessionInfo {
        public string gameID;

        public string creator;

        public class GameParameters {
            public string location;
            public int maxSessionPlayers;
            public int minSessionPlayers;
            public string name;
            public bool webSupport;
        }

        public GameParameters gameparameters;
        public bool launched;
        public string[] players;
        public string[] playerLocations;
        public string savegameid;
    }
}
