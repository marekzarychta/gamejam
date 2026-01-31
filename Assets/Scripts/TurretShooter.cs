using UnityEngine;
using System.Collections;

public class TurretShooter : MonoBehaviour
{
	[Header("Ustawienia Serii")]
	[SerializeField] private int shotsPerBurst = 3;
	[SerializeField] private float timeBetweenShots = 0.2f;
	[SerializeField] private float reloadTime = 2.0f;

	[Header("Pocisk")]
	[SerializeField] private TurretProjectile projectilePrefab;
	[SerializeField] private Transform muzzle;
	[SerializeField] private float projectileSpeed = 18f;
	[SerializeField] private float knockbackStrength = 12f;
	[SerializeField] private string playerTag = "Player";

	private Coroutine shootingCoroutine;

	// --- ZMIANA KLUCZOWA: Używamy OnEnable i OnDisable ---

	private void OnEnable()
	{
		// Kiedy tablet włączy ten komponent -> Zacznij strzelać
		StartShooting();
	}

	private void OnDisable()
	{
		// Kiedy tablet wyłączy ten komponent -> Przestań NATYCHMIAST
		StopShooting();
	}

	// -----------------------------------------------------

	public void StartShooting()
	{
		if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);
		shootingCoroutine = StartCoroutine(BurstRoutine());
	}

	public void StopShooting()
	{
		if (shootingCoroutine != null) StopCoroutine(shootingCoroutine);
		shootingCoroutine = null;
	}

	private IEnumerator BurstRoutine()
	{
		while (true)
		{
			for (int i = 0; i < shotsPerBurst; i++)
			{
				Fire();
				yield return new WaitForSeconds(timeBetweenShots);
			}
			yield return new WaitForSeconds(reloadTime);
		}
	}

	private void Fire()
	{
		Vector3 origin = muzzle != null ? muzzle.position : transform.position;
		Vector3 direction = muzzle != null ? muzzle.forward : transform.forward;

		if (projectilePrefab != null)
		{
			TurretProjectile proj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
			proj.speed = projectileSpeed;
			proj.Init(direction, knockbackStrength, playerTag);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Vector3 origin = muzzle != null ? muzzle.position : transform.position;
		Vector3 direction = muzzle != null ? muzzle.forward : transform.forward;
		Gizmos.DrawLine(origin, origin + direction * 5f);
	}

}