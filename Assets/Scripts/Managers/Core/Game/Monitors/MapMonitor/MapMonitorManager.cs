using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class MapMonitorManager : MonoBehaviour
{
	[SerializeField] private GameObject MapMonitor;

	private readonly int _coordsScale = 64;
	private readonly Vector2 _basePivot = new(0.5f, 0.5f);
	private readonly Vector3 _pathMapRotation = new(0, 0, 90);
	private readonly Vector3 _pathMapChoiceMenuScale = new(0.6f, 0.6f, 0.6f);
	private readonly float _pathMapChoiceMenuVerticalDelta = 100;
	private readonly float _pathMapPadding = 50;

	private GameManager _gameManager;
	private MapGenerationManager _mapGenerationManager;
	private MapMonitorRoadsManager _mapMonitorRoadsManager;
	private MapMonitorPointsManager _mapMonitorPointsManager;
	private MapMonitorChoiceMenuManager _mapMonitorChoiceMenuManager;
	private MapMonitorHudManager _mapMonitorHudManager;

	private Canvas _canvas;
	private RectTransform _canvasRect;
	private RectTransform _pathMapTransformerRect;

	public Dictionary<PathMap, GameObject> PathMapContainers = new();

	public void Initialize()
	{
		ObjectResolver.Register(this);

		_gameManager = ObjectResolver.Resolve<GameManager>();
		_mapGenerationManager = ObjectResolver.Resolve<MapGenerationManager>();
		_mapMonitorRoadsManager = ObjectResolver.Resolve<MapMonitorRoadsManager>();
		_mapMonitorPointsManager = ObjectResolver.Resolve<MapMonitorPointsManager>();
		_mapMonitorHudManager = ObjectResolver.Resolve<MapMonitorHudManager>();
		_mapMonitorChoiceMenuManager = ObjectResolver.Resolve<MapMonitorChoiceMenuManager>();

		_canvas = MapMonitor.GetComponentInChildren<Canvas>();
		_canvasRect = _canvas.GetComponent<RectTransform>();
	}

	public void CreateMonitorContent()
	{
		DrawBackground(_canvas.transform);
		_mapMonitorChoiceMenuManager.CreatePathMapChoiceMenu(_canvas.transform);
		_mapMonitorHudManager.CreatePathMapHud(_canvas.transform).SetActive(false);

		var pathMapTransformer = CreateMapTransformer(_canvas.transform);
		_pathMapTransformerRect = pathMapTransformer.GetComponent<RectTransform>();
		PathMapContainers = CreatePathMapSchemas(pathMapTransformer);
		SetPathMapOnChoicePosition();

		// TODO Change implementation here
		PathMapContainers[_gameManager.CurrentStation.Paths.First()].SetActive(true);
	}

	public void SetPathMapScale(Vector3 vector) =>
		_pathMapTransformerRect.localScale = vector;

	public void MovePathMapVertically(float delta) =>
		_pathMapTransformerRect.anchoredPosition += new Vector2(0, delta);

	public void MovePathMapHorizontally(float delta) =>
		_pathMapTransformerRect.anchoredPosition += new Vector2(delta, 0);

	public void SetPathMapOnChoicePosition()
	{
		SetPathMapScale(_pathMapChoiceMenuScale);
		MovePathMapVertically(_pathMapChoiceMenuVerticalDelta);
	}

	public void SetPathMapOnChosenPosition()
	{
		SetPathMapScale(Vector3.one * 0.9f);
		MovePathMapVertically(-_pathMapChoiceMenuVerticalDelta);
	}

	private Dictionary<PathMap, GameObject> CreatePathMapSchemas(GameObject transformer)
	{
		var pathMaps = _mapGenerationManager.Map.GetPathMaps();
		var pathMapNumber = pathMaps.Count;
		Dictionary<PathMap, GameObject> schemas = new(pathMapNumber);

		for (int i = 0; i < pathMapNumber; i++)
		{
			var pathMap = pathMaps[i];
			var pathMapContainer = MapMonitorHelper.CreateContainer($"PathMapContainer{i}", _canvas.transform, new(0, 0), _basePivot);

			NormalizePosition(pathMapContainer, pathMap);

			_mapMonitorRoadsManager.CreateRailroads(pathMapContainer.transform, pathMap);
			_mapMonitorPointsManager.CreateBranchPoints(pathMapContainer.transform, pathMap);

			pathMapContainer.transform.SetParent(transformer.transform, false);
			schemas[pathMap] = pathMapContainer;
			pathMapContainer.SetActive(false);
		}

		return schemas;
	}

	private void DrawBackground(Transform parent)
	{
		GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
		background.transform.SetParent(parent, false);
		MapMonitorHelper.SetStretchedAnchor(background);
		background.GetComponent<Image>().color = Color.black;
	}

	private GameObject CreateMapTransformer(Transform parent)
	{
		var transformer = new GameObject("MapTransformer", typeof(RectTransform));
		transformer.transform.SetParent(parent, false);
		transformer.AddComponent<RectMask2D>();
		var rect = transformer.GetComponent<RectTransform>();
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = new Vector2(_pathMapPadding, _pathMapPadding);
		rect.offsetMax = new Vector2(-_pathMapPadding, -_pathMapPadding);

		return transformer;
	}

	private void NormalizePosition(GameObject container, PathMap map)
	{
		var rect = container.GetComponent<RectTransform>();
		rect.Rotate(_pathMapRotation);

		var dimensions = map!.DefineDimensions();
		dimensions.K = _coordsScale;
		CentralizePathMap(rect, dimensions, _canvasRect);
	}

	private void CentralizePathMap(RectTransform container, PathMapDimensions dimensions, RectTransform parent)
	{
		var relatedCenterWidth = parent.rect.width / 2 - _pathMapPadding;
		var relatedCenterHeight = parent.rect.height / 2 - _pathMapPadding;


		var absMaxX = Mathf.Abs(dimensions.MaxX);
		var absMinX = Mathf.Abs(dimensions.MinX);
		var absMaxY = Mathf.Abs(dimensions.MaxY);
		var absMinY = Mathf.Abs(dimensions.MinY);

		var pathMapAbsCenterWidth = (absMaxY + absMinY) / 2;
		var pathMapAbsCenterHeight = (absMaxX + absMinX) / 2;

		if (absMaxY > absMinY)
			container.anchoredPosition = new Vector2(relatedCenterWidth + pathMapAbsCenterWidth - absMinY, 0);
		else if (absMaxY < absMinY)
			container.anchoredPosition = new Vector2(relatedCenterWidth - pathMapAbsCenterWidth + absMaxY, 0);
		else
			container.anchoredPosition = new Vector2(relatedCenterWidth, 0);

		container.anchoredPosition += new Vector2(0, relatedCenterHeight - pathMapAbsCenterHeight);
	}
}