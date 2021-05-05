using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameItem : MonoBehaviour
{
    public GameObject mOwner; //player
    public GameObject mLocation; //wagon
    public int mValue;
    public string mType;
    public bool mOnRoof;
    public GameObject button;

    [HideInInspector]
    public string choosingPlayer; //MQ: A bit jank but fixes a bug I was having. Records the player who's currently picking an item so it can be properly added

    [PunRPC]

    public void UpdateCanvasPosition(string destName, int offsetX) //Changes canvas object to non-canvas
    {
        int offsetY;

        if (mOnRoof)
        {
            offsetY = 20;
        }
        else
        {
            offsetY = -5;
        }


        RectTransform canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
        RectTransform thisRect = button.GetComponent<RectTransform>();

        GameObject dest = GameObject.Find(destName);
        Vector3 ViewportPosition = Camera.main.WorldToViewportPoint(dest.transform.position); //Transforms position depending on camera view

        //Canvas and viewpoint have different origin points so we have to do some maths

        Vector3 newPos = new Vector3(
 ((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f) + offsetX), //x
 ((ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f) + offsetY),          //y
 (0));                                                                                       //z

        thisRect.anchoredPosition = newPos;
    }

    [PunRPC]

    public void buttonEnable()
    {
        GameObject mainCanvas = GameObject.Find("Canvas");
        button.transform.SetParent(mainCanvas.transform);
        GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        button.GetComponent<Button>().onClick.AddListener(() => {manager.chosenLoot(this);});
    }

    [PunRPC]

    public void changeLocationData(string destName)
    {
        Debug.Log(this.name + " is now in " + destName);
        GameObject dest = GameObject.Find(destName);
        if (dest.CompareTag("Player")) //Removes from wagon and adds to player. If Cheyenne is stealing purse, this can also be called to remove from target player and add directly to player.
        {
            if (mLocation != null)
            {
                mLocation.GetComponent<Wagon>().removeItem(this);
                mLocation = null;
            }

            if (mOwner != null) //For Cheyenne's Power.
            {
                mOwner.GetComponent<PlayerDataHolder>().mPossessions.Remove(this);
            }

            mOwner = dest;
            mOwner.GetComponent<PlayerDataHolder>().mPossessions.Add(this);
            mOwner.GetComponent<PlayerDataHolder>().score += mValue;               //Scoreboard
            button.SetActive(false);
        }
        else if (dest.CompareTag("Wagon")) //Removes from player and adds to wagon
        {
            if (mOwner != null)
            {
                mOwner.GetComponent<PlayerDataHolder>().mPossessions.Remove(this);
                mOnRoof = mOwner.GetComponent<PlayerDataHolder>().mOnRoof;
                mOwner.GetComponent<PlayerDataHolder>().score -= mValue;           //Scoreboard
                mOwner = null;
            }
            mLocation = dest;
            dest.GetComponent<Wagon>().addItem(this);
            button.SetActive(true);
        }
    }
}