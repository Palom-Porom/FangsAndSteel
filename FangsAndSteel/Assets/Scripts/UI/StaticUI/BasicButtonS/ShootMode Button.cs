using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootModeButton : MonoBehaviour
{
    Button button;
    public void ShootMode()
    {
        StaticUIRefs.Instance.shootModeBut = true;
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ShootMode);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(ShootMode);
    }


}
