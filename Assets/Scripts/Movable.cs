using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movable : MonoBehaviour
{
    [Header("Direction hor/ver")]
    public bool horizontal;

    [Header("Params")]
    public float velocity = 2f;
    public Vector3 startPoint;
    public Vector3 endPointHorizontal;
    public Vector3 endPointVertical;

    private Vector3 currentTarget;
	private Rigidbody rb;

	void Awake()
    {
		rb = GetComponent<Rigidbody>();

		if (startPoint == Vector3.zero && endPointHorizontal == Vector3.zero && endPointVertical == Vector3.zero)
		{
			startPoint = transform.position;
		} 
		else if (startPoint == Vector3.zero)
		{
			startPoint = transform.position;
		}
	}

	void OnEnable()
	{
		Vector3 endPoint = horizontal ? endPointHorizontal : endPointVertical;
		float distanceToStart = Vector3.Distance(transform.position, startPoint);
		float distanceToEnd = Vector3.Distance(transform.position, endPoint);
		currentTarget = (distanceToStart < distanceToEnd) ? endPoint : startPoint;
	}

	// Fixed update bo fizyka
	void FixedUpdate()
    {
		if (!rb.isKinematic) return;
        MovePlatform();
    }

    void MovePlatform()
    {

		Vector3 direction = (currentTarget - rb.position).normalized;
		float distanceThisFrame = velocity * Time.fixedDeltaTime;

		RaycastHit[] hits = rb.SweepTestAll(direction, distanceThisFrame + 0.1f, QueryTriggerInteraction.Ignore);

		foreach (var hit in hits)
		{
			if (hit.transform.IsChildOf(transform)) continue;

			if (hit.transform == transform) continue;

			ToggleTarget();
			return;
		}

		Vector3 newPosition = Vector3.MoveTowards(rb.position, currentTarget, distanceThisFrame);
		rb.MovePosition(newPosition);

		if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
		{
			ToggleTarget();
		}
	}

    void ToggleTarget()
    {
		Vector3 endPoint = horizontal ? endPointHorizontal : endPointVertical;
		float distanceToStart = Vector3.Distance(currentTarget, startPoint);
		float distanceToEnd = Vector3.Distance(currentTarget, endPoint);

		if (distanceToStart < distanceToEnd)
		{
			currentTarget = endPoint;
		} else { 
			currentTarget = startPoint;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			other.transform.parent = transform;
		}
		else if (other.attachedRigidbody != null && !other.attachedRigidbody.isKinematic)
		{
			if (other.transform.position.y > transform.position.y)
			{
				other.transform.parent = transform;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			other.transform.parent = null;
		}
		else if (other.attachedRigidbody != null && !other.attachedRigidbody.isKinematic)
		{
			if (other.transform.parent == transform)
			{
				other.transform.parent = null; 
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;

		Vector3 origin = Application.isPlaying ? startPoint : transform.position;
		Vector3 dest = horizontal ? endPointHorizontal : endPointVertical;

		Gizmos.DrawLine(origin, dest);
		Gizmos.DrawSphere(origin, 0.1f);
		Gizmos.DrawSphere(dest, 0.1f);
	}

}
