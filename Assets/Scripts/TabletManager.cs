using UnityEngine;
using System.Collections.Generic;

public class TabletManager : MonoBehaviour
{
    public GameObject tabletPanel;
    public GameObject rowPrefab;

    [Header("Kontenery UI")]
    public Transform objectComponentsContainer;
    public Transform playerInventoryContainer;
    public GameObject noTargetMessage;
    public GameObject noComponentsMessage;

    private GlitchedObject currentDisplayedTarget;
    private PlayerController currentPlayer;

    private void Start()
    {
        tabletPanel.SetActive(true);
        if(noTargetMessage != null) noTargetMessage.SetActive(true);
    }

    public void InitializePlayerInventory(PlayerController player)
    {
        currentPlayer = player;
        RefreshPlayerList();
    }

    public void UpdateTargetView(GlitchedObject target)
    {
        if (currentDisplayedTarget == target) return;

        currentDisplayedTarget = target;
        ClearContainer(objectComponentsContainer);

        if (target != null)
        {
            if(noTargetMessage != null) noTargetMessage.SetActive(false);

            foreach (var comp in target.activeComponents)
            {
                // DOMYŚLNIE 1
                int count = 1;

                // WYJĄTEK DLA MATERIAŁÓW: POBIERZ ROZMIAR STOSU
                if (comp == GlitchComponentType.MaterialSkin)
                {
                    count = target.GetMaterialStackSize();
                }

                CreateRow(comp, count, objectComponentsContainer, true);
            }
        }
        else
        {
            if(noTargetMessage != null) noTargetMessage.SetActive(true);
        }
        
        // Musimy też odświeżyć listę gracza, bo zmieniają się stany przycisków (Active/Installed)
        RefreshPlayerList(); 
    }

    public void OnInventoryChanged()
    {
        RefreshPlayerList();
        
        // Wymuszamy odświeżenie celu
        GlitchedObject tempTarget = currentDisplayedTarget;
        currentDisplayedTarget = null;
        UpdateTargetView(tempTarget);
    }

    void RefreshPlayerList()
    {
        ClearContainer(playerInventoryContainer);

        if (currentPlayer == null) return;

        // --- ALGORYTM GRUPOWANIA ---
        // 1. Zliczamy ile mamy sztuk każdego typu
        Dictionary<GlitchComponentType, int> inventoryCounts = new Dictionary<GlitchComponentType, int>();

        foreach (var item in currentPlayer.playerInventory)
        {
            if (inventoryCounts.ContainsKey(item))
                inventoryCounts[item]++;
            else
                inventoryCounts.Add(item, 1);
        }

        // 2. Tworzymy wiersze na podstawie zgrupowanych wyników
        foreach (var kvp in inventoryCounts)
        {
            GlitchComponentType type = kvp.Key;
            int count = kvp.Value;

            CreateRow(type, count, playerInventoryContainer, false);
        }
        // ---------------------------
        
        if (currentPlayer.playerInventory.Count == 0) 
            if(noComponentsMessage != null) noComponentsMessage.SetActive(true);
        else 
            if(noComponentsMessage != null) noComponentsMessage.SetActive(false);
    }

    // Zaktualizowana funkcja CreateRow przyjmująca 'count'
    void CreateRow(GlitchComponentType type, int count, Transform container, bool isTaking)
    {
        GameObject newRow = Instantiate(rowPrefab, container);
        TabletRowUI rowScript = newRow.GetComponent<TabletRowUI>();
        
        rowScript.Setup(type, count, currentDisplayedTarget, currentPlayer, isTaking, OnInventoryChanged);
    }

    void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}