using UnityEngine;

public class AudioManager : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        DontDestroyOnLoad(this.gameObject);
    }
}
