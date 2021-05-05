using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ChooseBanditUI : MonoBehaviourPunCallbacks {
    [Header("Game Management")]
    public GameObject mStartButton;

    [Header("Character Buttons")]
    public Button Belle;
    public Text BelleOwner;

    public Button Cheyenne;
    public Text CheyenneOwner;

    public Button Django;
    public Text DjangoOwner;

    public Button Doc;
    public Text DocOwner;

    public Button Ghost;
    public Text GhostOwner;

    public Button Tuco;
    public Text TucoOwner;

    private void Start() {
        Belle.onClick.AddListener(() => OnCharacterChosen("Belle"));
        Cheyenne.onClick.AddListener(() => OnCharacterChosen("Cheyenne"));
        Django.onClick.AddListener(() => OnCharacterChosen("Django"));
        Doc.onClick.AddListener(() => OnCharacterChosen("Doc"));
        Ghost.onClick.AddListener(() => OnCharacterChosen("Ghost"));
        Tuco.onClick.AddListener(() => OnCharacterChosen("Tuco"));
    }

    public void OnCharacterChosen(string bandit) {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        properties.Add("Bandit", bandit);
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        Debug.Log(bandit + " is chosen as local player");

        PhotonView view = PhotonView.Get(this);
        view.RPC("UpdateCharacterSelectionInfo", RpcTarget.AllBuffered, bandit, PhotonNetwork.LocalPlayer.NickName);

        if (PhotonNetwork.IsMasterClient) {
            mStartButton.SetActive(true);
            mStartButton.GetComponent<Button>().onClick.AddListener(OnMasterEnterGame);
        }
    }

    public void OnMasterEnterGame() {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen= false;
            PhotonNetwork.LoadLevel("Gameplay");
        }
    }

    [PunRPC]
    public void UpdateCharacterSelectionInfo(string bandit, string playerName) {
        if (bandit == "Belle") {
            Belle.interactable = false;
            BelleOwner.text = playerName;

        } else if (bandit == "Cheyenne") {
            Cheyenne.interactable = false;
            CheyenneOwner.text = playerName;

        } else if (bandit == "Django") {
            Django.interactable = false;
            DjangoOwner.text = playerName;
            
        } else if (bandit == "Doc") {
            Doc.interactable = false;
            DocOwner.text = playerName;

        } else if (bandit == "Ghost") {
            Ghost.interactable = false;
            GhostOwner.text = playerName;

        } else if (bandit == "Tuco") {
            Tuco.interactable = false;
            TucoOwner.text = playerName;

        }
    }
}
