using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movable : MonoBehaviour
{
	[Header("Direction")]
	public bool horizontal;
	public float velocity = 2f;

	[Header("Detection")]
	public LayerMask obstacleMask;
	public LayerMask passengerMask;

	[Header("Points (Relative Offsets)")]
	// Teraz to s¹ wektory przesuniêcia wzglêdem Startu, a nie pozycje globalne!
	public Vector3 startPoint;
	public Vector3 endPointHorizontal;
	public Vector3 endPointVertical;

	[Header("Debug")]
	public bool showDebugLogs = true;
	public CharacterController manualPlayer;

	private Vector3 currentTarget;
	private Rigidbody rb;
	private BoxCollider solidCollider;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.interpolation = RigidbodyInterpolation.Interpolate;

		foreach (var col in GetComponentsInChildren<BoxCollider>())
		{
			if (!col.isTrigger)
			{
				solidCollider = col;
				break;
			}
		}

		if (solidCollider == null) Debug.LogError($"[Movable] BRAK COLLIDERA na obiekcie {name}!");

		// Ustawiamy startPoint raz na pocz¹tku gry
		if (startPoint == Vector3.zero) startPoint = transform.position;
	}

	void OnEnable()
	{
		// Wyznaczamy pierwszy cel
		// Teraz obliczamy pozycjê œwiata dodaj¹c offset do startu
		Vector3 worldEndPoint = GetWorldEndPoint();

		// Jeœli jesteœmy bli¿ej startu, jedziemy do koñca. Jak bli¿ej koñca, wracamy na start.
		float distToStart = Vector3.Distance(transform.position, startPoint);
		float distToEnd = Vector3.Distance(transform.position, worldEndPoint);

		currentTarget = (distToStart < distToEnd) ? worldEndPoint : startPoint;
	}

	// --- NOWOŒÆ: SNAP PO WY£¥CZENIU ---
	void OnDisable()
	{
		// Gdy zabieramy komponent, obiekt wraca na start
		// U¿ywamy rb.position i transform.position dla pewnoœci
		if (rb != null) rb.position = startPoint;
		transform.position = startPoint;
	}
	// ----------------------------------

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

		// 1. Wykrywanie Œcian
		if (rb.SweepTest(direction, out RaycastHit hit, distanceThisFrame + 0.1f, QueryTriggerInteraction.Ignore))
		{
			if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0)
			{
				if (showDebugLogs) Debug.Log($"[Movable] Œciana: {hit.collider.name}. Zawracam.");
				ToggleTarget();
				return;
			}
		}

		// 2. Ruch
		Vector3 oldPos = rb.position;
		Vector3 newPos = Vector3.MoveTowards(rb.position, currentTarget, distanceThisFrame);
		rb.MovePosition(newPos);

		Vector3 platformDelta = newPos - oldPos;

		// 3. Pasa¿erowie
		if (platformDelta.sqrMagnitude > 0.000001f)
		{
			MovePassengers(platformDelta);
		}

		// 4. Cel osi¹gniêty?
		if (Vector3.Distance(rb.position, currentTarget) < 0.001f) ToggleTarget();
	}

	void MovePassengers(Vector3 delta)
	{
		if (manualPlayer != null)
		{
			manualPlayer.Move(delta);
			return;
		}

		if (solidCollider == null) return;

		Transform t = solidCollider.transform;
		Vector3 boxSize = Vector3.Scale(solidCollider.size, t.lossyScale);
		Vector3 detectionSize = new Vector3(boxSize.x * 0.8f, 0.5f, boxSize.z * 0.8f);
		Vector3 detectionCenter = t.TransformPoint(solidCollider.center) + Vector3.up * (boxSize.y * 0.5f + 0.25f);

		Collider[] hits = Physics.OverlapBox(detectionCenter, detectionSize / 2f, t.rotation, passengerMask, QueryTriggerInteraction.Ignore);

		foreach (var hit in hits)
		{
			CharacterController cc = hit.GetComponent<CharacterController>();
			if (cc != null)
			{
				cc.Move(delta);
			}
		}
	}

	void ToggleTarget()
	{
		Vector3 worldEndPoint = GetWorldEndPoint();

		// Jeœli celem by³ start -> teraz koniec (z offsetem)
		// Jeœli celem by³ koniec -> teraz start
		if (Vector3.Distance(currentTarget, startPoint) < 0.1f)
			currentTarget = worldEndPoint;
		else
			currentTarget = startPoint;
	}

	// Pomocnicza funkcja do obliczania globalnej pozycji celu
	Vector3 GetWorldEndPoint()
	{
		Vector3 offset = horizontal ? endPointHorizontal : endPointVertical;
		return startPoint + offset;
	}

	// --- WIZUALIZACJA GIZMOS (ŒCIE¯KA) ---
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;

		// W edytorze (gdy gra nie dzia³a) u¿ywamy aktualnej pozycji jako startu, 
		// ¿eby widzieæ podgl¹d "na ¿ywo" podczas przesuwania obiektu.
		Vector3 s = Application.isPlaying ? startPoint : transform.position;

		// Obliczamy koniec dodaj¹c offset
		Vector3 offset = horizontal ? endPointHorizontal : endPointVertical;
		Vector3 e = s + offset;

		// Rysujemy liniê i kropki
		Gizmos.DrawLine(s, e);
		Gizmos.DrawSphere(s, 0.1f);
		Gizmos.DrawSphere(e, 0.1f);
	}
}