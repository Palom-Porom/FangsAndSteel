using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class PriorityDropdownScript : MonoBehaviour
{
    public TextMeshProUGUI TextBox1;
    public TMP_Dropdown dropdown1;

    public TextMeshProUGUI TextBox2;
    public TMP_Dropdown dropdown2;

    public TextMeshProUGUI TextBox3;
    public TMP_Dropdown dropdown3;

    public TextMeshProUGUI TextBox4;
    public TMP_Dropdown dropdown4;

    public TextMeshProUGUI TextBox5;
    public TMP_Dropdown dropdown5;

    public TextMeshProUGUI TextBox6;
    public TMP_Dropdown dropdown6;

    public TextMeshProUGUI TextBox7;
    public TMP_Dropdown dropdown7;

    public void Start()
    {
        //dropdown1.options.Clear();
        //dropdown2.options.Clear();
        //dropdown3.options.Clear();
        List<string> priorityList = new List<string>();
        priorityList.Add("Ближайший");
        priorityList.Add("По мин. HP");
        priorityList.Add("Пехотинец");
        priorityList.Add("Противотанковый пехотинец");
        priorityList.Add("Танк");
        priorityList.Add("Aртиллерия");
        priorityList.Add(" ");

        foreach (var priority in priorityList)
        {
            dropdown1.options.Add(new TMP_Dropdown.OptionData() { text = priority });
            dropdown2.options.Add(new TMP_Dropdown.OptionData() { text = priority });
            dropdown3.options.Add(new TMP_Dropdown.OptionData() { text = priority });
            dropdown4.options.Add(new TMP_Dropdown.OptionData() { text = priority });
            dropdown5.options.Add(new TMP_Dropdown.OptionData() { text = priority });
            dropdown6.options.Add(new TMP_Dropdown.OptionData() { text = priority });
            dropdown7.options.Add(new TMP_Dropdown.OptionData() { text = priority });
        }
        
        dropdown1.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown1); });
        dropdown1.value = 0;
        DropdownItemSelected(dropdown1);
        
        dropdown2.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown2); });
        dropdown2.value = 6;
        DropdownItemSelected(dropdown2);
        
        dropdown3.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown3); });
        dropdown3.value = 6;
        DropdownItemSelected(dropdown3);

        dropdown4.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown4); });
        dropdown4.value = 6;
        DropdownItemSelected(dropdown4);

        dropdown5.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown5); });
        dropdown5.value = 6;
        DropdownItemSelected(dropdown5);

        dropdown6.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown6); });
        dropdown6.value = 6;
        DropdownItemSelected(dropdown6);

        dropdown7.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown7); });
        dropdown7.value = 6;
        DropdownItemSelected(dropdown7);
    }

    void DropdownItemSelected(TMP_Dropdown dropdown)
    {
        int index = dropdown.value;
        if (dropdown == dropdown1)
        {
            TextBox1.text = dropdown.options[index].text;
        }
        else if (dropdown == dropdown2)
        {
            if (index == dropdown1.value)
            {
                dropdown1.value = 6;
                DropdownItemSelected(dropdown1);
            }
            TextBox2.text = dropdown.options[index].text;
        }
        else if (dropdown == dropdown3)
        {
            if (index == dropdown1.value)
            {
                dropdown1.value = 6;
                DropdownItemSelected(dropdown1);
            }
            if (index == dropdown2.value)
            {
                dropdown2.value = 6;
                DropdownItemSelected(dropdown2);
            }
            TextBox3.text = dropdown.options[index].text;
        }
        else if (dropdown == dropdown4)
        {
            if (index == dropdown1.value)
            {
                dropdown1.value = 6;
                DropdownItemSelected(dropdown1);
            }
            if (index == dropdown2.value)
            {
                dropdown2.value = 6;
                DropdownItemSelected(dropdown2);
            }
            if (index == dropdown3.value)
            {
                dropdown3.value = 6;
                DropdownItemSelected(dropdown3);
            }
            TextBox4.text = dropdown.options[index].text;
        }

        else if (dropdown == dropdown5)
        {
            if (index == dropdown1.value)
            {
                dropdown1.value = 6;
                DropdownItemSelected(dropdown1);
            }
            if (index == dropdown2.value)
            {
                dropdown2.value = 6;
                DropdownItemSelected(dropdown2);
            }
            if (index == dropdown3.value)
            {
                dropdown3.value = 6;
                DropdownItemSelected(dropdown3);
            }
            if (index == dropdown4.value)
            {
                dropdown4.value = 6;
                DropdownItemSelected(dropdown4);
            }
            TextBox5.text = dropdown.options[index].text;
        }

        else if (dropdown == dropdown6)
        {

            if (index == dropdown1.value)
            {
                dropdown1.value = 6;
                DropdownItemSelected(dropdown1);
            }
            if (index == dropdown2.value)
            {
                dropdown2.value = 6;
                DropdownItemSelected(dropdown2);
            }
            if (index == dropdown3.value)
            {
                dropdown3.value = 6;
                DropdownItemSelected(dropdown3);
            }
            if (index == dropdown4.value)
            {
                dropdown4.value = 6;
                DropdownItemSelected(dropdown4);
            }
            if (index == dropdown5.value)
            {
                dropdown5.value = 6;
                DropdownItemSelected(dropdown5);
            }
            TextBox6.text = dropdown.options[index].text;
        }

        else if (dropdown == dropdown7)
        {
            if (index == dropdown1.value)
            {
                dropdown1.value = 6;
                DropdownItemSelected(dropdown1);
            }
            if (index == dropdown2.value)
            {
                dropdown2.value = 6;
                DropdownItemSelected(dropdown2);
            }
            if (index == dropdown3.value)
            {
                dropdown3.value = 6;
                DropdownItemSelected(dropdown3);
            }
            if (index == dropdown4.value)
            {
                dropdown4.value = 6;
                DropdownItemSelected(dropdown4);
            }
            if (index == dropdown5.value)
            {
                dropdown5.value = 6;
                DropdownItemSelected(dropdown5);
            }
            if (index == dropdown6.value) 
            { 
                dropdown6.value = 6;
            }
            TextBox7.text = dropdown.options[index].text;
        }
       
    }

   


}
