using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Priority3ButtonScripts : MonoBehaviour
{
    Button Priority3Button;
    public GameObject dropdown3;

    public void Priority3ButtonClick()
    {
        dropdown3.SetActive(!dropdown3.activeSelf);
    }
    private void Awake()
    {
        Priority3Button = GetComponent<Button>();
        Priority3Button.onClick.AddListener(Priority3ButtonClick);
    }

    private void OnDestroy()
    {
        Priority3Button.onClick.RemoveListener(Priority3ButtonClick);

    }
}
