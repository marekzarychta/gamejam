/*using UnityEngine;
using System.Collections.Generic;

public class GlitchedObject : MonoBehaviour
{
    public enum options {_Collider};
    public List<options> currentOptions = new List<options>();
    public Collider objectCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
            objectCollider.enabled = currentOptions.Contains(options._Collider);
            
            
    }
}*/

using UnityEngine;
using System.Collections.Generic;

public enum GlitchComponentType
{
    Collider,   // Fizyczna ściana
    Gravity,    // Czy spada?
    Visibility  // Czy jest widoczny (Renderer)?
}

public class GlitchedObject : MonoBehaviour
{
	[Header("Startowe Cechy")]
	public List<GlitchComponentType> startingComponents;
	public List<GlitchComponentType> finalState = new List<GlitchComponentType>();

	[Header("Is important")]
	public bool isImportant;

    [Header("Referencje do komponentów Unity")]
    // Przypisz te pola w Inspectorze, jeśli obiekt ma je obsługiwać
    public Collider myCollider;
    public Rigidbody myRigidbody;
    public Renderer myRenderer;
    
    public List<GlitchComponentType> activeComponents = new List<GlitchComponentType>();
    
    private Material myMaterial;
    private int hoverPropertyID;
    
    void Start()
    {
        // Inicjalizacja listy na start
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
        
        UpdatePhysicalState();
    }

    // Funkcja wywoływana przez Tablet, gdy zmieniamy coś w obiekcie
    public void AddComponent(GlitchComponentType type)
    {
        if (!activeComponents.Contains(type))
        {
            activeComponents.Add(type);
            UpdatePhysicalState();
        }
    }

    public void RemoveComponent(GlitchComponentType type)
    {
        if (activeComponents.Contains(type))
        {
            activeComponents.Remove(type);
            UpdatePhysicalState();
        }
    }

    public bool HasComponent(GlitchComponentType type)
    {
        return activeComponents.Contains(type);
    }

    // Ta funkcja tłumaczy nasze "Enums" na zachowanie Unity
    void UpdatePhysicalState()
    {
        if (myCollider != null)
        {
            bool shouldHaveCollider = activeComponents.Contains(GlitchComponentType.Collider);
            if (shouldHaveCollider)
            {
                gameObject.layer = LayerMask.NameToLayer("glitchedObject");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("ghostObject");
            }
        }
        
        if (myRigidbody != null) 
            myRigidbody.useGravity = activeComponents.Contains(GlitchComponentType.Gravity);

		if (myRenderer != null)
			myRenderer.enabled = activeComponents.Contains(GlitchComponentType.Visibility);
	}

    public bool checkFixedState()
    {
        return new HashSet<GlitchComponentType>(activeComponents).SetEquals(finalState);
    }
    
    public void SetHighlight(bool active)
    {
        if (myMaterial == null) return;

        // W shaderze boolean to tak naprawdę integer (0 = false, 1 = true)
        myMaterial.SetInt(hoverPropertyID, active ? 1 : 0);
    }
}