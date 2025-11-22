using UnityEngine;

public class ImageRotator : MonoBehaviour
{
	public float angleStep = 30f;
	public float interval = 0.3f;

	private float _timer = 0f; 

	void Update()
	{
		_timer += Time.deltaTime;

		if (_timer < interval) return;
		_timer = 0f;
		transform.Rotate(0, 0, angleStep);
	}
}
