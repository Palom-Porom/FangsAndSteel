using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeForEndPursuitInput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private string input;
    public void ReadTimeInput(string s)
    {
        input = s;
        Debug.Log(input);
    }
}

