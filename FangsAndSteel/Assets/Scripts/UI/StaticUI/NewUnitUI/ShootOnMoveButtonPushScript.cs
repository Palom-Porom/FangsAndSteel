using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShootOnMoveButtonPushScript : MonoBehaviour
{
    Button ShootOnMoveButton;
    public void ShootOnMoveButtonPush()
    { 
        //if (ShootOnMoveButton.image.color == Color.red) { ShootOnMoveButton.image.color = Color.green; }
        //else if (ShootOnMoveButton.image.color == Color.green) { ShootOnMoveButton.image.color = Color.red; }
        StaticUIRefs.Instance.shootOnMoveBut = true;
    }
    private void Awake()
    {
        ShootOnMoveButton = GetComponent<Button>();
        ShootOnMoveButton.onClick.AddListener(ShootOnMoveButtonPush);
    }

    private void OnDestroy()
    {
        ShootOnMoveButton.onClick.RemoveListener(ShootOnMoveButtonPush);
    }
}
