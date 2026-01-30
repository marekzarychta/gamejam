using UnityEngine;

public class BallTest : MonoBehaviour
{
    [Header("Kuleczka")]
    public float velocity;
    public ParticleSystem system;

    void Update()
    { 
        transform.Translate(new Vector3(0.0f, velocity * Time.deltaTime, 0.0f));
        transform.localScale += new Vector3(1.0f * Time.deltaTime, 1.0f * Time.deltaTime, 1.0f * Time.deltaTime);
        CheckBoom();
    }

    private void CheckBoom()
    {
        if (transform.position.y >= 10.0f) {
            Instantiate(system, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }

}
