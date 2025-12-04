using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BranchTypeConfigs", menuName = "Configs/BranchTypeConfigs")]
public class BranchTypeConfigs : ScriptableObject
{
	public List<BranchTypeConfigsEntry> Entries;

	private Dictionary<BranchType, IEnumerable<BranchTypeConfig>> _cache;

	public Dictionary<BranchType, IEnumerable<BranchTypeConfig>> GetDictionary()
	{
		if (_cache != null)
			return _cache;

		_cache = new Dictionary<BranchType, IEnumerable<BranchTypeConfig>>();

		foreach (var entry in Entries)
		{
			_cache[entry.BranchType] = entry.Configs;
		}

		return _cache;
	}
}