using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class MonitorList : MonoBehaviour
{

    public GameObject text;
    public RawImage radiationIcon;
    public RawImage celsiusIcon;

    public enum ListItemType
    {
        Text,
        Radiation,
        Celsius
    }

    public class ListItem
    {
        public string label;
        public string value;
        public ListItemType type = ListItemType.Text;
    }

    public enum ListMode
    {
        Default,
        Medium,
        MediumLarge,
        Large
    }

    public void Render(List<ListItem> data, RectTransform list, ListMode mode = ListMode.Default)
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
            listItemRect.sizeDelta = new Vector2(
                mode == ListMode.MediumLarge ? 1650 : 958, 
                mode == ListMode.Medium || mode == ListMode.MediumLarge ? 128 : 64
            );

            HorizontalLayoutGroup listItemLayout = listItemObject.AddComponent<HorizontalLayoutGroup>();
            listItemLayout.childControlHeight = false;
            listItemLayout.childControlWidth = false;
            listItemLayout.childForceExpandHeight = false;
            listItemLayout.childAlignment = TextAnchor.MiddleCenter;
            listItemLayout.spacing = 16f;

            GameObject label = Instantiate(text, listItemRect);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(mode == ListMode.MediumLarge ? 566 : 466, 64);

            TMP_Text labelText = label.GetComponent<TMP_Text>();
            labelText.text = item.label;

            labelText.fontSize = mode switch
            {
                ListMode.Default => 48f,
                ListMode.Medium => 64f,
                ListMode.MediumLarge => 64f,
                ListMode.Large => 72f,
                _ => 48f,
            };

            GameObject valueObject = new("Value");
            if(item.type == ListItemType.Text) Destroy(valueObject);
            else
            {
                RectTransform rect = valueObject.AddComponent<RectTransform>();
                valueObject.transform.SetParent(listItemRect);
                rect.localScale = new Vector3(1, 1, 1);
            }

            GameObject value = item.type switch
            {
                ListItemType.Text => Instantiate(text, listItemRect),
                ListItemType.Radiation => valueObject,
                ListItemType.Celsius => valueObject,
                _ => Instantiate(text, listItemRect)
            };

            RectTransform valueRect = value.GetComponent<RectTransform>();
            valueRect.sizeDelta = new Vector2(mode == ListMode.MediumLarge ? 566 : 466, mode == ListMode.Medium || mode == ListMode.MediumLarge ? 128 : 64);
            valueRect.transform.SetParent(listItemRect);

            TMP_Text valueText;
            if(item.type != ListItemType.Text)
            {
                HorizontalLayoutGroup valueLayout = value.AddComponent<HorizontalLayoutGroup>();
                valueLayout.childControlWidth = false;
                valueLayout.childControlHeight = false;
                valueLayout.childAlignment = TextAnchor.MiddleRight;
                GameObject valueElement = Instantiate(text, valueRect);
                valueElement.GetComponent<RectTransform>().sizeDelta = new Vector2(305, 64f);
                valueText = valueElement.GetComponent<TMP_Text>();
                
                if(item.type == ListItemType.Radiation) Instantiate(radiationIcon, valueRect);
                if(item.type == ListItemType.Celsius) Instantiate(celsiusIcon, valueRect);
            } else
            {
                valueText = value.GetComponent<TMP_Text>();
            }
            
            valueText.text = item.value;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.fontSize = mode switch
            {
                ListMode.Default => 64f,
                ListMode.Medium => 96f,
                ListMode.MediumLarge => 96f,
                ListMode.Large => 96f,
                _ => 64f,
            };
        }
    }

}
