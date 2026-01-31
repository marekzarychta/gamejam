using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Potrzebne do obsługi Buttonów

public class NextLevelManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform victoryCanvas;
    public TMP_Text victoryText;
    public TMP_Text ratingText;
    
    [Header("Buttons")]
    public Button nextLevelButton;
    public Button restartButton;
    public Button continueButton;
    public TMP_Text nextLevelText;

    private bool shown = false;

    // Progi zaliczenia (0.5 = 50%)
    private const float PASS_THRESHOLD = 0.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && shown)
        {
            ContinueLevel();
        }
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (shown) return;
        if (other.CompareTag("Player"))
        {
            shown = true;
            CalculateCompletitionPercent();
            victoryCanvas.gameObject.SetActive(true);
            
            // Odblokuj kursor, żeby można było klikać w UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var player = other.GetComponent<PlayerController>();
            if (player != null) player.ToggleTablet(true);
            // Opcjonalnie: Zatrzymaj gracza/czas
            // Time.timeScale = 0f; 
        }
    }

    public void CalculateCompletitionPercent()
    {
        // Pobieramy procent (0.0 do 1.0)
        float percent = GameManager.Instance.ValidateGameEnd();
        
        // Konwertujemy na ładny format procentowy (np. 85%)
        float displayPercent = percent * 100f;

        // 1. USTAWIANIE VICTORY TEXT (Tytuł)
        if (Mathf.Approximately(percent, 1f)) // 100%
        {
            victoryText.text = "PERFECT!";
            victoryText.color = new Color(0f, 1f, 1f); // Cyan
        }
        else if (percent >= 0.75f) // 75% - 99%
        {
            victoryText.text = "GOOD JOB!";
            victoryText.color = Color.green;
        }
        else if (percent >= 0.50f) // 50% - 74%
        {
            victoryText.text = "SYSTEM STABLE";
            victoryText.color = Color.yellow;
        }
        else // < 50% (Porażka)
        {
            victoryText.text = "CRITICAL FAILURE";
            victoryText.color = Color.red;
        }

        // 2. USTAWIANIE RATING TEXT (Steam Style)
        string steamDesc = GetSteamRatingDescription(percent);
        Color ratingColor = GetRatingColor(percent);

        // Format: "95% (Overwhelmingly Positive)"
        ratingText.text = $"{displayPercent:F0}% ({steamDesc})";
        ratingText.color = ratingColor;
        
        if (percent < PASS_THRESHOLD)
        {
            if (nextLevelButton != null) nextLevelButton.interactable = false;
            nextLevelText.alpha = 0.4f;
        }
        else
        {
            if (nextLevelButton != null) nextLevelButton.interactable = true;
            nextLevelText.alpha = 1f;
        }
    }

    // Helper: Opisy w stylu Steam
    private string GetSteamRatingDescription(float p)
    {
        if (p >= 0.95f) return "Overwhelmingly Positive";
        if (p >= 0.80f) return "Very Positive";
        if (p >= 0.70f) return "Mostly Positive";
        if (p >= 0.40f) return "Mixed";
        if (p >= 0.20f) return "Mostly Negative";
        return "Overwhelmingly Negative";
    }

    // Helper: Kolory ocen
    private Color GetRatingColor(float p)
    {
        if (p >= 0.70f) return new Color(0.4f, 0.8f, 1f); // Jasny niebieski/zielony
        if (p >= 0.40f) return new Color(0.7f, 0.6f, 0.3f); // Brudny żółty (Mixed)
        return new Color(0.8f, 0.2f, 0.2f); // Czerwony
    }

    public void NextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("To był ostatni poziom!");
             SceneManager.LoadScene(0); 
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ContinueLevel()
    {
        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (player != null)
        {
            shown = false;
            player.endScreenMode = false;
            player.ToggleTablet(false);
            victoryCanvas.gameObject.SetActive(false);
        }
    }
}