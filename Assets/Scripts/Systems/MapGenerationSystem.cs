using UnityEngine;

public class MapGenerationSystem : MonoBehaviour
{
	public Map Map { get; private set; }

	void Awake()
    {
		MapFactory factory = new MapFactory();
		Map = factory.Create();
	}
}
