using TMPro;
using UnityEngine;

public class MonitorHeader : MonoBehaviour
{
    public GameObject title;
    public GameObject infoName;
    public GameObject infoValue;

    public GameObject Title()
    {
        return title;
    }

    public GameObject InfoName()
    {
        return infoName;
    }

    public GameObject InfoValue()
    {
        return infoValue;
    }

    public void SetValue(string value)
    {
        TMP_Text item = infoValue.GetComponent<TMP_Text>();
        item.text = value;
    }
}
