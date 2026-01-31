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
    [HideInInspector] public bool endScreenMode = false;
    private GlitchedObject currentHoveredObject;
    
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
    }

    void HandleRaycast()
    {

        if (isCursorMode) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // 1. Ustalamy, co jest NOWYM celem (może to być obiekt lub null)
        GlitchedObject newTarget = null;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            newTarget = hit.collider.GetComponent<GlitchedObject>();
            if (newTarget == null) newTarget = hit.collider.transform.parent.GetComponent<GlitchedObject>();
        } 

        // 2. LOGIKA PODŚWIETLANIA (MATRIX EFFECT)
        // Sprawdzamy, czy cel się zmienił od ostatniej klatki
        if (currentHoveredObject != newTarget)
        {
            // 1. Wyłącz outline na starym obiekcie
            if (currentHoveredObject != null)
            {
                currentHoveredObject.SetHoverOutline(false);
            }

            // 2. Włącz outline na nowym obiekcie
            if (newTarget != null)
            {
                newTarget.SetHoverOutline(true);
            }

            // C. Zapamiętujemy nowy cel jako aktualny
            currentHoveredObject = newTarget;
            
            // D. Przy okazji aktualizujemy widok tabletu (optymalizacja: tylko gdy cel się zmienia)
            tabletManager.UpdateTargetView(newTarget);
        }

        // 3. Obsługa Tooltipa (np. ikonka "E interact")
        if (newTarget != null)
        {
            if (interactionTooltip != null) interactionTooltip.gameObject.SetActive(true);
        }
        else
        {
            if (interactionTooltip != null) interactionTooltip.gameObject.SetActive(false);
        }
    }

    public void ToggleTablet(bool endScreen = false)
    {
        //endScreenMode = endScreen;
        
        //if (endScreenMode) return;

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

	public Material GetCurrentHeldMaterial()
	{
		if (collectedMaterials.Count > 0)
		{
			return collectedMaterials.Peek();
		}
		return null;
	}

}