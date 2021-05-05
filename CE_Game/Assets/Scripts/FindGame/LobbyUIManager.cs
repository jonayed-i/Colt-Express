using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviourPunCallbacks {
    public int PollingInterval;
    public InputField roomcodeinput;
    public Button joinroombutton;
    private CredentialManager credentialManager;

    private void Awake() {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start() {

        Button buttoncreate;
        buttoncreate = GameObject.Find("Create Game Button").GetComponent<Button>();
        buttoncreate.onClick.AddListener(createGame);
        joinroombutton.onClick.AddListener(joinroom);
        // get online player count
        credentialManager = GameObject.Find("CredentialManager").GetComponent<CredentialManager>();

        PhotonNetwork.NickName = credentialManager.getPlayerName();

        //StartCoroutine(pollingManager());
    }

    void joinroom()
    {
        PhotonNetwork.JoinRoom(roomcodeinput.text);
    }
    
    void createGame() {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        RoomOptions options = new RoomOptions();
        options.PlayerTtl = 0;
        options.EmptyRoomTtl = 0;

        PhotonNetwork.CreateRoom(credentialManager.getPlayerName(), options);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        RoomOptions options = new RoomOptions();
        options.PlayerTtl = 0;
        options.EmptyRoomTtl = 0;

        PhotonNetwork.CreateRoom(credentialManager.getPlayerName(), options);
    }

    public override void OnCreatedRoom() {
        PhotonNetwork.LoadLevel("CharacterSelection");
    }
/*
    IEnumerator pollingManager() {
        while (true) {
            StartCoroutine(polling());
            yield return new WaitForSeconds(PollingInterval);
        }
    }

    IEnumerator polling() {
        Debug.Log("Polling");

        int playerCount = 0;
        LobbyService.getPlayerCount((response) => {
            string extractedNumber = Regex.Match(response.Text, @"\d+").Value;
            // Debug.Log(extractedNumber);
            playerCount = int.Parse(extractedNumber);

            GameObject.Find("WelcomMessage").GetComponent<Text>().text =
                "Hello, " + credentialManager.getPlayerName() +
                "! Join " + playerCount + " other players for an exciting adventure!";
        });

        yield return new WaitForSeconds(1);
    }*/

}
