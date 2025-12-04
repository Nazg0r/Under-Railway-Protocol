using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMapBranchBuildingManager : MonoBehaviour
{
	[SerializeField] private GameObject ForwardBranch;
	[SerializeField] private GameObject LeftForwardBranch;
	[SerializeField] private GameObject LeftRightBranch;
	[SerializeField] private GameObject LeftForwardRightBranch;
	[SerializeField] private GameObject RightForwardBranch;

	[SerializeField] private GameObject Left_LeftForwardBranch;
	[SerializeField] private GameObject Left_LeftRightBranch;
	[SerializeField] private GameObject Left_LeftForwardRightBranch;
	[SerializeField] private GameObject Left_RightForwardBranch;

	[SerializeField] private GameObject Right_LeftForwardBranch;
	[SerializeField] private GameObject Right_LeftRightBranch;
	[SerializeField] private GameObject Right_LeftForwardRightBranch;
	[SerializeField] private GameObject Right_RightForwardBranch;

	private readonly int _coordsScale = 5;

	private Dictionary<BranchType, GameObject> _typeToObject;
	private Dictionary<BranchType, IEnumerable<BranchTypeConfig>> _branchTypeConfig;

	private List<CheckPresets> _branchTypeCheckPresets;

	public void Initialize()
	{
		ObjectResolver.Register(this);

		_branchTypeConfig = Resources.Load<BranchTypeConfigs>("Branches/BranchTypeConfigs").GetDictionary();

		_typeToObject = new()
		{
			{ BranchType.Forward, ForwardBranch },
			{ BranchType.LeftForward, LeftForwardBranch },
			{ BranchType.LeftForwardRight, LeftForwardRightBranch },
			{ BranchType.LeftRight, LeftRightBranch },
			{ BranchType.RightForward, RightForwardBranch },

			{ BranchType.Left_LeftForward, Left_LeftForwardBranch },
			{ BranchType.Left_LeftRight, Left_LeftRightBranch },
			{ BranchType.Left_LeftForwardRight, Left_LeftForwardRightBranch },
			{ BranchType.Left_RightForward, Left_RightForwardBranch },

			{ BranchType.Right_LeftForward, Right_LeftForwardBranch },
			{ BranchType.Right_LeftRight, Right_LeftRightBranch },
			{ BranchType.Right_LeftForwardRight, Right_LeftForwardRightBranch },
			{ BranchType.Right_RightForward, Right_RightForwardBranch },
		};

		_branchTypeCheckPresets = new()
		{
			new CheckPresets(BranchType.Forward, BranchType.Forward),
			new CheckPresets(BranchType.LeftForward, BranchType.RightForward),
			new CheckPresets(BranchType.RightForward, BranchType.LeftForward),
			new CheckPresets(BranchType.LeftForwardRight, BranchType.LeftForwardRight),
			new CheckPresets(BranchType.LeftRight, BranchType.LeftRight),

			new CheckPresets(BranchType.Left_LeftForward, BranchType.Right_RightForward),
			new CheckPresets(BranchType.Left_LeftRight, BranchType.Right_LeftRight),
			new CheckPresets(BranchType.Left_LeftForwardRight, BranchType.Right_LeftForwardRight),
			new CheckPresets(BranchType.Left_RightForward, BranchType.Right_LeftForward),

			new CheckPresets(BranchType.Right_LeftForward, BranchType.Left_RightForward),
			new CheckPresets(BranchType.Right_LeftRight, BranchType.Left_LeftRight),
			new CheckPresets(BranchType.Right_LeftForwardRight, BranchType.Left_LeftForwardRight),
			new CheckPresets(BranchType.Right_RightForward, BranchType.Left_LeftForward),
		};

	}

	public Transform CreateBranches(Vector2 delta, PathMap pathMap, Transform parent)
	{
		GameObject branchesContainer = new GameObject("Branches");
		branchesContainer.transform.SetParent(parent.transform, false);

		var branches = pathMap.GetBranches();

		foreach (var branch in branches)
		{
			if (branch == pathMap.InitialBranch)
			{
				delta = SetUpFirstBranch(delta, branch, branchesContainer.transform);
				continue;
			}
			CreateBranch(delta, branch, branchesContainer.transform);
		}

		return branchesContainer.transform;
	}


	private GameObject CreateBranch(Vector2 delta, Branch branch, Transform parent)
	{
		var outDirections = branch.Roads.Where(r => r.FirstBranchNode == branch).Select(r => r.RailroadStructure.First().Type);
		var inDirections = branch.Roads.Where(r => r.LastBranchNode == branch).Select(r => r.RailroadStructure.Last().Type);

		var typeInfo = DefineBranchType(outDirections, inDirections);

		var branchPrefab = _typeToObject[typeInfo.Type];

		var rotation = typeInfo.IsReversed ?
			Quaternion.Euler(0, 180, 0) :
			Quaternion.identity;

		var pos = new Vector3(
			branch.BasePoint.x * _coordsScale + delta.x,
			0,
			branch.BasePoint.y * _coordsScale + delta.y);

		var branchObj = Instantiate(branchPrefab, pos, rotation);
		branchObj.transform.SetParent(parent, false);

		return branchObj;
	}


	private BranchTypeInfo DefineBranchType(IEnumerable<DirectionType> outDirections, IEnumerable<DirectionType> inDirections)
	{
		BranchTypeInfo branchInfo = new BranchTypeInfo(BranchType.None);

		foreach (var preset in _branchTypeCheckPresets)
		{
			branchInfo = CheckBranchType(inDirections, outDirections, preset);
			if (branchInfo.Type != BranchType.None) return branchInfo;
		}

		return branchInfo;
	}

	private BranchTypeInfo CheckBranchType(IEnumerable<DirectionType> inDirections, IEnumerable<DirectionType> outDirections, CheckPresets preset)
	{
		var configs = _branchTypeConfig[preset.CheckType];
		foreach (var conf in configs)
		{
			var inSet = new HashSet<DirectionType>(conf.InDirections);
			var outSet = new HashSet<DirectionType>(conf.OutDirections);

			if (inSet.SetEquals(inDirections) && outSet.SetEquals(outDirections))
				return new BranchTypeInfo(preset.CheckType);

			if (inSet.SetEquals(outDirections) && outSet.SetEquals(inDirections))
				return new BranchTypeInfo(preset.ReverseMatchType, true);
		}

		return new BranchTypeInfo(BranchType.None);
	}

	private Vector2 SetUpFirstBranch(Vector2 delta, Branch branch, Transform parent)
	{
		var firstBranch = CreateBranch(delta, branch, parent);
		var firstBranchSize = WorldMapBuildingHelper.GetComplexObjectSize(firstBranch.transform);

		delta.x += firstBranchSize.x / 2;
		delta.y += firstBranchSize.z / 2;

		firstBranch.transform.position = new Vector3(delta.x, 0, delta.y);

		return delta;
	}
}