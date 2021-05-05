using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CardManager {
    [HideInInspector]
    public List<Card> mDrawingDeck = new List<Card>(); // specific to player
    public List<Card> mBulletCard = new List<Card>();

    public List<Card> mPlayerHand = new List<Card>();
    public List<Card> mScheminPile = new List<Card>();
    public List<Card> mActionPile = new List<Card>();
    public string myplayer; 


    public CardManager(string Player) {
        myplayer = Player;
        initialize(Player);
        
    }

    
    
    public void initialize(string p) { //Call initialize to get fresh cards for firstround

        Card h1 = new Card("move_hori", p, 1);
        Card h2 = new Card("move_hori", p, 1);

        Card c1 = new Card("move_vert", p, 2);
        Card c2 = new Card("move_vert", p, 2);

        Card l1 = new Card("steal", p, 3);
        Card l2 = new Card("steal", p, 3);

        Card s1 = new Card("shoot", p, 4);
        Card s2 = new Card("shoot", p, 4);

        Card p1 = new Card("punch", p, 5);

        Card m1 = new Card("marshall", p, 6);

        for (int i = 0; i < 6; i++) //Michael's addition 4.4.2021
        {
            Card bullet = new Card("bullet", p, 7);
            mBulletCard.Add(bullet);
        }


        mDrawingDeck.Add(m1);
        mDrawingDeck.Add(p1);
        mDrawingDeck.Add(s1);
        mDrawingDeck.Add(s2);
        mDrawingDeck.Add(l1);
        mDrawingDeck.Add(l2);
        mDrawingDeck.Add(c1);
        mDrawingDeck.Add(c2);
        mDrawingDeck.Add(h1);
        mDrawingDeck.Add(h2);
        mDrawingDeck.Add(m1);
        mDrawingDeck.Add(p1);
        mDrawingDeck.Add(s1);
        mDrawingDeck.Add(l1);
        mDrawingDeck.Add(c1);
        mDrawingDeck.Add(h1);
        shuffleCards();
        if (p == "Doc")
        {
            drawCards(7);
        }
        else
        {
            drawCards(6);
        }
    }

    
    
    public void reinitialize() //Call reinitialize to get fresh cards for subsequent round
    {
        List<Card> bulletsInHand = new List<Card>();
        String p = myplayer;
        if (mPlayerHand.Count > 0)
        {
            for (int i = 0; i < mPlayerHand.Count; i++)
            {
                if (mPlayerHand[i].action == 7) //keep bulletcards for next round
                {
                    bulletsInHand.Add(mPlayerHand[i]);
                }

            }
        }

        if (mDrawingDeck.Count > 0)
            {
                for (int i = 0; i < mDrawingDeck.Count; i++)  
                {
                    if (mDrawingDeck[i].action == 7) //keep bulletcards for next round
                    {
                        bulletsInHand.Add(mDrawingDeck[i]);
                    }
                
                }
        }
        mDrawingDeck.Clear();
        mPlayerHand.Clear();
        if (bulletsInHand.Count > 0)
        {
            for (int i = 0; i < bulletsInHand.Count; i++)  
            {
                mDrawingDeck.Add(bulletsInHand[i]);
                
            }
        }

        

        Card h1 = new Card("move_hori", p, 1);
        Card h2 = new Card("move_hori", p, 1);
        Card h3 = new Card("move_hori", p, 1);

        Card c1 = new Card("move_vert", p, 2);
        Card c2 = new Card("move_vert", p, 2);
        Card c3 = new Card("move_vert", p, 2);

        Card l1 = new Card("steal", p, 3);
        Card l2 = new Card("steal", p, 3);
        Card l3 = new Card("steal", p, 3);

        Card s1 = new Card("shoot", p, 4);
        Card s2 = new Card("shoot", p, 4);
        Card s3 = new Card("shoot", p, 4);

        Card p1 = new Card("punch", p, 5);
        Card p2 = new Card("punch", p, 5);

        Card m1 = new Card("marshall", p, 6);
        Card m2 = new Card("marshall", p, 6);


        mDrawingDeck.Add(m1);
        //mDrawingDeck.Add(p1);
        mDrawingDeck.Add(s1);
        mDrawingDeck.Add(s2);
        mDrawingDeck.Add(l1);
        mDrawingDeck.Add(l2);
        mDrawingDeck.Add(c1);
        mDrawingDeck.Add(c2);
        mDrawingDeck.Add(h1);
        mDrawingDeck.Add(h2);
        mDrawingDeck.Add(m1);
        //mDrawingDeck.Add(p1);
        mDrawingDeck.Add(s1);
        mDrawingDeck.Add(l1);
        mDrawingDeck.Add(c1);
        mDrawingDeck.Add(h1);
        mDrawingDeck.Add(s1);
        mDrawingDeck.Add(l1);
        mDrawingDeck.Add(c1);
        mDrawingDeck.Add(h1);
        
        
        shuffleCards();
        
        if (p == "Doc")
        {
            drawCards(7);
        }
        else
        {
            drawCards(6);
        }
    }
    public void drawCards(int count) {
        for (int i = 0; i < count; i++) {
            mPlayerHand.Add(mDrawingDeck[0]);
            //Debug.Log(mDrawingDeck[0]);
            mDrawingDeck.RemoveAt(0);

        }
        //Debug.Log(mDrawingDeck);
    }

    public void shuffleCards() {
        var shuffledcards = mDrawingDeck.OrderBy(a => Guid.NewGuid()).ToList();
        mDrawingDeck = shuffledcards;
    }
    public void receivebulletCard(string owner) {
        Card bullet = new Card("bullet", owner, 7);
        mDrawingDeck.Add(bullet);
    }

    public void receiveCard(int pile, Card c)
    {
        if (pile == 0)
        {
            mDrawingDeck.Add(c);
        }
        else if (pile == 1)
        {
            mPlayerHand.Add(c);
        }
    }

    public bool removeBulletCard() //returns false if bullet deck is empty
    {
        if (mBulletCard.Count == 0)
        {
            return false;
        }
        else
        {
            mBulletCard.RemoveAt(0);
            return true;
        }
    }

    public Card playCard(Card c) {
        mPlayerHand.Remove(c);
        return c;
    }

}

