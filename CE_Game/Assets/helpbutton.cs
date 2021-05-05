using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson.PunDemos;

public class helpbutton : MonoBehaviour
{
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() => {
            Application.OpenURL("https://drive.google.com/file/d/1ArRhqmLW_Rb3taS30q5bzAVVF_Kwo0JV/view?usp=sharing");
            
        });
    }

    public void goinstructions()
    {
        Application.OpenURL("https://drive.google.com/file/d/1ArRhqmLW_Rb3taS30q5bzAVVF_Kwo0JV/view?usp=sharing");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
