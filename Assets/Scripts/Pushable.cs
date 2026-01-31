using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pushable : MonoBehaviour
{
	[Header("Ustawienia Pchania")]
	public float pushPower = 2.0f;

	private Rigidbody rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	public void Push(Vector3 desiredVelocity, Transform pusher)
	{
		if (!this.enabled) return;

		if (rb.isKinematic) return;

		if (desiredVelocity.magnitude < 0.1f) return;

		Vector3 direction = desiredVelocity.normalized;
		float checkDistance = desiredVelocity.magnitude * Time.deltaTime + 0.1f;

		if (!IsPathBlocked(direction, checkDistance, pusher))
		{
			rb.linearVelocity = desiredVelocity;
		}

	}

	private bool IsPathBlocked(Vector3 direction, float distance, Transform pusher)
	{
		RaycastHit[] hits = rb.SweepTestAll(direction, distance, QueryTriggerInteraction.Ignore);

		foreach (var hit in hits)
		{
			if (hit.normal.y > 0.5f) continue;

			if (hit.transform == pusher) continue;

			return true;
		}
		return false;
	}

}