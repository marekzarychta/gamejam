using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveToCheckpoint()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = transform.position;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) GameManager.Instance.checkpoint = this;
    }
}
