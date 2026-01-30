using UnityEngine;
using System.Collections.Generic;

public class GlitchedObject : MonoBehaviour
{
    public enum options {_Collider};
    public List<options> currentOptions = new List<options>();
    public Collider objectCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
            objectCollider.enabled = currentOptions.Contains(options._Collider);
    }
}
