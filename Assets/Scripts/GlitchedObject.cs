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

	public HashSet<GlitchComponentType> activeComponents = new HashSet<GlitchComponentType>();

	private Stack<Material> materialHistory = new Stack<Material>();

	private static Material nakedMaterial;
	
	private Material myMaterial;
    private int hoverPropertyID;

	void Start()
	{
		if (nakedMaterial == null)
		{
			nakedMaterial = new Material(Shader.Find("Standard"));
			nakedMaterial.color = new Color(1f, 0f, 1f); // W�ciek�y R� (Magenta)
			nakedMaterial.name = "ERROR_NO_TEXTURE";
		}

		foreach (var comp in startingComponents)
		{
			activeComponents.Add(comp);
		}

		if (myRenderer != null)
        {
            // Pobieramy instancję materiału, żeby nie zmieniać wszystkich obiektów na raz
            myMaterial = myRenderer.material; 
            // Zamieniamy tekst "_IsHovered" na szybki ID
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
			if (type == GlitchComponentType.MaterialSkin && isNew)
			{
				if (myRenderer != null && materialHistory.Count == 0)
				{
					materialHistory.Push(myRenderer.material);
				}
			}
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
			myRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

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
			}
		}
	}

	public bool checkFixedState()
	{
		return activeComponents.SetEquals(finalState);
	}
    
    public void SetHighlight(bool active)
    {
        if (myMaterial == null) return;

        // W shaderze boolean to tak naprawdę integer (0 = false, 1 = true)
        myMaterial.SetInt(hoverPropertyID, active ? 1 : 0);
    }

}