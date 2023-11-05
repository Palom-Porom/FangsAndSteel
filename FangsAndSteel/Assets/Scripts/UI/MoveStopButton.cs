using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveStopButton : MonoBehaviour
{
    Button button;
    public void StopMove()
    {
        StaticUIRefs.Instance.stopMoveBut = true;
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StopMove);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(StopMove);
    }

}


