using UnityEngine;

public class WorldMapStationBuildingManager : MonoBehaviour
{
	[SerializeField] private GameObject _stationPrefab;

	private const float RotationBase = 90f;

	public void Initialize()
	{
		ObjectResolver.Register(this);
	}

	public Vector2 CreateStation(Vector2 delta, Transform parent)
	{
		var pos = new Vector3(delta.x, 0, delta.y);
		var stationIns = Instantiate(_stationPrefab, pos, Quaternion.Euler(0f, RotationBase, 0f), parent);

		var stationObj = stationIns.transform.GetChild(0);
		Vector3 stationSize = stationObj.GetComponent<Renderer>().bounds.size;

		return new Vector2(
			stationSize.x / 2 + stationObj.transform.position.x,
			stationObj.transform.position.z);
	}
}