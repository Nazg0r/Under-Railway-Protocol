using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LocalEventManager))]
[RequireComponent(typeof(MonitorElements))]
public class MonitorShop : MonoBehaviour
{
    private LocalEventManager events;

    private Canvas canvas;
    private GameObject main;
    private MonitorsManager manager;

    private GameObject loadingPopup;
    private MonitorTextPopup textPopup;

    private class Item
    {
        public string id;
        public GameObject block;
        public Button button;
        public GameObject info;
        public GameObject counter;
        public GameObject total;
        public Button submit;
    };

    private List<Item> itemsList = new();
    
    private MonitorHeader header;

    private MonitorControls controls;

    private UIMonitorFactory.StartPlace startPlace;

    private enum ActiveLevel
    {
        None,
        Root,
        Sub
    }

    private class NavigationIndex
    {
        public int root;
        public int sub;
    }

    private ActiveLevel activeLevel = ActiveLevel.None;

    private NavigationIndex navigationIndex = new()
    {
        root = -1,
        sub = -1
    };

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

        if (navigationIndex.sub == 0)
        {
            LocalEventManager counterEvents = itemsList[navigationIndex.sub].counter.GetComponent<LocalEventManager>();
            counterEvents.Invoke("OnDisactive");
        }
    }

    private List<MonitorList.ListItem> GenerateShopItemInfo(MonitorsManager.ShopProduct item)
    {
        MonitorsManager.Product productItem = manager.playerData.products.Find(element => element.id == item.id);
        int available = 0;
        if(productItem != null)
        {
            available = productItem.available;
        }

        List<MonitorList.ListItem> list = new()
        {
            new MonitorList.ListItem
            {
                label = "Available",
                value = item.available.ToString()
            },
            new MonitorList.ListItem
            {
                label = "Price per one",
                value = item.pricePerOne + "$"
            },
            new MonitorList.ListItem
            {
                label = "You have",
                value = available + "/100"
            }
        };

        return list;
    }

    private List<MonitorList.ListItem> GenerateShopTotal(MonitorsManager.ShopProduct item, int count = 0)
    {
        List<MonitorList.ListItem> total = new()
        {
            new MonitorList.ListItem
            {
                label = "Total",
                value = item.pricePerOne * count + "$"
            }
        };

        return total;
    }

    void Start()
    {
        UIMonitorFactory.Instance.Init();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform mainRect = main.GetComponent<RectTransform>();

        header = UIMonitorFactory.Instance.Header("Shop", "Balance:", manager.playerData.wallet + "$", mainRect);
        textPopup = UIMonitorFactory.Instance.TextPopup("You are not connected", "press SPACE to connect", canvasRect);

        loadingPopup = UIMonitorFactory.Instance.LoadingPopup(canvasRect);
        loadingPopup.SetActive(false);

        startPlace = UIMonitorFactory.Instance.Start(mainRect);

        Render();
    }

    public void Render()
    {
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
        
        foreach (var item in manager.shopProducts)
        {
            Button button = UIMonitorFactory.Instance.Button(item.title, 56, navigationRect);

            GameObject block = UIMonitorFactory.Instance.Layout("vertical", 72f, blocksRect);
            RectTransform blockRect = block.GetComponent<RectTransform>();
            VerticalLayoutGroup blockLayout = block.GetComponent<VerticalLayoutGroup>();
            blockLayout.childAlignment = TextAnchor.UpperCenter;
            block.SetActive(false);

            GameObject hero = UIMonitorFactory.Instance.Layout("vertical", 24f, blockRect);
            RectTransform heroRect = hero.GetComponent<RectTransform>();
            VerticalLayoutGroup heroLayout = hero.GetComponent<VerticalLayoutGroup>();
            heroLayout.childAlignment = TextAnchor.UpperCenter;
            heroRect.sizeDelta = new Vector2(blockWidth, 210f);

            GameObject title = UIMonitorFactory.Instance.Text(item.title, 72f, heroRect);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(blockWidth, 90);
            TMP_Text titleValue = title.GetComponent<TMP_Text>();
            titleValue.alignment = TextAlignmentOptions.Center;

            GameObject description = UIMonitorFactory.Instance.Text(item.description, 40f, heroRect);
            RectTransform descriptionRect = description.GetComponent<RectTransform>();
            descriptionRect.sizeDelta = new Vector2(blockWidth - 290f, 90);
            TMP_Text descriptionValue = description.GetComponent<TMP_Text>();
            descriptionValue.alignment = TextAlignmentOptions.Center;

            GameObject body = UIMonitorFactory.Instance.Layout("vertical", 24f, blockRect);
            RectTransform bodyRect = body.GetComponent<RectTransform>();
            bodyRect.sizeDelta = new Vector2(964, 829);
            VerticalLayoutGroup bodyLayout = body.GetComponent<VerticalLayoutGroup>();
            bodyLayout.childAlignment = TextAnchor.UpperCenter;

            GameObject info = UIMonitorFactory.Instance.List(GenerateShopItemInfo(item), bodyRect);
            GameObject counter = UIMonitorFactory.Instance.Counter("Amount", bodyRect);
            MonitorCounter counterSettings = counter.GetComponent<MonitorCounter>();
            counterSettings.maxValue = item.available;

            LocalEventManager counterEvents = counter.GetComponent<LocalEventManager>();
            counterEvents.Subscribe("OnActive", () => activeLevel = ActiveLevel.None);
            counterEvents.Subscribe("OnDisactive", () => activeLevel = ActiveLevel.Sub);
            counterEvents.Subscribe("OnChange", () => OnChangeInput());
            
            GameObject total = UIMonitorFactory.Instance.List(GenerateShopTotal(item), bodyRect, MonitorList.ListMode.Large);
            Button submit = UIMonitorFactory.Instance.Button("Buy", 96f, bodyRect);

            itemsList.Add(new Item
            {
                id = item.id,
                block = block,
                button = button,
                info = info,
                counter = counter,
                total = total,
                submit = submit
            });
        }
    
        if(navigationIndex.root >= 0)
        {
            SelectButton(navigationIndex.root);

            if(navigationIndex.sub == 0)
            {
                LocalEventManager counterEvents = itemsList[navigationIndex.root].counter.GetComponent<LocalEventManager>();
                counterEvents.Invoke("OnFocus");
            } else if(navigationIndex.sub == 1)
            {
                LocalEventManager submitEvents = itemsList[navigationIndex.root].submit.GetComponent<LocalEventManager>();
                submitEvents.Invoke("OnFocus");
            }
        }
    }

    IEnumerator LoadAfter(bool? success = null)
    {
        yield return new WaitForSeconds(1f);
        if(success != null)
        {
            loadingPopup.GetComponentInChildren<TMP_Text>().text = success == true ? "Success" : "Fail";
            yield return new WaitForSeconds(1f);
        }

        isLoading = false;
        isConnected = true;
        loadingPopup.SetActive(false);
        if(success != null) loadingPopup.GetComponentInChildren<TMP_Text>().text = "Loading";
    }

    private void Buy()
    {
        MonitorCounter counter = itemsList[navigationIndex.root].counter.GetComponent<MonitorCounter>();
        if (counter.value <= 0) return;

        MonitorsManager.ShopProduct item = manager.shopProducts[navigationIndex.root];
        int available = item.available;

        double price = item.pricePerOne * counter.value;
        if (manager.playerData.wallet - price <= 0) return;
        if (available - counter.value < 0) return;

        isLoading = true;
        loadingPopup.SetActive(true);
        StartCoroutine(LoadAfter(true));

        manager.playerData.wallet -= price;
        header.SetValue(manager.playerData.wallet + "$");

        item.available = available - counter.value;
        counter.maxValue = item.available;
        
        MonitorsManager.Product playerProductItem = manager.playerData.products.Find(element => element.id == item.id);
        if (playerProductItem == null)
        {
            manager.playerData.products.Add(new MonitorsManager.Product
            {
                id = item.id,
                available = 0
            });

            playerProductItem = manager.playerData.products.Find(element => element.id == item.id);
        }
        
        playerProductItem.available += counter.value;

        Render();
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

        if (activeLevel == ActiveLevel.Sub)
        {
            if (navigationIndex.sub == 1) Buy();
        }
    }
    
    private void SelectButton(int index)
    {
        navigationIndex.root = index;

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
        if (activeLevel != ActiveLevel.Sub) return;

        LocalEventManager counterEvents = itemsList[navigationIndex.root].counter.GetComponent<LocalEventManager>();
        LocalEventManager submitEvents = itemsList[navigationIndex.root].submit.GetComponent<LocalEventManager>();

        if (direction.x < -0.5f)
        {
            activeLevel = ActiveLevel.Root;
            navigationIndex.sub = -1;
            submitEvents.Invoke("OnBlur");
            counterEvents.Invoke("OnBlur");
            return;
        }

        if (direction.y < -0.5f)
        {
            navigationIndex.sub++;
            if (navigationIndex.sub >= 2) navigationIndex.sub = 0;

        }
        else if (direction.y > 0.5f)
        {
            navigationIndex.sub--;
            if (navigationIndex.sub < 0) navigationIndex.sub = 1;
        }

        counterEvents.Invoke(navigationIndex.sub == 0 ? "OnFocus" : "OnBlur");
        submitEvents.Invoke(navigationIndex.sub == 1 ? "OnFocus" : "OnBlur");

    }

    private void OnNavigate(Vector2 direction)
    {
        if (!onFocus) return;

        if (itemsList.Count == 0 || !isConnected) return;

        if (activeLevel == ActiveLevel.Root)
        {
            if (direction.x > 0.5f)
            {
                activeLevel = ActiveLevel.Sub;
                navigationIndex.sub = 0;
                ProcessSubNavigate(direction);
                return;
            }

            if (direction.y < -0.5f)
            {
                navigationIndex.root++;
                if (navigationIndex.root >= itemsList.Count) navigationIndex.root = 0;
                SelectButton(navigationIndex.root);
            }
            else if (direction.y > 0.5f)
            {
                navigationIndex.root--;
                if (navigationIndex.root < 0) navigationIndex.root = itemsList.Count - 1;
                SelectButton(navigationIndex.root);
            }
        }

        ProcessSubNavigate(direction);
    }

    private void OnChangeInput()
    {
        if(navigationIndex.root != -1)
        {
            MonitorList total = itemsList[navigationIndex.root].total.GetComponent<MonitorList>();
            MonitorCounter counter = itemsList[navigationIndex.root].counter.GetComponent<MonitorCounter>();
            total.Render(
                GenerateShopTotal(manager.shopProducts[navigationIndex.root], counter.value),
                itemsList[navigationIndex.root].total.GetComponent<RectTransform>(),
                MonitorList.ListMode.Large
            );
        }
    }
}
