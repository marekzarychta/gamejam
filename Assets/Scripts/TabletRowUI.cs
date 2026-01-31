using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabletRowUI : MonoBehaviour
{
	[Header("UI References")]
	public TextMeshProUGUI componentNameText;
	public TextMeshProUGUI quantityText;
	public Button actionButton;
	public TextMeshProUGUI buttonText;

	[Header("Visuals")]
	public Image image; // To jest tło wiersza
	public RawImage materialPreviewIcon; // <--- NOWOŚĆ: Mały kwadracik (przypisz w prefabie!)

	private GlitchComponentType myType;
	private GlitchedObject currentTarget;
	private PlayerController player;

	private System.Action onActionCompleted;
	private bool isTakingMode;

	// Przechowujemy oryginalny kolor tła, żeby go przywracać
	private Color defaultBackgroundColor = Color.white;
	private bool initialized = false;

	void Awake()
	{
		if (image != null)
		{
			defaultBackgroundColor = image.color;
			initialized = true;
		}
	}

	public void Setup(GlitchComponentType type, int quantity, GlitchedObject target, PlayerController playerRef, bool takingMode, bool isMissing, System.Action refreshCallback)
	{
		myType = type;
		currentTarget = target;
		player = playerRef;
		isTakingMode = takingMode;
		onActionCompleted = refreshCallback;

		// Zabezpieczenie inicjalizacji koloru (gdyby Awake nie zadziałał przed Setupem)
		if (!initialized && image != null)
		{
			defaultBackgroundColor = image.color;
			initialized = true;
		}

		// --- RESET WIZUALNY ---
		// Zawsze czyścimy wygląd przed ustawieniem nowych danych
		if (materialPreviewIcon != null)
		{
			materialPreviewIcon.gameObject.SetActive(false);
			materialPreviewIcon.texture = null;
			materialPreviewIcon.color = Color.white;
		}
		if (image != null)
		{
			image.color = defaultBackgroundColor;
		}
		// ----------------------

		if (isMissing)
		{
			componentNameText.text = type.ToString() + " <color=red>(MISSING)</color>";
			quantityText.gameObject.SetActive(false);

			buttonText.text = "";
			actionButton.interactable = false; // Nie można "zabrać" braku
			actionButton.image.color = Color.gray;
			return; // Kończymy konfigurację tutaj dla brakujących
		}

		componentNameText.text = type.ToString();

		// --- OBSŁUGA WIZUALNA MATERIAŁÓW (NOWOŚĆ) ---
		if (myType == GlitchComponentType.MaterialSkin)
		{
			UpdateMaterialVisuals();
		}
		// --------------------------------------------

		if (quantity > 1)
		{
			quantityText.text = "x" + quantity.ToString();
			quantityText.gameObject.SetActive(true);
		} else
		{
			quantityText.text = "";
			quantityText.gameObject.SetActive(false);
		}

		if (isTakingMode)
		{
			buttonText.text = "-";
			actionButton.image.color = new Color(1f, 0.5f, 0.5f);
			actionButton.interactable = true;
		} else
		{
			buttonText.text = "+";
			actionButton.image.color = new Color(0.5f, 1f, 0.5f);

			if (target != null && target.HasComponent(myType))
			{
				if (myType == GlitchComponentType.MaterialSkin)
				{
					actionButton.interactable = true;
					buttonText.text = "+";
				} else
				{
					actionButton.interactable = false;
					buttonText.text = "/";
					actionButton.image.color = Color.gray;
				}
			} else
			{
				actionButton.interactable = true;
			}
		}
	}

	// Nowa metoda obsługująca wygląd materiałów
	void UpdateMaterialVisuals()
	{
		Material matToDisplay = null;

		if (isTakingMode) // WIDOK CELU (Target) - Zmieniamy tło wiersza
		{
			if (currentTarget != null)
			{
				// Używamy metody dodanej wcześniej do GlitchedObject
				matToDisplay = currentTarget.GetCurrentMaterial();
			}

			if (matToDisplay != null && image != null)
			{
				Color matColor = matToDisplay.color;
				matColor.a = 0.25f; // Duża przezroczystość (25%)
				image.color = matColor;
			}
		} else // WIDOK GRACZA (Inventory) - Pokazujemy małą ikonkę
		{
			// Używamy metody dodanej wcześniej do PlayerController
			matToDisplay = player.GetCurrentHeldMaterial();

			if (matToDisplay != null && materialPreviewIcon != null)
			{
				materialPreviewIcon.gameObject.SetActive(true);

				if (matToDisplay.mainTexture != null)
				{
					materialPreviewIcon.texture = matToDisplay.mainTexture;
					materialPreviewIcon.color = Color.white;
				} else
				{
					materialPreviewIcon.texture = null;
					materialPreviewIcon.color = matToDisplay.color;
				}
			}
		}
	}

	public void OnButtonClicked()
	{
		if (currentTarget == null && !isTakingMode)
		{
			//Debug.LogWarning("Nie ma obiektu docelowego!");
			return;
		}

		if (myType == GlitchComponentType.MaterialSkin)
		{
			HandleMaterialTransfer();
		} else
		{
			HandleStandardTransfer();
		}

		onActionCompleted?.Invoke();
	}

	void HandleMaterialTransfer()
	{
		if (isTakingMode)
		{
			Material stolenMat = currentTarget.PopMaterial();

			if (stolenMat != null)
			{
				player.collectedMaterials.Push(stolenMat);
				player.playerInventory.Add(myType);
				if (currentTarget.GetMaterialStackSize() == 0)
				{
					currentTarget.RemoveComponent(myType);
				}
			}
		} else
		{
			if (player.collectedMaterials.Count > 0)
			{
				Material matToGive = player.collectedMaterials.Pop();

				currentTarget.AddComponent(myType);
				currentTarget.PushMaterial(matToGive);

				player.playerInventory.Remove(myType);
			}
		}
	}

	void HandleStandardTransfer()
	{
		if (isTakingMode)
		{
			currentTarget.RemoveComponent(myType);
			player.playerInventory.Add(myType);
		} else
		{
			if (!currentTarget.HasComponent(myType))
			{
				player.playerInventory.Remove(myType);
				currentTarget.AddComponent(myType);
			}
		}
	}
}