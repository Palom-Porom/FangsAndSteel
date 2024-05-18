using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class СhangingTextUI : MonoBehaviour
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

        bool EventIsTrue = true;
        if (countclick == 1)
        {
            text.text = "Для движения камерой используется сочетание клавиш WASD. \nТак, если Вы нажмете на: \nW, то камера сдвинется вперед\nS, то камера начнет движение назад \nA, то камера будет двигаться влево \nD, то камера сдвинется вправо \nчтобы приблизить поле боя - крутите колесико мышки от себя \nчтобы отдалить - наборот, крутите колесико мышки на себя.";
            textButton.text = "Окей";
        }
        if (countclick == 2)
        { text.text = "Eсли Вы хотите узнать где находятся отряды противника, Вы можете нажать на кнопку \"Показать зону с противниками\". Зная вражеские позиции Вам, бесспорно, легче отправить юнитов к противнику навстречу, для начала срежения. Cоветую так и сделать. \nДля этого выбирите любого Вашего юнита (нажав на него левой кнопкой мыши). Далее нажмите ПКМ по позиции, в которую Вы бы хотели отправить робота."; }
        if (countclick == 3) { text.text = " "; }
        if ((countclick == 4) && (EventIsTrue)) { text.text = "  "; }
        if (countclick == 5) { text.text = " "; }
        if (countclick == 6) { text.text = " "; }
        if (countclick == 7) { text.text = " "; }
        if (countclick == 8) { text.text = " "; }
        if (countclick == 9) { text.text = " "; }
        if (countclick == 10) { text.text = " "; }
        
    }
    /*
    /// <summary>
    /// Функция для затемнения всей области кроме той, где есть вражеские юниты 
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
