using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class �hangingTextUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI textButton;
    //public GameObject DarkenPanels;
    public int countclick;
    // Start is called before the first frame update
    void Start()
    {
        //DarkenPanels.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonClick()
    {
        countclick++;
        if (countclick == 1)
        {
            text.text = "��� �������� ������� ������������ ��������� ������ WASD. \n���, ���� �� ������� ��: \nW, �� ������ ��������� ������\nS, �� ������ ������ �������� ����� \nA, �� ������ ����� ��������� ����� \nD, �� ������ ��������� ������ \n����� ���������� ���� ��� - ������� �������� ����� �� ���� \n����� �������� - �������, ������� �������� ����� �� ����.";
            textButton.text = "����";
        }
        if (countclick == 2)
        {
            text.text = " "; //StartCoroutine(HighlightingTheEnemy()); }
            if (countclick == 3) { text.text = " "; }
            if (countclick == 4) { text.text = " "; }
            if (countclick == 5) { text.text = " "; }
            if (countclick == 6) { text.text = " "; }
            if (countclick == 7) { text.text = " "; }
            if (countclick == 8) { text.text = " "; }
            if (countclick == 9) { text.text = " "; }
            if (countclick == 10) { text.text = " "; }
        }
    }
    /*
    /// <summary>
    /// ������� ��� ���������� ���� ������� ����� ���, ��� ���� ��������� ����� 
    /// </summary>
    /// <returns></returns>
    private IEnumerator HighlightingTheEnemy()
    {
        DarkenPanels.SetActive(true);
        yield return new WaitForSecondsRealtime(10f);
        DarkenPanels.SetActive(false);
    }
    */
}
