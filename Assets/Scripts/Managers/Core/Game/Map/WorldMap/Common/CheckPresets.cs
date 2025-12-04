public struct CheckPresets
{
	public readonly WorldMapBranchBuildingManager.BranchType CheckType;
	public readonly WorldMapBranchBuildingManager.BranchType ReverseMatchType;

	public CheckPresets(WorldMapBranchBuildingManager.BranchType checkType, WorldMapBranchBuildingManager.BranchType reverseMatchType)
	{
		CheckType = checkType;
		ReverseMatchType = reverseMatchType;
	}
}