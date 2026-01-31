using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movable : MonoBehaviour
{
	[Header("Direction")]
	public bool horizontal;
	public float velocity = 2f;

	[Header("Detection (Kluczowe!)")]
	public LayerMask obstacleMask;  // Warstwa œcian (nie gracza!)
	public LayerMask passengerMask; // Warstwa gracza

	[Header("Debug & Manual Override")]
	public bool showDebugLogs = true;
	public CharacterController manualPlayer; // Przeci¹gnij tu Gracza rêcznie, jeœli automat zawiedzie!

	[Header("Points")]
	public Vector3 startPoint;
	public Vector3 endPointHorizontal;
	public Vector3 endPointVertical;

	private Vector3 currentTarget;
	private Rigidbody rb;
	private BoxCollider solidCollider;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.interpolation = RigidbodyInterpolation.Interpolate;

		// Szukamy collidera platformy (tego twardego)
		foreach (var col in GetComponentsInChildren<BoxCollider>())
		{
			if (!col.isTrigger)
			{
				solidCollider = col;
				break;
			}
		}

		if (solidCollider == null) Debug.LogError($"[Movable] BRAK COLLIDERA na obiekcie {name}!");

		if (startPoint == Vector3.zero) startPoint = transform.position;
	}

	void OnEnable()
	{
		Vector3 endPoint = horizontal ? endPointHorizontal : endPointVertical;
		currentTarget = (Vector3.Distance(transform.position, startPoint) < Vector3.Distance(transform.position, endPoint))
						? endPoint : startPoint;
	}

	void FixedUpdate()
	{
		MovePlatform();
	}

	void MovePlatform()
	{
		Vector3 direction = (currentTarget - rb.position).normalized;
		float distanceThisFrame = velocity * Time.fixedDeltaTime;
		float distToTarget = Vector3.Distance(rb.position, currentTarget);

		if (distanceThisFrame > distToTarget) distanceThisFrame = distToTarget;

		// 1. Wykrywanie Œcian (SweepTest)
		if (rb.SweepTest(direction, out RaycastHit hit, distanceThisFrame + 0.1f, QueryTriggerInteraction.Ignore))
		{
			// Sprawdzamy, czy to œciana (czy jest w masce przeszkód)
			if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0)
			{
				if (showDebugLogs) Debug.Log($"[Movable] Uderzy³em w œcianê: {hit.collider.name}. Zawracam.");
				ToggleTarget();
				return;
			}
		}

		// 2. Ruch Windy
		Vector3 oldPos = rb.position;
		Vector3 newPos = Vector3.MoveTowards(rb.position, currentTarget, distanceThisFrame);
		rb.MovePosition(newPos);

		Vector3 platformDelta = newPos - oldPos;

		// 3. Ci¹gniêcie Pasa¿erów
		if (platformDelta.sqrMagnitude > 0.000001f)
		{
			MovePassengers(platformDelta);
		}

		// 4. Czy dojechaliœmy?
		if (Vector3.Distance(rb.position, currentTarget) < 0.001f) ToggleTarget();
	}

	void MovePassengers(Vector3 delta)
	{
		// Opcja A: Rêcznie przypisany gracz (Test Ostateczny)
		if (manualPlayer != null)
		{
			manualPlayer.Move(delta);
			return; // Jeœli u¿ywamy manuala, pomijamy automat
		}

		// Opcja B: Automat (Skaner)
		if (solidCollider == null) return;

		// Logika pude³ka detekcji
		Transform t = solidCollider.transform; // U¿ywamy transformu collidera (mo¿e byæ dzieckiem)
		Vector3 boxSize = Vector3.Scale(solidCollider.size, t.lossyScale); // Skala œwiatowa

		// Pude³ko jest trochê mniejsze od platformy (0.8 szerokoœci) i wystaje 0.5m w górê
		Vector3 detectionSize = new Vector3(boxSize.x * 0.8f, 0.5f, boxSize.z * 0.8f);
		Vector3 detectionCenter = t.TransformPoint(solidCollider.center) + Vector3.up * (boxSize.y * 0.5f + 0.25f);

		Collider[] hits = Physics.OverlapBox(detectionCenter, detectionSize / 2f, t.rotation, passengerMask, QueryTriggerInteraction.Ignore);

		if (hits.Length > 0)
		{
			foreach (var hit in hits)
			{
				CharacterController cc = hit.GetComponent<CharacterController>();
				if (cc != null)
				{
					if (showDebugLogs) Debug.Log($"[Movable] Ci¹gnê pasa¿era: {cc.name}");
					cc.Move(delta);
				}
			}
		} else
		{
			// Odkomentuj to, jeœli chcesz widzieæ spam w konsoli gdy nikogo nie ma
			// if(showDebugLogs) Debug.Log("[Movable] Pusto na dachu...");
		}
	}

	void ToggleTarget()
	{
		Vector3 endPoint = horizontal ? endPointHorizontal : endPointVertical;
		currentTarget = (currentTarget == startPoint) ? endPoint : startPoint;
	}

	// Rysowanie Debugowe (Pude³ko detekcji)
	private void OnDrawGizmos()
	{
		if (solidCollider == null) solidCollider = GetComponentInChildren<BoxCollider>();
		if (solidCollider != null)
		{
			Gizmos.color = new Color(0, 1, 0, 0.4f); // Pó³przezroczysty zielony

			Transform t = solidCollider.transform;
			Vector3 boxSize = Vector3.Scale(solidCollider.size, t.lossyScale);
			Vector3 detectionSize = new Vector3(boxSize.x * 0.8f, 0.5f, boxSize.z * 0.8f);
			Vector3 center = t.TransformPoint(solidCollider.center) + Vector3.up * (boxSize.y * 0.5f + 0.25f);

			Gizmos.matrix = Matrix4x4.TRS(center, t.rotation, detectionSize);
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
		}
	}
}