using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Railroad/BranchType")]
public class BranchType : ScriptableObject
{
	public string Name;
	public List<Direction> Directions;
}