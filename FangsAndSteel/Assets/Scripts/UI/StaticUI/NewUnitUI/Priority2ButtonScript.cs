using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Priority2ButtonScript : MonoBehaviour
{
    Button Priority2Button;
    public GameObject dropdown2;

    public void Priority2ButtonClick()
    {
        dropdown2.SetActive(!dropdown2.activeSelf);
    }
    private void Awake()
    {
        Priority2Button = GetComponent<Button>();
        Priority2Button.onClick.AddListener(Priority2ButtonClick);
    }

    private void OnDestroy()
    {
        Priority2Button.onClick.RemoveListener(Priority2ButtonClick);

    }
}
