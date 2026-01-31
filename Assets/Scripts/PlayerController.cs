using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Ekwipunek")]
    public List<GlitchComponentType> playerInventory = new List<GlitchComponentType>();
    public Stack<Material> collectedMaterials = new Stack<Material>();

    [Header("Ustawienia")]
    public float interactionDistance = 5f;
    public LayerMask interactableLayer;
    public Camera playerCamera;
    
    [Header("Referencje")]
    public TabletManager tabletManager;
    public FPPController fppController;
    public Tablet tabletMovement;
    public Transform cursorCanvas;
    public Transform interactionTooltip;

    private bool isCursorMode = false;

    void Start()
    {
        tabletManager.InitializePlayerInventory(this);
    }

    void Update()
    {
        HandleRaycast();

        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleTablet();
        }
        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log($"SPACE | isCursorMode={isCursorMode} | fppEnabled={(fppController ? fppController.enabled : false)}");

    }

    void HandleRaycast()
    {
        if (isCursorMode) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Używamy QueryTriggerInteraction.Collide, żeby widzieć też obiekty bez fizyki (duchy)
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            GlitchedObject target = hit.collider.GetComponent<GlitchedObject>();
            // Wysyłamy cel do tabletu (może być null, jeśli trafiliśmy w coś innego na tej warstwie)
            tabletManager.UpdateTargetView(target);
            interactionTooltip.gameObject.SetActive(true);
        }
        else
        {
            // Patrzymy w niebo/podłogę -> czyścimy listę celu
            tabletManager.UpdateTargetView(null);
            interactionTooltip.gameObject.SetActive(false);
        }
    }

    void ToggleTablet()
    {
        isCursorMode = !isCursorMode;

        if (isCursorMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (fppController != null) fppController.enabled = false;
            
            tabletMovement.SetState(true);
            cursorCanvas.gameObject.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (fppController != null) fppController.enabled = true;
            
            tabletMovement.SetState(false);
            cursorCanvas.gameObject.SetActive(true);
            
            HandleRaycast(); 
        }
    }
}