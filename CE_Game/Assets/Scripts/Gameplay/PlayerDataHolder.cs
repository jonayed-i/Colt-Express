using System;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataHolder : MonoBehaviour
{
    
    public bool mAnotherAction = false;
    public int mLocomotiveLocation;
    public bool mOnRoof;
    public List<GameItem> mPossessions;
    public CardManager MycardManager;
    public int score;
    public int bulletsShot;

    public int getLocation() {
        return 0;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();
            manager.instantiateItem("Purse", gameObject, 0);
            bulletsShot = 0;
            score = 250;
        }
    }
}
