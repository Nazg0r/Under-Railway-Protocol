using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class MapMonitorPointsManager : MonoBehaviour 
{
	[SerializeField] private Sprite PassivePoint;
	[SerializeField] private Sprite FocusPoint;
	[SerializeField] private Sprite NextChoice;
	[SerializeField] private Sprite Active;

	private readonly int _coordsScale = 64;
	private readonly Vector2 _branchPointSize = new(80, 80);
	private readonly Vector2 _baseAnchor = new(0.5f, 0.5f);
	private readonly Vector2 _basePivot = new(0.5f, 0.5f);

	public readonly Dictionary<PathMap, List<PointConfig>> PointsConfigs = new();
	public void Initialize()
	{
		ObjectResolver.Register(this);
	}

	public void CreateBranchPoints(Transform parent, PathMap map)
	{
		var branches = map.GetBranches();
		var branchesNumber = branches.Count;

		if (branches == null || branchesNumber < 1)
			Debug.Log("There are no branches in the map instance");

		var container = MapMonitorHelper.CreateContainer("Points", parent, _baseAnchor, _basePivot);
		PointsConfigs[map] = new(branchesNumber);

		foreach (var branch in branches)
		{
			var point = DrawBranchPoint(branch, container.transform);
			PointsConfigs[map].Add(point);
		}
	}
	public void UpdatePointSprite(PointConfig config)
	{
		var pointSprite = config.Point.GetComponent<Image>();

		pointSprite.sprite = config.Status switch
		{
			PointStatus.Active => Active,
			PointStatus.OnFocus => FocusPoint,
			PointStatus.NextChoice => NextChoice,
			PointStatus.Passive => PassivePoint,
			PointStatus.Previous => Active,
			_ => PassivePoint
		};
	}

	private PointConfig DrawBranchPoint(Branch branch, Transform parent)
	{
		GameObject point = new GameObject("PassivePoint" + branch.Name.Last(), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
		point.transform.SetParent(parent, false);

		Image image = point.GetComponent<Image>();
		image.sprite = PassivePoint;
		image.preserveAspect = true;

		var rect = point.GetComponent<RectTransform>();

		MapMonitorHelper.SetupAnchor(rect, _baseAnchor, _basePivot);
		var position = branch.BasePoint * _coordsScale;
		rect.anchoredPosition = position;
		rect.sizeDelta = _branchPointSize;

		return new PointConfig(point, branch, PointStatus.Passive);
	}
}
