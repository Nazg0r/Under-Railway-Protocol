using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Railroad/BranchStructures")]
public class BranchStructures : ScriptableObject
{
	public List<BranchStructureBinding> Bindings;
}