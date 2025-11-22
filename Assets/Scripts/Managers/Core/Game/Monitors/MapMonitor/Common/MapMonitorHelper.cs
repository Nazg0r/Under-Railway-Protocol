using UnityEngine;

public static class MapMonitorHelper
{
	public static GameObject CreateContainer(string name, Transform parent, Vector2 anchor, Vector2 pivot)
	{
		var obj = new GameObject(name, typeof(RectTransform));
		obj.transform.SetParent(parent, false);
		SetupAnchor(obj.GetComponent<RectTransform>(), anchor, pivot);
		return obj;
	}

	public static void SetupAnchor(RectTransform rect, Vector2 anchor, Vector2 pivot)
	{
		rect.anchorMin = anchor;
		rect.anchorMax = anchor;
		rect.pivot = pivot;
	}

	public static void SetStretchedAnchor(GameObject container)
	{
		var rect = container.GetComponent<RectTransform>();
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}

	public static Color ParseColor(string hexColor)
	{
		return ColorUtility.TryParseHtmlString(hexColor, out var color)
			? color
			: Color.white;
	}
}