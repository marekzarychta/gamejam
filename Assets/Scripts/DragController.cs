using UnityEngine;

public class DragController : MonoBehaviour
{
    [Header("Ustawienia")]
    public float grabDistance = 4f;
    public float dragSpeed = 10f;
    public float rotationSpeed = 2f;
    public LayerMask dragLayer;

    [Header("Fizyka")]
    public float heavyMassThreshold = 10f; 
    public float heavyDragDamping = 5f;
    
    // NOWE: Jak bardzo spowolnić gracza (0.3 = gracz porusza się z 30% prędkości)
    [Range(0.1f, 1f)]
    public float heavyWalkSpeedMultiplier = 0.3f; 

    [Header("Referencje")]
    public Camera playerCamera;
    public Transform holdParent;
    public FPPController fppController;

    private Rigidbody heldObject;
    private bool isHeavyObject = false; 

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (heldObject == null) TryGrabObject();
            else DropObject();
        }

        if (heldObject != null)
        {
            // Zabezpieczenie: jeśli w trakcie trzymania obiekt zniknie
            if (fppController != null) 
            {
                 // Jeśli trzymamy R (obracanie), wyłączamy sterowanie całkowicie,
                 // w przeciwnym razie upewniamy się, że jest włączone
                if (!Input.GetKey(KeyCode.R)) fppController.enabled = true;
            }

            if (Input.GetMouseButtonUp(1)) DropObject();

            if (Input.GetKey(KeyCode.R))
            {
                if (fppController != null) fppController.enabled = false;
                
                float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
                float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;
                heldObject.transform.Rotate(Vector3.up, -rotX, Space.World);
                heldObject.transform.Rotate(Vector3.right, rotY, Space.World);
            }
        }
    }

    void FixedUpdate()
    {
        if (heldObject != null)
        {
            MoveObject();
        }
    }

    void TryGrabObject()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grabDistance, dragLayer))
        {
            if (hit.collider.GetComponent<Pushable>() != null && hit.rigidbody != null)
            {
                if (!hit.rigidbody.isKinematic)
                {
                    heldObject = hit.rigidbody;

                    if (heldObject.mass >= heavyMassThreshold)
                    {
                        isHeavyObject = true;
                        heldObject.useGravity = true; 
                        heldObject.linearDamping = heavyDragDamping; 
                        heldObject.constraints = RigidbodyConstraints.FreezeRotation; 
                        
                        // NOWE: Spowalniamy gracza
                        if (fppController != null) 
                            fppController.SetSpeedMultiplier(heavyWalkSpeedMultiplier);
                    }
                    else
                    {
                        isHeavyObject = false;
                        heldObject.useGravity = false; 
                        heldObject.linearDamping = 10f;
                        heldObject.angularDamping = 5f;
                        heldObject.freezeRotation = true; 
                        
                        // Lekkie obiekty nie spowalniają (lub możesz ustawić np. 0.9f)
                        if (fppController != null) 
                            fppController.SetSpeedMultiplier(1f);
                    }
                }
            }
        }
    }

    void MoveObject()
    {
        Vector3 direction = holdParent.position - heldObject.position;
        
        if (isHeavyObject)
        {
            Vector3 targetVelocity = direction * dragSpeed;
            targetVelocity.y = heldObject.linearVelocity.y; 
            heldObject.linearVelocity = targetVelocity;
        }
        else
        {
            heldObject.linearVelocity = direction * dragSpeed;
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.useGravity = true;
            heldObject.linearDamping = 1f;
            heldObject.angularDamping = 0.5f;
            heldObject.freezeRotation = false;
            heldObject.constraints = RigidbodyConstraints.None;
            heldObject = null;
            
            // NOWE: Przywracamy pełną prędkość gracza
            if (fppController != null) fppController.SetSpeedMultiplier(1f);
        }
    }
}