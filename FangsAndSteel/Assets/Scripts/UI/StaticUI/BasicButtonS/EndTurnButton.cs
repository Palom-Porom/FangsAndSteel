using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    Button button;

    private void OnClick()
    {
        StaticUIRefs.Instance.endTurnBut = true;
        //Debug.Log("EndTurn Button was clicked");
    }

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }
}
