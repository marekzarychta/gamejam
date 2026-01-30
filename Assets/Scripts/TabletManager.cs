using UnityEngine;
using System;
using System.Collections.Generic;

public class TabletManager : MonoBehaviour
{
    public GameObject tabletPanel;
    public GameObject rowPrefab;

    [Header("Listy UI")]
    public Transform objectComponentsContainer; // Kontener na komponenty obiektu
    public Transform playerInventoryContainer;  // Kontener na ekwipunek gracza

    // Zapamiętujemy referencje, żeby móc odświeżyć widok
    private GlitchedObject currentTarget;
    private PlayerController currentPlayer;

    private void Start()
    {
        tabletPanel.SetActive(false);
    }

    public void OpenTabletInterface(GlitchedObject target, PlayerController player)
    {
        currentTarget = target;
        currentPlayer = player;
        tabletPanel.SetActive(true);

        RefreshLists();
    }

    public void CloseTabletInterface()
    {
        tabletPanel.SetActive(false);
    }

    // Ta funkcja buduje obie listy od zera
    void RefreshLists()
    {
        ClearContainer(objectComponentsContainer);
        ClearContainer(playerInventoryContainer);

        // 1. LISTA OBIEKTU (Co obiekt ma teraz?)
        foreach (var comp in currentTarget.activeComponents)
        {
            CreateRow(comp, objectComponentsContainer, true);
        }

        // 2. LISTA GRACZA (Co gracz może dodać?)
        // Wyświetlamy tylko te komponenty, które gracz ma w plecaku,
        // ALE pomijamy te, które obiekt już posiada (nie można mieć 2x Gravity)
        foreach (var comp in currentPlayer.playerInventory)
        {
            if (!currentTarget.HasComponent(comp))
            {
                CreateRow(comp, playerInventoryContainer, false);
            }
        }
    }

    void CreateRow(GlitchComponentType type, Transform container, bool isTaking)
    {
        GameObject newRow = Instantiate(rowPrefab, container);
        TabletRowUI rowScript = newRow.GetComponent<TabletRowUI>();
        
        // Przekazujemy RefreshLists jako "callback"
        rowScript.Setup(type, currentTarget, currentPlayer, isTaking, RefreshLists);
    }

    void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}