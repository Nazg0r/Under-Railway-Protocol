using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonitorPagination : MonoBehaviour
{
    
    private LocalEventManager events;
    private MonitorsManager manager;
    private MonitorControls controls;

    public GameObject wrapper;
    public Image background;
    public TMP_Text page;
    public TMP_Text previousButton;
    public TMP_Text nextButton;
    public RectTransform blocksWrapper;
    public List<GameObject> blocksList;

    private bool onFocus = false;
    public int paged = 1;
    public int pages = 1;

    void Awake()
    {
        events = GetComponent<LocalEventManager>();
        events.Subscribe("OnFocus", OnFocus);
        events.Subscribe("OnBlur", OnBlur);
        controls = new MonitorControls();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        manager = MonitorsManager.Instance;
    }

    public void Render(int length)
    {
        if(length == 1 || length == 0) wrapper.SetActive(false);
        else wrapper.SetActive(true);
        
        page.text = paged + "/" + length + "";
        pages = length;
    }

    private void OnFocus()
    {
        onFocus = true;

        background.color = manager.secondaryColor;
        page.color = manager.primaryColor;
        previousButton.color = manager.primaryColor;
        nextButton.color = manager.primaryColor;

        controls.UI.Navigation.performed += (ctx) => OnChange(ctx.ReadValue<Vector2>());
    }

    private void OnBlur()
    {
        onFocus = false;

        background.color = manager.primaryColor;
        page.color = manager.secondaryColor;
        previousButton.color = manager.secondaryColor;
        nextButton.color = manager.secondaryColor;

        controls.UI.Navigation.performed -= (ctx) => OnChange(ctx.ReadValue<Vector2>());
    }

    public void UpdatePages()
    {
        for (int index = 0; index < pages; index++)
        {
            blocksList[index].SetActive(index == paged - 1);
        }

        page.text = paged + "/" + pages + "";
        Canvas.ForceUpdateCanvases();
    }

    private void ToNext()
    {
        if(paged == pages) return;
        if(paged == 0)
        {
            paged++;
            return;
        }

        paged++;

        UpdatePages();
    }

    private void ToPrev()
    {
        if(paged <= 1) return;

        paged--;

        UpdatePages();
    }

    private void OnChange(Vector2 direction)
    {
        if (!onFocus) return;

        if (direction.x > 0.5f)
        {
            ToNext();
        } else if(direction.x < -0.5f)
        {
            ToPrev();
        }
    }
}
