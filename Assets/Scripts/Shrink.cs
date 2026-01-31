using UnityEngine;
using System.Collections;

public class Shrink : MonoBehaviour
{
	[Header("Settings")]
	public float scaleFactor = 0.5f;
	public float duration = 0.5f;

	private Coroutine scalingCoroutine;

	void OnEnable()
	{
		Vector3 startScale = transform.localScale;
		Vector3 startPos = transform.position;

		Vector3 targetScale = startScale * scaleFactor;

		float heightDifference = startScale.y - targetScale.y;

		Vector3 targetPos = startPos - (Vector3.up * (heightDifference / 2f));

		if (scalingCoroutine != null) StopCoroutine(scalingCoroutine);
		scalingCoroutine = StartCoroutine(LerpScale(startScale, targetScale, startPos, targetPos));
	}

	void OnDisable()
	{
		if (scalingCoroutine != null) StopCoroutine(scalingCoroutine);

		Vector3 currentScale = transform.localScale;

		Vector3 targetScale = currentScale / scaleFactor;

		float heightDifference = targetScale.y - currentScale.y;

		transform.localScale = targetScale;

		transform.position += Vector3.up * (heightDifference / 2f);
	}

	private IEnumerator LerpScale(Vector3 startS, Vector3 endS, Vector3 startP, Vector3 endP)
	{
		float time = 0;

		while (time < duration)
		{
			time += Time.deltaTime;
			float t = time / duration;
			t = Mathf.SmoothStep(0, 1, t);

			transform.localScale = Vector3.Lerp(startS, endS, t);
			transform.position = Vector3.Lerp(startP, endP, t);

			yield return null;
		}

		transform.localScale = endS;
		transform.position = endP;
	}
}