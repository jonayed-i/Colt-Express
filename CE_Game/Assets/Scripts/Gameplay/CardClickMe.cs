using Photon.Pun;
using UnityEngine;

public class CardClickMe : MonoBehaviour {
    // Start is called before the first frame update


    private void OnMouseDown() {
        Debug.Log("this was selected yaaa");

        string name = this.GetComponent<CardDescription>().name;
        string owner = this.GetComponent<CardDescription>().owner;
        int action = this.GetComponent<CardDescription>().action;
        bool isHidden = this.GetComponent<CardDescription>().ishidden;

        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.playCard(name, action, owner, isHidden);
        Destroy(gameObject);
    }

    [PunRPC]
    public void hide() {
        gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
        Debug.Log("Card set to hidden");
    }

    [PunRPC]
    public void unhide() {
        gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
        Debug.Log("Card set to visible");
    }

    [PunRPC]
    public void updatePosition() {
        GameObject pinpoint = GameObject.Find("stealinpile");
        gameObject.transform.position = pinpoint.transform.position;
        Debug.Log("Card moved");
    }
}
