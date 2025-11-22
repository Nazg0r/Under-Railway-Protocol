using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class MapMonitorHudManager : MonoBehaviour
{
	private readonly Vector2 _basePivot = new(0.5f, 0.5f);
	private readonly string _mainColor = "#AC1010";
	private readonly int _hudPadding = 20;
	private readonly float _hintsSpacing = 10f;
	private readonly float _hintsFontSize = 36f;
	private readonly float _textHeght = 50f;

	private readonly Dictionary<string, string> _hints = new()
	{
		{"Back", "Q - Back"},
		{"Build", "ENTER - Build path"},
		// {"ZoomIn", "Z - Zoom In"},
		// {"ZoomOut", "X - Zoom Out"},
		{"Navigate", "W A S D - Navigate"},
	};

	private MapMonitorEventBus _eventBus;

	public GameObject Hud { get; private set; }

	public void Initialize()
	{
		ObjectResolver.Register(this);
		_eventBus = ObjectResolver.Resolve<MapMonitorEventBus>();
	}

	public GameObject CreatePathMapHud(Transform parent)
	{
		Hud = MapMonitorHelper.CreateContainer("Hud", parent, new(0.5f, 0.5f), _basePivot);
		MapMonitorHelper.SetStretchedAnchor(Hud);

		GameObject hints = CreateHintsContainer(Hud.transform);
		Color textColor = MapMonitorHelper.ParseColor(_mainColor);

		foreach (var (objName, content) in _hints)
			CreateLabel(hints.transform, objName, content, textColor);

		CreateFuelRequiredInfo(Hud.transform, textColor);

		return Hud;
	}

	private GameObject CreateHintsContainer(Transform parent)
	{
		GameObject hints = MapMonitorHelper.CreateContainer("Hints", parent, new(0f, 0f), _basePivot);

		var hintsRect = hints.GetComponent<RectTransform>();

		var hintsHeight = _hints.Count * _textHeght + 2 * _hudPadding + (_hints.Count - 1) * _hintsSpacing;

		hintsRect.anchoredPosition = new Vector2(220f, hintsHeight/2);
		hintsRect.sizeDelta = new Vector2(400f, hintsHeight);

		var layout = hints.AddComponent<VerticalLayoutGroup>();
		layout.padding = new RectOffset(_hudPadding, _hudPadding, _hudPadding, _hudPadding);
		layout.spacing = _hintsSpacing;

		layout.childAlignment = TextAnchor.LowerLeft;
		layout.childControlHeight = true;
		layout.childControlWidth = true;
		layout.childForceExpandHeight = false;
		layout.childForceExpandWidth = true;

		var fitter = hints.AddComponent<ContentSizeFitter>();
		fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

		return hints;
	}

	private void CreateLabel(Transform parent, string objName, string content, Color textColor)
	{
		GameObject labelObj = new GameObject(objName, typeof(RectTransform));
		labelObj.transform.SetParent(parent, false);
		var label = labelObj.AddComponent<TextMeshProUGUI>();

		var layoutElement = labelObj.AddComponent<LayoutElement>();
		layoutElement.preferredHeight = _textHeght;
		layoutElement.preferredWidth = 280f;

		var zoomHintRect = labelObj.GetComponent<RectTransform>();
		zoomHintRect.sizeDelta = new Vector2(280f, _textHeght);

		label.text = content;
		label.color = textColor;
		label.fontSize = _hintsFontSize;
		label.fontStyle = FontStyles.Bold;
	}

	private void CreateFuelRequiredInfo(Transform parent, Color textColor)
	{
		GameObject textObj = new GameObject("FuelRequired", typeof(RectTransform));
		textObj.transform.SetParent(parent, false);
		var content = textObj.AddComponent<TextMeshProUGUI>();

		var textRect = textObj.GetComponent<RectTransform>();
		textRect.anchorMin = Vector2.right;
		textRect.anchorMax = Vector2.right;
		textRect.anchoredPosition = new Vector2(-165f - _hudPadding, _textHeght / 2 + _hudPadding);
		textRect.sizeDelta = new Vector2(330f, _textHeght);

		content.text = "Fuel required:";
		content.color = textColor;
		content.fontSize = _hintsFontSize;
		content.fontStyle = FontStyles.Bold;

		_eventBus.onRequiredFuelCalculated.AddListener(fuelCount =>
			content.text = $"Fuel required: {fuelCount}");
	}
}