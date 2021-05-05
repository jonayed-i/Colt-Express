using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TrainPassenger : MonoBehaviour {

    [Header("TrainInformation")]
    public GameObject PinPoint;

    [PunRPC]
    public void UpdatePosition(string wagonName, string pinPointName) {
        GameObject wagon = GameObject.Find(wagonName);
        GameObject pinpoint = wagon.transform.Find(pinPointName).gameObject;
        if (pinpoint != null) {
            this.PinPoint = pinpoint;
        }

        pinpoint.GetComponent<PinPointInfo>().PinPointTaker = this.gameObject;

        if (this.CompareTag("Player"))
        {
            PlayerDataHolder currentData = this.GetComponent<PlayerDataHolder>();
            GameManager GM = GameObject.Find("GameManager").GetComponent<GameManager>();

            GM.mLocomotives[currentData.mLocomotiveLocation].removePlayer(gameObject);

            if (wagonName.Equals("Locomotive"))
            {
                currentData.mLocomotiveLocation = 4;
            }
            else if (wagonName.Equals("Wagon"))
            {
                currentData.mLocomotiveLocation = 3;
            }
            else if (wagonName.Equals("Wagon (1)"))
            {
                currentData.mLocomotiveLocation = 2;
            }
            else if (wagonName.Equals("Wagon (2)"))
            {
                currentData.mLocomotiveLocation = 1;
            }
            else if (wagonName.Equals("Wagon (3)"))
            {
                currentData.mLocomotiveLocation = 0;
            }
            else
            {
                Debug.LogWarning("Something went wrong with UpdatePosition");
            }

            currentData.mOnRoof = pinPointName.Contains("Up");
            wagon.GetComponent<Wagon>().addPlayer(gameObject, currentData.mOnRoof);
        }
    }
   

    private void Update() {
        if (PinPoint != null)
            this.transform.position = PinPoint.transform.position;
    }
}
