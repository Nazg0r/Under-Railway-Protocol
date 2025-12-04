using TMPro;
using UnityEngine;

public class MonitorTextPopup : MonoBehaviour
{
    public GameObject title;
    public GameObject text;
    public GameObject wrapper;

    public TextMeshProUGUI TitleValue()
    {
        return title.GetComponent<TextMeshProUGUI>();
    }

    public TextMeshProUGUI TextValue()
    {
        return text.GetComponent<TextMeshProUGUI>();
    }

    public void SetTitle(string value = "")
    {
        title.GetComponent<TextMeshProUGUI>().text = value;
    }

    public void SetText(string value = "")
    {
        text.GetComponent<TextMeshProUGUI>().text = value;
    }

    public void SetContent(string titleValue = "", string textValue = "")
    {
        title.GetComponent<TextMeshProUGUI>().text = titleValue;
        text.GetComponent<TextMeshProUGUI>().text = textValue;
    }
}
