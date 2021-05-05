using Photon.Pun;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// only one per game
public class GameManager : MonoBehaviourPunCallbacks {
    [HideInInspector]
    public int mCurrentStatus;
    [HideInInspector]
    public Player mCurrentPlayer;
    [HideInInspector]
    public List<Player> mAllPlayers = new List<Player>();

    public List<Wagon> mLocomotives;

    [HideInInspector] public List<Card> PlayedCards = new List<Card>();
    [HideInInspector] public List<GameObject> PlayedCardsPrefab = new List<GameObject>();
    [HideInInspector] public List<Card> stealingpile = new List<Card>();
    [HideInInspector] public List<GameObject> stealinpileprefabs = new List<GameObject>();

    [HideInInspector] public GameObject localPlayer;

    [Header("Bandit Prefabs")] public GameObject Tuco;
    public GameObject Belle;
    public GameObject Doc;
    public GameObject Ghost;
    public GameObject Django;
    public GameObject Cheyenne;

    [Header("Spawn")]
    public List<GameObject> mWagonPinPoints;

    public GameObject cardcanvas;
    [Header("CardSpawn")]
    public List<GameObject> cardpositions;

    public GameObject cardPrefab;

    [HideInInspector]
    public List<Round> rounds = new List<Round>();
    public int CurrentRoundindex = 0;
    public Round currentRound;
    //public int currentPlayerIndex;
    private int curPlayerIndex = 0;
    public CardSystem cardSystem;

    [Header("Buttons")]
    public GameObject ShootButton;
    public List<GameObject> ShootButtonList;
    public Button rubys;
    public Button strongboxes;
    public Button purses;



    [Header("TurnType Indicator")]
    public Text turnTypeIndicator;

    public Text moneyIndicator;
    public int numRoundsPlaying = 5; //howmanyroundsareweplying: if we lets players choose how many rounds they want to play else default is 5
    public int howmanyplayersplayedthisturn = 0;


    //wagons
    public GameObject locomotive;
    public GameObject wagon;
    public GameObject wagon1;
    public GameObject wagon2;
    public GameObject wagon3;
    public GameObject SchemingSong;
    public GameObject StealingSong;

    public GameObject tumble;           //Tumbleweed
    public GameObject scoreBoardScreen; //Scoreboard


    public GameObject Marshall;
    private int areWeSpeeding = 0;

    private void Start() {
        Destroy(GameObject.Find("LobbyMusic"));
        locomotive = GameObject.Find("Locomotive");
        wagon = GameObject.Find("Wagon");
        wagon1 = GameObject.Find("Wagon (1)");
        wagon2 = GameObject.Find("Wagon (2)");
        wagon3 = GameObject.Find("Wagon (3)");


        SpawnBandits();
        instantiateMarshall();
        initializePlayerList();
        mCurrentStatus = 1;

        spawnItems();
        // to test move marhsall
        //moveMarshal();
        //test punch

        createRounds();
        currentRound = rounds[0];
        currentRound.startNextTurn();
        PhotonView.Get(this).RPC("swapsong", RpcTarget.All, 0);
        initiateTurn();
        //nextRound();
        // set money indicator

    }





    /// RoundGener

    /// <summary>
    ///  SCHEMING
    /// </summary>



    public void createRounds() {
        Round r1 = new Round();
        Round r2 = new Round();


        Round r5 = new Round();

        r1.turns.Add("Standard");
        r1.turns.Add("Speeding");
        r1.turns.Add("Tunnel");
        r1.turns.Add("Switching");
        r1.mEvent = "Braking";

        r2.turns.Add("Standard");
        r2.turns.Add("Tunnel");
        r2.turns.Add("Standard");
        r2.turns.Add("Standard");
        r2.mEvent = "TakeItAll";

        rounds.Add(r1);
        rounds.Add(r2);

        if (numRoundsPlaying > 3) {
            Round r3 = new Round();
            r3.turns.Add("Tunnel");
            r3.turns.Add("Switching");
            r3.turns.Add("Speeding");
            r3.turns.Add("Tunnel");
            r3.mEvent = "SwivelArm";
            rounds.Add(r3);
        }
        if (numRoundsPlaying > 4) {
            Round r4 = new Round();
            r4.turns.Add("Standard");
            r4.turns.Add("Tunnel");
            r4.turns.Add("Standard");
            r4.turns.Add("Standard");
            r4.mEvent = "Rebellion";
            rounds.Add(r4);
        }

        r5.turns.Add("Tunnel");
        r5.turns.Add("Speeding");
        r5.turns.Add("Standard");
        r5.mEvent = "HostageConductor";


        rounds.Add(r5);
    }

    [PunRPC]
    public void swapsong(int song) {
        if (song == 0) {
            StealingSong.SetActive(false);
            SchemingSong.SetActive(true);
        } else if (song == 1) {
            SchemingSong.SetActive(false);
            StealingSong.SetActive(true);

        }
    }

