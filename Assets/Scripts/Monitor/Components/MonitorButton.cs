using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LocalEventManager))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class MonitorButton : MonoBehaviour
{

    private LocalEventManager events;

    private Image background;
    private TMP_Text text;
    private Button current;

    Color primaryColor = Color.black;
    Color secondaryColor = new(172f / 255f, 16f / 255f, 16f / 255f);

    void Awake()
    {
        events = GetComponent<LocalEventManager>();
        background = GetComponent<Image>();
        text = GetComponentInChildren<TMP_Text>();
        current = GetComponent<Button>();

        events.Subscribe("OnFocus", OnFocus);
        events.Subscribe("OnBlur", OnBlur);
    }

    public void OnFocus()
    {
        background.color = secondaryColor;
        text.color = primaryColor;
    }

    public void OnBlur()
    {
        background.color = primaryColor;
        text.color = secondaryColor;
    }

    public void ResizeButton(Button button)
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        RectTransform rect = button.GetComponent<RectTransform>();

        Vector2 textSize = new(text.preferredWidth, text.preferredHeight);

        float paddingX = 5f;

        rect.sizeDelta = new Vector2(textSize.x + paddingX, text.fontSize + 5f);
    }
    
    public void SetTitle(string title)
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>();
        text.text = title;

        if (current == null) current = GetComponent<Button>();
        ResizeButton(current);
    }
}
