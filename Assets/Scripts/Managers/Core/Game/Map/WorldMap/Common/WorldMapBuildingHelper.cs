using UnityEngine;

public static class WorldMapBuildingHelper
{
	public static Vector3 GetComplexObjectSize(Transform parent)
	{
		Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();

		if (renderers.Length == 0)
			return Vector3.zero;

		Bounds combinedBounds = renderers[0].bounds;

		for (int i = 1; i < renderers.Length; i++)
			combinedBounds.Encapsulate(renderers[i].bounds);

		return combinedBounds.size;
	}
}
