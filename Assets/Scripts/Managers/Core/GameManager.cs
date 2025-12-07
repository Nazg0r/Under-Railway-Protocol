using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private GameState _currentState;
	private GameState _previousState;

	public event Action<GameState> OnGameStateChanged;
	public event Func<GameState, Task> OnGameStateChangedAsync;

	public Station CurrentStation { get; private set; }
	public Station SecondStation { get; private set; }
	public Vector2 Delta { get; set; }

	public async Task Initialize()
    {
		ObjectResolver.Register(this);
	}

    public async Task ChangeState(GameState newState)
    {
		if (newState == _currentState) return;

		_previousState = _currentState;
		_currentState = newState;

		HandleStateExit(_previousState);
		HandleStateEnter(_currentState);
		
		await InvokeOnGameStateChangedAsync(_currentState);
		OnGameStateChanged?.Invoke(_currentState);
    }

    private async Task InvokeOnGameStateChangedAsync(GameState state)
    {
	    if (OnGameStateChangedAsync != null)
	    {
		    var tasks = OnGameStateChangedAsync.GetInvocationList()
			    .Cast<Func<GameState, Task>>()
			    .Select(handler => handler?.Invoke(state));

		    await Task.WhenAll(tasks);
	    }
	}

	private void HandleStateExit(GameState state)
    {
	    Debug.Log($"[GameManager]: Exit from state \"{state}\"");
	}

	private void HandleStateEnter(GameState state)
	{
		Debug.Log($"[GameManager]: Enter to state \"{state}\"");
	}

	public void SetCurrentStation(Station station)
	{
		CurrentStation = station;
	}

	public void SetNextStation(Station station)
	{
		CurrentStation = station;
	}

}

public enum GameState
{
	Initializing,
	MainMenu,
	Loading,
	Preparation,
	Moving
}
