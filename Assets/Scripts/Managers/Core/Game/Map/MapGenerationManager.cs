using UnityEngine;

public class MapGenerationManager : MonoBehaviour
{
	public Map Map { get; private set;  }

	private void Awake()
	{
		ObjectResolver.Register(this);
	}

	public void Initialize() =>
		CreateMap();

	private void CreateMap()
	{
		MapFactory _factory = new();
		Map = _factory.Create();
	}
}
