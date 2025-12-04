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
    private MonitorControls controls;

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
        public int? root;
        public int? wagonsList;
        public int? wagonItem;
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

    private NavigationIndex currentIndex = new()
    {
        root = 0,
        wagonsList = null,
        wagonItem = null
    };
    
    private ActiveLevel activeLevel = ActiveLevel.None;

    private bool isConnected = false;
    private bool isLoading = false;
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

        if (currentIndex.wagonItem == 0 && currentIndex.wagonsList != null)
        {
            LocalEventManager amountEvents = wagons[currentIndex.wagonsList ?? 0].amount.GetComponent<LocalEventManager>();
            amountEvents.Invoke("OnDisactive");
        }
    }

    private List<UIMonitorFactory.ListItem> GenerateWagonInfo(MonitorsManager.Wagon item)
    {
        List<UIMonitorFactory.ListItem> list = new()
        {
            new UIMonitorFactory.ListItem
            {
                label = "Current Load",
                value = item.currentLoad + " kg"
            },
            new UIMonitorFactory.ListItem
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

            GameObject info = UIMonitorFactory.Instance.List(GenerateWagonInfo(item), blockRect, UIMonitorFactory.ListMode.Medium);
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

        if(currentIndex.wagonsList != null)
        {
            SelectButton(currentIndex.wagonsList ?? 0);
        }

        if(currentIndex.wagonItem != null)
        {
            LocalEventManager amountEvents = wagons[currentIndex.wagonsList ?? 0].amount.GetComponent<LocalEventManager>();
            LocalEventManager submitEvents = wagons[currentIndex.wagonsList ?? 0].submit.GetComponent<LocalEventManager>();

            if(currentIndex.wagonItem == 0) amountEvents.Invoke("OnFocus");
            if(currentIndex.wagonItem == 1) submitEvents.Invoke("OnFocus");
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
            if (currentIndex.wagonItem == 1) SubmitLoad();
        }
    }

    private void SubmitLoad()
    {
        MonitorCounter amount = wagons[currentIndex.wagonsList ?? 0].amount.GetComponent<MonitorCounter>();
        if(amount.value <= 0) return;

        isLoading = true;
        loadingPopup.SetActive(true);
        StartCoroutine(LoadAfter(true));
        
        manager.cargos[currentIndex.root ?? 0].available -= amount.value;
        manager.wagons[currentIndex.wagonsList ?? 0].currentLoad += amount.value;

        SelectTabButton(currentIndex.root ?? 0);
        Render();
    }

    private void SelectTabButton(int index)
    {
        currentIndex.root = index;

        for (int i = 0; i < cargos.Count; i++)
        {
            LocalEventManager buttonEvents = cargos[i].target.GetComponent<LocalEventManager>();
            buttonEvents.Invoke(i == index ? "OnFocus" : "OnBlur");
        }

        header.SetValue(manager.cargos[index].available + " kg");
    }

    private void SelectButton(int index)
    {
        currentIndex.wagonsList = index;

        for (int i = 0; i < wagons.Count; i++)
        {
            LocalEventManager buttonEvents = wagons[i].target.GetComponent<LocalEventManager>();
            buttonEvents.Invoke(i == index ? "OnFocus" : "OnBlur");
            wagons[i].block.SetActive(i == index);

            MonitorCounter counterSettings = wagons[i].amount.GetComponent<MonitorCounter>();
            counterSettings.maxValue = Math.Min(manager.wagons[i].capacity - manager.wagons[i].currentLoad, manager.cargos[currentIndex.root ?? 0].available);
        }
    }

    private void TabsNavigate(Vector2 direction)
    {
        if (activeLevel != ActiveLevel.Root) return;
        if(currentIndex.root == null) return;
        
        int max = cargos.Count;

        if (direction.y < -0.5f)
        {
            activeLevel = ActiveLevel.WagonsList;
            currentIndex.wagonsList = -1;
            WagonsListNavigate(direction);
            return;
        }

        currentIndex.root = direction.x switch
        {
            < -0.5f => currentIndex.root - 1,
            > 0.5f => currentIndex.root + 1,
            _ => currentIndex.root,
        };

        if(currentIndex.root >= max) currentIndex.root = 0;
        else if(currentIndex.root < 0) currentIndex.root = max - 1;

        SelectTabButton(currentIndex.root ?? 0);
    }

    private void WagonsListNavigate(Vector2 direction)
    {
        if (activeLevel != ActiveLevel.WagonsList) return;
        if(currentIndex.wagonsList == null) return;
        
        int max = wagons.Count;

        if (direction.y > 0.5f && currentIndex.wagonsList == 0)
        {
            activeLevel = ActiveLevel.Root;
            SelectButton(-1);
            currentIndex.wagonsList = null;
            return;
        }

        if (direction.x > 0.5f)
        {
            activeLevel = ActiveLevel.WagonItem;
            currentIndex.wagonItem = 0;
            WagonItemNavigate(direction);
            return;
        }

        currentIndex.wagonsList = direction.y switch
        {
            < -0.5f => currentIndex.wagonsList + 1,
            > 0.5f => currentIndex.wagonsList - 1,
            _ => currentIndex.wagonsList,
        };

        if(currentIndex.wagonsList >= max) currentIndex.wagonsList = 0;
        else if(currentIndex.wagonsList < 0) currentIndex.wagonsList = max - 1;

        if(currentIndex.wagonsList != null) SelectButton(currentIndex.wagonsList ?? 0);
    }

    private void WagonItemNavigate(Vector2 direction)
    {
        if (activeLevel != ActiveLevel.WagonItem) return;
        if(currentIndex.wagonItem == null) return;
        
        int max = 2;
        LocalEventManager amountEvents = wagons[currentIndex.wagonsList ?? 0].amount.GetComponent<LocalEventManager>();
        LocalEventManager submitEvents = wagons[currentIndex.wagonsList ?? 0].submit.GetComponent<LocalEventManager>();

        if (direction.x < -0.5f)
        {
            activeLevel = ActiveLevel.WagonsList;
            currentIndex.wagonItem = null;
            submitEvents.Invoke("OnBlur");
            amountEvents.Invoke("OnBlur");
            return;
        }

        currentIndex.wagonItem = direction.y switch
        {
            < -0.5f => currentIndex.wagonItem + 1,
            > 0.5f => currentIndex.wagonItem - 1,
            _ => currentIndex.wagonItem,
        };

        if(currentIndex.wagonItem >= max) currentIndex.wagonItem = 0;
        else if(currentIndex.wagonItem < 0) currentIndex.wagonItem = max - 1;

        amountEvents.Invoke(currentIndex.wagonItem == 0 ? "OnFocus" : "OnBlur");
        submitEvents.Invoke(currentIndex.wagonItem == 1 ? "OnFocus" : "OnBlur");
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

    private void OnChangeInput()
    {
        /* if(currentIndex != -1)
        {
            MonitorList total = itemsList[currentIndex].total.GetComponent<MonitorList>();
            MonitorCounter counter = itemsList[currentIndex].counter.GetComponent<MonitorCounter>();
            total.Render(
                GenerateShopTotal(manager.shopProducts[currentIndex], counter.value),
                itemsList[currentIndex].total.GetComponent<RectTransform>(),
                UIMonitorFactory.ListMode.Large
            );
        } */
    }
}
