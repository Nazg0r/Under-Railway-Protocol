using System.Threading.Tasks;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
	[SerializeField] private GameManager _gameManager;
	[SerializeField] private SceneManager _sceneManager;
	[SerializeField] private UIScreenManager _uiScreenManager;

	private async Task Start()
	{
		DontDestroyOnLoad(gameObject);

		await _gameManager.Initialize();
		await _uiScreenManager.Initialize();
		_sceneManager.Initialize();

		await _gameManager.ChangeState(GameState.MainMenu);
	}
}
