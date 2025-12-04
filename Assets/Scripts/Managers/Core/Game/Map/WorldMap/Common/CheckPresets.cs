public struct CheckPresets
{
	public readonly BranchType CheckType;
	public readonly BranchType ReverseMatchType;

	public CheckPresets(BranchType checkType, BranchType reverseMatchType)
	{
		CheckType = checkType;
		ReverseMatchType = reverseMatchType;
	}
}