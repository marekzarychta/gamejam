using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabletRowUI : MonoBehaviour
{
    public TextMeshProUGUI componentNameText;
    public TextMeshProUGUI quantityText; // <--- NOWE POLE (Dodaj mały tekst w rogu przycisku)
    public Button actionButton;
    public TextMeshProUGUI buttonText;

    private GlitchComponentType myType;
    private GlitchedObject currentTarget;
    private PlayerController player;
    
    private System.Action onActionCompleted; 
    private bool isTakingMode;

    // Dodajemy parametr 'int quantity'
    public void Setup(GlitchComponentType type, int quantity, GlitchedObject target, PlayerController playerRef, bool takingMode, System.Action refreshCallback)
    {
        myType = type;
        currentTarget = target;
        player = playerRef;
        isTakingMode = takingMode;
        onActionCompleted = refreshCallback;

        componentNameText.text = type.ToString();

        // Obsługa wyświetlania ilości
        if (quantity > 1)
        {
            quantityText.text = "x" + quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false); // Ukrywamy, jak jest tylko 1 sztuka
        }

        if (isTakingMode)
        {
            buttonText.text = "REMOVE";
            actionButton.image.color = new Color(1f, 0.5f, 0.5f);
            actionButton.interactable = true;
        }
        else
        {
            buttonText.text = "ADD";
            actionButton.image.color = new Color(0.5f, 1f, 0.5f);

            // WAŻNE: Jeżeli obiekt już ma ten komponent, nie możemy dodać drugiego takiego samego
            // Blokujemy przycisk, żeby nie tracić komponentów bez sensu
            if (target != null && target.HasComponent(myType))
            {
                actionButton.interactable = false;
                buttonText.text = "ADDED";
                actionButton.image.color = Color.gray;
            }
            else
            {
                actionButton.interactable = true;
            }
        }
    }

    public void OnButtonClicked()
    {
        if (currentTarget == null && !isTakingMode) 
        {
            Debug.LogWarning("Nie ma obiektu docelowego!");
            return;
        }

        if (isTakingMode)
        {
            // Zabieramy od obiektu -> Dodajemy do listy gracza (kolejna sztuka)
            currentTarget.RemoveComponent(myType);
            player.playerInventory.Add(myType);
        }
        else
        {
            // Dajemy obiektowi
            if (!currentTarget.HasComponent(myType))
            {
                // Remove usuwa TYLKO PIERWSZE wystąpienie z listy. 
                // Jeśli masz Gravity, Gravity - usunie jedno, drugie zostanie.
                player.playerInventory.Remove(myType);
                currentTarget.AddComponent(myType);
            }
        }

        onActionCompleted?.Invoke();
    }
}