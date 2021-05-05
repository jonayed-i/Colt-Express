using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataHolders;

public class CredentialManager : MonoBehaviour {
    private PlayerInfo mPlayerInfo;
    private string mPlayerName;

    // Start is called before the first frame update
    void Start() {
        Object.DontDestroyOnLoad(this);
    }

    public void setPlayerInfo(string name) {
        this.mPlayerName = name;

        // get player name
       // LobbyService.getUsernameWithToken(mPlayerInfo.access_token, (response) => {
            
        //});
    }

    public string getAccessToken() {
        return mPlayerInfo.access_token;
    }

    public string getRefreshToken() {
        return mPlayerInfo.refresh_token;
    }

    public int getExpiration() {
        return mPlayerInfo.expires_in;
    }

    public string getPlayerName() {
        return mPlayerName;
    }
}
