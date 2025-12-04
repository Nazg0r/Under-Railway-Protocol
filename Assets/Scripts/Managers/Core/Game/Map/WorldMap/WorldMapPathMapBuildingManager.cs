using System.Collections.Generic;
using UnityEngine;

public class WorldMapPathMapBuildingManager : MonoBehaviour
{
	[SerializeField] private GameObject ForwardPrefab;
	[SerializeField] private GameObject SmallForwardPrefab;
	[SerializeField] private GameObject RightPrefab;
	[SerializeField] private GameObject LeftPrefab;

	private WorldMapBranchBuildingManager _branchBuildingManager;

	private readonly int _coordsScale = 5;

	private const float RotationBase = 90f;
	private const float RightRotationOffset = 135f;
	private const float LeftRotationOffset = 225f;

	private Dictionary<DirectionType, GameObject> _directionToPrefab;

	public void Initialize()
	{
		ObjectResolver.Register(this);
		_branchBuildingManager = ObjectResolver.Resolve<WorldMapBranchBuildingManager>();

		_directionToPrefab = new()
		{
			{ DirectionType.Forward, ForwardPrefab },
			{ DirectionType.SmallForward, SmallForwardPrefab },
			{ DirectionType.Left, LeftPrefab },
			{ DirectionType.Right, RightPrefab },
		};
	}

	// private void CreateRailroads(PathMap pathMap,Transform parent)
	// {
	// 	var pathMaps = _mapManager.Map.GetPathMaps().Take(1);
	//
	// 	foreach (var pathMap in pathMaps)
	// 		CreatePathMap(pathMap, parent);
	// }

	public void CreatePathMap(PathMap pathMap, Transform parent)
	{
		GameObject pathMapContainer = new GameObject($"PathMap-{pathMap.Id}");
		pathMapContainer.transform.SetParent(parent.transform, false);

		var railroads = pathMap.GetRailroads();

		for (var i = 0; i < railroads.Count; i++)
		{
			var railroad = railroads[i];
			var name = $"Railroad{i}";
			CreateRailroad(railroad, name, pathMapContainer.transform);
		}


		_branchBuildingManager.CreateBranches(pathMap, pathMapContainer.transform);
	}

	private void CreateRailroad(Railroad railroad, string railroadName, Transform parent)
	{
		GameObject railroadContainer = new GameObject(railroadName);
		railroadContainer.transform.SetParent(parent, false);

		var pieces = railroad.RailroadStructure;

		for (int i = 1; i < pieces.Count - 1; i++)
		{
			var piece = pieces[i];
			CreateRailroadPiece(piece, railroadContainer.transform);
		}
	}

	private void CreateRailroadPiece(RailroadPiece piece, Transform parent)
	{
		Vector3 pos;
		Quaternion angle;

		if (piece.Type is DirectionType.Left or DirectionType.Right)
		{
			pos = GetCurvePosition(piece);
			angle = GetCurveRotation(piece);
		}
		else
		{
			pos = GetLinePosition(piece);
			angle = GetLineRotation(piece);
		}

		var prefab = _directionToPrefab[piece.Type];

		var pieceObj = Instantiate(prefab, pos, angle);
		pieceObj.transform.SetParent(parent);
	}

	private Vector3 GetLinePosition(RailroadPiece piece)
	{
		var x = (piece.StartPoint.x + piece.EndPoint.x) / 2 * _coordsScale;
		var z = (piece.StartPoint.y + piece.EndPoint.y) / 2 * _coordsScale;
		return new Vector3(x, 0, z);
	}

	private Vector3 GetCurvePosition(RailroadPiece piece)
	{
		var x = piece.StartPoint.x * _coordsScale;
		var z = piece.StartPoint.y * _coordsScale;
		return new Vector3(x, 0, z);
	}

	private Quaternion GetLineRotation(RailroadPiece piece)
	{
		return Mathf.Approximately(Mathf.Abs(piece.Rotation.eulerAngles.y) / 45 % 2, 1)
			? piece.Rotation * Quaternion.Euler(0f, RotationBase, 0f)
			: piece.Rotation * Quaternion.Euler(0f, 2 * RotationBase, 0f);
	}

	private Quaternion GetCurveRotation(RailroadPiece piece)
	{
		return piece.Type switch
		{
			DirectionType.Left => Quaternion.Euler(0, -1 * (piece.Rotation.eulerAngles.y + LeftRotationOffset), 0),
			DirectionType.Right => Quaternion.Euler(0, -1 * (piece.Rotation.eulerAngles.y + RightRotationOffset), 0),
			_ => Quaternion.identity
		};
	}
}