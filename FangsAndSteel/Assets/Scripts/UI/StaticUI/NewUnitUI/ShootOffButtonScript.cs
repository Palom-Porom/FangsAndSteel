using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShootOffButtonScript : MonoBehaviour
{
    Button ShootOffButton;
    public void ClickShootOffButton()
    {
        if (ShootOffButton.image.color == Color.green)
        { ShootOffButton.image.color = Color.red; }
        else if (ShootOffButton.image.color == Color.red)
        { ShootOffButton.image.color = Color.green; }
    }
    private void Awake()
    {
        ShootOffButton = GetComponent<Button>();
        ShootOffButton.onClick.AddListener(ClickShootOffButton);

    }
    private void OnDestroy()
    {
        ShootOffButton.onClick.RemoveListener(ClickShootOffButton);
    }
}
