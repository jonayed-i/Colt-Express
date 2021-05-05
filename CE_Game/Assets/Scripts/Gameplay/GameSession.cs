using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    private string gameID;
    private string gameName;

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
