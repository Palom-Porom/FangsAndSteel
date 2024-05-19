using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenCompund : MonoBehaviour
{
    public Button fullScreenButton;
    // Start is called before the first frame update
    void Start()
    {
        
            fullScreenButton.transform.GetChild(0).gameObject.SetActive(Screen.fullScreen);
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
