using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ResumeButtonScript : MonoBehaviour
{
    Button button;
    private EscButtonPush escButtonPushScript;
    
    private void Awake()
    {
        escButtonPushScript = StaticUIRefs.Instance.gameObject.GetComponent<EscButtonPush>();
        button = GetComponent<Button>();
        button.onClick.AddListener(escButtonPushScript.OpenCloseMenu);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(escButtonPushScript.OpenCloseMenu);
    }
}
