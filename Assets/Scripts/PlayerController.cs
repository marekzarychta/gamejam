using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Ekwipunek")]
    public List<GlitchComponentType> playerInventory = new List<GlitchComponentType>();

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

    private bool isCursorMode = false; // Czy jesteśmy w trybie klikania?
    private GlitchedObject currentHoveredObject;
    
    void Start()
    {
        // Inicjalizujemy stałą listę gracza
        tabletManager.InitializePlayerInventory(this);
    }

    void Update()
    {
        // 1. Ciągły Raycast - aktualizuje UI zależnie od tego na co patrzysz
        HandleRaycast();

        // 2. Przełączanie trybu (Kamera vs Kursor)
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleTablet();
        }
    }

    void HandleRaycast()
    {
        // Jeśli jesteśmy w trybie kursora (klikamy), nie aktualizujemy celu
        if (isCursorMode) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // 1. Ustalamy, co jest NOWYM celem (może to być obiekt lub null)
        GlitchedObject newTarget = null;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            newTarget = hit.collider.GetComponent<GlitchedObject>();
        }

        // 2. LOGIKA PODŚWIETLANIA (MATRIX EFFECT)
        // Sprawdzamy, czy cel się zmienił od ostatniej klatki
        if (currentHoveredObject != newTarget)
        {
            // A. Wyłączamy efekt na starym obiekcie (jeśli istniał)
            if (currentHoveredObject != null)
            {
                currentHoveredObject.SetHighlight(false);
            }

            // B. Włączamy efekt na nowym obiekcie (jeśli istnieje)
            if (newTarget != null)
            {
                newTarget.SetHighlight(true);
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