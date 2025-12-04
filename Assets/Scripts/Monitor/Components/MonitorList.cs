using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class MonitorList : MonoBehaviour
{

    public GameObject text;

    public void Render(List<UIMonitorFactory.ListItem> data, RectTransform list, UIMonitorFactory.ListMode mode = UIMonitorFactory.ListMode.Default)
    {
        foreach (Transform child in list)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in data)
        {
            GameObject listItemObject = new("ListItem");
            listItemObject.transform.SetParent(list);
            RectTransform listItemRect = listItemObject.AddComponent<RectTransform>();
            listItemRect.localScale = new Vector3(1f, 1f, 1f);
            listItemRect.sizeDelta = new Vector2(958, mode == UIMonitorFactory.ListMode.Medium ? 128 : 64);

            HorizontalLayoutGroup listItemLayout = listItemObject.AddComponent<HorizontalLayoutGroup>();
            listItemLayout.childControlHeight = false;
            listItemLayout.childControlWidth = false;
            listItemLayout.childForceExpandHeight = false;
            listItemLayout.childAlignment = TextAnchor.UpperLeft;
            listItemLayout.spacing = 16f;

            GameObject label = Instantiate(text, listItemRect);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(466, mode == UIMonitorFactory.ListMode.Medium ? 128 : 64);

            TMP_Text labelText = label.GetComponent<TMP_Text>();
            labelText.text = item.label;

            labelText.fontSize = mode switch
            {
                UIMonitorFactory.ListMode.Default => 48f,
                UIMonitorFactory.ListMode.Medium => 64f,
                UIMonitorFactory.ListMode.Large => 72f,
                _ => 48f,
            };

            GameObject value = Instantiate(text, listItemRect);
            RectTransform valueRect = value.GetComponent<RectTransform>();
            valueRect.sizeDelta = new Vector2(466, mode == UIMonitorFactory.ListMode.Medium ? 128 : 64);

            TMP_Text valueText = value.GetComponent<TMP_Text>();
            valueText.text = item.value;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.fontSize = mode switch
            {
                UIMonitorFactory.ListMode.Default => 64f,
                UIMonitorFactory.ListMode.Medium => 96f,
                UIMonitorFactory.ListMode.Large => 96f,
                _ => 64f,
            };
        }
    }

}
