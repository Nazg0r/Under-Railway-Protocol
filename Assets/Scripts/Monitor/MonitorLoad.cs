using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonitorLoad : MonoBehaviour
{
    private LocalEventManager events;
    private MonitorsManager manager;
    private LocalEventManager managerEvents;
    private GameInputs controls;

    private Canvas canvas;
    private GameObject main;

    private MonitorHeader header;
    private GameObject loadingPopup;
    private MonitorTextPopup textPopup;
    private UIMonitorFactory.StartPlace startPlace;

    private enum ActiveLevel
    {
        None,
        Root,
        WagonsList,
        WagonItem,
        SubElement
    }

    private class NavigationIndex
    {
        public int root;
        public int wagonsList;
        public int wagonItem;
    }

    private class Wagon
    {
        public string id;
        public Button target;
        public GameObject block;
        public GameObject amount;
        public Button submit;
    };

    private class Cargo
    {
        public string id;
        public Button target;
    };

    private List<Cargo> cargos = new();
    private List<Wagon> wagons = new();

    private ActiveLevel activeLevel = ActiveLevel.None;

    private NavigationIndex navigationIndex = new()
    {
        root = -1,
        wagonsList = -1,
        wagonItem = -1
    };

    private bool isConnected = false;
    private bool isLoading = false;
    private bool onFocus = false;

    private void Awake()
    {
        MonitorElements elements = GetComponent<MonitorElements>();
        canvas = elements.canvas;
        main = elements.main;
        
        controls = new GameInputs();
        events = GetComponent<LocalEventManager>();
        manager = MonitorsManager.Instance;

        managerEvents = manager.GetComponent<LocalEventManager>();
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
        if (activeLevel == ActiveLevel.None) activeLevel = ActiveLevel.Root;
        onFocus = true;
    }
    
    private void OnBlur()
    {
        onFocus = false;

        if (navigationIndex.wagonItem == 0 && navigationIndex.wagonsList != -1)
        {
            LocalEventManager amountEvents = wagons[navigationIndex.wagonsList].amount.GetComponent<LocalEventManager>();
            amountEvents.Invoke("OnDisactive");
        }
    }

    private List<MonitorList.ListItem> GenerateWagonInfo(MonitorsManager.Wagon item)
    {
        List<MonitorList.ListItem> list = new()
        {
            new MonitorList.ListItem
            {
                label = "Current Load",
                value = item.currentLoad + " kg"
            },
            new MonitorList.ListItem
            {
                label = "Capacity",
                value = item.capacity + " kg"
            }
        };

        return list;
    }

    void Start()
    {
        UIMonitorFactory.Instance.Init();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform mainRect = main.GetComponent<RectTransform>();

        header = UIMonitorFactory.Instance.Header("Load a wagon", "Available:", manager.cargos[0].available + " kg", mainRect);
        textPopup = UIMonitorFactory.Instance.TextPopup("You are not connected", "press SPACE to connect", canvasRect);

        loadingPopup = UIMonitorFactory.Instance.LoadingPopup(canvasRect);
        loadingPopup.SetActive(false);

        startPlace = UIMonitorFactory.Instance.StartWithTabs(mainRect);
        RectTransform cargoNavigationRect = startPlace.navigation;
        startPlace.blocks.anchoredPosition = new Vector2(0f, -130f);

        foreach (var item in manager.cargos)
        {
            Button target = UIMonitorFactory.Instance.Button(item.title, 72, cargoNavigationRect);
            MonitorButton settings = target.GetComponent<MonitorButton>();
            settings.height = 110;
            settings.ResizeButton(target);

            cargos.Add(new Cargo
            {
                id = item.id,
                target = target
            });
        }

        SelectTabButton(0);
        Render();

    }

    public void Render()
    {
        foreach (Transform child in startPlace.blocks)
        {
            Destroy(child.gameObject);
        }

        wagons.Clear();

        UIMonitorFactory.StartPlace inner = UIMonitorFactory.Instance.Start(startPlace.blocks, 230f);
        inner.navigation.sizeDelta = new Vector2(485, 600);

        foreach (var item in manager.wagons)
        {
            Button target = UIMonitorFactory.Instance.Button(item.title, 96, inner.navigation);

            GameObject block = UIMonitorFactory.Instance.Layout("vertical", 72f, inner.blocks);
            RectTransform blockRect = block.GetComponent<RectTransform>();
            VerticalLayoutGroup blockLayout = block.GetComponent<VerticalLayoutGroup>();
            blockLayout.childAlignment = TextAnchor.UpperCenter;
            block.SetActive(false);

            GameObject info = UIMonitorFactory.Instance.List(GenerateWagonInfo(item), blockRect, MonitorList.ListMode.Medium);
            GameObject amount = UIMonitorFactory.Instance.Counter("Amount", blockRect);
            MonitorCounter counterSettings = amount.GetComponent<MonitorCounter>();
            counterSettings.maxValue = Math.Min(item.capacity - item.currentLoad, manager.cargos[0].available);

            Button submit = UIMonitorFactory.Instance.Button("Load", 96, blockRect);
            amount.GetComponent<LocalEventManager>().Subscribe("OnDisactive", () =>
            {
                activeLevel = ActiveLevel.WagonItem;
            });
            amount.GetComponent<LocalEventManager>().Subscribe("OnActive", () =>
            {
                activeLevel = ActiveLevel.SubElement; 
            });
            
            wagons.Add(new Wagon
            {
                id = item.id,
                target = target,
                block = block,
                amount = amount,
                submit = submit
            });
        }

        SelectButton(navigationIndex.wagonsList);

        if(navigationIndex.wagonItem != -1)
        {
            LocalEventManager amountEvents = wagons[navigationIndex.wagonsList].amount.GetComponent<LocalEventManager>();
            LocalEventManager submitEvents = wagons[navigationIndex.wagonsList].submit.GetComponent<LocalEventManager>();

            if(navigationIndex.wagonItem == 0) amountEvents.Invoke("OnFocus");
            if(navigationIndex.wagonItem == 1) submitEvents.Invoke("OnFocus");
        }
    }

    IEnumerator LoadAfter(bool? success = null)
    {
        yield return new WaitForSeconds(1f);
        if(success != null)
        {
            loadingPopup.GetComponentInChildren<TMP_Text>().text = success == true ? "Success" : "Fail";
            yield return new WaitForSeconds(0.5f);
        }

        isLoading = false;
        isConnected = true;
        loadingPopup.SetActive(false);
        if(success != null) loadingPopup.GetComponentInChildren<TMP_Text>().text = "Loading...";
    }

    private void OnSubmit()
    {
        if (!isConnected && !isLoading && activeLevel == ActiveLevel.Root)
        {
            isLoading = true;
            textPopup.wrapper.SetActive(false);
            loadingPopup.SetActive(true);
            StartCoroutine(LoadAfter());
        }

        if (activeLevel == ActiveLevel.WagonItem)
        {
            if (navigationIndex.wagonItem == 1) SubmitLoad();
        }
    }

    private void SubmitLoad()
    {
        MonitorCounter amount = wagons[navigationIndex.wagonsList].amount.GetComponent<MonitorCounter>();
        if(amount.value <= 0) return;

        isLoading = true;
        loadingPopup.SetActive(true);
        StartCoroutine(LoadAfter(true));
        
        manager.cargos[navigationIndex.root].available -= amount.value;
        manager.wagons[navigationIndex.wagonsList].currentLoad += amount.value;

        SelectTabButton(navigationIndex.root);
        managerEvents.Invoke("OnLoadCargo");
        Render();
    }

    private void SelectTabButton(int index)
    {
        navigationIndex.root = index;

        for (int i = 0; i < cargos.Count; i++)
        {
            LocalEventManager buttonEvents = cargos[i].target.GetComponent<LocalEventManager>();
            buttonEvents.Invoke(i == index ? "OnFocus" : "OnBlur");
        }

        header.SetValue(manager.cargos[index].available + " kg");
    }

    private void SelectButton(int index)
    {
        navigationIndex.wagonsList = index;

        for (int i = 0; i < wagons.Count; i++)
        {
            LocalEventManager buttonEvents = wagons[i].target.GetComponent<LocalEventManager>();
            buttonEvents.Invoke(i == index ? "OnFocus" : "OnBlur");
            wagons[i].block.SetActive(i == index);

            MonitorCounter counterSettings = wagons[i].amount.GetComponent<MonitorCounter>();
            counterSettings.maxValue = Math.Min(manager.wagons[i].capacity - manager.wagons[i].currentLoad, manager.cargos[navigationIndex.root].available);
        }
    }

    private void TabsNavigate(Vector2 direction)
    {
        if (activeLevel != ActiveLevel.Root) return;
        
        int max = cargos.Count;

        if (direction.y < -0.5f)
        {
            activeLevel = ActiveLevel.WagonsList;
            WagonsListNavigate(direction);
            return;
        }

        navigationIndex.root = direction.x switch
        {
            < -0.5f => navigationIndex.root - 1,
            > 0.5f => navigationIndex.root + 1,
            _ => navigationIndex.root,
        };

        if(navigationIndex.root >= max) navigationIndex.root = 0;
        else if(navigationIndex.root < 0) navigationIndex.root = max - 1;

        SelectTabButton(navigationIndex.root);
    }

    private void WagonsListNavigate(Vector2 direction)
    {
        if (activeLevel != ActiveLevel.WagonsList) return;
        
        int max = wagons.Count;

        if (direction.y > 0.5f && navigationIndex.wagonsList == 0)
        {
            activeLevel = ActiveLevel.Root;
            navigationIndex.wagonsList = -1;
            SelectButton(-1);
            return;
        }

        if (direction.x > 0.5f)
        {
            activeLevel = ActiveLevel.WagonItem;
            navigationIndex.wagonItem = 0;
            WagonItemNavigate(direction);
            return;
        }

        navigationIndex.wagonsList = direction.y switch
        {
            < -0.5f => navigationIndex.wagonsList + 1,
            > 0.5f => navigationIndex.wagonsList - 1,
            _ => navigationIndex.wagonsList,
        };

        if(navigationIndex.wagonsList >= max) navigationIndex.wagonsList = 0;
        else if(navigationIndex.wagonsList < 0) navigationIndex.wagonsList = max - 1;

        SelectButton(navigationIndex.wagonsList);
    }

    private void WagonItemNavigate(Vector2 direction)
    {
        if (activeLevel != ActiveLevel.WagonItem) return;
        
        int max = 2;
        LocalEventManager amountEvents = wagons[navigationIndex.wagonsList].amount.GetComponent<LocalEventManager>();
        LocalEventManager submitEvents = wagons[navigationIndex.wagonsList].submit.GetComponent<LocalEventManager>();

        if (direction.x < -0.5f)
        {
            activeLevel = ActiveLevel.WagonsList;
            navigationIndex.wagonItem = -1;
            submitEvents.Invoke("OnBlur");
            amountEvents.Invoke("OnBlur");
            return;
        }

        navigationIndex.wagonItem = direction.y switch
        {
            < -0.5f => navigationIndex.wagonItem + 1,
            > 0.5f => navigationIndex.wagonItem - 1,
            _ => navigationIndex.wagonItem,
        };

        if(navigationIndex.wagonItem >= max) navigationIndex.wagonItem = 0;
        else if(navigationIndex.wagonItem < 0) navigationIndex.wagonItem = max - 1;

        amountEvents.Invoke(navigationIndex.wagonItem == 0 ? "OnFocus" : "OnBlur");
        submitEvents.Invoke(navigationIndex.wagonItem == 1 ? "OnFocus" : "OnBlur");
    }

    private void OnNavigate(Vector2 direction)
    {
        if (!onFocus) return;

        if (cargos.Count == 0 || wagons.Count == 0 || !isConnected) return;
        
        switch (activeLevel)
        {
            case ActiveLevel.Root:
                {
                    TabsNavigate(direction);
                    break;
                }
            case ActiveLevel.WagonsList:
                {
                    WagonsListNavigate(direction);
                    break;
                }
            case ActiveLevel.WagonItem:
                {
                    WagonItemNavigate(direction);
                    break;
                }
            default: break;
        }
    }

}