    public void nextRound() {
        CurrentRoundindex++;
        if (CurrentRoundindex == numRoundsPlaying) {
            Debug.Log("GAME OVER BRO");
            if (PhotonNetwork.IsMasterClient)
            {
                scoreBoard();
            }
            //PhotonView.Get(this).RPC("scoreBoard", RpcTarget.All);
            return;
        }


        howmanyplayersplayedthisturn = 0;
        Debug.Log("NEXTO ROUND CALLED");
        currentRound = rounds[CurrentRoundindex];
        curPlayerIndex = CurrentRoundindex % PhotonNetwork.PlayerList.Length;
        currentRound.startNextTurn();

        PhotonView.Get(cardSystem.gameObject).RPC("refreshCardsOnAll", RpcTarget.All);
        PhotonView.Get(cardSystem.gameObject).RPC("RefreshDrawCardsButton", RpcTarget.All);
        initiateTurn();

    }
    public void initiateTurn() {
        Debug.Log("TURN INITIATION REACHED" + "ROUND: " + CurrentRoundindex + " TURN INDEX : " + currentRound.turnCount() + "how many" + howmanyplayersplayedthisturn + "WHOS TURN" + curPlayerIndex);

        PhotonView view = PhotonView.Get(this);
        if (howmanyplayersplayedthisturn == PhotonNetwork.PlayerList.Length) {
            curPlayerIndex = CurrentRoundindex % PhotonNetwork.PlayerList.Length;

            if (currentRound.turnCount() == 0) {
                Debug.Log("WE HAVE ENTERED STEALING");
                PhotonView.Get(cardSystem.gameObject).RPC("destroyhands", RpcTarget.All);
                if (PhotonNetwork.IsMasterClient) {
                    initStealingTurn();
                }

                string stealdisplay = "current round: " + (CurrentRoundindex + 1) + " Stealing: " + currentRound.mEvent;
                view.RPC("swapsong", RpcTarget.All, 1);
                view.RPC("updateTurnTypeOnAll", RpcTarget.All, stealdisplay);
                return;
            } else {
                Debug.Log("start next turn:");
                currentRound.startNextTurn();
                if (currentRound.mCurrentTurn == "Speeding" && areWeSpeeding == 0) {
                    PhotonView.Get(this).RPC("setAreWeSpeeding", RpcTarget.All, 1);
                } else if (currentRound.mCurrentTurn != "Speeding") {
                    PhotonView.Get(this).RPC("setAreWeSpeeding", RpcTarget.All, 0);
                }

                //Debug.Log("turncount:" + currentRound.turnCount());

                if (currentRound.mCurrentTurn == "Switching") {
                    curPlayerIndex = (CurrentRoundindex + PhotonNetwork.PlayerList.Length - 1); // % PhotonNetwork.PlayerList.Length
                }


                howmanyplayersplayedthisturn = 0;
                initiateTurn();
            }
        } else {
            if (currentRound.mCurrentTurn == "Speeding" && areWeSpeeding == 2) {
                PhotonView.Get(this).RPC("setAreWeSpeeding", RpcTarget.All, 0);
            }



            //Debug.Log("Next Player: " + PhotonNetwork.PlayerList[curPlayerIndex].CustomProperties["Bandit"].ToString());
            //Debug.Log("Current player index" + curPlayerIndex + "how many players played" + howmanyplayersplayedthisturn);
            view.RPC("handleTurn", RpcTarget.All,
                PhotonNetwork.PlayerList[curPlayerIndex % PhotonNetwork.PlayerList.Length].CustomProperties["Bandit"],
                currentRound.mCurrentTurn);

            string display;


            display = "Round: " + (CurrentRoundindex + 1) + " Turns Left: " + (currentRound.turnCount() + 1) + " " + currentRound.mCurrentTurn;


            view.RPC("updateTurnTypeOnAll", RpcTarget.All, display);
        }

    }

    [PunRPC]
    public void updateTurnTypeOnAll(string display) {
        turnTypeIndicator.text = display;
    }


    [PunRPC]
    public void handleTurn(string bandit, string turnType) {
        // turn specific logic
        if (turnType == "Tunnel") {
            //cardSystem.hideStealingPileCards();
        } else {
            //cardSystem.displayStealingPile();
        }

        // draw cards
        if (PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString() == bandit) {
            //Debug.Log("handleturn reached for " + PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString());
            cardSystem.promptChooseCard();
        } else {

            cardSystem.greyOutHands();
        }
    }

    [PunRPC]
    public void onTurnHandled() {
        Debug.Log("ON TURN HANDLE CALLED");


        if (areWeSpeeding == 0) {

            if (currentRound.mCurrentTurn == "Switching") {
                curPlayerIndex = (curPlayerIndex - 1) ; //% PhotonNetwork.PlayerList.Length
            } else {
                curPlayerIndex = (curPlayerIndex + 1) % PhotonNetwork.PlayerList.Length;
            }
            if (currentRound.mCurrentTurn == "Speeding") {
                PhotonView.Get(this).RPC("setAreWeSpeeding", RpcTarget.All, 1);
            }
            howmanyplayersplayedthisturn++;
            initiateTurn();
        } else {
            PhotonView.Get(this).RPC("setAreWeSpeeding", RpcTarget.All, 2);
            initiateTurn();
        }
    }

