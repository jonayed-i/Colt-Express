using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class LobbyCreateGame : MonoBehaviour {
    private Button buttonCreate;
    private InputField InputField_Creator;
    private InputField InputField_Players;


    // Start is called before the first frame update
    void Start() {
        // get references
        buttonCreate = GameObject.Find("createGameCanvas/Button_Create").GetComponent<Button>();

        InputField_Creator = GameObject.Find("createGameCanvas/InputField_Creator").GetComponent<InputField>();

        if (InputField_Creator == null) {
            Debug.Log("hi");
        }

        InputField_Players = GameObject.Find("createGameCanvas/InputField_Players").GetComponent<InputField>();
    }
}





