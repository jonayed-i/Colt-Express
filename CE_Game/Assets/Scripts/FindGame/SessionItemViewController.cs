using Assets.Scripts.DataHolders;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class SessionItemViewController : MonoBehaviourPunCallbacks {
    [HideInInspector]
    public string gameID;
    public string gameName;
    public SessionInfo info { get; set; }

    private Button joinButton;
    private Button deleteButton;
    private CredentialManager credentialManager;

    public void Start() {
        credentialManager = GameObject.Find("CredentialManager").GetComponent<CredentialManager>();

        joinButton = this.transform.Find("JoinButton").GetComponent<Button>();
        joinButton.onClick.AddListener(onJoinGame);

        deleteButton = this.transform.Find("DeleteButton").GetComponent<Button>();
        if (this.info.creator == credentialManager.getPlayerName()) {
            deleteButton.gameObject.SetActive(true);
            deleteButton.onClick.AddListener(onDeleteGame);
        } else {
            deleteButton.gameObject.SetActive(false);
        }
    }

    public void onJoinGame() {
        SessionInfoProvider sessionInfoProvider = GameObject.Find("SessionInfoProvider").GetComponent<SessionInfoProvider>();
        sessionInfoProvider.info = this.info;

        Debug.Log("Adding player to session on Lobby Service");
        LobbyService.putPlayerToSession(credentialManager.getPlayerName(), credentialManager.getAccessToken(), gameID, (response) => {
            Debug.Log("Adding Player to Session: response - " + response.StatusCode);
            Debug.Log(response.ToString());
        });

        
        PhotonNetwork.JoinRoom(info.creator);
        
    }

    public void onDeleteGame() {
        LobbyService.deleteSession(credentialManager.getAccessToken(), this.info.gameID, (response) => {
            Debug.Log("Session deleted");
        });
    }

}
