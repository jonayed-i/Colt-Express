using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    [HideInInspector]
    public string mBandit;
    public string mName;
    public CardManager mActionCardManager; // action card : move, shoot ... etc
    public bool mIsWaitingForInput;
    public int mLocomotiveLocation;
    public bool mOnRoof;
    public bool hasAnotherAction;
    public List<GameItem> mItems;
    public int score;
    public int bulletsShot;

    public Player(string name, string bandit)
    {
        this.mName = name;
        this.mBandit = bandit;
        this.mItems = new List<GameItem>();
        this.bulletsShot = 0;
        this.score = 250;
    }
    public void addToPossession(GameItem toAdd)
    {
        mItems.Add(toAdd);
    }
}
