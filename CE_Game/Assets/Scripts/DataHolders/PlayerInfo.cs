using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataHolders {
    [System.Serializable]
    public class PlayerInfo {
        public string access_token;
        public int expires_in;
        public string refresh_token;
        public string scope;
        public string token_type;
    }
}
