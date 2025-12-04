public struct BranchTypeInfo
{
	public readonly WorldMapBranchBuildingManager.BranchType Type;
	public readonly bool IsReversed;

	public BranchTypeInfo(WorldMapBranchBuildingManager.BranchType type, bool isReversed = false)
	{
		Type = type;
		IsReversed = isReversed;
	}
}