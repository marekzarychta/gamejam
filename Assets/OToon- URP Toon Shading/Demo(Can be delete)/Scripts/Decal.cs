using UnityEngine;

public class Decal : MonoBehaviour
{
    public int timeToLive = 100;

    void Start()
    {

    }

    void Update()
    {
        timeToLive--;



        if (timeToLive <= 0)
        {
            Destroy(gameObject);
        }
    }
}
