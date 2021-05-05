using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CardDriver : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public void Start()
    {
        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList) {
            Debug.Log(p.NickName + " chose " + p.CustomProperties["Bandit"]);
            CardManager c = new CardManager(p.CustomProperties["Bandit"].ToString());
            Debug.Log(c.mDrawingDeck);
        }
        Card s1 = new Card("Move", "pat", 1);
        
        
        Debug.Log(s1);
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
