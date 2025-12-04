using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LocalEventManager))]
public class MonitorAvailableQuests : MonoBehaviour
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
        public List<Button> submit;
    };

    private readonly List<Item> itemsList = new();

    private bool isConnected = false;
    private bool isLoading = false;
    private int currentIndex = -1;
    private bool isActive = false;
    private bool onFocus = false;
    private bool isActiveSub = false;
    private int currentSubIndex = 0;

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
        controls.UI.Submit.performed += ctx => OnSubmit();

        events.Subscribe("OnFocus", OnFocus);
        events.Subscribe("OnBlur", OnBlur);
    }

    void OnDisable()
    {
        controls.UI.Disable();
        controls.UI.Navigation.performed -= ctx => OnNavigate(ctx.ReadValue<Vector2>());
        controls.UI.Submit.performed -= ctx => OnSubmit();

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

        UIMonitorFactory.Instance.Header("Available Quests", "Connected:", "ST-90", mainRect);
        textPopup = UIMonitorFactory.Instance.TextPopup("You are not connected", "press SPACE to connect", canvasRect);

        loadingPopup = UIMonitorFactory.Instance.LoadingPopup(canvasRect);
        loadingPopup.SetActive(false);

        startPlace = UIMonitorFactory.Instance.Start(mainRect, 120f);
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

        if(manager.quests.Count == 0 && textPopup)
        {
            textPopup.SetContent("No quests available", "You currently have no available quests.");
            textPopup.wrapper.SetActive(true);

            return;
        }

        int indexItem = 0;
        foreach (var item in manager.quests)
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

                submit.Add(UIMonitorFactory.Instance.Button("Accept", 96, footerBlockRect));
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
                submit = submit,
            });

            indexItem++;
        }
    }

    private void OnSubmit()
    {
        if(!onFocus) return;
        
        if (!isConnected && !isLoading && isActive)
        {
            isLoading = true;
            isConnected = true;
            textPopup.wrapper.SetActive(false);
            StartCoroutine(ConnectToStation());
            Render();
        }

        if (!isActive && isActiveSub)
        {
            if (currentSubIndex == 0 && currentIndex != -1) Accept();
        }
    }

    IEnumerator ConnectToStation()
    {
        yield return _waitForSeconds1;
        isConnected = true;
    }

    IEnumerator UpdateMonitor()
    {
        isLoading = true;
        loadingPopup.SetActive(true);
        yield return _waitForSeconds1;
        isLoading = false;
        loadingPopup.SetActive(false);
    }

    private void Accept()
    {
        int paged = itemsList[currentIndex].inner.controls.paged;
        int questIndex = paged - 1;
        MonitorsManager.QuestElement quest = manager.quests[currentIndex].list[questIndex];

        manager.quests[currentIndex].list.Remove(quest);
        manager.AcceptQuest(manager.quests[currentIndex], quest);
        int questsLength = manager.quests[currentIndex].list.Count;

        if(questsLength == 1)
        {
            itemsList[currentIndex].inner.pagination.GetComponent<LocalEventManager>().Invoke("OnBlur");
            currentSubIndex = 0;
        }

        if(questsLength == 0)
        {
            currentSubIndex = -1;
            manager.quests.Remove(manager.quests[currentIndex]);
            currentIndex = -1;
            isActiveSub = false;
            isActive = true;
        }

        Render();
        
        if(currentIndex >= 0)
        {
            int newPages = itemsList[currentIndex].inner.controls.pages;
            int newPaged = newPages <= paged ? newPages : paged;
            if(newPages > 1)
            {
                itemsList[currentIndex].inner.controls.paged = newPaged;
                itemsList[currentIndex].inner.controls.UpdatePages();
            }
            
            if(questsLength >= 1)
            {
                itemsList[currentIndex].submit[newPaged - 1].GetComponent<LocalEventManager>().Invoke("OnFocus");
            }
        }
        
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
        int maxItems = paginationIsActive ? 2 : 1;

        int paged = itemsList[currentIndex].inner.controls.paged;
        if(paged == 0 && paginationIsActive)
        {
            paginationEvents.Invoke("OnFocus");
            return;
        }

        LocalEventManager submitEvents = itemsList[currentIndex].submit[paged - 1].GetComponent<LocalEventManager>();

        if (direction.x < -0.5f && isActiveSub && (paged == 1 || currentSubIndex == 0))
        {
            isActive = true;
            isActiveSub = false;
            submitEvents.Invoke("OnBlur");
            if(paginationIsActive) paginationEvents.Invoke("OnBlur");
            return;
        }

        currentSubIndex = direction.y switch
        {
            < -0.5f => currentSubIndex + 1,
            > 0.5f => currentSubIndex - 1,
            _ => currentSubIndex,
        };

        if(currentSubIndex >= maxItems) currentSubIndex = 0;
        else if(currentSubIndex < 0) currentSubIndex = maxItems - 1;

        submitEvents.Invoke(currentSubIndex == 0 ? "OnFocus" : "OnBlur");
        if(paginationIsActive) paginationEvents.Invoke(currentSubIndex == 1 ? "OnFocus" : "OnBlur");
    }

    private void OnNavigate(Vector2 direction)
    {
        if (!onFocus) return;

        if (itemsList.Count == 0 || !isConnected || isLoading) return;

        if (isActive)
        {
            if (direction.x > 0.5f && !isActiveSub)
            {
                isActive = false;
                isActiveSub = true;
                if(currentSubIndex == 1) itemsList[currentIndex].inner.controls.paged = 0;
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
