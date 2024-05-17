using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class GetBuyBordersScript : MonoBehaviour
{
    //public Transform RedBordersParent;
    //public Transform BlueBordersParent;

    private void Awake()
    {
        //RedBordersParent = transform.GetChild(0);
        //BlueBordersParent = transform.GetChild(1);
    }

    public float GetRightBorder(int team) { return transform.GetChild(team - 1).GetChild(0).position.x; }
    public float GetTopBorder(int team) { return transform.GetChild(team - 1).GetChild(1).position.z; }
    public float GetBottomBorder(int team) { return transform.GetChild(team - 1).GetChild(2).position.z; }
    public float GetLeftBorder(int team) { return transform.GetChild(team - 1).GetChild(3).position.x; }

    public float[] GetAllBorders(int team) { return new float[] { GetRightBorder(team), GetTopBorder(team), GetBottomBorder(team), GetLeftBorder(team) }; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
