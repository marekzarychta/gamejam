using UnityEngine;

public class Rotatable : MonoBehaviour
{
	public float rotateFactor = 90.0f;

	public bool useGlobalUp = true;

	private float currentSpeed = 0.0f;

	void OnEnable()
	{
		currentSpeed = rotateFactor;
	}

	void OnDisable()
	{
		currentSpeed = 0.0f;
	}

	void Update()
	{
		if (useGlobalUp)
		{
			transform.Rotate(Vector3.up * currentSpeed * Time.deltaTime, Space.World);
		} else
		{
			transform.Rotate(0f, currentSpeed * Time.deltaTime, 0f, Space.Self);
		}
	}

	// Dodatkowy podgl¹d w edytorze, ¿ebyœ widzia³ oœ obrotu
	void OnDrawGizmosSelected()
	{
		Gizmos.color = useGlobalUp ? Color.green : Color.yellow;
		Vector3 axis = useGlobalUp ? Vector3.up : transform.up;

		// Rysuje liniê o d³ugoœci 2m pokazuj¹c¹ oœ obrotu
		Gizmos.DrawLine(transform.position, transform.position + axis * 2f);
		Gizmos.DrawSphere(transform.position + axis * 2f, 0.1f);
	}
}