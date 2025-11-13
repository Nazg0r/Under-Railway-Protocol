using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIScreenManager : MonoBehaviour
{
	[SerializeField] private AssetReference _loadingScreen;
	[SerializeField] private GameObject _initializationScreen;
	private GameManager _gameManager;
	private GameObject _loadingScreenInstance;

	public async Task Initialize()
	{
		ObjectResolver.Register(this);
		_gameManager = ObjectResolver.Resolve<GameManager>();
		_gameManager.OnGameStateChanged += DisableLoadingScreen;
		_gameManager.OnGameStateChangedAsync += ShowLoadingScreen;
		_gameManager.OnGameStateChanged += DisableInitializationScreen;
	}

	private async Task ShowLoadingScreen(GameState state)
	{
		if (state == GameState.Loading)
		{
			var handle = Addressables.InstantiateAsync(_loadingScreen);
			_loadingScreenInstance = await handle.Task;
		}
	}

	private void DisableLoadingScreen(GameState state)
	{
		if (state == GameState.Preparation)
		{
			DisableLoadedScreen(ref _loadingScreenInstance);
		}
	}

	private void DisableInitializationScreen(GameState state)
	{
		if (state == GameState.MainMenu)
			Destroy(_initializationScreen);
	}

	private void DisableLoadedScreen(ref GameObject screenInstance)
	{
		if (screenInstance == null) return;

		screenInstance.SetActive(false);
		Addressables.ReleaseInstance(screenInstance);
		screenInstance = null;
	}
}