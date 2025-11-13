using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
	[SerializeField] private GameObject _mainMenuScreen;
	private Button _newGameButton;
	private GameManager _gameManager;

	private void Awake()
	{
		ObjectResolver.Register(this);
		_gameManager = ObjectResolver.Resolve<GameManager>();
		_newGameButton = _mainMenuScreen.GetComponentInChildren<Button>();
		_newGameButton.onClick.AddListener(() => _gameManager.ChangeState(GameState.Loading));
		_gameManager.OnGameStateChanged += DestroyInitializationScreen;
	}

	private void DestroyInitializationScreen(GameState state)
	{
		if (state == GameState.Preparation)
			Destroy(_mainMenuScreen);
	}
}
