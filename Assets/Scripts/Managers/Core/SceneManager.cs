using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
	[SerializeField] private AssetReference _gameScene;
	[SerializeField] private AssetReference _mainMenuScene;

	private GameManager _gameManager;

	public void Initialize()
	{
		ObjectResolver.Register(this);
		_gameManager = ObjectResolver.Resolve<GameManager>();
		_gameManager.OnGameStateChangedAsync += LoadMainMenuScene;
		_gameManager.OnGameStateChangedAsync += LoadGameScene;
		_gameManager.OnGameStateChanged += ChangeStateAfterGameSceneLoaded;
	}
	private async Task LoadMainMenuScene(GameState state)
	{
		if (state == GameState.MainMenu)
		{
			var handler = Addressables.LoadSceneAsync(_mainMenuScene, LoadSceneMode.Additive);
			await handler.Task;
		}
	}
	private async Task LoadGameScene(GameState state)
	{
		if (state == GameState.Loading)
		{
			 var handler = Addressables.LoadSceneAsync(_gameScene, LoadSceneMode.Additive);
			 await handler.Task;
		}
	}

	private void ChangeStateAfterGameSceneLoaded(GameState state)
	{
		if (state == GameState.Loading)
			_gameManager.ChangeState(GameState.Preparation);
	}
}