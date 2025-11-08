using System;
using System.Collections.Generic;

[Serializable]
public struct BranchConnection
{
	public int From;
	public int To;
	public List<string> structure;
}