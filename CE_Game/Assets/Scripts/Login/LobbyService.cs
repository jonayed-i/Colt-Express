// Author: Feiyang Li
// LobbyService class abstracts the process of communicating with Lobby Service.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using System;

public class LobbyService : MonoBehaviour
{
    public static string BASIC_URL = "http://35.203.44.22:4242";
    public static string LSOauthBaseURL = BASIC_URL + "/oauth";
    public static string LSOnlineCountURL = BASIC_URL + "/api/online";
    public static string LSUserBaseURL = BASIC_URL + "/api/users";
    public static string LSGameServiceURL = BASIC_URL + "/api/gameservices";
    public static string LSGameSessionURL = BASIC_URL + "/api/sessions";

    /// <summary>
    /// Abstraction of /oauth/token endpoint, Authenticate with Lobby Service asynchronously 
    /// </summary>
    /// <param name="username">A string containing username</param>
    /// <param name="password">A string containing password</param>
    /// <param name="action">Callback function that takes ResponseHelper as a parameter; implement this as an arrow function such as (response) => {xxx}</param>
    public static void authenticate(string username, string password, Action<ResponseHelper> callback) {
        // create a new request
        Debug.Log("Authentication started");
        RequestHelper request = new RequestHelper();
        request.Method = "POST";
        request.Uri = LSOauthBaseURL + "/token";
        request.Params = new Dictionary<string, string>();
        request.Params.Add("grant_type", "password");
        request.Params.Add("username", username);
        request.Params.Add("password", password);
        request.Timeout = 10;
        request.Headers = new Dictionary<string, string>();

        // set headers to handle basic authentication
        request.Headers.Add("Authorization",
            "Basic " + System.Convert.ToBase64String(
                System.Text.Encoding.GetEncoding("UTF-8").GetBytes("bgp-client-name:bgp-client-pw")));
        request.ContentType = "application/json";

        RestClient.Request(request).Then(response => {
            // Debug.Log("Lobby Service Response Received: " + response.Text);
            callback(response);
        });
    }

    public static void getUsernameWithToken(string access_token, Action<ResponseHelper> callback) {
        RequestHelper request = new RequestHelper();
        request.Uri = LSOauthBaseURL + "/username";
        request.Method = "GET";
        request.Params = new Dictionary<string, string>();
        request.Params.Add("access_token", access_token);

        RestClient.Request(request).Then(response => {
            callback(response);
        });
    }

    /// <summary>
    /// Get player count. Abstraction of /api/online endpoint. 
    /// This function can also be used to check lobby service status
    /// </summary>
    /// <param name="action">Callback</param>
    public static void getPlayerCount(Action<ResponseHelper> callback) {
        RestClient.Get(LSOnlineCountURL).Then(res => {
            callback(res);
        });
    }

    public static void getUser(string username, string token, Action<ResponseHelper> callback) {
        RequestHelper request = new RequestHelper();
        request.Method = "GET";
        request.Uri = LSUserBaseURL + "/" + username;
        request.Params = new Dictionary<string, string>();
        request.Params.Add("access_token", token);

        RestClient.Request(request).Then((response) => {
            callback(response);
        });
    }

    /// <summary>
    /// TODO: Implement postPassword when needed
    /// </summary>
    /// <param name="username"></param>
    /// <param name="token"></param>
    /// <param name="callback"></param>
    public static void postPassword(string username, string token, Action<ResponseHelper> callback) {

    }

    /// <summary>
    /// Abstraction of GET /api/gameservices. Returns a list of all registered gameservices
    /// </summary>
    /// <param name="callback"></param>
    public static void getGameServiceList(Action<ResponseHelper> callback) {
        RestClient.Get(LSGameServiceURL).Then((response) => {
            callback(response);
        });
    }

    public static void getGameServiceDetails(string gameServiceName, Action<ResponseHelper> callback) {
        RestClient.Get(LSGameServiceURL + "/" + gameServiceName).Then((response) => {
            callback(response);
        });
    }

    /// <summary>
    /// Abstraction of GET /api/sessions. Returns a list of all game sessions
    /// </summary>
    /// <param name="callback"></param>
    public static void getSessions(Action<ResponseHelper> callback) {
        RestClient.Get(LSGameSessionURL).Then((response) => {
            callback(response);
        });
    }

    /// <summary>
    /// Abstraction of POST /api/sessions. Create a new session in the lobby service
    /// </summary>
    /// <param name="token"></param>
    /// <param name="creator">must match the owner of access token</param>
    /// <param name="gameName">name for game service. The service must have been registered in LS previously</param>
    /// <param name="saveGame"></param>
    public static void postSession(string token, string creator, string gameName, string saveGame, Action<ResponseHelper> callback) {
        RequestHelper request = new RequestHelper();
        request.Method = "POST";
        request.ContentType = "application/json";
        request.Uri = LSGameSessionURL;
        request.Params = new Dictionary<string, string>();
        request.Params.Add("access_token", token);
        request.BodyString = "{\"game\":\"" + gameName + "\", \"creator\":\"" + creator + "\", \"savegame\":\"" + saveGame + "\"}";

        RestClient.Request(request).Then((response) => {
            callback(response);
        });
    }

    /// <summary>
    /// abstraction of DELETE /api/sessions/{session}
    /// !!NOTICE: Only the creator of a game can delete the game
    /// </summary>
    /// <param name="token">access token for player</param>
    /// <param name="sessionID"></param>
    /// <param name="callback"></param>
    public static void deleteSession(string token, string sessionID, Action<ResponseHelper> callback) {
        RequestHelper request = new RequestHelper();
        request.Method = "DELETE";
        request.ContentType = "application/json";
        request.Uri = LSGameSessionURL + "/" + sessionID;
        request.Params = new Dictionary<string, string>();
        request.Params.Add("access_token", token);
        RestClient.Request(request).Then((response) => {
            callback(response);
        });
    }

    /// <summary>
    /// abstraction of PUT /api/sessions/{session}/players/{player}
    /// @TODO: needs testing and validation
    /// </summary>
    /// <param name="token"></param> access_token for player. Player role required
    /// <param name="callback"></param> callback that's used when received response.
    public static void putPlayerToSession(string playerName, string token, string sessionID, Action<ResponseHelper> callback) {
        RequestHelper request = new RequestHelper();
        request.Method = "PUT";
        request.Params = new Dictionary<string, string>();
        request.Params.Add("access_token", token);
        request.Uri = LSGameSessionURL + "/" + sessionID + "/players/" + playerName;

        RestClient.Request(request).Then((response) => {
            callback(response);
        });
    }
}
