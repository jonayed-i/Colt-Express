using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MasterClientDebugScript : MonoBehaviourPunCallbacks {

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        if (PhotonNetwork.IsMasterClient)
            Debug.Log(newPlayer.NickName + " joined the room");
        
    }

    public void triggerPrintBanditConfigurationOnMaster() {
        PhotonView view = PhotonView.Get(this);
        view.RPC("printChosenBandits", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void printChosenBandits() {
        Debug.Log("Bandit configuration update: ");
        if (PhotonNetwork.IsMasterClient) {
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList) {
                Debug.Log(p.NickName + " chose " + p.CustomProperties["Bandit"]);
            }
        }
    }
}
