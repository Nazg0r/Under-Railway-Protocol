using System.Collections.Generic;
using UnityEngine;

public class MapMonitorAnimationController : MonoBehaviour
{
	public void Initialize()
	{
		ObjectResolver.Register(this);
	}

	public void AddAnimationsToPoints(IEnumerable<PointConfig> pointConfigs)
	{
		foreach (var conf in pointConfigs)
		{
			conf.Point.AddComponent<ImageRotator>().enabled = false;
			conf.Point.AddComponent<ImagePulse>().enabled = false;
			conf.OnStatusChanged += OnPointStatusChanged;
		}
	}

	private void OnPointStatusChanged(PointStatus status, GameObject point)
	{
		switch (status)
		{
			case PointStatus.NextChoice:
				ToggleAnimation<ImageRotator>(point, true);
				break;

			case PointStatus.OnFocus:
				ToggleAnimation<ImagePulse>(point, true);
				break;

			default:
				ToggleAnimation<ImageRotator>(point, false);
				ToggleAnimation<ImagePulse>(point, false);
				break;
		}
	}

	private void ToggleAnimation<T>(GameObject point, bool enable) where T : MonoBehaviour
	{
		var comp = point.GetComponent<T>();
		if (comp == null) return;

		comp.enabled = enable;

		if (enable) return;

		if (typeof(T) == typeof(ImageRotator))
			point.transform.localRotation = Quaternion.identity;

		if (typeof(T) == typeof(ImagePulse))
			point.transform.localScale = Vector3.one;
	}
}