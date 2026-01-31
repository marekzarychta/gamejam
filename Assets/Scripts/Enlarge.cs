using UnityEngine;

public class Enlarge : MonoBehaviour
{
	public float scaleFactor = 2.0f;

	void OnEnable()
	{ 
		Vector3 oldScale = transform.localScale;

		transform.localScale *= scaleFactor;

		float heightDifference = transform.localScale.y - oldScale.y;

		transform.position += Vector3.up * (heightDifference / 2f);
	}

	void OnDisable()
	{
		transform.localScale /= scaleFactor;
	}
}