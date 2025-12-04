using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonitorWagonsStatistic : MonoBehaviour
{
    private LocalEventManager events;
    private MonitorsManager manager;
    private MonitorControls controls;

    private GameObject main;

    private UIMonitorFactory.StartPlace startPlace;

    private enum ActiveLevel
    {
        None,
        Root
    }

    private class NavigationIndex
    {
        public int? root;
    }

    private class Wagon
    {
        public string id;
        public Button target;
        public GameObject block;
    };

    private List<Wagon> wagons = new();

    private NavigationIndex navigationIndex = new()
    {
        root = 0
    };
    
    private ActiveLevel activeLevel = ActiveLevel.None;

    private bool onFocus = false;

    private void Awake()
    {
        MonitorElements elements = GetComponent<MonitorElements>();
        main = elements.main;
        
        controls = new MonitorControls();
        events = GetComponent<LocalEventManager>();
        manager = MonitorsManager.Instance;

        LocalEventManager managerEvents = manager.GetComponent<LocalEventManager>();
        managerEvents.Subscribe("OnLoadCargo", Render);

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
        if (activeLevel == ActiveLevel.None) activeLevel = ActiveLevel.Root;
        onFocus = true;
    }
    
    private void OnBlur()
    {
        onFocus = false;
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
            },
            new MonitorList.ListItem
            {
                label = "Radiation",
                value = "100",
                type = MonitorList.ListItemType.Radiation
            },
            new MonitorList.ListItem
            {
                label = "Temperature",
                value = "21",
                type = MonitorList.ListItemType.Celsius
            }
        };

        return list;
    }

    void Start()
    {
        UIMonitorFactory.Instance.Init();

        RectTransform mainRect = main.GetComponent<RectTransform>();

        UIMonitorFactory.Instance.MainTitle("Statistic", mainRect);

        startPlace = UIMonitorFactory.Instance.StartWithTabs(mainRect);
        startPlace.blocks.anchoredPosition = new Vector2(0f, -130f);
        
        Render();
    }

    public void Render()
    {
        RectTransform navigationRect = startPlace.navigation;

        foreach (Transform child in navigationRect)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in startPlace.blocks)
        {
            Destroy(child.gameObject);
        }

        wagons.Clear();

        foreach (var item in manager.wagons)
        {
            Button target = UIMonitorFactory.Instance.Button(item.title, 72, navigationRect);
            MonitorButton settings = target.GetComponent<MonitorButton>();
            settings.height = 110;
            settings.ResizeButton(target);

            GameObject block = new ("Block");
            RectTransform blockRect = block.AddComponent<RectTransform>();
            blockRect.transform.SetParent(startPlace.blocks);
            blockRect.localScale = new Vector3(1, 1, 1);
            VerticalLayoutGroup layout = block.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childAlignment = TextAnchor.UpperCenter;
            
            UIMonitorFactory.Instance.List(GenerateWagonInfo(item), blockRect, MonitorList.ListMode.MediumLarge);

            wagons.Add(new Wagon
            {
                id = item.id,
                target = target,
                block = block
            });
        }

        SelectTabButton(0);
    }
    
    private void SelectTabButton(int index)
    {
        navigationIndex.root = index;

        for (int i = 0; i < wagons.Count; i++)
        {
            LocalEventManager buttonEvents = wagons[i].target.GetComponent<LocalEventManager>();
            buttonEvents.Invoke(i == index ? "OnFocus" : "OnBlur");
            wagons[i].block.SetActive(index == i);
        }
        
    }

    private void OnNavigate(Vector2 direction)
    {
        if (!onFocus) return;

        if (wagons.Count == 0) return;
        
        navigationIndex.root = direction.x switch
        {
            > 0.5f => navigationIndex.root + 1,
            < -0.5f => navigationIndex.root - 1,
            _ => navigationIndex.root
        };

        if(navigationIndex.root < 0) navigationIndex.root = wagons.Count - 1;
        if(navigationIndex.root >= wagons.Count) navigationIndex.root = 0;

        SelectTabButton(navigationIndex.root ?? 0);
    }
    
}
