using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeForEndPursuitInput : MonoBehaviour
{
    private int input;
    private TMP_InputField inputField;
    public bool isCosmeticChangeOfValue = false;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();    
    }

    public void ReadTimeInput(string s)
    {
        //Debug.Log(inputField.text);
        //Debug.Log(s);
        if (int.TryParse(s, out input) && input < 100000 && input > 0)
        {
            //if (isCosmeticChangeOfValue) { isCosmeticChangeOfValue = false; return; }
            //StaticUIRefs.Instance.newPursuitTimeForEnd = input;
            //Debug.Log("1");
        }
        else
        {
            //Debug.Log("2");
            inputField.text = "1";
            input = 1;
        }
        //Debug.Log(input);
        //if (isCosmeticChangeOfValue) { isCosmeticChangeOfValue = false; return; }
        StaticUIRefs.Instance.newPursuitTimeForEnd = input;
        //Debug.Log(StaticUIRefs.Instance.newPursuitTimeForEnd);

        //Debug.Log(input);
    }
}

