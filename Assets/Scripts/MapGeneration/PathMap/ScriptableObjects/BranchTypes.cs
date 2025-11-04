using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Railroad/BranchTypes")]
public class BranchTypes : ScriptableObject
{
	public List<BranchTypeBinding> Bindings;
}