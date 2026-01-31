using UnityEngine;
using System.Collections.Generic;

public enum GlitchComponentType
{
	Collider,
	Gravity,
	Visibility,
	Pushable,
	Movable,
	MaterialSkin
}

public class GlitchedObject : MonoBehaviour
{
	[Header("Startowe Cechy")]
	public List<GlitchComponentType> startingComponents;
	public List<GlitchComponentType> finalState = new List<GlitchComponentType>();

	[Header("Is important")]
	public bool isImportant;

	[Header("Referencje")]
	public Collider myCollider;
	public Rigidbody myRigidbody;
	public Renderer myRenderer;
	public Pushable myPushable;
	public Movable myMovable;
	public Material nakedMaterial;

	public HashSet<GlitchComponentType> activeComponents = new HashSet<GlitchComponentType>();

	private Stack<Material> materialHistory = new Stack<Material>();
	
	private Material myMaterial;
	private MaterialPropertyBlock _propBlock;
    private int hoverPropertyID;

	void Start()
	{
		if (nakedMaterial == null)
		{
			Debug.LogWarning("BRAK PRZYPISANEGO NAKED MATERIAL W OBIEKCIE: " + gameObject.name);
		}
		
		/*
		if (nakedMaterial == null)
		{
			nakedMaterial = new Material(Shader.Find("Standard"));
			nakedMaterial.color = new Color(1f, 0f, 1f); // W�ciek�y R� (Magenta)
			nakedMaterial.name = "ERROR_NO_TEXTURE";
		}*/

		foreach (var comp in startingComponents)
		{
			activeComponents.Add(comp);
		}

		_propBlock = new MaterialPropertyBlock();
		if (myRenderer != null)
        {
            //myMaterial = myRenderer.material; 
            hoverPropertyID = Shader.PropertyToID("_IsHovered");
        }

		if (myRenderer != null)
		{
			if (activeComponents.Contains(GlitchComponentType.MaterialSkin))
			{
				materialHistory.Push(myRenderer.material);
			}
		}

		UpdatePhysicalState();
	}

	public void AddComponent(GlitchComponentType type)
	{
		bool isNew = !activeComponents.Contains(type);
		if (activeComponents.Add(type))
		{
			/*if (type == GlitchComponentType.MaterialSkin && isNew)
			{
				if (myRenderer != null && materialHistory.Count == 0)
				{
					materialHistory.Push(myRenderer.material);
				}
			}*/
			UpdatePhysicalState();
		}
	}

	public void RemoveComponent(GlitchComponentType type)
	{
		if (activeComponents.Remove(type))
		{	
			UpdatePhysicalState();
		}
	}

	public bool HasComponent(GlitchComponentType type)
	{
		return activeComponents.Contains(type);
	}

	public void PushMaterial(Material newMat)
	{
		materialHistory.Push(newMat);
		UpdateVisuals();
	}

	public Material PopMaterial()
	{
		if (materialHistory.Count > 0)
		{
			Material mat = materialHistory.Pop();
			UpdateVisuals();
			return mat;
		}
		return null; 
	}

	public int GetMaterialStackSize()
	{
		return materialHistory.Count;
	}

	void UpdateVisuals()
	{
		if (myRenderer == null) return;

		if (activeComponents.Contains(GlitchComponentType.MaterialSkin) && materialHistory.Count > 0)
		{
			myRenderer.material = materialHistory.Peek();
		} else
		{
			myRenderer.material = nakedMaterial;
		}
	}

	void UpdatePhysicalState()
	{
		// COLLIDER
		if (myCollider != null)
		{
			bool hasCollider = activeComponents.Contains(GlitchComponentType.Collider);
			gameObject.layer = LayerMask.NameToLayer(hasCollider ? "glitchedObject" : "ghostObject");
		}

		if (myRenderer != null)
		{
			myRenderer.enabled = activeComponents.Contains(GlitchComponentType.Visibility);
			UpdateVisuals();
		}

		bool isMovable = activeComponents.Contains(GlitchComponentType.Movable);
		bool isPushable = activeComponents.Contains(GlitchComponentType.Pushable);
		bool hasGravity = activeComponents.Contains(GlitchComponentType.Gravity);

		if (myMovable != null) myMovable.enabled = isMovable;
		if (myPushable != null) myPushable.enabled = isPushable;

		if (myRigidbody != null)
		{
			// MOVABLE (Winda/Platforma) - ma najwyższy priorytet
			if (isMovable) 
			{
				myRigidbody.isKinematic = true; // MUSI być true, żeby skrypt Movable nim sterował
				myRigidbody.useGravity = false;
				myRigidbody.constraints = RigidbodyConstraints.None;
				myRigidbody.interpolation = RigidbodyInterpolation.None; // Kinematic move position tego nie potrzebuje aż tak
			} 
			// PUSHABLE (Fizyczna skrzynia)
			else if (isPushable) 
			{
				myRigidbody.isKinematic = false; // Fizyka musi działać
				myRigidbody.useGravity = hasGravity; // Grawitacja zależy od komponentu Gravity
				myRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				
				// Ustawiamy masę lub damping, żeby czuć ciężar
				myRigidbody.linearDamping = 1f; 
				myRigidbody.angularDamping = 0.5f;
				myRigidbody.constraints = RigidbodyConstraints.None; // Pozwalamy się przewracać
			}
			// DEFAULT (Ani to, ani to - np. statyczna ściana albo lewitujący obiekt)
			else 
			{
				// Jeśli ma grawitację, to spada, jeśli nie, to wisi w powietrzu (Kinematic)
				if (hasGravity)
				{
					myRigidbody.isKinematic = false;
					myRigidbody.useGravity = true;
				}
				else
				{
					myRigidbody.isKinematic = true; // Zastyga w powietrzu
					myRigidbody.useGravity = false;
				}
			}
			
			/*myRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

			if (isMovable) {
				myRigidbody.isKinematic = true;
				myRigidbody.useGravity = false;
				myRigidbody.constraints = RigidbodyConstraints.None;
			} else {
				if (myRigidbody.isKinematic)
				{
					myRigidbody.isKinematic = false;
					myRigidbody.linearVelocity = Vector3.zero;
					myRigidbody.angularVelocity = Vector3.zero;
				}
                else
                {
					myRigidbody.isKinematic = false;
				}

				myRigidbody.useGravity = hasGravity;

				if (isPushable)
				{
					myRigidbody.linearDamping = 5f;
					myRigidbody.angularDamping = 5f;
					myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
				} else
				{
					myRigidbody.linearDamping = 0.5f;
					myRigidbody.constraints = RigidbodyConstraints.None;
				}
			}*/
		}
	}

	public bool checkFixedState()
	{
		return activeComponents.SetEquals(finalState);
	}
    
    public void SetHighlight(bool active)
    {
        if (myRenderer == null) return;

        // 1. Pobierz aktualne właściwości z Renderera (żeby nie nadpisać innych zmian)
        myRenderer.GetPropertyBlock(_propBlock);

        // 2. Ustaw wartość w bloku (zamiast w materiale)
        _propBlock.SetInt(hoverPropertyID, active ? 1 : 0);

        // 3. Zaaplikuj blok z powrotem na Renderer
        myRenderer.SetPropertyBlock(_propBlock);
    }

}