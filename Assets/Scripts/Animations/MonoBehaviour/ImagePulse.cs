using System;
using UnityEngine;

public class ImagePulse : MonoBehaviour
{
	public float minScale = 0.9f;
	public float maxScale = 1.1f;
	public float interval = 0.3f;
	public float scaleStep = 1f;

	private bool _isGrowing;
	private Vector3 _scaleTo;


	private float _timer = 0;

	private void Awake()
	{
		_scaleTo = Vector3.one * minScale;
	}

	private void Update()
	{
		_timer += Time.deltaTime;

		if (_timer < interval) return;

		_timer = 0;

		transform.localScale = Vector3.Lerp(transform.localScale, _scaleTo, scaleStep);

		if (!(Vector3.Distance(transform.localScale, _scaleTo) < 0.01f)) return;

		_isGrowing = !_isGrowing;
		_scaleTo = _isGrowing ? 
			Vector3.one * maxScale :
			Vector3.one * minScale;
	}
}
