using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Ekwipunek")]
    // Lista komponentów, które gracz niesie w tablecie
    public List<GlitchComponentType> playerInventory = new List<GlitchComponentType>();

    [Header("Ustawienia")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    public Camera playerCamera;
    
    [Header("UI Reference")]
    public FPPController fppController;
    public TabletManager tabletManager; // Referencja do skryptu UI

    private bool isTabletOpen = false;

    void Update()
    {
        // Wyjście z tabletu
        if (isTabletOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        {
            CloseTablet();
            return;
        }

        // Interakcja tylko gdy tablet zamknięty
        if (!isTabletOpen && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            GlitchedObject target = hit.collider.GetComponent<GlitchedObject>();
            if (target != null)
            {
                OpenTablet(target);
            }
        }
    }

    void OpenTablet(GlitchedObject target)
    {
        isTabletOpen = true;
        
        // Odblokuj kursor myszy
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Zatrzymaj kamerę (jeśli masz skrypt FirstPersonController, wyłącz go tutaj)
        // np. GetComponent<FirstPersonController>().enabled = false;
        if (fppController != null) fppController.enabled = false; // <--- TO ZATRZYMA KAMERĘ
        
        tabletManager.OpenTabletInterface(target, this);
    }

    public void CloseTablet()
    {
        isTabletOpen = false;

        // Zablokuj kursor z powrotem do gry FPP
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Włącz kamerę z powrotem
        // np. GetComponent<FirstPersonController>().enabled = true;
        if (fppController != null) fppController.enabled = true; // <--- TO WŁĄCZY KAMERĘ
        
        tabletManager.CloseTabletInterface();
    }
}