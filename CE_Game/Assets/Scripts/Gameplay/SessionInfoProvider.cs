using UnityEngine;
using Assets.Scripts.DataHolders;

public class SessionInfoProvider : MonoBehaviour {
    [HideInInspector]
    private string gameID;
    private string gameName;
    public string choosenCharacter;
    public SessionInfo info { get; set; }


    public void Start() {
        Object.DontDestroyOnLoad(this);
    }

    public void setGameID(string id) {
        this.gameID = id;
    }

    public void setGameName(string name) {
        this.gameName = name;
    }


    public string getGameID() {
        return this.gameID;
    }

    public string getGameName() {
        return this.gameName;
    }
}
