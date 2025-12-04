using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIMonitorFactory
{
    private static UIMonitorFactory _instance;
    public static UIMonitorFactory Instance => _instance ??= new UIMonitorFactory();
    private GameObject header;
    private GameObject loadingPopup;
    private GameObject textPopup;
    private GameObject text;
    private Button button;
    private GameObject counterPrefab;
    private GameObject paginationPrefab;

    public class ListItem
    {
        public string label;
        public string value;
    }

    public class StartPlace
    {
        public RectTransform navigation;
        public RectTransform blocks;
    }

    public class BlocksWithPagination
    {
        public RectTransform wrapper;
        public List<GameObject> blocks;
        public GameObject pagination;
        public MonitorPagination controls;
    }

    public enum ListMode
    {
        Default,
        Medium,
        Large
    }

    public void Init()
    {
        header = Resources.Load<GameObject>("Monitors/Components/Header");
        loadingPopup = Resources.Load<GameObject>("Monitors/Components/LoadingPopup");
        textPopup = Resources.Load<GameObject>("Monitors/Components/TextPopup");
        button = Resources.Load<Button>("Monitors/Components/Button");
        text = Resources.Load<GameObject>("Monitors/Components/Text");
        counterPrefab = Resources.Load<GameObject>("Monitors/Components/Counter");
        paginationPrefab = Resources.Load<GameObject>("Monitors/Components/Pagination");
    }

    public MonitorHeader Header(string title, string infoName, string infoValue, RectTransform wrapper)
    {
        GameObject headerObject = Object.Instantiate(header, wrapper);

        RectTransform rt = headerObject.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 550);

        MonitorHeader components = headerObject.GetComponent<MonitorHeader>();
        if (components.Title().TryGetComponent<TextMeshProUGUI>(out var titleValue))
        {
            titleValue.text = title;
        }

        if (components.InfoName().TryGetComponent<TextMeshProUGUI>(out var infoNameValue))
        {
            infoNameValue.text = infoName;
        }

        if (components.InfoValue().TryGetComponent<TextMeshProUGUI>(out var infoItemValue))
        {
            infoItemValue.text = infoValue;
        }

        Canvas.ForceUpdateCanvases();

        return components;
    }

    public MonitorTextPopup TextPopup(string title, string text, RectTransform wrapper)
    {
        GameObject textPopupObject = Object.Instantiate(textPopup, wrapper);
        RectTransform rt = textPopupObject.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -12);

        MonitorTextPopup textPopupBlock = textPopupObject.GetComponent<MonitorTextPopup>();
        textPopupBlock.wrapper = textPopupObject;
        textPopupBlock.TitleValue().text = title;
        textPopupBlock.TextValue().text = text;

        Canvas.ForceUpdateCanvases();

        return textPopupBlock;
    }

    public GameObject LoadingPopup(RectTransform wrapper)
    {
        GameObject loadingPopupObject = Object.Instantiate(loadingPopup, wrapper);

        Canvas.ForceUpdateCanvases();
        return loadingPopupObject;
    }

    public Button Button(string title, float fontSize, RectTransform wrapper)
    {
        Button buttonObject = Object.Instantiate(button, wrapper);
        TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>();
        if (fontSize > 0) text.fontSize = fontSize;

        MonitorButton monitorButton = buttonObject.GetComponent<MonitorButton>(); ;
        monitorButton.SetTitle(title);

        Canvas.ForceUpdateCanvases();

        return buttonObject;
    }

    public GameObject Layout(string mode, float spacing, RectTransform wrapper)
    {
        GameObject layout = new("Layout");
        layout.transform.SetParent(wrapper, false);

        if (mode == "horizontal")
        {
            HorizontalLayoutGroup layoutGroup = layout.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.spacing = spacing;
        }
        else if (mode == "vertical")
        {
            VerticalLayoutGroup layoutGroup = layout.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.spacing = spacing;
        }

        Canvas.ForceUpdateCanvases();
        return layout;
    }

    public GameObject Text(string title, float fontSize, RectTransform wrapper)
    {
        GameObject textObject = Object.Instantiate(text, wrapper);
        TMP_Text textValue = textObject.GetComponentInChildren<TMP_Text>();
        textValue.text = title;
        if (fontSize > 0) textValue.fontSize = fontSize;

        Canvas.ForceUpdateCanvases();

        return textObject;
    }

    public GameObject List(List<ListItem> list, RectTransform wrapper, ListMode mode = ListMode.Default)
    {
        GameObject listObject = new("List");
        listObject.transform.SetParent(wrapper, false);
        RectTransform listRect = listObject.AddComponent<RectTransform>();

        float height = mode == ListMode.Medium ? 128f : 64f;
        listRect.sizeDelta = new Vector2(964f, 100f + (height * list.Count) + (16f * list.Count));

        VerticalLayoutGroup listLayout = listObject.AddComponent<VerticalLayoutGroup>();
        listLayout.childControlHeight = false;
        listLayout.childControlWidth = false;
        listLayout.childForceExpandHeight = false;
        listLayout.childAlignment = TextAnchor.UpperLeft;
        listLayout.spacing = 16f;

        MonitorList listFactory = listObject.AddComponent<MonitorList>();
        listFactory.text = text;
        listFactory.Render(list, listRect, mode);

        Canvas.ForceUpdateCanvases();

        return listObject;
    }

    public GameObject Counter(string label, RectTransform wrapper)
    {
        GameObject counterObject = Object.Instantiate(counterPrefab, wrapper);
        MonitorCounter counter = counterObject.GetComponent<MonitorCounter>();
        counter.SetLabel(label);

        return counterObject;
    }

    public StartPlace Start(RectTransform mainRect, float paddingTop = 0f)
    {
        GameObject inner = Instance.Layout("horizontal", 32f, mainRect);
        RectTransform innerRect = inner.GetComponent<RectTransform>();
        innerRect.sizeDelta = new Vector2(1948, 1214 - paddingTop);
        innerRect.anchoredPosition = new Vector2(0, -125 - paddingTop);

        GameObject navigation = Instance.Layout("vertical", 48f, innerRect);
        RectTransform navigationRect = navigation.GetComponent<RectTransform>();
        navigationRect.sizeDelta = new Vector2(628, 974);

        GameObject blocks = Instance.Layout("vertical", 72f, innerRect);
        RectTransform blocksRect = blocks.GetComponent<RectTransform>();
        blocksRect.sizeDelta = new Vector2(1288, 1214);

        VerticalLayoutGroup blocksLayout = blocks.GetComponent<VerticalLayoutGroup>();
        blocksLayout.childControlHeight = true;
        blocksLayout.childForceExpandHeight = true;
        blocksLayout.childControlWidth = true;
        blocksLayout.childForceExpandWidth = true;

        Canvas.ForceUpdateCanvases();

        return new StartPlace
        {
            navigation = navigationRect,
            blocks = blocksRect
        };
    }
    
    public BlocksWithPagination Blocks(int length, RectTransform wrapper)
    {
        List<GameObject> blocks = new();
        GameObject inner = new("Inner");
        RectTransform innerRect = inner.AddComponent<RectTransform>();
        innerRect.SetParent(wrapper);
        innerRect.localScale = new Vector3(1f, 1f, 1f);

        for (int index = 0; index < length; index++)
        {
            GameObject block = Layout("vertical", 16f, innerRect);
            ContentSizeFitter sizeFitter = block.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            block.SetActive(index == 0);
            blocks.Add(block);
        }

        GameObject pagination = Object.Instantiate(paginationPrefab, wrapper);
        MonitorPagination paginationControls = pagination.GetComponent<MonitorPagination>();
        paginationControls.blocksWrapper = innerRect;
        paginationControls.blocksList = blocks;
        paginationControls.Render(length);

        return new BlocksWithPagination
        {
            wrapper = innerRect,
            blocks = blocks,
            pagination = pagination,
            controls = paginationControls,
        };
    }
}
