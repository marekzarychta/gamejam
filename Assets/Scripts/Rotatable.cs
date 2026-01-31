using UnityEngine;

public class Rotatable : MonoBehaviour
{
	public float rotateFactor = 2.5f;
	private float rotateSpeed = 0.0f;

	void OnEnable()
	{
		rotateSpeed = rotateFactor;
	}

	void OnDisable()
	{
		rotateSpeed = 0.0f;
	}

	void Update()
	{
		gameObject.transform.Rotate(0.0f, rotateSpeed * Time.deltaTime, 0.0f);	
	}

}
