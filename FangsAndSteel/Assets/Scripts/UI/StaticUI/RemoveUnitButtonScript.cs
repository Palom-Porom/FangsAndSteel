using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoveUnitButtonScript : MonoBehaviour
{
    Button button;
    
    void RemoveUnitButtonClick()
    {
        StaticUIRefs.Instance.removeUnitButton = true;
    }

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(RemoveUnitButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(RemoveUnitButtonClick);
    }
}
