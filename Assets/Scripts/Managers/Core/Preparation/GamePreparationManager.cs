using UnityEngine;

public class GamePreparationManager : MonoBehaviour
{
	[SerializeField] MapGenerationManager _mapGenerationManager;
	[SerializeField] MapMonitorPreparationManager _mapMonitorPreparationManager;
	private GameManager _gameManager;

	private void Awake()
	{
		ObjectResolver.Register(this);
		_gameManager = ObjectResolver.Resolve<GameManager>();

		_gameManager.OnGameStateChanged += PrepareGame;
	}

	private void PrepareGame(GameState state)
	{
		if (state == GameState.Preparation)
		{
			_mapGenerationManager.Initialize();
			_mapMonitorPreparationManager.Initialize();
		}
	}
}