    /// <summary>
    /// Stealing
    /// </summary>
    public void initStealingTurn() {
        // find top card
        if (cardSystem.stealingPile.Count != 0) {
            Debug.Log(cardSystem.stealingPile.Count + " BEFORE REMOVE");
            Card topCard = cardSystem.stealingPile[0];
            Debug.Log("STEALING PHASE CARD PLAYED ACTION, OWNER:  " + topCard.name + topCard.owner);
            cardSystem.stealingPile.RemoveAt(0);
            Debug.Log(cardSystem.stealingPile.Count + " AFTER REMOVE");
            if (cardSystem.stealingPile.Count != 0) {
                Card nextCard = cardSystem.stealingPile[0];
                PhotonView.Get(cardSystem.gameObject).RPC("showNextStealinginStealingPile", RpcTarget.All, nextCard.name, nextCard.owner, nextCard.action);
            }

            PhotonView.Get(cardSystem.gameObject).RPC("showcurrentstealing", RpcTarget.All, topCard.name, topCard.owner, topCard.action);
            PhotonView.Get(this).RPC("handleStealingTurn", RpcTarget.All, topCard.name, topCard.owner, topCard.action);

        } else {
            // Reset Stealing pile and switch back to schemin'
            PhotonView.Get(cardSystem.gameObject).RPC("destroycurrentstealing", RpcTarget.All);
            PhotonView.Get(this).RPC("swapsong", RpcTarget.All, 0);
            Debug.Log("I am calling next round from init stealing turn");
            if (currentRound.mEvent == "Braking") {
                //do braking 
                GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject p in Players) {
                    string pCurPinPoint = p.GetComponent<TrainPassenger>().PinPoint.name;

                    PhotonView pView = PhotonView.Get(p);

                    if (pCurPinPoint.Contains("Up") && p.GetComponent<TrainPassenger>().PinPoint.transform.parent.gameObject != locomotive) {
                        int pinpointIndex = Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);
                        if (pinpointIndex % 3 == 0)
                            pView.RPC("UpdatePosition", RpcTarget.All, wagon.name, "PinPoint_UpBack");
                        else if (pinpointIndex % 3 == 1)
                            pView.RPC("UpdatePosition", RpcTarget.All, wagon.name, "PinPoint_UpMid");
                        else if (pinpointIndex % 3 == 2)
                            pView.RPC("UpdatePosition", RpcTarget.All, wagon.name, "PinPoint_UpFront");
                    }

                }
            }
            if (currentRound.mEvent == "SwivelArm") {
                //do SwivelArm
                GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject p in Players) {
                    string pCurPinPoint = p.GetComponent<TrainPassenger>().PinPoint.name;

                    PhotonView pView = PhotonView.Get(p);

                    if (pCurPinPoint.Contains("Up")) {
                        int pinpointIndex = Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);
                        if (pinpointIndex % 3 == 0)
                            pView.RPC("UpdatePosition", RpcTarget.All, wagon3.name, "PinPoint_UpBack");
                        else if (pinpointIndex % 3 == 1)
                            pView.RPC("UpdatePosition", RpcTarget.All, wagon3.name, "PinPoint_UpMid");
                        else if (pinpointIndex % 3 == 2)
                            pView.RPC("UpdatePosition", RpcTarget.All, wagon3.name, "PinPoint_UpFront");
                    }

                }
            }
            if (currentRound.mEvent == "Rebellion") {
                Debug.Log("CAling rebellion");
                Rebellion();
            }
            if (currentRound.mEvent == "TakeItAll") {
                instantiateItem("Suitcase", Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.gameObject, -150); //Instantiates new suitcase in Marshall's Wagon (Positioning might be slightly off but hopefully it'll do)
            }
            if (currentRound.mEvent == "HostageConductor")
            {
                foreach(GameObject player in mLocomotives[4].mPlayersInside)
                {
                    instantiateItem("Purse", player, 0);
                }
                foreach(GameObject player in mLocomotives[4].mPlayersOnRoof)
                {
                    instantiateItem("Purse", player, 0);
                }
            }

            nextRound();

        }

    }

    [PunRPC]
    public void handleStealingTurn(string cardName, string cardOwner, int action) {
        // find card and remove
        Card topCard = new Card(cardName, cardOwner, action);
        /*
        Card topCardInPile = null;
        foreach (Card c in cardSystem.stealingPile) {
            if (c.name == topCard.name &&
                c.owner == topCard.owner &&
                c.action == topCard.action) {
                topCardInPile = c;
            }
        }

        if (topCardInPile != null) {
            cardSystem.stealingPile.Remove(topCardInPile);
            cardSystem.displayStealingPile();
        }
        */

        if (PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString() == cardOwner) {
            cardSystem.promptCardAction(cardName, cardOwner, action);
        }
    }

    [PunRPC]
    public void onStealingTurnHandled() {
        Debug.Log("ON STEALING HANDLE CALLED");
        PhotonView.Get(this).RPC("updatescoredisplay", RpcTarget.All);

        if (PhotonNetwork.IsMasterClient) {
            initStealingTurn();
        }

    }

    [PunRPC]
    public void setAreWeSpeeding(int i) {
        areWeSpeeding = i;
    }
    public void Rebellion() {
        GameObject[] mAllPlayerObjects = GameObject.FindGameObjectsWithTag("Player");



        foreach (GameObject player in mAllPlayerObjects) {
            if (!player.GetComponent<PlayerDataHolder>().mOnRoof) {
                string targetBandit = player.name.Replace("(Clone)", "").ToString();
                Debug.Log("Rebellion called on" + targetBandit);
                PhotonView view = cardSystem.gameObject.GetComponent<PhotonView>();
                if (view != null) {
                    view.RPC("getshot", RpcTarget.All, "Marshall", targetBandit);
                }

            }


        }
    }

    public void SpawnBandits() {
        // instantiate players
        string chosenBandit = PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString();
        int SpawnAt = Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer) + 1;

        if (chosenBandit == "Tuco") {
            localPlayer = PhotonNetwork.Instantiate("Tuco", new Vector3(0, 0, 0), Quaternion.identity);


            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("Bandit", "Tuco");
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            GameObject.Find("SessionInfoProvider").GetComponent<SessionInfoProvider>().choosenCharacter = "Tuco";

            PhotonView view = localPlayer.GetComponent<PhotonView>();

            if (view != null) {
                view.RPC("UpdatePosition", RpcTarget.All, mWagonPinPoints[SpawnAt].transform.parent.name, mWagonPinPoints[SpawnAt].name);
                SpawnAt++;
            } else {
                Debug.LogWarning("Tuco Prefab missing PhotonView");
            }

        } else if (chosenBandit == "Belle") {
            localPlayer = PhotonNetwork.Instantiate("Belle", new Vector3(0, 0, 0), Quaternion.identity);

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("Bandit", "Belle");
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            GameObject.Find("SessionInfoProvider").GetComponent<SessionInfoProvider>().choosenCharacter = "Belle";

            PhotonView view = localPlayer.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("UpdatePosition", RpcTarget.All, mWagonPinPoints[SpawnAt].transform.parent.name, mWagonPinPoints[SpawnAt].name);
                SpawnAt++;
            } else {
                Debug.LogWarning("Belle prefab missing PhotonView");
            }

        } else if (chosenBandit == "Doc") {
            localPlayer = PhotonNetwork.Instantiate("Doc", new Vector3(0, 0, 0), Quaternion.identity);

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("Bandit", "Doc");
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            GameObject.Find("SessionInfoProvider").GetComponent<SessionInfoProvider>().choosenCharacter = "Doc";

            PhotonView view = localPlayer.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("UpdatePosition", RpcTarget.All, mWagonPinPoints[SpawnAt].transform.parent.name, mWagonPinPoints[SpawnAt].name);
                SpawnAt++;
            } else {
                Debug.LogWarning("Doc prefab missing PhotonView");
            }

        } else if (chosenBandit == "Cheyenne") {
            localPlayer = PhotonNetwork.Instantiate("Cheyenne", new Vector3(0, 0, 0), Quaternion.identity);

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("Bandit", "Cheyenne");
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            GameObject.Find("SessionInfoProvider").GetComponent<SessionInfoProvider>().choosenCharacter = "Cheyenne";


            PhotonView view = localPlayer.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("UpdatePosition", RpcTarget.All, mWagonPinPoints[SpawnAt].transform.parent.name, mWagonPinPoints[SpawnAt].name);
                SpawnAt++;
            } else {
                Debug.LogWarning("Cheyenne prefab missing PhotonView");
            }


        } else if (chosenBandit == "Django") {
            localPlayer = PhotonNetwork.Instantiate("Django", new Vector3(0, 0, 0), Quaternion.identity);

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("Bandit", "Django");
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            GameObject.Find("SessionInfoProvider").GetComponent<SessionInfoProvider>().choosenCharacter = "Django";

            PhotonView view = localPlayer.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("UpdatePosition", RpcTarget.All, mWagonPinPoints[SpawnAt].transform.parent.name, mWagonPinPoints[SpawnAt].name);
                SpawnAt++;
            } else {
                Debug.LogWarning("Django prefab missing PhotonView");
            }

        } else if (chosenBandit == "Ghost") {
            localPlayer = PhotonNetwork.Instantiate("Ghost", new Vector3(0, 0, 0), Quaternion.identity);

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("Bandit", "Ghost");
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            GameObject.Find("SessionInfoProvider").GetComponent<SessionInfoProvider>().choosenCharacter = "Ghost";

            PhotonView view = localPlayer.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("UpdatePosition", RpcTarget.All, mWagonPinPoints[SpawnAt].transform.parent.name, mWagonPinPoints[SpawnAt].name);
                SpawnAt++;
            } else {
                Debug.LogWarning("Ghost prefab missing PhotonView");
            }
        }
    }

    public void instantiateCards() {
        string Banditname = PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString();

        CardManager c = new CardManager(Banditname);
        localPlayer.GetComponent<PlayerDataHolder>().MycardManager = c;
        int i = 0;

        foreach (Card card in c.mPlayerHand) {
            GameObject newcard = Instantiate(cardPrefab, cardpositions[i].transform.position, Quaternion.identity);

            newcard.transform.parent = Camera.main.transform;
            string spritelocation = "cards/" + card.owner + "_" + card.name;
            Debug.Log(spritelocation);
            Sprite mySprite = Resources.Load<Sprite>(spritelocation);

            newcard.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = mySprite;

            newcard.GetComponent<CardDescription>().name = card.name;
            newcard.GetComponent<CardDescription>().action = card.action;
            newcard.GetComponent<CardDescription>().owner = card.owner;
            newcard.GetComponent<CardDescription>().ishidden = card.isHidden;

            i++;
        }
    }

    public void playCard(string name, int action, string owner, bool isHidden) {
        Debug.LogError("You Played This Car" + name + owner);
        if (PlayedCardsPrefab.Count != 0) {
            GameObject previousplayedcard = PlayedCardsPrefab[PlayedCardsPrefab.Count - 1];
            PhotonView view = previousplayedcard.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("hide", RpcTarget.All);
            } else {
                Debug.LogWarning("Hide Card missing PhotonView");
            }
        }

        GameObject newcard = PhotonNetwork.Instantiate("cardprefab", cardpositions[9].transform.position, Quaternion.identity);
        newcard.transform.parent = Camera.main.transform;
        string spritelocation = "cards/" + owner + "_" + name;
        Debug.Log(spritelocation);
        Sprite mySprite = Resources.Load<Sprite>(spritelocation);

        newcard.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = mySprite;

        newcard.GetComponent<CardDescription>().name = name;
        newcard.GetComponent<CardDescription>().action = action;
        newcard.GetComponent<CardDescription>().owner = owner;
        newcard.GetComponent<CardDescription>().ishidden = isHidden;
        Card playedCard = new Card(name, owner, action);
        PlayedCards.Add(playedCard);
        PlayedCardsPrefab.Add(newcard);
        Debug.LogError(PlayedCards);
    }

    public void initializePlayerList() {
        if (PhotonNetwork.IsMasterClient) {
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList) {
                Debug.Log(p.NickName + " chose " + p.CustomProperties["Bandit"]);
                Player player = new Player(p.NickName, p.CustomProperties["Bandit"].ToString());
                if (mAllPlayers == null) mAllPlayers = new List<Player>();
                mAllPlayers.Add(player);
            }
        }
    }

    public void instantiateMarshall() {
        if (PhotonNetwork.IsMasterClient) {
            Marshall = PhotonNetwork.Instantiate("Marshall", new Vector3(0, 0, 0), Quaternion.identity);
            GameObject locomotive = GameObject.Find("Locomotive");

            Marshall.transform.SetParent(locomotive.transform);

            PhotonView view = Marshall.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("UpdatePosition", RpcTarget.All, mWagonPinPoints[23].transform.parent.name, mWagonPinPoints[23].name);
            } else {
                Debug.LogWarning("Marshall Prefab missing PhotonView");
            }
        }
    }

    public void switchPhase(int phase) {
        mCurrentStatus = phase;
    }

    public void spawnItems() {
        if (PhotonNetwork.IsMasterClient) {
            foreach (GameObject w in GameObject.FindGameObjectsWithTag("Wagon")) {
                if (w.name.Equals("Locomotive")) {
                    //instantiateItem("Suitcase", w, -150);
                    instantiateItem("Suitcase", Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.gameObject, -150);
                } else {
                    instantiateItem("Ruby", w, -40);
                    instantiateItem("Purse", w, 40);
                }
            }
        }
    }

    public void instantiateItem(string mType, GameObject pos, int offsetX) //Instantiates specified loot type at specified location
    {
        GameObject newItem;
        GameObject mainCanvas = GameObject.Find("Canvas");

        if (mType.Equals("Ruby")) //Note that instead of if statement I could have PhotonNetwork.Instantiate(mType+" Button"...
                                  //but photon scares me so I opted not to just in case. Also they may need diff offsets.
        {
            newItem = PhotonNetwork.Instantiate("Ruby Button", new Vector3(0, 0, 0), Quaternion.identity);
            newItem.GetComponent<GameItem>().mValue = 500;

            PhotonView view = newItem.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("buttonEnable", RpcTarget.All);
                view.RPC("UpdateCanvasPosition", RpcTarget.All, pos.name, offsetX);
                view.RPC("changeLocationData", RpcTarget.All, pos.name);
            } else {
                Debug.LogWarning("Ruby prefab missing PhotonView");
            }
        } else if (mType.Equals("Purse")) {
            newItem = PhotonNetwork.Instantiate("Purse Button", new Vector3(0, 0, 0), Quaternion.identity);
            int[] values = new int[] { 250, 300, 350, 400, 450, 500 };
            int rand = values[UnityEngine.Random.Range(0, 5)];
            newItem.GetComponent<GameItem>().mValue = rand;

            PhotonView view = newItem.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("buttonEnable", RpcTarget.All);
                view.RPC("UpdateCanvasPosition", RpcTarget.All, pos.name, offsetX);
                view.RPC("changeLocationData", RpcTarget.All, pos.name);
            } else {
                Debug.LogWarning("Ruby prefab missing PhotonView");
            }
        } else if (mType.Equals("Suitcase")) {
            newItem = PhotonNetwork.Instantiate("Suitcase Button", new Vector3(0, 0, 0), Quaternion.identity);
            newItem.GetComponent<GameItem>().mValue = 1000;

            PhotonView view = newItem.GetComponent<PhotonView>();
            if (view != null) {
                view.RPC("buttonEnable", RpcTarget.All);
                view.RPC("UpdateCanvasPosition", RpcTarget.All, pos.name, offsetX);
                view.RPC("changeLocationData", RpcTarget.All, pos.name);
            } else {
                Debug.LogWarning("Ruby prefab missing PhotonView");
            }
        } else {
            Debug.Log("GameManager: Requested item " + mType + " doesn't exsit");
            newItem = null;
        }

    }

    public void updateCylinder(int bulletCount) {
        string spritePath = "cylinder/" + bulletCount.ToString();
        Debug.Log(spritePath);
        Sprite curSprite = Resources.Load<Sprite>(spritePath);
        GameObject curCylinder = GameObject.Find("Cylinder");
        curCylinder.GetComponentInChildren<SpriteRenderer>().sprite = curSprite;
    }

    public void Rob(string currentBandit) //Makes each GameItem button in player's Wagon interactable.
    {
        Debug.Log("Rob called");

        PlayerDataHolder currentPlayerData = GameObject.Find(currentBandit).GetComponent<PlayerDataHolder>();
        Wagon cLocation = mLocomotives[currentPlayerData.mLocomotiveLocation];

        if (!cLocation.mItems.Exists(item => item.mOnRoof == currentPlayerData.mOnRoof)) //If there's no item on the same floor as the player
        {
            Debug.LogWarning("No items to loot");
            PhotonView gmView = PhotonView.Get(this);
            gmView.RPC("messedUp", RpcTarget.All);
            
        }

        foreach (GameItem i in cLocation.mItems) {

            PhotonView view = i.button.GetComponent<PhotonView>();
            if (view != null) {
                if (view.Owner != PhotonNetwork.LocalPlayer) {
                    view.TransferOwnership(PhotonNetwork.LocalPlayer); //Makes current player the owner so they can interact    
                }
            } else {
                Debug.LogWarning(i.name + "prefab missing PhotonView");
            }

            if (i.mOnRoof == currentPlayerData.mOnRoof) {
                i.button.GetComponent<Button>().interactable = true;
                i.choosingPlayer = currentBandit;
            }
        }
    }

    public void chosenLoot(GameItem loot) //Activates when one of the GameItem buttons is pressed
    {
        GameObject cLocation = loot.mLocation;

        foreach (GameItem i in cLocation.GetComponent<Wagon>().mItems) {
            i.button.GetComponent<Button>().interactable = false;
        }

        PhotonView view = loot.GetComponent<PhotonView>();
        if (view != null) {
            view.RPC("changeLocationData", RpcTarget.All, loot.choosingPlayer); //Changes where the item is in data and updates active status (also updates score)

            PhotonView gmView = PhotonView.Get(this);
            gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);

        } else {
            Debug.LogWarning("Rob Failure");
        }
    }

    public void Shoot(string currentBandit) //Finds shoot targets and creates a button above them
    {
        if (cardSystem.getBulletCount() == 0) {
            Debug.LogWarning("Out of Bullets");
            PhotonView gmView = PhotonView.Get(this);
            gmView.RPC("messedUp", RpcTarget.All);
        } else {
            GameObject currentPlayer = GameObject.Find(currentBandit);
            List<GameObject> targets = new List<GameObject>();
            List<GameObject> inRightTrain = new List<GameObject>();
            List<GameObject> inLeftTrain = new List<GameObject>();

            int pos = currentPlayer.GetComponent<PlayerDataHolder>().mLocomotiveLocation;
            int right = pos + 1;
            int left = pos - 1;

            if (currentPlayer.GetComponent<PlayerDataHolder>().mOnRoof) //If player is on roof, targets include nearest player to the right and left
            {
                Debug.Log(currentBandit + " Shoot on Roof");
                while (left >= 0 && inLeftTrain.Count == 0) {
                    inLeftTrain = mLocomotives[left].mPlayersOnRoof;
                    left--;
                }
                if (inLeftTrain.Count > 0) {
                    targets.Add(inLeftTrain[0]);
                }

                while (right <= 4 && inRightTrain.Count == 0) {
                    inRightTrain = mLocomotives[right].mPlayersOnRoof;
                    right++;
                }
                if (inRightTrain.Count > 0) {
                    targets.Add(inRightTrain[0]);
                }
            } else //If player is inside, targets include all players in rightmost and leftmost wagons
              {
                Debug.Log(currentBandit + " Shoot in Train");
                if (left >= 0) {
                    inLeftTrain = mLocomotives[left].mPlayersInside;
                }
                if (right <= 4) {
                    inRightTrain = mLocomotives[right].mPlayersInside;
                }
                targets.AddRange(inRightTrain); //This will of course be 0 elements if player is in Locomotive
                targets.AddRange(inLeftTrain); //Ditto for caboose
            }

            if (currentBandit.Equals("Tuco(Clone)")) //If Player is Tuco, you can shoot a bandit on the other floor of your car
            {
                if (currentPlayer.GetComponent<PlayerDataHolder>().mOnRoof) {
                    targets.AddRange(mLocomotives[pos].mPlayersInside);
                } else {
                    targets.AddRange(mLocomotives[pos].mPlayersOnRoof);
                }
            }

            if (targets.Count == 0) {
                Debug.LogWarning("No targets to shoot");
                PhotonView gmView = PhotonView.Get(this);
                gmView.RPC("messedUp", RpcTarget.All);
            } else if (targets.Count > 1) //Belle can only be a target if she's the only option
              {
                targets.RemoveAll(p => p.name.Equals("Belle(Clone)"));
            }

            ShootButtonList = new List<GameObject>();
            foreach (GameObject p in targets) //The player sprite will become the parent of the shoot button
            {
                GameObject b = Instantiate(ShootButton, new Vector3(0, 0, 0), Quaternion.identity);
                b.transform.SetParent(GameObject.Find("Canvas").transform);

                UpdateCanvasPosition(p, b, new Vector3(0, 30, 0));

                b.GetComponent<Button>().onClick.AddListener(() => { chosenShootTarget(p.name); });
                currentPlayer.GetComponent<PlayerDataHolder>().bulletsShot += 1;
                ShootButtonList.Add(b); //A list of shootbuttons so they can be destroyed later
            }
        }
    }

    public void chosenShootTarget(string targetBandit) //Adds bullet to chosen target and destroys shoot buttons
    {
        GameObject target = GameObject.Find(targetBandit);

        PlayerDataHolder localPlayerData = localPlayer.GetComponent<PlayerDataHolder>();
        PlayerDataHolder targetData = target.GetComponent<PlayerDataHolder>();


        PhotonView view = cardSystem.gameObject.GetComponent<PhotonView>();
        if (view != null) {
            view.RPC("getshot", RpcTarget.All, localPlayer.name, targetBandit); //Changes where the item is in data
        }
        updateCylinder(cardSystem.getBulletCount());

        if (localPlayer.name.Equals("Django(Clone)")) //Django Power. Shoots them to Locomotive or Wagon(3)
        {
            if (targetData.mLocomotiveLocation - localPlayerData.mLocomotiveLocation > 0) {
                GameObject destPinpoint = mLocomotives[4].transform.Find("PinPoint_Up").gameObject;
                MoveCharacterDup(target, destPinpoint);
            } else {
                GameObject destPinpoint = mLocomotives[0].transform.Find("PinPoint_Back").gameObject;
                MoveCharacterDup(target, destPinpoint);
            }
        }

        Debug.Log("Shoot " + targetBandit); //tmp for test

        foreach (GameObject b in ShootButtonList) {
            Destroy(b);
        }

        PhotonView gmView = PhotonView.Get(this);
        gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
    }

    //instantiate scoreboard when game is over 
    public Text textWinner;
  
    public void scoreBoard()
    {
        int HighestScore = 0;
        int bullets = 0;
        GameObject mostBulls = localPlayer;
        GameObject winner = localPlayer;

        GameObject[] mAllPlayerObjects = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in mAllPlayerObjects)
        {
            int temp = player.GetComponent<PlayerDataHolder>().bulletsShot;

            if (temp > bullets)
            {
                bullets = temp;
                mostBulls = player;
            }

        }

        mostBulls.GetComponent<PlayerDataHolder>().score += 1000;
        Debug.Log("Player with more bullets is " + mostBulls);

        foreach (GameObject player in mAllPlayerObjects)
        {
            int temp = player.GetComponent<PlayerDataHolder>().score;

            if (temp > HighestScore)
            {
                HighestScore = temp;
                winner = player;
            }

        }

        string displaywinner = "";
        
        displaywinner += winner.name.Replace("(Clone)", "");
        displaywinner += " " + HighestScore + "$";
        displaywinner += "\n Gunslinger: " + mostBulls.name.Replace("(Clone)", "");
        PhotonView.Get(this).RPC("doscoreboardonall", RpcTarget.All, displaywinner);
        /*scoreBoardScreen.SetActive(true);
        GameObject button = GameObject.FindGameObjectWithTag("ButtonLobby");
        button.SetActive(true);
        button.GetComponent<Button>().interactable = true;
        button.GetComponent<Button>().onClick.AddListener(() =>
            {
                
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LeaveLobby();
                SceneManager.LoadScene(1);
                //leavelobby
            }
        );*/

        Debug.Log("Winner is " + winner.name);
    }

    [PunRPC]
    public void doscoreboardonall(string text)
    {
        textWinner.fontSize = 25;
        textWinner.text = text;
        scoreBoardScreen.SetActive(true);
        GameObject button = GameObject.FindGameObjectWithTag("ButtonLobby");
        button.SetActive(true);
        button.GetComponent<Button>().interactable = true;
        button.GetComponent<Button>().onClick.AddListener(() =>
            {
                
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LeaveLobby();
                SceneManager.LoadScene(1);
                //leavelobby
            }
        );
    }

    [PunRPC]
    public IEnumerator messedUp()
    {
        //tw = GameObject.FindGameObjectWithTag("tumble");
        tumble.SetActive(true);
        Debug.Log("CALLING FUCKED UP TW ");
        yield return new WaitForSeconds(2);
        Debug.Log("DONE FUCKED UP TW ");
        tumble.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView gmView = PhotonView.Get(this);
            Debug.Log("MESSED UP CALLED STEALING HANDLED");
            gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
        }
        
    }



    public void moveMarshal() {
        Marshall = GameObject.Find("Marshall(Clone)");
        string Wagon = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
        string pinPointName = Marshall.GetComponent<TrainPassenger>().PinPoint.name;

        GameObject prevWagon;
        GameObject postWagon;

        if (Wagon == "Wagon") {
            prevWagon = locomotive;
            postWagon = wagon1;
        } else if (Wagon == "Wagon (1)") {
            prevWagon = wagon;
            postWagon = wagon2;
        } else if (Wagon == "Wagon (2)") {
            prevWagon = wagon1;
            postWagon = wagon3;
        } else if (Wagon == "Wagon (3)") {
            prevWagon = wagon2;
            postWagon = null;
        } else if (Wagon == "Locomotive") {
            prevWagon = null;
            postWagon = wagon;
        } else {
            Debug.Log("Wagon Not Found");
            return;
        }
        if (prevWagon == locomotive) {
            prevWagon.transform.Find("PinPoint").GetComponentInChildren<Button>(true).gameObject
                .GetComponentInChildren<Text>().text = "MoveMarshal";
            prevWagon.transform.Find("PinPoint").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons

                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);

                    button.GetComponentInChildren<Text>().text = "Here";
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                // move marshall to chosen position 
                MoveCharacterDup(Marshall, prevWagon.transform.Find("PinPoint").gameObject);


                //bandit escape part 

                string MarPos = prevWagon.transform.Find("PinPoint").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall", player.name.Replace("(Clone)", ""));
                        MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }
                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("995 UP CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);

            });

        }
        // activate prevWagon pinpoint buttons;
        if (prevWagon != null && prevWagon != locomotive) {
            prevWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).gameObject
                .GetComponentInChildren<Text>(true).text = "MoveMarshal";
            prevWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons

                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Text>().text = "Here";
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                // move marshall to chosen position 
                MoveCharacterDup(Marshall, prevWagon.transform.Find("PinPoint_Mid").gameObject);

                //bandit escape part 
                string MarPos = prevWagon.transform.Find("PinPoint_Mid").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall",
                            player.name.Replace("(Clone)", ""));
                        MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("marsh CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);

            });
            prevWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).gameObject.GetComponentInChildren<Text>().text = "MoveMarshal";
            prevWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Text>().text = "Here";
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(Marshall, prevWagon.transform.Find("PinPoint_Front").gameObject);
                //bandit escape part 
                string MarPos = prevWagon.transform.Find("PinPoint_Front").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall",
                            player.name.Replace("(Clone)", ""));
                        MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("marshCALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });
            prevWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).gameObject
                .GetComponentInChildren<Text>().text = "MoveMarshal";
            prevWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Text>().text = "Here";
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(Marshall, prevWagon.transform.Find("PinPoint_Back").gameObject);
                //bandit escape part 
                string MarPos = prevWagon.transform.Find("PinPoint_Back").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall",
                            player.name.Replace("(Clone)", ""));
                        MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("marsh CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });

        }

        if (postWagon != null) {
            postWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).gameObject
            .GetComponentInChildren<Text>().text = "MoveMarshal";
            postWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).gameObject
                .SetActive(true);
            postWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Text>().text = "Here";
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(Marshall, postWagon.transform.Find("PinPoint_Mid").gameObject);
                //bandit escape part 
                string MarPos = postWagon.transform.Find("PinPoint_Mid").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall",
                            player.name.Replace("(Clone)", ""));
                        MoveCharacterDup(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("marshCALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });
            postWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).gameObject
                .GetComponentInChildren<Text>().text = "MoveMarshal";

            postWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).gameObject
                .SetActive(true);
            postWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).onClick.AddListener(
                () => {

                        // disable all buttons
                        foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                        button.SetActive(false);
                        button.GetComponentInChildren<Text>().text = "Here";
                        button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                    }

                    MoveCharacterDup(Marshall, postWagon.transform.Find("PinPoint_Front").gameObject);
                        //bandit escape part 
                        string MarPos = postWagon.transform.Find("PinPoint_Front").gameObject.transform.parent.name;

                    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                        //find players in the same position as marshall and change position to roof 
                        foreach (GameObject player in players) {
                        string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                        if (playerPos == MarPos) {
                            PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall",
                                player.name.Replace("(Clone)", ""));
                            MoveCharacterDup(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                        }
                    }

                    PhotonView gmView = PhotonView.Get(this);
                    Debug.Log("marsh CALLED STEALING HANDLED");
                    gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);

                });
            postWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).gameObject
                .GetComponentInChildren<Text>().text = "MoveMarshal";

            postWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).gameObject
                .SetActive(true);
            postWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons 
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Text>().text = "Here";
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(Marshall, postWagon.transform.Find("PinPoint_Back").gameObject);
                //bandit escape part 
                string MarPos = postWagon.transform.Find("PinPoint_Back").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall",
                            player.name.Replace("(Clone)", ""));
                        MoveCharacterDup(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("marchCALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);

            });
        }

    }

    [PunRPC]
    public void updatescoredisplay() {
        GameObject[] mAllPlayerObjects = GameObject.FindGameObjectsWithTag("Player");


        moneyIndicator.text = "";
        foreach (GameObject player in mAllPlayerObjects) {
            int temp = player.GetComponent<PlayerDataHolder>().score;

            string banditname = player.name.Replace("(Clone)", "");
            moneyIndicator.text += banditname + "- Score: " + temp + "\n";

        }
    } 

    public void MoveCharacterDup(GameObject character, GameObject PinPoint) {
        PhotonView view = PhotonView.Get(character);
        view.RPC("UpdatePosition", RpcTarget.All, PinPoint.transform.parent.name, PinPoint.name);

        // PhotonView gmView = PhotonView.Get(gameObject);
        //gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
    }

    public void punch(string currentBandit) {
        //find current wagon 

        GameObject currentPlayer = GameObject.Find(currentBandit);
        string Wagon = currentPlayer.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
        string pinPointName = currentPlayer.GetComponent<TrainPassenger>().PinPoint.name;


        //find who to punch 
        List<GameObject> players = findTargetPunch(Wagon, pinPointName, currentPlayer);

        //remove current player
        // players.Remove(currentPlayer);

        foreach (GameObject player in players) {
            //set players selectable 

            player.GetComponent<TrainPassenger>().PinPoint.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            player.GetComponent<TrainPassenger>().PinPoint.GetComponentInChildren<Button>(true).gameObject
                .GetComponentInChildren<Text>().text = "Punch";

            player.GetComponent<TrainPassenger>().PinPoint.GetComponentInChildren<Button>(true).gameObject.SetActive(true);

        }
        //add the listener in another loop (makes me think its less buggy?)
        foreach (GameObject player in players) {

            player.GetComponent<TrainPassenger>().PinPoint.GetComponentInChildren<Button>(true).onClick.AddListener(() => {
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    
                    button.SetActive(false);
                    button.GetComponentInChildren<Text>().text = "Here";
                  
                }
                //choose which loot to drop part
                punchDropLoot(player);
                // PhotonView gmView = PhotonView.Get(this);
                //gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });


        }

        if (players.Count == 0) {
            Debug.Log("No punch target");
            
            PhotonView gmView = PhotonView.Get(this);
            gmView.RPC("messedUp", RpcTarget.All);
        }
    }

    public void movePlayerPunch(GameObject player) {
        GameObject prevWagon;
        GameObject postWagon;

        string Wagon = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
        GameObject pinPoint = player.GetComponent<TrainPassenger>().PinPoint;


        if (Wagon == "Wagon") {
            prevWagon = locomotive;
            postWagon = wagon1;
        } else if (Wagon == "Wagon (1)") {
            prevWagon = wagon;
            postWagon = wagon2;
        } else if (Wagon == "Wagon (2)") {
            prevWagon = wagon1;
            postWagon = wagon3;
        } else if (Wagon == "Wagon (3)") {
            prevWagon = wagon2;
            postWagon = null;
        } else if (Wagon == "Locomotive") {
            prevWagon = null;
            postWagon = wagon;
        } else {
            Debug.Log("Wagon Not Found");
            return;
        }
        Marshall = GameObject.Find("Marshall(Clone)");
        // activate prevWagon pinpoint buttons;
        if (prevWagon == locomotive) {
            GameObject pin = prevWagon.transform.Find("PinPoint").gameObject;
            prevWagon.transform.Find("PinPoint").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons

                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton"))
                {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                // move marshall to chosen position 
                MoveCharacterDup(player, prevWagon.transform.Find("PinPoint").gameObject);


                //bandit escape part 

                string MarPos = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;


                //find players in the same position as marshall and change position to roof 
                string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;

                if (playerPos == MarPos) {
                    Debug.Log("marshall encountrered");
                    //   PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                    MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                }

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });
        }
        if (prevWagon != null & prevWagon != locomotive) {
            prevWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons

                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                // move marshall to chosen position 
                MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_Mid").gameObject);


                //bandit escape part 

                string MarPos = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;


                string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                if (playerPos == MarPos) {
                    //   PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                    MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                }
                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);

            });

            prevWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_Front").gameObject);
                //bandit escape part 
                string MarPos = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;


                string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                if (playerPos == MarPos) {
                    //   PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                    MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                }
                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });

            prevWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_Back").gameObject);
                //bandit escape part 
                string MarPos = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;

                string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                if (playerPos == MarPos) {
                    //   PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                    MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                }
                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });
        }


        if (postWagon != null) {
            postWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            postWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(player, postWagon.transform.Find("PinPoint_Mid").gameObject);
                //bandit escape part 
                string MarPos = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;

                string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                if (playerPos == MarPos) {
                    //   PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                    MoveCharacterDup(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                }
                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });

            postWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            postWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(player, postWagon.transform.Find("PinPoint_Front").gameObject);
                //bandit escape part 
                string MarPos = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;

                string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                if (playerPos == MarPos) {
                    //   PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                    MoveCharacterDup(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                }
                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });

            postWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            postWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                MoveCharacterDup(player, postWagon.transform.Find("PinPoint_Back").gameObject);
                //bandit escape part 
                string MarPos = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;

                string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                if (playerPos == MarPos) {
                    //   PhotonView.Get(cardSystem.gameObject).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                    MoveCharacterDup(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                }
                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });
        }
               
        if (prevWagon == locomotive & pinPoint.name.Contains("Up")) {
            GameObject pin = prevWagon.transform.Find("PinPoint_Up").gameObject;
            prevWagon.transform.Find("PinPoint_Up").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Up").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons

                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                // move marshall to chosen position 
                MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_Up").gameObject);

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });
        }
        if (prevWagon != locomotive & pinPoint.name.Contains("Up") & prevWagon!=null) {
            GameObject pin = prevWagon.transform.Find("PinPoint_UpMid").gameObject;
            prevWagon.transform.Find("PinPoint_UpMid").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_UpMid").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons

                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                // move marshall to chosen position 
                MoveCharacterDup(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });
        }
        if (pinPoint.name.Contains("Up") & postWagon!=null) {
            GameObject pin = postWagon.transform.Find("PinPoint_UpMid").gameObject;
            postWagon.transform.Find("PinPoint_UpMid").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            postWagon.transform.Find("PinPoint_UpMid").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons

                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                    button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                }

                // move marshall to chosen position 
                MoveCharacterDup(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);

                PhotonView gmView = PhotonView.Get(this);
                Debug.Log("Punch CALLED STEALING HANDLED");
                gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
            });
        }


    }

    public void dropLootTest(string player) {
        punchDropLoot(GameObject.Find(player));
    }

    public void punchDropLoot(GameObject player) {


        string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
        string pinpoint = player.GetComponent<TrainPassenger>().PinPoint.name;
        GameObject Wagon = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.gameObject;

        List<GameItem> possessions = player.GetComponent<PlayerDataHolder>().mPossessions;

        GameItem ruby = null;
        GameItem strongbox = null;
        GameItem purse = null;

        foreach (GameItem item in possessions) {
            if (item.mType.Equals("Ruby")) {
                ruby = item;
            }
            else if (item.mType.Equals("Purse")) {
                purse = item;
            }
            else if (item.mType.Equals("Suitcase")) {
                strongbox = item;
            }
        }


        if (ruby != null) {

            rubys.gameObject.SetActive(true);
            rubys.onClick.AddListener(() => {
                rubys.gameObject.SetActive(false);

                dropLoot(ruby);
                //player.GetComponent<PlayerDataHolder>().score -= ruby.mValue;

                movePlayerPunch(player);
            });
        }

        if (strongbox != null) {

            strongboxes.gameObject.SetActive(true);

            strongboxes.onClick.AddListener(() => {
                strongboxes.gameObject.SetActive(false);


                dropLoot(strongbox);
                //player.GetComponent<PlayerDataHolder>().score -= strongbox.mValue;

                movePlayerPunch(player);

            });
        }

        if (purse != null) {

            Debug.Log("player now has" + player.GetComponent<PlayerDataHolder>().mPossessions.Count + "items");
            purses.gameObject.SetActive(true);
            purses.onClick.AddListener(() => {

                purses.gameObject.SetActive(false);

                dropLoot(purse);
                //player.GetComponent<PlayerDataHolder>().score -= purse.mValue;

                movePlayerPunch(player);
                Debug.Log("purse now has" + player.GetComponent<PlayerDataHolder>().mPossessions.Count + "items");

            });
        } else {
            Debug.Log("Nothing to drop");
            movePlayerPunch(player);
        }


    }

    public void dropLoot(GameItem loot) //Drops indicated loot from player onto player locomotive. *FOR PUNCH*
    {
        Wagon cLocation = mLocomotives[loot.mOwner.GetComponent<PlayerDataHolder>().mLocomotiveLocation]; //Would've preferred this to be a GameObject, but mLocomotives is already List<Wagon>
        GameObject owner = loot.mOwner;
        PhotonView view = loot.GetComponent<PhotonView>();

        if (view != null) {
            view.RPC("changeLocationData", RpcTarget.All, cLocation.wagonGameObject.name); //Changes where the item is in data and updates active status (also updates scores)
            view.RPC("UpdateCanvasPosition", RpcTarget.All, cLocation.wagonGameObject.name, -40);
        } else {
            Debug.LogWarning("Drop Loot Failure");
        }

    }

    public List<GameObject> findTargetPunch(string wagon, string pinPointName, GameObject currPlayer) {

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<GameObject> targetPlayers = new List<GameObject>();

        //find players in the same position as marshall and change position to roof 
        foreach (GameObject player in players) {
            string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
            string playerPin = player.GetComponent<TrainPassenger>().PinPoint.name;
            if (player == currPlayer) {
                continue;
            } else if (playerPos == wagon && !pinPointName.Contains("Up") && !playerPin.Contains("Up")) {
                targetPlayers.Add(player);
            } else if (playerPos == wagon && pinPointName.Contains("Up") && playerPin.Contains("Up")) {
                targetPlayers.Add(player);
            } else {
                Debug.Log("No one near to punch");
            }
        }


        if (targetPlayers.Count > 2 && targetPlayers.Contains(Belle)) {
            targetPlayers.Remove(Belle);
            Debug.Log("Belle removed");
        }


        return targetPlayers;


    }

    public void UpdateCanvasPosition(GameObject dest, GameObject toMove, Vector3 offset) //For changing canvas object position to match that of a non-canvas object
    {
        RectTransform canvasRect = toMove.transform.parent.GetComponent<RectTransform>();
        RectTransform thisRect = toMove.GetComponent<RectTransform>();

        Vector3 ViewportPosition = Camera.main.WorldToViewportPoint(dest.transform.position); //Transforms position depending on camera view

        //Canvas and viewpoint have different origin points so we have to do some maths

        Vector3 newPos = new Vector3(
            ((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)), //x
            ((ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)),          //y
            (0)) + offset;                                                                                       //z

        thisRect.anchoredPosition = newPos;
    }

}

public enum GameStatus {
    HORSES0,
    SCHEMIN1,
    STEALING2,
    COMPLETED3
}


