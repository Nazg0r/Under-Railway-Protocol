using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapMonitorChoiceMenuManager : MonoBehaviour
{
	private GameManager _gameManager;
	private MapMonitorEventBus _eventBus;

	private readonly string _mainColor = "#AC1010";
	private readonly float _titleFontSize = 96f;
	private readonly float _buttonsFontSize = 48f;
	private readonly float _buttonsSpacing = 20f;
	private readonly int _buttonsPadding = 20;
	private readonly Vector2 _basePivot = new(0.5f, 0.5f);

	public GameObject ChoiceMenu { get; private set; }

	public Station CurrentStation;

	public List<GameObject> ChoiceMenuButtons = new();

	public void Initialize()
	{
		ObjectResolver.Register(this);
		_gameManager = ObjectResolver.Resolve<GameManager>();
		_eventBus = ObjectResolver.Resolve<MapMonitorEventBus>();

		CurrentStation = _gameManager.CurrentStation;
	}

	public void CreatePathMapChoiceMenu(Transform parent)
	{
		ChoiceMenu = MapMonitorHelper.CreateContainer("ChoiceMenuContainer", parent, new(0.5f, 0.5f), _basePivot);
		MapMonitorHelper.SetStretchedAnchor(ChoiceMenu);
		CreatePathMapChoiceMenuTitle(ChoiceMenu);
		CreatePathMapChoiceMenuButtons(ChoiceMenu);
		EventSystem.current.SetSelectedGameObject(ChoiceMenuButtons.First().gameObject);
	}

	private void CreatePathMapChoiceMenuTitle(GameObject parent)
	{
		GameObject titleObj = new GameObject("Title", typeof(RectTransform));
		var title = titleObj.AddComponent<TextMeshProUGUI>();

		title.transform.SetParent(parent.transform, false);

		var rect = titleObj.GetComponent<RectTransform>();

		rect.sizeDelta = new Vector2(900f, 300f);
		rect.anchorMin = new Vector2(0.5f, 0.9f);
		rect.anchorMax = new Vector2(0.5f, 0.9f);
		rect.pivot = _basePivot;

		title.text = "Choose next station";
		title.color = MapMonitorHelper.ParseColor(_mainColor);
		title.fontSize = _titleFontSize;
		title.alignment = TextAlignmentOptions.Center;
	}

	private void CreatePathMapChoiceMenuButtons(GameObject parent)
	{
		var neighborStations = CurrentStation.GetNeighborStations();

		var menuButtons = MapMonitorHelper.CreateContainer("MenuButtons", parent.transform, new(0.5f, 0), _basePivot);

		var containerRect = menuButtons.GetComponent<RectTransform>();
		containerRect.anchoredPosition = new Vector2(0, 250f);
		containerRect.sizeDelta = new Vector2(360f, 600f);


		var layout = menuButtons.AddComponent<VerticalLayoutGroup>();
		layout.padding = new RectOffset(_buttonsPadding, _buttonsPadding, _buttonsPadding, _buttonsPadding);
		layout.spacing = _buttonsSpacing;

		layout.childAlignment = TextAnchor.UpperCenter;
		layout.childControlHeight = true;
		layout.childControlWidth = true;
		layout.childForceExpandHeight = false;
		layout.childForceExpandWidth = true;

		var fitter = menuButtons.AddComponent<ContentSizeFitter>();
		fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


		foreach (var station in neighborStations)
		{
			var button = CreateMenuButton(menuButtons.transform, station);
			ChoiceMenuButtons.Add(button);
		}

		SetNavigation(ChoiceMenuButtons.Select(b => b.GetComponent<Button>()).ToList());
	}

	private GameObject CreateMenuButton(Transform parent, Station station)
	{
		GameObject buttonObj = new GameObject("Choice_" + station.Name, typeof(RectTransform));
		buttonObj.transform.SetParent(parent, false);

		var buttonRect = buttonObj.GetComponent<RectTransform>();
		buttonRect.sizeDelta = new Vector2(280, 60f);

		var layoutElement = buttonObj.AddComponent<LayoutElement>();
		layoutElement.preferredHeight = 70f;
		layoutElement.preferredWidth = 700f;

		buttonObj.AddComponent<Image>();

		Color mainColor = MapMonitorHelper.ParseColor(_mainColor);

		Button button = buttonObj.AddComponent<Button>();
		var colors = button.colors;
		colors.normalColor = Color.black;
		colors.highlightedColor = Color.black;
		colors.selectedColor = mainColor;
		colors.pressedColor = Color.black;
		colors.colorMultiplier = 1f;
		colors.fadeDuration = 0f;
		button.colors = colors;

		var navigation = button.navigation;
		navigation.mode = Navigation.Mode.Automatic;
		button.navigation = navigation;

		GameObject textObj = new GameObject("Text", typeof(RectTransform));
		textObj.transform.SetParent(buttonObj.transform, false);
		MapMonitorHelper.SetStretchedAnchor(textObj);

		var textComponent = textObj.AddComponent<TextMeshProUGUI>();
		textComponent.text = station.Name;
		textComponent.color = mainColor;
		textComponent.fontSize = _buttonsFontSize;
		textComponent.alignment = TextAlignmentOptions.Center;
		textComponent.fontStyle = FontStyles.Bold;

		var selectListener = buttonObj.AddComponent<ButtonSelectListener>();
		selectListener.OnSelected += () => textComponent.color = Color.black;
		selectListener.OnSelected += () => _eventBus.onStationSelected.Invoke(station);
		selectListener.OnDeselected += () => textComponent.color = mainColor;
		selectListener.OnDeselected += () => _eventBus.onStationDeselected.Invoke(station);

		button.onClick.AddListener(() => _eventBus.onStationChosen.Invoke(station));
		button.onClick.AddListener(() => textComponent.color = mainColor);

		return buttonObj;
	}

	private void SetNavigation(List<Button> buttons)
	{
		if (buttons == null || buttons.Count == 0)
			return;

		for (int i = 0; i < buttons.Count; i++)
		{
			var navigation = buttons[i].navigation;
			navigation.mode = Navigation.Mode.Explicit;

			navigation.selectOnDown = i < buttons.Count - 1 ? buttons[i + 1] : null;

			navigation.selectOnUp = i > 0 ? buttons[i - 1] : null;

			buttons[i].navigation = navigation;
		}
	}
}
