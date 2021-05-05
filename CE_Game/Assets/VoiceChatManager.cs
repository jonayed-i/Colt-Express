using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Voice.Unity;

public class VoiceChatManager : MonoBehaviour
{
    public Button VCToggle;
    public Text VCStateIndicator;
    public bool CurrentVCState = true;
    public Recorder Mic;

    private void Start() {
        VCStateIndicator.text = "Mic On";
        if (VCToggle != null) {
            VCToggle.onClick.AddListener(() => {
                UpdateVCState(!CurrentVCState);
            });
        }
    }

    public void UpdateVCState(bool isEnabled) {
        
        if (isEnabled) {
            Mic.TransmitEnabled = true;
            VCStateIndicator.text = "Mic On";
        } else {
            Mic.TransmitEnabled = false;
            VCStateIndicator.text = "Mic Off";
        }

        CurrentVCState = !CurrentVCState;
    }

    private void OnDestroy() {
        if (VCToggle != null) {
            VCToggle.onClick.RemoveAllListeners();
        }
        
    }
}
