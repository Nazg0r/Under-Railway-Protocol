using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[RequireComponent(typeof(LocalEventManager))]
public class MonitorCounter : MonoBehaviour
{
    private LocalEventManager events;
    private GameInputs controls;

    public GameObject label;
    public Image background;
    public Button minus;
    private TMP_Text minusText;
    public Button plus;
    private TMP_Text plusText;
    public TMP_Text input;
    public int value = 0;

    public int maxValue = 100;
    private Coroutine repeatChangeInput;

    Color primaryColor = Color.black;
    Color secondaryColor = new(172f / 255f, 16f / 255f, 16f / 255f);

    private bool isActive = false;
    private bool isFocus = false;
    private float blinkInterval = 0.25f;
    private Coroutine blinkRoutine;

    private enum ChangeAction
    {
        Minus,
        Plus
    }

    void Awake()
    {
        controls = new GameInputs();

        events = GetComponent<LocalEventManager>();
        events.Subscribe("OnFocus", OnFocus);
        events.Subscribe("OnBlur", OnBlur);
        events.Subscribe("OnDisactive", OnDisactive);
    }

    void Start()
    {
        minusText = minus.GetComponentInChildren<TMP_Text>();
        plusText = plus.GetComponentInChildren<TMP_Text>();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.UI.Submit.performed += ctx => OnActive();
        controls.UI.NumberPad.performed += OnInput;
        controls.UI.Delete.performed += OnDelete;
        controls.UI.Minus.performed += (ctx) => OnMinus();
        controls.UI.Plus.performed += (ctx) => OnPlus();
        controls.UI.Minus.canceled += (ctx) => OnStopChanging();
        controls.UI.Plus.canceled += (ctx) => OnStopChanging();
    }

    void OnDisable()
    {
        controls.Disable();
        controls.UI.Submit.performed -= ctx => OnActive();
        controls.UI.NumberPad.performed -= OnInput;
        controls.UI.Delete.performed -= OnDelete;
        controls.UI.Minus.performed -= (ctx) => OnMinus();
        controls.UI.Plus.performed -= (ctx) => OnPlus();
        controls.UI.Minus.canceled -= (ctx) => OnStopChanging();
        controls.UI.Plus.canceled -= (ctx) => OnStopChanging();
    }

    private void InvokeChangeEvent()
    {
        value = int.Parse(input.text);
        events.Invoke("OnChange");
    }

    public void OnActive()
    {
        if (isActive)
        {
            StopBlink();
        }
        else if (isFocus)
        {
            StartBlink();
        }
    }


    public void OnDisactive()
    {
        StopBlink();
    }

    public void StartBlink()
    {
        if (isActive) return;
        isActive = true;
        blinkRoutine = StartCoroutine(Blink());
        events.Invoke("OnActive");
    }

    public void StopBlink()
    {
        if (!isActive) return;
        isActive = false;

        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        input.enabled = true;
        events.Invoke("OnDisactive");
        InvokeChangeEvent();
    }

    private IEnumerator Blink()
    {
        while (isActive)
        {
            input.enabled = !input.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void OnInput(InputAction.CallbackContext context)
    {
        if (!isActive) return;

        var keyControl = context.control as KeyControl;
        if (keyControl == null) return;

        string keyName = keyControl.keyCode.ToString();

        char lastChar = keyName[keyName.Length - 1];
        if (char.IsDigit(lastChar))
        {
            int number = lastChar - '0';
            if (input.text == "0") input.text = "" + number;
            else input.text += number;

            var valueInt = int.Parse(input.text);
            if (valueInt > maxValue) input.text = maxValue.ToString();

            InvokeChangeEvent();
        }
    }

    private void OnDelete(InputAction.CallbackContext context)
    {
        if (!isActive) return;

        string value = input.text;
        string result = value.Substring(0, value.Length - 1);
        if (result.Length == 0)
        {
            input.text = "0";
        }
        else
        {
            input.text = result;
        }

        InvokeChangeEvent();
    }

    private void OnPlus()
    {
        if (!isActive) return;

        repeatChangeInput ??= StartCoroutine(ChangeInputWithDelay(ChangeAction.Plus));
    }

    private void OnMinus()
    {
        if (!isActive) return;

        repeatChangeInput ??= StartCoroutine(ChangeInputWithDelay(ChangeAction.Minus));
    }

    private void OnStopChanging()
    {
        if (!isActive) return;

        if (repeatChangeInput != null)
        {
            StopCoroutine(repeatChangeInput);
            repeatChangeInput = null;
        }
    }

    private void ChangeInput(ChangeAction action)
    {
        if (!isActive) return;
        if (action == ChangeAction.Minus)
        {
            var valueInt = int.Parse(input.text);
            if (valueInt <= 0) input.text = "0";
            else input.text = valueInt - 1 + "";
        }
        else
        {
            var valueInt = int.Parse(input.text);
            if (valueInt < maxValue) input.text = valueInt + 1 + "";
            else input.text = maxValue.ToString();
        }

        InvokeChangeEvent();
    }

    private IEnumerator ChangeInputWithDelay(ChangeAction action)
    {
        while (true)
        {
            ChangeInput(action);
            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                ChangeInput(action);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void SetLabel(string text)
    {
        TMP_Text labelValue = label.GetComponent<TMP_Text>();
        labelValue.text = text;
    }

    public void OnFocus()
    {
        background.color = secondaryColor;
        minusText.color = primaryColor;
        plusText.color = primaryColor;
        input.color = primaryColor;

        isFocus = true;
    }

    public void OnBlur()
    {
        background.color = primaryColor;
        minusText.color = secondaryColor;
        plusText.color = secondaryColor;
        input.color = secondaryColor;

        isFocus = false;

        StopBlink();
    }

    public void Reset()
    {
        input.text = "0";
        value = 0;
    }
}
