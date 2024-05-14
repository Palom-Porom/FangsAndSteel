using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ScreenResolutionDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public void Awake()
    {
        MenuGameResolutionCompound(dropdown);
    }
    public static void ChangeResolution(TMP_Dropdown dropdown)
    {
        var s = dropdown.options[dropdown.value].text;
        //Debug.Log(Regex.Match(s, @"(\d{3,4})x").Groups[1].ToString());
        //Debug.Log(Regex.Match(s, @"x(\d{3,4})").Groups[1].ToString());
        var s1 = int.Parse(Regex.Match(s, @"(\d{3,4})x").Groups[1].ToString());
        var s2 = int.Parse(Regex.Match(s, @"x(\d{3,4})").Groups[1].ToString());
        Screen.SetResolution(s1,s2,Screen.fullScreen);
    }

    public static void MenuGameResolutionCompound(TMP_Dropdown dropdown)
    {
        var s = $"{Screen.currentResolution.width}" + "x" + $"{Screen.currentResolution.height}";
        var i = 0;
        foreach (var op in dropdown.options)
        {
            if (s == op.text)
            {
                dropdown.value = i;
                break;
            }
            i++;
        }
    }
}
