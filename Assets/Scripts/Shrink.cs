using UnityEngine;

public class Shrink : MonoBehaviour
{
	public float scaleFactor = 0.5f;

	void OnEnable()
	{
		transform.localScale *= scaleFactor;
	}

	void OnDisable()
	{
		Vector3 oldScale = transform.localScale;

		transform.localScale /= scaleFactor;

		float heightDifference = transform.localScale.y - oldScale.y;
		transform.position += Vector3.up * (heightDifference / 2f);
	}
}