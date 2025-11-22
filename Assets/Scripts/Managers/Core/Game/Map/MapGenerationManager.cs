using UnityEngine;

public class MapGenerationManager : MonoBehaviour
{
	public Map Map { get; private set;  }
	private GameManager _gameManager;

	public void Initialize(){
		ObjectResolver.Register(this);
		_gameManager = ObjectResolver.Resolve<GameManager>();

		MapFactory factory = new();
		Map = factory.Create();

		_gameManager.SetCurrentStation(Map.InitialStation);
	}
}