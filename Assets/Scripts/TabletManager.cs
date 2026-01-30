using UnityEngine;
using System;

public class TabletManager : MonoBehaviour
{
    public GameObject tabletPanel; // Cały UI Panel (tło tabletu)
    public Transform contentListContainer; // Obiekt "Content" wewnątrz ScrollRect
    public GameObject rowPrefab; // Prefab z TabletRowUI

    private void Start()
    {
        tabletPanel.SetActive(false);
    }

    public void OpenTabletInterface(GlitchedObject target, PlayerController player)
    {
        tabletPanel.SetActive(true);

        // 1. Wyczyść starą listę
        foreach (Transform child in contentListContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Wygeneruj nową listę dla WSZYSTKICH możliwych typów komponentów
        foreach (GlitchComponentType type in Enum.GetValues(typeof(GlitchComponentType)))
        {
            GameObject newRow = Instantiate(rowPrefab, contentListContainer);
            TabletRowUI rowScript = newRow.GetComponent<TabletRowUI>();
            
            rowScript.Setup(type, target, player);
        }
    }

    public void CloseTabletInterface()
    {
        tabletPanel.SetActive(false);
    }
}