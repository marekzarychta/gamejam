using UnityEngine;
using UnityEngine.UI;
using TMPro; // Użyj TextMeshPro dla lepszej jakości tekstu

public class TabletRowUI : MonoBehaviour
{
    public TextMeshProUGUI componentNameText;
    public Button actionButton; // Jeden przycisk, który zmienia funkcję (Weź / Daj)
    public TextMeshProUGUI buttonText;

    private GlitchComponentType myType;
    private GlitchedObject currentTarget;
    private PlayerController player;

    // Funkcja konfigurująca wygląd wiersza
    public void Setup(GlitchComponentType type, GlitchedObject target, PlayerController playerRef)
    {
        myType = type;
        currentTarget = target;
        player = playerRef;

        componentNameText.text = type.ToString();

        UpdateButtonState();
        
        // Resetowanie listenerów przycisku
        //actionButton.onClick.RemoveAllListeners();
        //actionButton.onClick.AddListener(OnButtonClicked);
    }

    void UpdateButtonState()
    {
        bool objectHasIt = currentTarget.HasComponent(myType);
        bool playerHasIt = player.playerInventory.Contains(myType);

        if (objectHasIt)
        {
            // Obiekt to ma -> Możemy zabrać
            actionButton.interactable = true;
            buttonText.text = "ZABIERZ";
            actionButton.image.color = Color.red; // Opcjonalnie: kolor
        }
        else if (playerHasIt)
        {
            // Obiekt nie ma, ale gracz ma -> Możemy dać
            actionButton.interactable = true;
            buttonText.text = "WSTAW";
            actionButton.image.color = Color.green;
        }
        else
        {
            // Nikt nie ma -> Nieaktywne
            actionButton.interactable = false;
            buttonText.text = "BRAK";
            actionButton.image.color = Color.gray;
        }
    }

    public void OnButtonClicked()
    {
        Debug.Log("Kliknięto przycisk! Typ: " + myType); // <--- To nam powie, czy Unity w ogóle widzi kliknięcie

        bool objectHasIt = currentTarget.HasComponent(myType);

        if (objectHasIt)
        {
            Debug.Log("Zabieram komponent...");
            currentTarget.RemoveComponent(myType);
            player.playerInventory.Add(myType);
        }
        else
        {
            Debug.Log("Wstawiam komponent...");
            player.playerInventory.Remove(myType);
            currentTarget.AddComponent(myType);
        }

        UpdateButtonState();
    }
}