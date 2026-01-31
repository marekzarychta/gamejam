using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 18f;
    public float lifetime = 3f;

    [Header("Hit")]
    public float knockbackStrength = 20f;
    public string playerTag = "Player";

    private Vector3 dir;

    public void Init(Vector3 direction, float knockback, string playerTagValue)
    {
        dir = direction.normalized;
        knockbackStrength = knockback;
        playerTag = playerTagValue;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //trafiienie
        if (other.CompareTag(playerTag))
        {
            var fpp = other.GetComponentInParent<FPPController>();
            if (fpp != null)
            {
                fpp.AddKnockback(dir * knockbackStrength);
            }

            Destroy(gameObject);
            return;
        }

        //znika na solid obiektach
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
