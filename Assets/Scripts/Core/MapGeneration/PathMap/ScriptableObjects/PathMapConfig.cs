using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Railroad/PathMapConfig")]
public class PathMapConfig : ScriptableObject
{
	public int From;
	public int To;
	public string Id;
	public int BranchNum;
	public List<BranchConnection> Structure;
}