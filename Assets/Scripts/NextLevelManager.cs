using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Potrzebne do obsługi Buttonów

public class NextLevelManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform victoryCanvas;
    public TMP_Text ratingText;
    public TMP_Text nameText;
    public TMP_Text reviewText;
    public Image positiveImage;
    public Image negativeImage;
    
    [Header("Buttons")]
    public Button nextLevelButton;
    public Button restartButton;
    public Button continueButton;
    public TMP_Text nextLevelText;

    [Header("Audio")]
    public AudioClip victorySound;
    public AudioClip failureSound;
    
    private bool shown = false;

    // Progi zaliczenia (0.5 = 50%)
    private const float PASS_THRESHOLD = 0.5f;

    void Start()
    {
        nextLevelButton.onClick.AddListener(NextLevel);
        restartButton.onClick.AddListener(RestartLevel);
        continueButton.onClick.AddListener(ContinueLevel);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && shown)
        {
            shown = false;
            victoryCanvas.gameObject.SetActive(false);
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

        if (percent >= PASS_THRESHOLD)
        {
            AudioManager.Instance.PlayUISFX(victorySound);
        }
        else
        {
            AudioManager.Instance.PlayUISFX(failureSound);
        }
        
        // 1. USTAWIANIE VICTORY TEXT (Tytuł)
        if (Mathf.Approximately(percent, 1f))
        {
            nameText.text = "Stinky Toe";
            reviewText.text = "This should be the golden industry standard!!!";
        }
        else if (percent >= 0.7f)
        {
            nameText.text = "Evil Vampire";
            reviewText.text = "Can't believe it, so little bugs!";
        }
        else if (percent >= 0.50f)
        {
            nameText.text = "XSlayerX";
            reviewText.text = "It's pretty good, but undercooked";
        }
        else if (percent >= 0.2f)
        {
            nameText.text = "Tickle Berry";
            reviewText.text = "They really didn't care about this game";
        }
        else
        {
            nameText.text = "Paris Hilton xxx";
            reviewText.text = "WHO TF PLAYTESTED THIS ####??";
        }

        if (percent >= 0.5f)
        {
            positiveImage.gameObject.SetActive(true);
            negativeImage.gameObject.SetActive(false);
        }
        else
        {
            positiveImage.gameObject.SetActive(false);
            negativeImage.gameObject.SetActive(true);
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