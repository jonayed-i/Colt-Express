using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataHolders {
    [System.Serializable]
    public class SessionResponse {
        public Dictionary<string, SessionInfo> sessions;
    }
}
