using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSpeedButton : MonoBehaviour
{
    Button button;
    public void ChangeSpeed()
    {
        StaticUIRefs.Instance.changeSpeedBut = true;
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ChangeSpeed);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(ChangeSpeed);
    }


}
