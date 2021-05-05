using Assets.Scripts.DataHolders;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateLobby : MonoBehaviour {
    public GameObject SessionItemPrefab;
    public int PollingInterval;

    public void Start() {
        //StartCoroutine(pollingManager());
    }

    IEnumerator pollingManager() {
        while (true) {
            StartCoroutine(polling());
            yield return new WaitForSeconds(PollingInterval);
        }
    }

    IEnumerator polling() {
        Debug.Log("Polling for new games");

        Transform sessionListTransform = GameObject.Find("SessionList/Viewport/Content").transform;

        List<SessionInfo> sessions = new List<SessionInfo>();

        int numSessions = sessions.Count;

        LobbyService.getSessions((response) => {
            Debug.Log("sessions: " + response.Text);

            // clear up the previous children
            foreach (Transform child in sessionListTransform) {
                GameObject.Destroy(child.gameObject);
            }

            // add new children
            JObject responseObj = JObject.Parse(response.Text);
            JObject sessionsList = JObject.Parse(responseObj["sessions"].ToString());
            IEnumerator<KeyValuePair<string, JToken>> enumerator = sessionsList.GetEnumerator();

            while (enumerator.MoveNext()) {
                KeyValuePair<string, JToken> current = enumerator.Current;
                JObject sessionDetail = JObject.Parse(current.Value.ToString());
                SessionInfo infoObj = new SessionInfo();
                infoObj.gameparameters = new SessionInfo.GameParameters();

                // set game ID 
                infoObj.gameID = current.Key;

                // create a session info object
                infoObj.creator = sessionDetail.SelectToken("creator").ToString();

                // set player counts
                infoObj.gameparameters.maxSessionPlayers = int.Parse(sessionDetail.SelectToken("gameParameters").SelectToken("maxSessionPlayers").ToString());
                infoObj.gameparameters.minSessionPlayers = int.Parse(sessionDetail.SelectToken("gameParameters").SelectToken("minSessionPlayers").ToString());

                sessions.Add(infoObj);
            }



            for (int i = 0; i < sessions.Count; i++) {
                Debug.Log("Adding new sessions to list");
                GameObject sessionItem = Instantiate(SessionItemPrefab);
                sessionItem.GetComponent<SessionItemViewController>().gameID = sessions[i].gameID;
                sessionItem.GetComponent<SessionItemViewController>().gameName = sessions[i].gameparameters.name;
                sessionItem.GetComponent<SessionItemViewController>().info = sessions[i];
                sessionItem.transform.Find("GameName").GetComponent<Text>().text = sessions[i].creator + "'s session";
                sessionItem.transform.Find("PlayerStats").GetComponent<Text>().text = sessions[i].gameparameters.minSessionPlayers.ToString() + " / " + sessions[i].gameparameters.maxSessionPlayers.ToString();

                sessionItem.transform.SetParent(sessionListTransform, false);
            }
        });

        yield return new WaitForSeconds(100);
    }
}