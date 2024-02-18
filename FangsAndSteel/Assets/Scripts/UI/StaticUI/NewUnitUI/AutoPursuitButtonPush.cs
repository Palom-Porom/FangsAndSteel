using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoPursuitButtonPush : MonoBehaviour
{
    Button AutoPursuitButton;
    //Image AutoPursuitButtonColor;
    public void ActivateAutoPursuit()
    {
        if (AutoPursuitButton.image.color == Color.green)
        { AutoPursuitButton.image.color = Color.red; }
        else if (AutoPursuitButton.image.color == Color.red)
        {AutoPursuitButton.image.color = Color.green; }
    }

    private void Awake()
    {
        AutoPursuitButton = GetComponent<Button>();
        AutoPursuitButton.onClick.AddListener(ActivateAutoPursuit);
    }

    private void OnDestroy()
    {
        AutoPursuitButton.onClick.RemoveListener(ActivateAutoPursuit);
    }
}
