using UnityEngine;

public class Tablet : MonoBehaviour
{
	[Header("Animation Settings")]
	public float smoothSpeed = 10f;

	[Header("Visible State (Tablet Up)")]
	public Vector3 visiblePosition;
	public Vector3 visibleRotation;
	public Vector3 visibleScale = Vector3.one;

	[Header("Hidden State (Tablet Down)")]
	public Vector3 hiddenPosition;
	public Vector3 hiddenRotation;
	public Vector3 hiddenScale = Vector3.one * 0.1f;

	private bool isVisible = false;

	private Vector3 targetPosition;
	private Quaternion targetRotation;
	private Vector3 targetScale;

	void Start()
	{
		SetState(false);

		transform.localPosition = targetPosition;
		transform.localRotation = targetRotation;
		transform.localScale = targetScale;
	}

	void Update()
	{
		transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);

		transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);

		transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * smoothSpeed);
	}

	public void SetState(bool visible)
	{
		isVisible = visible;

		if (isVisible)
		{
			targetPosition = visiblePosition;
			targetRotation = Quaternion.Euler(visibleRotation);
			targetScale = visibleScale;
		} else
		{
			targetPosition = hiddenPosition;
			targetRotation = Quaternion.Euler(hiddenRotation);
			targetScale = hiddenScale;
		}
	}

	[ContextMenu("Save Current As Visible")]
	void SaveVisible()
	{
		visiblePosition = transform.localPosition;
		visibleRotation = transform.localRotation.eulerAngles;
		visibleScale = transform.localScale;
	}

	[ContextMenu("Save Current As Hidden")]
	void SaveHidden()
	{
		hiddenPosition = transform.localPosition;
		hiddenRotation = transform.localRotation.eulerAngles;
		hiddenScale = transform.localScale;
	}

}