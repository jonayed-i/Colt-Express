using System;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSystem : MonoBehaviour {

    [HideInInspector]
    private CardManager manager;
    private string localBandit;

    public GameObject gameManager;

    [Header("UI")]
    public GameObject CardPrefab;
    public GameObject TopCardPrefab;

    public GameObject handScrollViewContent;

    public GameObject stealingPileContent;

    public GameObject CurrentStealingCard;

    public List<Card> stealingPile = new List<Card>();

    public Button drawCardsButton;

    [Header("Wagons")]
    public GameObject locomotive;
    public GameObject wagon;
    public GameObject wagon1;
    public GameObject wagon2;
    public GameObject wagon3;

    private void Start() {
        localBandit = PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString();
        manager = new CardManager(localBandit);
        setupCards();

        drawCardsButton.onClick.AddListener(() => {
            manager.drawCards(3);
            //greyOutHands();
            drawCardsButton.interactable = false;
            PhotonView view = PhotonView.Get(gameManager);
            view.RPC("onTurnHandled", RpcTarget.MasterClient);
            drawCardsButton.onClick.RemoveAllListeners();
        });
    }

    [PunRPC]
    public void RefreshDrawCardsButton() {
        drawCardsButton.interactable = true;
        drawCardsButton.onClick.RemoveAllListeners();
        drawCardsButton.onClick.AddListener(() => {
            manager.drawCards(3);
            //greyOutHands();
            drawCardsButton.interactable = false;
            PhotonView view = PhotonView.Get(gameManager);
            view.RPC("onTurnHandled", RpcTarget.MasterClient);

        });
    }

    public void setupCards() {
        displayHands();
    }

    [PunRPC]
    public void refreshCardsOnAll() {
        localBandit = PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString();
        //manager = new CardManager(localBandit);
        manager.reinitialize();
        //
    }

    public void displayHands() {
        //Debug.Log("Displaycards selected");
        List<Card> hand = manager.mPlayerHand;

        foreach (Transform child in handScrollViewContent.transform) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < hand.Count; i++) {
            Card curCardData = manager.mPlayerHand[i];

            string spritePath = "cards/" + curCardData.owner + "_" + curCardData.name;

            Sprite curSprite = Resources.Load<Sprite>(spritePath);

            GameObject curCard = Instantiate(CardPrefab);
            curCard.GetComponentInChildren<Image>().sprite = curSprite;

            Transform cardListTransform = handScrollViewContent.transform;
            curCard.transform.SetParent(cardListTransform);

            if (curCardData.action != 7) {
                curCard.GetComponentInChildren<Button>().onClick.AddListener(() => {
                    playCard(curCardData);
                    //greyOutHands();
                });
            }

        }
    }

    public void greyOutHands() {
        //Debug.Log("greyoutcards selected");
        List<Card> hand = manager.mPlayerHand;

        foreach (Transform child in handScrollViewContent.transform) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < hand.Count; i++) {
            Card curCardData = manager.mPlayerHand[i];

            string spritePath = "cards/" + curCardData.owner + "_" + curCardData.name;
            Sprite curSprite = Resources.Load<Sprite>(spritePath);

            GameObject curCard = Instantiate(CardPrefab);
            curCard.GetComponentInChildren<Image>().sprite = curSprite;

            Transform cardListTransform = handScrollViewContent.transform;
            curCard.transform.SetParent(cardListTransform);

            curCard.GetComponentInChildren<Button>().interactable = false;
        }
    }

    [PunRPC]
    public void destroyhands() {
        foreach (Transform child in handScrollViewContent.transform) {
            Destroy(child.gameObject);
        }
    }

    public void playCard(Card c) {
        manager.mPlayerHand.Remove(c);
        displayHands();
        PhotonView view = PhotonView.Get(this);
        view.RPC("hidecardornot", RpcTarget.MasterClient, c.name, c.owner, c.action, c.isHidden);
        //view.RPC("playCardOnAll", RpcTarget.All, c.name, c.owner, c.action, c.isHidden);
    }

    [PunRPC]
    public void hidecardornot(string cardName, string owner, int action, bool isHidden)
    {
        
        if (PhotonNetwork.IsMasterClient)
        {
            bool shouldIHide = false;
            if (owner == "Ghost" && gameManager.GetComponent<GameManager>().currentRound.turnCount() == 4) {
                shouldIHide = true;
            } else if (owner == "Ghost" && gameManager.GetComponent<GameManager>().currentRound.turnCount() == 3 && gameManager.GetComponent<GameManager>().CurrentRoundindex == 4) {
                shouldIHide = true;
            } else if (gameManager.GetComponent<GameManager>().currentRound.mCurrentTurn == "Tunnel") {
                shouldIHide = true;
            }
            PhotonView view = PhotonView.Get(this);
            view.RPC("playCardOnAll", RpcTarget.All, cardName, owner, action, shouldIHide);
        }
    }

    [PunRPC]
    public void playCardOnAll(string cardName, string owner, int action, bool isHidden) {
        Card c = new Card(cardName, owner, action);
        stealingPile.Add(c);
/*
        bool shouldIHide = false;
        //ghost power check
        if (owner == "Ghost" && gameManager.GetComponent<GameManager>().currentRound.turnCount() == 4) {
            shouldIHide = true;
        } else if (owner == "Ghost" && gameManager.GetComponent<GameManager>().currentRound.turnCount() == 3 && gameManager.GetComponent<GameManager>().CurrentRoundindex == 4) {
            shouldIHide = true;
        } else if (gameManager.GetComponent<GameManager>().currentRound.mCurrentTurn == "Tunnel") {
            shouldIHide = true;
        }*/

        if (!isHidden) {
            displayStealingPile();
        } else {
            hideStealingPileCards();
        }


        if (PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString() == owner) {
            PhotonView view = PhotonView.Get(gameManager);
            view.RPC("onTurnHandled", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    public void showcurrentstealing(string cardName, string owner, int action) {
        foreach (Transform child in CurrentStealingCard.transform) {
            Destroy(child.gameObject);
        }

        //Debug.Log("showcurrentstealingreached");
        Card c = new Card(cardName, owner, action);

        string spritePath;
        Sprite curSprite;

        spritePath = "cards/" + c.owner + "_" + c.name;
        curSprite = Resources.Load<Sprite>(spritePath);

        GameObject curCard = Instantiate(TopCardPrefab);
        curCard.GetComponentInChildren<Image>().sprite = curSprite;

        Transform parent = CurrentStealingCard.transform;
        curCard.transform.SetParent(parent);
    }

    [PunRPC]
    public void destroycurrentstealing() {
        foreach (Transform child in CurrentStealingCard.transform) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in stealingPileContent.transform) {
            Destroy(child.gameObject);
        }
    }
    [PunRPC]
    public void showNextStealinginStealingPile(string cardName, string owner, int action) {
        foreach (Transform child in stealingPileContent.transform) {
            Destroy(child.gameObject);
        }

        if (stealingPile.Count != 0) {
            Card curCardData = new Card(cardName, owner, action); ;
            string spritePath;
            Sprite curSprite;

            spritePath = "cards/" + curCardData.owner + "_" + curCardData.name;
            curSprite = Resources.Load<Sprite>(spritePath);

            GameObject curCard = Instantiate(TopCardPrefab);
            curCard.GetComponentInChildren<Image>().sprite = curSprite;

            Transform schemingTransform = stealingPileContent.transform;
            curCard.transform.SetParent(schemingTransform);
        }



    }

    public void displayStealingPile() {
        foreach (Transform child in stealingPileContent.transform) {
            Destroy(child.gameObject);
        }

        if (stealingPile.Count != 0) {
            Card curCardData = stealingPile[stealingPile.Count - 1];
            string spritePath;
            Sprite curSprite;

            spritePath = "cards/" + curCardData.owner + "_" + curCardData.name;
            curSprite = Resources.Load<Sprite>(spritePath);

            GameObject curCard = Instantiate(TopCardPrefab);
            curCard.GetComponentInChildren<Image>().sprite = curSprite;

            Transform schemingTransform = stealingPileContent.transform;
            curCard.transform.SetParent(schemingTransform);
        }



    }

    public void hideStealingPileCards() {
        foreach (Transform child in stealingPileContent.transform) {
            Destroy(child.gameObject);
        }

        string spritePath;
        Sprite curSprite;

        spritePath = "cards/Gunslinger_cards7";
        curSprite = Resources.Load<Sprite>(spritePath);

        GameObject curCard = Instantiate(TopCardPrefab);
        curCard.GetComponentInChildren<Image>().sprite = curSprite;

        Transform schemingTransform = stealingPileContent.transform;
        curCard.transform.SetParent(schemingTransform);

    }

    public void promptChooseCard() {
        displayHands();
    }

    [PunRPC]
    public void getshot(string shooter, string victim) {
        Card bullet = new Card("bullet", shooter.Replace("(Clone)", ""), 7);
        if (localBandit == victim.Replace("(Clone)", "")) {
            Debug.Log("Local bandit is victim");
            manager.mDrawingDeck.Add(bullet);
        } else if (localBandit == shooter.Replace("(Clone)", "")) {
            Debug.Log("Local bandit is shooter");
            manager.removeBulletCard();
        }
    }

    public int getBulletCount() {
        return manager.mBulletCard.Count;
    }


    public void promptCardAction(string name, string owner, int action) {
        if (PhotonNetwork.LocalPlayer.CustomProperties["Bandit"].ToString() == owner) {
            Debug.Log("CardSystem: Local Playing Card " + action);
            if (action == 1) {
                Debug.Log("move hori played");

                // move horizontal
                GameObject curBandit = GameObject.Find(owner + "(Clone)");
                string Wagon = curBandit.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                string pinPointName = curBandit.GetComponent<TrainPassenger>().PinPoint.name;

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

                // activate prevWagon pinpoint buttons;
                if (prevWagon != null) {
                    if (prevWagon == locomotive) {
                        if (pinPointName.Contains("Up")) {
                            ActivatePinPointButton(curBandit, prevWagon, "PinPoint_Up");
                        } else
                            ActivatePinPointButton(curBandit, prevWagon, "PinPoint");
                    } else {
                        if (pinPointName.Contains("Up")) {
                            ActivatePinPointButton(curBandit, prevWagon, "PinPoint_UpMid");
                            //ActivatePinPointButton(curBandit, prevWagon, "PinPoint_UpBack");
                            //ActivatePinPointButton(curBandit, prevWagon, "PinPoint_UpFront");
                        } else {
                            ActivatePinPointButton(curBandit, prevWagon, "PinPoint_Mid");
                            //ActivatePinPointButton(curBandit, prevWagon, "PinPoint_Back");
                            //ActivatePinPointButton(curBandit, prevWagon, "PinPoint_Front");
                        }
                            
                    }
                }

                if (postWagon != null) {
                    if (pinPointName.Contains("Up")) {
                        ActivatePinPointButton(curBandit, postWagon, "PinPoint_UpMid");
                        //ActivatePinPointButton(curBandit, postWagon, "PinPoint_UpFront");
                        //ActivatePinPointButton(curBandit, postWagon, "PinPoint_UpBack");
                    } else {
                        ActivatePinPointButton(curBandit, postWagon, "PinPoint_Mid");
                        //ActivatePinPointButton(curBandit, postWagon, "PinPoint_Front");
                        //ActivatePinPointButton(curBandit, postWagon, "PinPoint_Back");
                    }
                        
                }



            } else if (action == 2) {
                // move vertical
                Debug.Log("move vert played");
                GameObject curBandit = GameObject.Find(owner + "(Clone)");
                string Wagon = curBandit.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                string pinPointName = curBandit.GetComponent<TrainPassenger>().PinPoint.name;

                GameObject curWagon;

                if (Wagon == "Wagon") {
                    curWagon = wagon;
                } else if (Wagon == "Wagon (1)") {
                    curWagon = wagon1;
                } else if (Wagon == "Wagon (2)") {
                    curWagon = wagon2;
                } else if (Wagon == "Wagon (3)") {
                    curWagon = wagon3;
                } else if (Wagon == "Locomotive") {
                    curWagon = locomotive;
                } else {
                    Debug.Log("Wagon Not Found");
                    return;
                }

                // activate prevWagon pinpoint buttons;
                if (curWagon != locomotive) {
                    if (pinPointName.Contains("Up")) {
                        ActivatePinPointButton(curBandit, curWagon, "PinPoint_Mid");
                        //ActivatePinPointButton(curBandit, curWagon, "PinPoint_Front");
                        //ActivatePinPointButton(curBandit, curWagon, "PinPoint_Back");
                    }
                    else {
                        ActivatePinPointButton(curBandit, curWagon, "PinPoint_UpMid");
                        //ActivatePinPointButton(curBandit, curWagon, "PinPoint_UpFront");
                        //ActivatePinPointButton(curBandit, curWagon, "PinPoint_UpBack");
                    }
                        

                } else {

                    if (pinPointName.Contains("Up")) {
                        ActivatePinPointButton(curBandit, curWagon, "PinPoint");
                    } else {
                        ActivatePinPointButton(curBandit, curWagon, "PinPoint_Up");
                    }
                }


            } else if (action == 3) {
                // loot
                gameManager.GetComponent<GameManager>().Rob(owner + "(Clone)");
                Debug.Log("LOOTPLAYED");
            } else if (action == 4) {
                // shoot
                gameManager.GetComponent<GameManager>().Shoot(owner + "(Clone)");
                Debug.Log("SHOOT PLAYED");

            } else if (action == 5) {
                gameManager.GetComponent<GameManager>().punch(owner + "(Clone)");
                Debug.Log("PUNCHPLAYED");

            } else if (action == 6) {
                // move marshall
                gameManager.GetComponent<GameManager>().moveMarshal();
                Debug.Log("MARSH PLAYED");
            }
        }
    }

    public void MoveMarshall() {
        GameObject Marshall = GameObject.Find("Marshall(Clone)");
        string Wagon = Marshall.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
        string pinPointName = Marshall.GetComponent<TrainPassenger>().PinPoint.name;

        GameObject prevWagon;
        GameObject postWagon;

        GameObject locomotive = GameObject.Find("Locomotive");
        GameObject wagon = GameObject.Find("Wagon");
        GameObject wagon1 = GameObject.Find("Wagon (1)");
        GameObject wagon2 = GameObject.Find("Wagon (2)");
        GameObject wagon3 = GameObject.Find("Wagon (3)");


        if (Wagon == "Wagon") {
            prevWagon = locomotive;
            postWagon = wagon;
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

        // activate prevWagon pinpoint buttons;
        if (prevWagon != null) {
            prevWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons

                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                }

                // move marshall to chosen position 
                MoveCharacter(Marshall, prevWagon.transform.Find("PinPoint_Mid").gameObject);

                //bandit escape part 
                string MarPos = prevWagon.transform.Find("PinPoint_Mid").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(this).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                        MoveCharacter(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }

            });

            prevWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                }

                MoveCharacter(Marshall, prevWagon.transform.Find("PinPoint_Front").gameObject);
                //bandit escape part 
                string MarPos = prevWagon.transform.Find("PinPoint_Front").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(this).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                        MoveCharacter(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }
            });

            prevWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            prevWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                }

                MoveCharacter(Marshall, prevWagon.transform.Find("PinPoint_Back").gameObject);
                //bandit escape part 
                string MarPos = prevWagon.transform.Find("PinPoint_Back").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(this).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                        MoveCharacter(player, prevWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }
            });
        }


        if (postWagon != null) {
            postWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            postWagon.transform.Find("PinPoint_Mid").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                }

                MoveCharacter(Marshall, postWagon.transform.Find("PinPoint_Mid").gameObject);
                //bandit escape part 
                string MarPos = postWagon.transform.Find("PinPoint_Mid").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(this).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                        MoveCharacter(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }
            });

            postWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            postWagon.transform.Find("PinPoint_Front").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                }

                MoveCharacter(Marshall, postWagon.transform.Find("PinPoint_Front").gameObject);
                //bandit escape part 
                string MarPos = postWagon.transform.Find("PinPoint_Front").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(this).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                        MoveCharacter(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }
            });

            postWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).gameObject.SetActive(true);
            postWagon.transform.Find("PinPoint_Back").GetComponentInChildren<Button>(true).onClick.AddListener(() => {

                // disable all buttons
                foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                    button.SetActive(false);
                }

                MoveCharacter(Marshall, postWagon.transform.Find("PinPoint_Back").gameObject);
                //bandit escape part 
                string MarPos = postWagon.transform.Find("PinPoint_Back").gameObject.transform.parent.name;

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                //find players in the same position as marshall and change position to roof 
                foreach (GameObject player in players) {
                    string playerPos = player.GetComponent<TrainPassenger>().PinPoint.transform.parent.name;
                    if (playerPos == MarPos) {
                        PhotonView.Get(this).RPC("getshot", RpcTarget.All, "Marshall", player.name);
                        MoveCharacter(player, postWagon.transform.Find("PinPoint_UpMid").gameObject);
                    }
                }
            });
        }

    }

    private void ActivatePinPointButton(GameObject curBandit, GameObject Wagon, string PinPointName) {
        if (Wagon.transform.Find(PinPointName) == null) return;

        GameObject PinPointObject = Wagon.transform.Find(PinPointName).gameObject;
        Button PinPointButton = PinPointObject.transform.Find("Canvas/Button").gameObject.GetComponent<Button>();
        PinPointButton.gameObject.SetActive(true);
        PinPointButton.onClick.AddListener(() => {

            // disable all buttons
            foreach (GameObject button in GameObject.FindGameObjectsWithTag("PinPointButton")) {
                button.SetActive(false);
                button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            }

            GameObject MarshallLocation = GameObject.Find("Marshall(Clone)").GetComponent<TrainPassenger>().PinPoint.transform.parent.gameObject;
            if (Wagon == MarshallLocation) {
                if (Wagon == locomotive)
                    MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_Up").gameObject);
                else {
                    int pinPointIndex = Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);
                    if (pinPointIndex % 3 == 0) {
                        MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_UpBack").gameObject);
                    } else if (pinPointIndex % 3 == 1) {
                        MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_UpMid").gameObject);
                    } else if (pinPointIndex % 3 == 2) {
                        MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_UpFront").gameObject);
                    }

                    //MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_UpMid").gameObject);
                }
                    
            } else {
                int pinPointIndex = Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);
                if (PinPointName.Contains("Up")) {
                    if (Wagon == locomotive) {
                        MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_Up").gameObject);
                    } else {
                        if (pinPointIndex % 3 == 0) {
                            MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_UpBack").gameObject);
                        } else if (pinPointIndex % 3 == 1) {
                            MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_UpMid").gameObject);
                        } else if (pinPointIndex % 3 == 2) {
                            MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_UpFront").gameObject);
                        }
                    }
                    
                } else {
                    if (Wagon == locomotive) {
                        MoveCharacter(curBandit, Wagon.transform.Find("PinPoint").gameObject);
                    } else {
                        if (pinPointIndex % 3 == 0) {
                            MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_Back").gameObject);
                        } else if (pinPointIndex % 3 == 1) {
                            MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_Mid").gameObject);
                        } else if (pinPointIndex % 3 == 2) {
                            MoveCharacter(curBandit, Wagon.transform.Find("PinPoint_Front").gameObject);
                        }
                    }
                }
                
                //MoveCharacter(curBandit, PinPointButton.transform.parent.parent.gameObject);
            }
        });
    }

    public void MoveCharacter(GameObject character, GameObject PinPoint) {
        PhotonView view = PhotonView.Get(character);
        view.RPC("UpdatePosition", RpcTarget.All, PinPoint.transform.parent.name, PinPoint.name);

        PhotonView gmView = PhotonView.Get(gameManager);
        gmView.RPC("onStealingTurnHandled", RpcTarget.MasterClient);
    }
}
