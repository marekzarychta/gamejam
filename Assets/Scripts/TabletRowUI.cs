using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabletRowUI : MonoBehaviour
{
    public TextMeshProUGUI componentNameText;
    public TextMeshProUGUI quantityText;
    public Button actionButton;
    public TextMeshProUGUI buttonText;

    private GlitchComponentType myType;
    private GlitchedObject currentTarget;
    private PlayerController player;
    
    private System.Action onActionCompleted; 
    private bool isTakingMode;

    public void Setup(GlitchComponentType type, int quantity, GlitchedObject target, PlayerController playerRef, bool takingMode, System.Action refreshCallback)
    {
        myType = type;
        currentTarget = target;
        player = playerRef;
        isTakingMode = takingMode;
        onActionCompleted = refreshCallback;

        componentNameText.text = type.ToString();

        if (quantity > 1)
        {
            quantityText.text = "x" + quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }

        if (isTakingMode)
        {
            buttonText.text = "-";
            actionButton.image.color = new Color(1f, 0.5f, 0.5f);
            actionButton.interactable = true;
        }
        else
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
        } else {
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