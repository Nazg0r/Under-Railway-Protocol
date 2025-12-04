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
			{ BranchType.Forward, ForwardBranch},
			{ BranchType.LeftForward, LeftForwardBranch },
			{ BranchType.LeftForwardRight, LeftForwardRightBranch },
			{ BranchType.LeftRight, LeftRightBranch },
			{ BranchType.RightForward, RightForwardBranch },
		};

		_branchTypeCheckPresets = new()
		{
			new CheckPresets(BranchType.Forward, BranchType.Forward),
			new CheckPresets(BranchType.LeftForward, BranchType.RightForward),
			new CheckPresets(BranchType.RightForward, BranchType.LeftForward),
			new CheckPresets(BranchType.LeftForwardRight, BranchType.LeftForwardRight),
			new CheckPresets(BranchType.LeftRight, BranchType.LeftRight)
		};

	}

	public void CreateBranches(PathMap pathMap, Transform parent)
	{
		GameObject branchesContainer = new GameObject("Branches");
		branchesContainer.transform.SetParent(parent.transform, false);

		var branches = pathMap.GetBranches();

		foreach (var branch in branches)
		{
			var outDirections = branch.Roads.Where(r => r.FirstBranchNode == branch).Select(r => r.RailroadStructure.First().Type);
			var inDirections = branch.Roads.Where(r => r.LastBranchNode == branch).Select(r => r.RailroadStructure.Last().Type);

			var typeInfo = DefineBranchType(outDirections, inDirections);

			var branchPrefab = _typeToObject[typeInfo.Type];

			var rotation = typeInfo.IsReversed ?
				Quaternion.Euler(0, 180, 0) :
				Quaternion.identity;


			var pos = new Vector3(branch.BasePoint.x * _coordsScale, 0, branch.BasePoint.y * _coordsScale);

			var branchObj = Instantiate(branchPrefab, pos, rotation);
			branchObj.transform.SetParent(branchesContainer.transform, false);
		}
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
}