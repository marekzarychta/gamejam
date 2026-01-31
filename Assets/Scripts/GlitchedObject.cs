using UnityEngine;
using System.Collections.Generic;

public enum GlitchComponentType
{
	Collider,
	Gravity,
	Visibility,
	Pushable,
	Movable,
	Enlarge,
	Shrink,
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
	public Enlarge myEnlarge;
	public Shrink myShrink;
	public Material nakedMaterial;

	public HashSet<GlitchComponentType> activeComponents = new HashSet<GlitchComponentType>();

	private Stack<Material> materialHistory = new Stack<Material>();
	
	private MaterialPropertyBlock _propBlock;
    private int hoverPropertyID;
	
	// NOWE: Zapamiętujemy czy gracz patrzy na obiekt
	private bool _isHoveredByPlayer = false;

	void Awake()
	{
		if (nakedMaterial == null)
		{
			Debug.LogWarning("BRAK PRZYPISANEGO NAKED MATERIAL W OBIEKCIE: " + gameObject.name);
		}

		foreach (var comp in startingComponents)
		{
			activeComponents.Add(comp);
		}

		_propBlock = new MaterialPropertyBlock();
		if (myRenderer != null)
        {
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
		UpdateHighlightState(); // Inicjalizacja podświetlenia
	}

	public void AddComponent(GlitchComponentType type)
	{
		bool isNew = !activeComponents.Contains(type);
		if (activeComponents.Add(type))
		{
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

		if (myEnlarge != null) myEnlarge.enabled = activeComponents.Contains(GlitchComponentType.Enlarge);
		if (myShrink != null) myShrink.enabled = activeComponents.Contains(GlitchComponentType.Shrink);

		if (myRigidbody != null)
		{
			if (isMovable) 
			{
				myRigidbody.isKinematic = true; 
				myRigidbody.useGravity = false;
				myRigidbody.constraints = RigidbodyConstraints.None;
				myRigidbody.interpolation = RigidbodyInterpolation.None; 
			} 
			else if (isPushable) 
			{
				myRigidbody.isKinematic = false; 
				myRigidbody.useGravity = hasGravity; 
				myRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				
				myRigidbody.linearDamping = 1f; 
				myRigidbody.angularDamping = 0.5f;
				myRigidbody.constraints = RigidbodyConstraints.None; 
			}
			else 
			{
				if (hasGravity)
				{
					myRigidbody.isKinematic = false;
					myRigidbody.useGravity = true;
				}
				else
				{
					myRigidbody.isKinematic = true; 
					myRigidbody.useGravity = false;
				}
			}
		}

		// Po każdej zmianie fizycznej sprawdzamy, czy naprawiliśmy obiekt (żeby ewentualnie zgasić matrixa)
		UpdateHighlightState();
	}

	public float checkFixedState()
	{
		if (finalState.Count == 0) return 1f;

		int matches = 0;
		foreach (var requiredComponent in finalState)
		{
			if (activeComponents.Contains(requiredComponent))
			{
				matches++;
			}
		}

		return (float)matches / finalState.Count;
	}
    
    // Zmodyfikowana funkcja, którą wywołuje PlayerController
    public void SetHighlight(bool active)
    {
        //_isHoveredByPlayer = active;
        //UpdateHighlightState();
    }

    // Wewnętrzna logika decydująca o świeceniu
    private void UpdateHighlightState()
    {
        if (myRenderer == null) return;

        bool shouldGlow = _isHoveredByPlayer;

        // Jeśli obiekt jest ważny i NIE jest w pełni naprawiony (< 99%), to świeci zawsze
        if (isImportant && checkFixedState() < 0.99f)
        {
            shouldGlow = true;
        }

        myRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetInt(hoverPropertyID, shouldGlow ? 1 : 0);
        myRenderer.SetPropertyBlock(_propBlock);
    }

	// returns top material from the stack without popping
	public Material GetCurrentMaterial()
	{
		if (activeComponents.Contains(GlitchComponentType.MaterialSkin) && materialHistory.Count > 0)
		{
			return materialHistory.Peek();
		}
		return nakedMaterial;
	}

}