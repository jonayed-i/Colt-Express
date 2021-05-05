// Author: Feiyang Li

using Assets.Scripts.DataHolders;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
// using UnityEngine.UIElements;

public class LoginUIManager : MonoBehaviourPunCallbacks {
    private Button buttonLogin;
    private Button buttonHelp;
    private InputField inputField_Username;
    private InputField inputField_Password;

    

    // Start is called before the first frame update
    void Start() {
        // get references
        buttonLogin = GameObject.Find("LoginCanvas/Button_Login").GetComponent<Button>();
        buttonHelp = GameObject.Find("LoginCanvas/Button_Help").GetComponent<Button>();
        inputField_Username = GameObject.Find("LoginCanvas/InputField_Username").GetComponent<InputField>();
        //inputField_Password = GameObject.Find("LoginCanvas/InputField_Password").GetComponent<InputField>();
    }

    public void login() {
        Debug.Log("Login Button Clicked");
        string username = inputField_Username.text;
       // string password = inputField_Password.text;
        CredentialManager credentialManager = GameObject.Find("CredentialManager").GetComponent<CredentialManager>();
        credentialManager.setPlayerInfo(username);

        PhotonNetwork.ConnectUsingSettings();
        // example for calling authenticate
        /*LobbyService.authenticate(username, password,
            // response here refers to the response from lobby service. It is a RequestHelper object
            (response) => {
                // code in this block is executed whenever a response is received
                Debug.Log(response.Text);

                // parse response
                PlayerInfo info = JsonUtility.FromJson<PlayerInfo>(response.Text);

                // create credential manager
                CredentialManager credentialManager = GameObject.Find("CredentialManager").GetComponent<CredentialManager>();
                credentialManager.setPlayerInfo(info);

                PhotonNetwork.ConnectUsingSettings();
            });*/
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Photon: Connected to Master Server");
        SceneManager.LoadScene("LobbyFindGame");
    }
}
