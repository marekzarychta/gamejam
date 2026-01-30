using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabletRowUI : MonoBehaviour
{
    public TextMeshProUGUI componentNameText;
    public Button actionButton;
    public TextMeshProUGUI buttonText;

    private GlitchComponentType myType;
    private GlitchedObject currentTarget;
    private PlayerController player;
    
    // Zmienna przechowująca funkcję odświeżania z Managera
    private System.Action onActionCompleted; 
    private bool isTakingMode; // true = zabieramy od obiektu, false = dajemy obiektowi

    public void Setup(GlitchComponentType type, GlitchedObject target, PlayerController playerRef, bool takingMode, System.Action refreshCallback)
    {
        myType = type;
        currentTarget = target;
        player = playerRef;
        isTakingMode = takingMode;
        onActionCompleted = refreshCallback;

        componentNameText.text = type.ToString();

        // Konfiguracja wyglądu w zależności od trybu
        if (isTakingMode)
        {
            buttonText.text = "ZABIERZ";
            actionButton.image.color = new Color(1f, 0.5f, 0.5f); // Czerwonawy
        }
        else
        {
            buttonText.text = "WSTAW";
            actionButton.image.color = new Color(0.5f, 1f, 0.5f); // Zielonkawy
        }
    }

    // Podpięte pod przycisk w Unity
    public void OnButtonClicked()
    {
        if (isTakingMode)
        {
            // Zabieramy od obiektu -> do gracza
            currentTarget.RemoveComponent(myType);
            player.playerInventory.Add(myType);
        }
        else
        {
            // Dajemy od gracza -> do obiektu
            player.playerInventory.Remove(myType);
            currentTarget.AddComponent(myType);
        }

        // Ważne: Mówimy Managerowi "Coś się zmieniło, przerysuj listy!"
        onActionCompleted?.Invoke();
    }
}