public struct BranchTypeInfo
{
	public readonly BranchType Type;
	public readonly bool IsReversed;

	public BranchTypeInfo(BranchType type, bool isReversed = false)
	{
		Type = type;
		IsReversed = isReversed;
	}
}