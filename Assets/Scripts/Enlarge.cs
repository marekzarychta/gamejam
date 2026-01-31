using UnityEngine;
using System.Collections;

public class Enlarge : MonoBehaviour
{
	[Header("Settings")]
	public float scaleFactor = 2.0f;
	public float duration = 0.5f; 

	private Vector3 originalScale; 
	private Coroutine scalingCoroutine;

	void OnEnable()
	{
		Vector3 startScale = transform.localScale;

		Vector3 targetScale = startScale * scaleFactor;

		float heightDifference = targetScale.y - startScale.y;
		Vector3 startPos = transform.position;
		Vector3 targetPos = startPos + (Vector3.up * (heightDifference / 2f));


		if (scalingCoroutine != null) StopCoroutine(scalingCoroutine);
		scalingCoroutine = StartCoroutine(LerpScale(startScale, targetScale, startPos, targetPos));
	}

	void OnDisable()
	{

		if (scalingCoroutine != null) StopCoroutine(scalingCoroutine);

		transform.localScale /= scaleFactor;

	}

	private IEnumerator LerpScale(Vector3 startS, Vector3 endS, Vector3 startP, Vector3 endP)
	{
		float time = 0;

		while (time < duration)
		{
			// P³ynne przejœcie
			time += Time.deltaTime;
			float t = time / duration;

			// Opcjonalnie: Dodaj wyg³adzanie (SmoothStep) dla ³adniejszego efektu
			t = Mathf.SmoothStep(0, 1, t);

			transform.localScale = Vector3.Lerp(startS, endS, t);
			transform.position = Vector3.Lerp(startP, endP, t);

			yield return null;
		}

		// Na koniec upewniamy siê, ¿e wartoœci s¹ idealne
		transform.localScale = endS;
		transform.position = endP;
	}
}