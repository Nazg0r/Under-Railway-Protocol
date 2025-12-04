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

    private bool isConnected = false;
    private bool isLoading = false;
    private int currentIndex = -1;
    private bool onFocus = false;
    private bool isActive = false;
    private bool isActiveSub = false;
    private int currentSubIndex = 0;

    private MonitorsManager manager;

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
        if (!isActiveSub) isActive = true;
        onFocus = true;
    }
    
    private void OnBlur()
    {
        onFocus = false;

        isActive = false;

        if (currentSubIndex == 0)
        {
            LocalEventManager counterEvents = itemsList[currentIndex].counter.GetComponent<LocalEventManager>();
            counterEvents.Invoke("OnDisactive");
        }
    }

    private List<UIMonitorFactory.ListItem> GenerateShopItemInfo(MonitorsManager.ShopProduct item)
    {
        MonitorsManager.Product productItem = manager.playerData.products.Find(element => element.id == item.id);
        int available = 0;
        if(productItem != null)
        {
            available = productItem.available;
        }

        List<UIMonitorFactory.ListItem> list = new()
        {
            new UIMonitorFactory.ListItem
            {
                label = "Available",
                value = item.available.ToString()
            },
            new UIMonitorFactory.ListItem
            {
                label = "Price per one",
                value = item.pricePerOne + "$"
            },
            new UIMonitorFactory.ListItem
            {
                label = "You have",
                value = available + "/100"
            }
        };

        return list;
    }

    private List<UIMonitorFactory.ListItem> GenerateShopTotal(MonitorsManager.ShopProduct item, int count = 0)
    {
        List<UIMonitorFactory.ListItem> total = new()
        {
            new UIMonitorFactory.ListItem
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

        UIMonitorFactory.StartPlace startPlace = UIMonitorFactory.Instance.Start(mainRect);
        RectTransform navigationRect = startPlace.navigation;
        RectTransform blocksRect = startPlace.blocks;

        float blockWidth = manager.blockWidth;

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
            counterEvents.Subscribe("OnActive", () => isActiveSub = false);
            counterEvents.Subscribe("OnDisactive", () => isActiveSub = true);
            counterEvents.Subscribe("OnChange", () => OnChangeInput());
            
            GameObject total = UIMonitorFactory.Instance.List(GenerateShopTotal(item), bodyRect, UIMonitorFactory.ListMode.Large);
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
    }

    IEnumerator LoadAfter()
    {
        yield return new WaitForSeconds(1f);
        isLoading = false;
        isConnected = true;
        loadingPopup.SetActive(false);
    }

    IEnumerator BuyTargetSuccess()
    {
        MonitorButton submit = itemsList[currentIndex].submit.GetComponent<MonitorButton>();
        submit.SetTitle("Success");
        yield return new WaitForSeconds(1f);
        submit.SetTitle("Buy");
    }

    private void Buy()
    {
        MonitorCounter counter = itemsList[currentIndex].counter.GetComponent<MonitorCounter>();
        if (counter.value <= 0) return;

        MonitorsManager.ShopProduct item = manager.shopProducts[currentIndex];
        int available = item.available;

        double price = item.pricePerOne * counter.value;
        if (manager.playerData.wallet - price <= 0) return;
        if (available - counter.value < 0) return;

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
        
        MonitorList info = itemsList[currentIndex].info.GetComponent<MonitorList>();
        info.Render(GenerateShopItemInfo(item), itemsList[currentIndex].info.GetComponent<RectTransform>());
        MonitorList total = itemsList[currentIndex].total.GetComponent<MonitorList>();
        total.Render(GenerateShopTotal(item), itemsList[currentIndex].total.GetComponent<RectTransform>(), UIMonitorFactory.ListMode.Large);

        counter.Reset();
        StartCoroutine(BuyTargetSuccess());
    }

    private void OnSubmit()
    {
        if (!isConnected && !isLoading && isActive)
        {
            isLoading = true;
            textPopup.wrapper.SetActive(false);
            loadingPopup.SetActive(true);
            StartCoroutine(LoadAfter());
        }

        if (!isActive && isActiveSub)
        {
            if (currentSubIndex == 1) Buy();
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

        LocalEventManager counterEvents = itemsList[currentIndex].counter.GetComponent<LocalEventManager>();
        LocalEventManager submitEvents = itemsList[currentIndex].submit.GetComponent<LocalEventManager>();

        if (direction.x < -0.5f && isActiveSub)
        {
            isActive = true;
            isActiveSub = false;
            currentSubIndex = -1;
            submitEvents.Invoke("OnBlur");
            counterEvents.Invoke("OnBlur");
            return;
        }

        if (direction.y < -0.5f)
        {
            currentSubIndex++;
            if (currentSubIndex >= 2) currentSubIndex = 0;

        }
        else if (direction.y > 0.5f)
        {
            currentSubIndex--;
            if (currentSubIndex < 0)
            {
                currentSubIndex = 1;
            }
        }

        counterEvents.Invoke(currentSubIndex == 0 ? "OnFocus" : "OnBlur");
        submitEvents.Invoke(currentSubIndex == 1 ? "OnFocus" : "OnBlur");

    }

    private void OnNavigate(Vector2 direction)
    {
        if (!onFocus) return;

        if (itemsList.Count == 0 || !isConnected) return;

        if (isActive)
        {
            if (direction.x > 0.5f && !isActiveSub)
            {
                isActive = false;
                isActiveSub = true;
                currentSubIndex = 0;
                ProcessSubNavigate(direction);
                return;
            }

            if (direction.y < -0.5f)
            {
                currentIndex++;
                if (currentIndex >= itemsList.Count) currentIndex = 0;
                SelectButton(currentIndex);
            }
            else if (direction.y > 0.5f)
            {
                currentIndex--;
                if (currentIndex < 0) currentIndex = itemsList.Count - 1;
                SelectButton(currentIndex);
            }
        }

        ProcessSubNavigate(direction);
    }

    private void OnChangeInput()
    {
        if(currentIndex != -1)
        {
            MonitorList total = itemsList[currentIndex].total.GetComponent<MonitorList>();
            MonitorCounter counter = itemsList[currentIndex].counter.GetComponent<MonitorCounter>();
            total.Render(
                GenerateShopTotal(manager.shopProducts[currentIndex], counter.value),
                itemsList[currentIndex].total.GetComponent<RectTransform>(),
                UIMonitorFactory.ListMode.Large
            );
        }
    }
}
