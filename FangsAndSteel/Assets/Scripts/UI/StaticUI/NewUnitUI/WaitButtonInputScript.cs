using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitButtonInputScript : MonoBehaviour
{
    private string input;
    public void ReadTimeInput(string s)
    {
        input = s;
        Debug.Log(input);
    }
}

