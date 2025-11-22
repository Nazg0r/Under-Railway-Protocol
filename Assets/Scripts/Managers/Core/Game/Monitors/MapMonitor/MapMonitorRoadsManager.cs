using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class MapMonitorRoadsManager : MonoBehaviour
{
	[SerializeField] private Sprite _activeLine;
	[SerializeField] private Sprite _activeSmallLine;
	[SerializeField] private Sprite _activeRight;
	[SerializeField] private Sprite _activeLeft;

	[SerializeField] private Sprite _passiveLine;
	[SerializeField] private Sprite _passiveSmallLine;
	[SerializeField] private Sprite _passiveRight;
	[SerializeField] private Sprite _passiveLeft;

	[SerializeField] private Sprite _anomalyLine;
	[SerializeField] private Sprite _anomalySmallLine;
	[SerializeField] private Sprite _anomalyRight;
	[SerializeField] private Sprite _anomalyLeft;

	private const float SpriteRotationBase = 270f;
	private const float RightRotationOffset = 315f;
	private const float LeftRotationOffset = 225f;
	private const float CurveScale = 0.84f;
	private readonly float _smallForwardScale = 1;

	private readonly int _coordsScale = 64;
	private readonly Vector2 _spriteSize = new(64, 64);
	private readonly Vector2 _baseAnchor = new(0.5f, 0.5f);
	private readonly Vector2 _basePivot = new(0.5f, 0.5f);

	private Dictionary<DirectionType, RoadPieceConfig> _roadPieceConfigs;
	private Dictionary<RailroadStatus, Dictionary<string, Sprite>> _spriteMap;

	public readonly Dictionary<PathMap, List<RailroadConfig>> RailroadsConfigs = new();

	public void Initialize()
	{
		ObjectResolver.Register(this);

		_roadPieceConfigs = new()
		{
			{ DirectionType.Forward, new RoadPieceConfig(_passiveLine, SpriteRotationBase, Vector3.one) },
			{ DirectionType.SmallForward, new RoadPieceConfig(_passiveSmallLine, SpriteRotationBase, new Vector3(1, _smallForwardScale, 1)) },
			{ DirectionType.Right, new RoadPieceConfig(_passiveRight, RightRotationOffset, Vector3.one * CurveScale) },
			{ DirectionType.Left, new RoadPieceConfig(_passiveLeft, LeftRotationOffset, Vector3.one * CurveScale) }
		};

		_spriteMap = new Dictionary<RailroadStatus, Dictionary<string, Sprite>>
		{
			{
				RailroadStatus.Passive,
				new Dictionary<string, Sprite>
				{
					{ nameof(DirectionType.Forward), _passiveLine },
					{ nameof(DirectionType.Left), _passiveLeft },
					{ nameof(DirectionType.Right), _passiveRight },
					{ nameof(DirectionType.SmallForward), _passiveSmallLine },
				}
			},
			{
				RailroadStatus.Active,
				new Dictionary<string, Sprite>
				{
					{ nameof(DirectionType.Forward), _activeLine },
					{ nameof(DirectionType.Left), _activeLeft },
					{ nameof(DirectionType.Right), _activeRight },
					{ nameof(DirectionType.SmallForward), _activeSmallLine },
				}
			},
			{
				RailroadStatus.Anomaly,
				new Dictionary<string, Sprite>
				{
					{ nameof(DirectionType.Forward), _anomalyLine },
					{ nameof(DirectionType.Left), _anomalyLeft },
					{ nameof(DirectionType.Right), _anomalyRight },
					{ nameof(DirectionType.SmallForward), _anomalySmallLine },
				}
			}
		};
	}
	public void CreateRailroads(Transform parent, PathMap map)
	{
		var railroads = map
			.GetBranches()
			.SelectMany(b => b.Roads)
			.Distinct()
			.ToList();

		var railroadsNumber = railroads.Count;

		if (railroads == null || railroadsNumber < 1)
			Debug.LogError("There are no railroads in the map instance");

		RailroadsConfigs[map] = new(railroadsNumber);

		for (int i = 0; i < railroadsNumber; i++)
		{
			var container = MapMonitorHelper.CreateContainer($"Railroad-{i}", parent, _baseAnchor, _basePivot);

			DrawRoadSprites(container, railroads[i]);
			var config = PrepareRailroadConfig(container, railroads[i]);
			RailroadsConfigs[map].Add(config);
		}
	}

	public void UpdateRailroadSprite(RailroadConfig config)
	{
		var lookup = _spriteMap[config.Status];

		foreach (var spriteObj in config.RailroadSprites)
		{
			if (lookup.TryGetValue(spriteObj.name, out var sprite))
				spriteObj.GetComponent<Image>().sprite = sprite;
		}
	}

	private RailroadConfig PrepareRailroadConfig(GameObject railroadsContainer, Railroad railroad)
	{
		var parent = railroadsContainer.transform;
		List<GameObject> railroadSpriteObjects = new(parent.childCount);
		railroadSpriteObjects.AddRange(from Transform child in parent select child.gameObject);

		var railroadBranches = new List<Branch>()
		{
			railroad.FirstBranchNode,
			railroad.LastBranchNode
		};

		return new RailroadConfig(railroadSpriteObjects, railroadBranches, RailroadStatus.Passive, railroad);
	}

	private void DrawRoadSprites(GameObject railroadsContainer, Railroad railroad)
	{
		foreach (var piece in railroad.RailroadStructure)
		{
			if (_roadPieceConfigs.TryGetValue(piece.Type, out var config))
				DrawRoadSprite(piece.Type.ToString(), railroadsContainer, piece, config);
		}
	} 

	private void DrawRoadSprite(string name, GameObject parent, RailroadPiece piece, RoadPieceConfig conf)
	{
		var spriteObj = CreateUIElement(name, conf.Sprite, typeof(Image));
		spriteObj.transform.SetParent(parent.transform, false);

		var rect = spriteObj.GetComponent<RectTransform>();
		rect.pivot = new Vector2(0.5f, 0);

		var angle = new Vector3(0, 0, conf.BaseRotation + piece.Rotation.eulerAngles.y);
		rect.Rotate(angle);

		var position = piece.StartPoint * _coordsScale;
		rect.anchoredPosition = position;
		rect.sizeDelta = _spriteSize;
		rect.localScale = conf.Scale;
	}

	private GameObject CreateUIElement(string name, Sprite sprite, Type type)
	{
		var obj = new GameObject(name, typeof(RectTransform), type);
		obj.GetComponent<Image>().sprite = sprite;
		return obj;
	}

	private struct RoadPieceConfig
	{
		public readonly Sprite Sprite;
		public readonly float BaseRotation;
		public readonly Vector3 Scale;

		public RoadPieceConfig(Sprite sprite, float rotation, Vector3 scale)
		{
			Sprite = sprite;
			BaseRotation = rotation;
			Scale = scale;
		}
	}
}