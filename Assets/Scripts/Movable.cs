using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movable : MonoBehaviour
{
    [Header("Direction hor/ver")]
    public bool horizontal;

    [Header("Params")]
    public float velocity;
    public Vector3 startPoint;
    public Vector3 endPointHorizontal;
    public Vector3 endPointVertical;

    private Vector3 currentTarget;
	private Rigidbody rb;

	void Awake()
    {
		rb = GetComponent<Rigidbody>();

		if (startPoint == Vector3.zero) startPoint = transform.position;
        SetNextTarget();
    }

	// Fixed update bo fizyka
	void FixedUpdate()
    {
		if (!rb.isKinematic) return;
        MovePlatform();
    }

    void MovePlatform()
    {

		Vector3 newPosition = Vector3.MoveTowards(rb.position, currentTarget, velocity * Time.fixedDeltaTime);
		rb.MovePosition(newPosition);

		if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
		{
			ToggleTarget();
		}
	}

    void SetNextTarget()
    {
		currentTarget = horizontal ? endPointHorizontal : endPointVertical;
	}

    void ToggleTarget()
    {
		Vector3 endPoint = horizontal ? endPointHorizontal : endPointVertical;
		currentTarget = (currentTarget == startPoint) ? endPoint : startPoint;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
			other.transform.parent = transform;
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
			other.transform.parent = null;
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
