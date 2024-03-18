using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Priority1ButtonScript : MonoBehaviour
{
    public Button Priority1Button;
    public Button Priority2Button;
    public Button Priority3Button;
    public Button Priority4Button;
    public Button Priority5Button;
    public Button Priority6Button;
    public Button Priority7Button;
    public void PriorityButtonClick(Button PriorityButton)
    {
        var dropdown = PriorityButton.transform.GetChild(1).gameObject;
        dropdown.SetActive(!dropdown.activeSelf);
    }
    private void Awake()
    {
        Priority1Button.onClick.AddListener(delegate { PriorityButtonClick(Priority1Button); });
        Priority2Button.onClick.AddListener(delegate { PriorityButtonClick(Priority2Button); });
        Priority3Button.onClick.AddListener(delegate { PriorityButtonClick(Priority3Button); });
        Priority4Button.onClick.AddListener(delegate { PriorityButtonClick(Priority4Button); });
        Priority5Button.onClick.AddListener(delegate { PriorityButtonClick(Priority5Button); });
        Priority6Button.onClick.AddListener(delegate { PriorityButtonClick(Priority6Button); });
        Priority7Button.onClick.AddListener(delegate { PriorityButtonClick(Priority7Button); });
    }

    private void OnDestroy()
    {
        Priority1Button.onClick.RemoveAllListeners();
        Priority2Button.onClick.RemoveAllListeners();
        Priority3Button.onClick.RemoveAllListeners();
        Priority4Button.onClick.RemoveAllListeners();
        Priority5Button.onClick.RemoveAllListeners();
        Priority6Button.onClick.RemoveAllListeners();
        Priority7Button.onClick.RemoveAllListeners();
    }
}
