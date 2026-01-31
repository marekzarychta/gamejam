using UnityEngine;

public class TurretShooter : MonoBehaviour
{
    [Header("Strefa")]
    [SerializeField] private Collider protectedZone;

    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float fireRate = 6f;      //strzalow na s
    [SerializeField] private float range = 25f;        // dystans

    [Header("Line of sight")]
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength = 12f;

    [Header("Muzzle")]
    [SerializeField] private Transform muzzle;

    [Header("Projectile (VFX) ")]
    [SerializeField] private TurretProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 18f;

    [Header("Zone check settings")]
    [SerializeField] private float zoneProbeHeight = 1.0f; // gdzie sprawdzac gracza
    [SerializeField] private float zoneToleranceSqr = 0.01f; // toelracnja

    private FPPController target;
    private float cooldown;

    private void Update()
    {
        if (protectedZone == null) return;

        // szukanie gracza
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player == null) return;

            target = player.GetComponentInParent<FPPController>();
            if (target == null) return;
        }

        // check strefy
        Vector3 probe = target.transform.position + Vector3.up * zoneProbeHeight;
        if (!IsInsideZone(probe))
            return;

        // cooldown
        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        // origin strzału
        Vector3 origin = muzzle != null
            ? muzzle.position
            : (transform.position + transform.forward * 0.6f + Vector3.up * 0.9f);

        Vector3 toPlayer = probe - origin;
        float dist = toPlayer.magnitude;

        if (dist > range)
            return;

        Vector3 dir = (dist > 0.0001f) ? (toPlayer / dist) : transform.forward;

        // line of sight
        if (requireLineOfSight)
        {
            if (!Physics.Raycast(origin, dir, out RaycastHit hit, range, lineOfSightMask, QueryTriggerInteraction.Ignore))
                return;

            if (!hit.collider.CompareTag(playerTag))
                return;
        }

        Fire(origin, dir);

        cooldown = 1f / fireRate;
    }

    private void Fire(Vector3 origin, Vector3 dir)
    {
        //dla prefabu pocisku poakxzujemy gdzie leci
        if (projectilePrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            TurretProjectile proj = Instantiate(projectilePrefab, origin, rot);
            proj.speed = projectileSpeed;
            proj.Init(dir, knockbackStrength, playerTag);
        }
        else
        {
            // Fallback tylko knockback
            target.AddKnockback(dir * knockbackStrength);
        }
    }

    private bool IsInsideZone(Vector3 worldPos)
    {
        // jesli punkt jest wewnatrz collidera -> ClosestPoint == ten sam punkt
        Vector3 closest = protectedZone.ClosestPoint(worldPos);
        return (closest - worldPos).sqrMagnitude < zoneToleranceSqr;
    }

    // opcj.
    public void SetZone(Collider newZone) => protectedZone = newZone;
    public Collider GetZone() => protectedZone;
}
