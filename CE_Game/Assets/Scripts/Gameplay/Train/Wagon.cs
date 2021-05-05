using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wagon : MonoBehaviour
{
    public List<GameItem> mItems;
    public List<GameObject> mPlayersInside;
    public List<GameObject> mPlayersOnRoof;
    public int mPosition;
    public GameObject wagonGameObject;

    [Header("PinPoints")]
    public List<GameObject> PinPoints;

    public Wagon(int pos)
    {
        mPosition = pos;
        mItems = new List<GameItem>();
        mPlayersInside = new List<GameObject>();
        mPlayersOnRoof = new List<GameObject>();
    }

    public void resetWagon(Wagon w) //For syncing emergencies
    {
        mItems = w.mItems;
        mPlayersInside = w.mPlayersInside;
        mPlayersOnRoof = w.mPlayersOnRoof;
        mPosition = w.mPosition;
        wagonGameObject = w.wagonGameObject;
    }

    public void addItem(GameItem toAdd)
    {
        mItems.Add(toAdd);
    }

    public bool removeItem(GameItem toRemove)
    {
        return mItems.Remove(toRemove);
    }

    public bool removePlayer(GameObject toRemove)
    {
        if (toRemove.GetComponent<PlayerDataHolder>().mOnRoof)
        {
            return mPlayersOnRoof.Remove(toRemove);
        }
        else
        {
            return mPlayersInside.Remove(toRemove);
        }
    }

    public void addPlayer(GameObject toAdd, bool onRoof)
    {
        if (onRoof)
        {
            mPlayersOnRoof.Add(toAdd);
        }
        else
        {
            mPlayersInside.Add(toAdd);
        }
    }

    public void findFreePinPoint() {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject p in Players) {
            if (p.GetComponent<TrainPassenger>() != null) {
                
            } else {
                Debug.Log("Ill formed Player: no train passenger found when trying to find a free pinpoint in " + this.gameObject);
            }
        }
    }
}