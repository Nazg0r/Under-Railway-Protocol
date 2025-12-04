using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LocalEventManager))]
public class MonitorAcceptedQuests : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    private LocalEventManager events;

    private Canvas canvas;
    private GameObject main;
    private MonitorControls controls;
    private MonitorsManager manager;

    private GameObject loadingPopup;
    private MonitorTextPopup textPopup;
    private UIMonitorFactory.StartPlace startPlace;

    private class Item
    {
        public string id;
        public GameObject block;
        public Button button;
        public GameObject info;
        public UIMonitorFactory.BlocksWithPagination inner;
    };

    private readonly List<Item> itemsList = new();

    private bool isLoading = false;
    private int currentIndex = -1;
    private int currentSubIndex = -1;
    private bool isActive = false;
    private bool isActiveSub = false;
    private bool onFocus = false;

    private void Awake()
    {
        MonitorElements elements = GetComponent<MonitorElements>();
        canvas = elements.canvas;
        main = elements.main;

        controls = new MonitorControls();
        events = GetComponent<LocalEventManager>();
        manager = MonitorsManager.Instance;
    }

    void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.Navigation.performed += ctx => OnNavigate(ctx.ReadValue<Vector2>());

        events.Subscribe("OnFocus", OnFocus);
        events.Subscribe("OnBlur", OnBlur);
    }

    void OnDisable()
    {
        controls.UI.Disable();
        controls.UI.Navigation.performed -= ctx => OnNavigate(ctx.ReadValue<Vector2>());

        events.Unsubscribe("OnFocus", OnFocus);
        events.Unsubscribe("OnBlur", OnBlur);
    }

    private void OnFocus()
    {
        onFocus = true;
        if(!isActiveSub) isActive = true;
    }
    private void OnBlur()
    {
        onFocus = false;
        isActive = false;
    }

    private List<UIMonitorFactory.ListItem> GenerateGoals(List<MonitorsManager.QuestGoal> items)
    {
        List<UIMonitorFactory.ListItem> list = new();
        foreach (var item in items)
        {
            list.Add(new UIMonitorFactory.ListItem
            {
                label = item.name,
                value = "x" + item.length
            });
        }

        return list;
    }

    void Start()
    {
        UIMonitorFactory.Instance.Init();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform mainRect = main.GetComponent<RectTransform>();

        UIMonitorFactory.Instance.Header("Accepted Quests", "", "", mainRect);
        textPopup = UIMonitorFactory.Instance.TextPopup("You have no quests", "Your selected quests will be displayed here", canvasRect);
        textPopup.wrapper.SetActive(manager.acceptedQuests.Count == 0);

        loadingPopup = UIMonitorFactory.Instance.LoadingPopup(canvasRect);
        
        startPlace = UIMonitorFactory.Instance.Start(mainRect, 120f);

        manager.events.Subscribe("UpdateAcceptedQuests", Render);
        Render();
    }

    private void Render()
    {
        StartCoroutine(UpdateMonitor());

        RectTransform navigationRect = startPlace.navigation;
        RectTransform blocksRect = startPlace.blocks;

        float blockWidth = manager.blockWidth;

        foreach (Transform child in navigationRect)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in blocksRect)
        {
            Destroy(child.gameObject);
        }

        itemsList.Clear();

        if(manager.acceptedQuests.Count == 0) {
            textPopup.wrapper.SetActive(true);
            return;
        }
        else if(textPopup.wrapper.activeSelf) textPopup.wrapper.SetActive(false);

        int indexItem = 0;
        foreach (var item in manager.acceptedQuests)
        {
            Button button = UIMonitorFactory.Instance.Button(item.station + " (" + item.list.Count + ")", 56, navigationRect);

            GameObject block = UIMonitorFactory.Instance.Layout("vertical", 72f, blocksRect);
            RectTransform blockRect = block.GetComponent<RectTransform>();
            VerticalLayoutGroup blockLayout = block.GetComponent<VerticalLayoutGroup>();
            blockLayout.childAlignment = TextAnchor.UpperCenter;

            GameObject title = UIMonitorFactory.Instance.Text(item.station, 96f, blockRect);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(blockWidth, 90);
            TMP_Text titleValue = title.GetComponent<TMP_Text>();
            titleValue.alignment = TextAlignmentOptions.Center;

            GameObject body = UIMonitorFactory.Instance.Layout("vertical", 24f, blockRect);
            RectTransform bodyRect = body.GetComponent<RectTransform>();
            bodyRect.sizeDelta = new Vector2(964, 829);
            VerticalLayoutGroup bodyLayout = body.GetComponent<VerticalLayoutGroup>();
            bodyLayout.childAlignment = TextAnchor.UpperCenter;

            UIMonitorFactory.BlocksWithPagination inner = UIMonitorFactory.Instance.Blocks(item.list.Count, bodyRect);
            List<Button> submit = new();

            for (int index = 0; index < inner.blocks.Count; index++)
            {
                RectTransform rect = inner.blocks[index].GetComponent<RectTransform>();
                UIMonitorFactory.Instance.List(GenerateGoals(item.list[index].goals), rect, UIMonitorFactory.ListMode.Medium);

                GameObject footerBlock = UIMonitorFactory.Instance.Layout("vertical", 24f, rect);
                RectTransform footerBlockRect = footerBlock.GetComponent<RectTransform>();
                footerBlockRect.sizeDelta = new Vector2(964, 182);
                footerBlock.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.LowerCenter;
            }

            Canvas.ForceUpdateCanvases();

            RectTransform firstBlockRect = inner.blocks[0].GetComponent<RectTransform>();
            inner.wrapper.sizeDelta = new Vector2(firstBlockRect.rect.width, firstBlockRect.rect.height);
            
            GameObject footer = UIMonitorFactory.Instance.Layout("horizontal", 24f, bodyRect);
            RectTransform footerRect = footer.GetComponent<RectTransform>();
            footerRect.sizeDelta = new Vector2(964, 182);
            footer.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.LowerCenter;

            RectTransform pagination = inner.pagination.GetComponent<RectTransform>();
            pagination.SetParent(footerRect);

            block.SetActive(indexItem == currentIndex);
            button.GetComponent<LocalEventManager>().Invoke(currentIndex == indexItem ? "OnFocus" : "OnBlur");

            itemsList.Add(new Item
            {
                id = item.station,
                block = block,
                button = button,
                inner = inner,
            });

            indexItem++;
        }
    }

    IEnumerator UpdateMonitor()
    {
        isLoading = true;
        loadingPopup.SetActive(true);
        yield return _waitForSeconds1;
        isLoading = false;
        loadingPopup.SetActive(false);
    }

    private void SelectButton(int index)
    {
        currentIndex = index;

        for (int i = 0; i < itemsList.Count; i++)
        {
            LocalEventManager buttonEvents = itemsList[i].button.GetComponent<LocalEventManager>();
            buttonEvents.Invoke(i == index ? "OnFocus" : "OnBlur");
        }

        for (int i = 0; i < itemsList.Count; i++)
        {
            itemsList[i].block.SetActive(i == index);
        }
    }

    private void ProcessSubNavigate(Vector2 direction)
    {
        if (!isActiveSub) return;

        LocalEventManager paginationEvents = itemsList[currentIndex].inner.pagination.GetComponent<LocalEventManager>();
        bool paginationIsActive = itemsList[currentIndex].inner.controls.wrapper.activeSelf;
        int maxItems = 0;

        int paged = itemsList[currentIndex].inner.controls.paged;
        if(paged == 0 && paginationIsActive)
        {
            paginationEvents.Invoke("OnFocus");
            return;
        }

        if (direction.x < -0.5f && isActiveSub && paged == 1)
        {
            isActive = true;
            isActiveSub = false;
            if(paginationIsActive) paginationEvents.Invoke("OnBlur");
            return;
        }

        if(currentSubIndex >= maxItems) currentSubIndex = 0;
        else if(currentSubIndex < 0) currentSubIndex = maxItems - 1;

        if(paginationIsActive) paginationEvents.Invoke(currentSubIndex == 0 ? "OnFocus" : "OnBlur");
    }

    private void OnNavigate(Vector2 direction)
    {
        if (!onFocus) return;

        if (itemsList.Count == 0 || isLoading) return;

        if (isActive)
        {
            if (direction.x > 0.5f && !isActiveSub)
            {
                isActive = false;
                isActiveSub = true;
                if(currentSubIndex == 0) itemsList[currentIndex].inner.controls.paged = 0;
                ProcessSubNavigate(direction);
                return;
            }

            if (direction.y < -0.5f)
            {
                currentIndex++;
                if (currentIndex >= itemsList.Count) currentIndex = 0;
                SelectButton(currentIndex);
                if(currentSubIndex != 0) currentSubIndex = 0;
            }
            else if (direction.y > 0.5f)
            {
                currentIndex--;
                if (currentIndex < 0) currentIndex = itemsList.Count - 1;
                SelectButton(currentIndex);
                if(currentSubIndex != 0) currentSubIndex = 0;
            }
        }

        ProcessSubNavigate(direction);
    }
}
