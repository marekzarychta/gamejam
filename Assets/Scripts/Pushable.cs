using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pushable : MonoBehaviour
{
	[Header("Ustawienia Pchania")]
	public float pushPower = 2.0f;

	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();

		if (rb != null)
		{
			rb.linearDamping = 5f;
			rb.angularDamping = 5f;

			// Blokujemy wywracanie siê (tylko przesuwanie, bez turlania)
			rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		}
	}

	public void Push(Vector3 desiredVelocity, Transform pusher)
	{
		if (desiredVelocity.magnitude < 0.1f) return;

		Vector3 direction = desiredVelocity.normalized;

		float checkDistance = desiredVelocity.magnitude * Time.deltaTime + 0.1f;

		RaycastHit[] hits = rb.SweepTestAll(direction, checkDistance, QueryTriggerInteraction.Ignore);

		foreach (var hit in hits)
		{
			if (hit.normal.y > 0.5f) continue;

			if (hit.transform == pusher) continue;

			Rigidbody hitRb = hit.collider.attachedRigidbody;

			if (hitRb == null)
			{
				return;
			}

			if (hitRb != null)
			{
				Pushable otherPushable = hitRb.GetComponent<Pushable>();

				if (otherPushable == null)
				{
					return;
				}
			}
		}

		rb.linearVelocity = desiredVelocity;
	}
}