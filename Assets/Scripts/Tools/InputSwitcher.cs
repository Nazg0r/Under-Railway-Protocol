using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class InputSwitcher : MonoBehaviour
{
	private List<InputSystemUIInputModule> _presets;

	public void Initialize()
	{
		ObjectResolver.Register(this);
		_presets = 
			FindAnyObjectByType<EventSystem>()
			.GetComponents<InputSystemUIInputModule>()
			.ToList();
		_presets[1].enabled = false;

	}

	public void SwitchInputsToKeyboardMouse()
	{
		if (_presets[1].enabled) _presets[1].enabled = false;
		if (!_presets[0].enabled) _presets[0].enabled = true;
	}

	public void SwitchInputsToKeyboard()
	{
		if (_presets[0].enabled) _presets[0].enabled = false;
		if (!_presets[1].enabled) _presets[1].enabled = true;
	}
}
