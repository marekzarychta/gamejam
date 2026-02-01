using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public enum GlitchComponentType
{
	Collider,
	Gravity,
	Visibility,
	Pushable,
	Movable,
	Rotatable,
	Enlarge,
	Shrink,
	MaterialSkin,
	Shooting
}

public class GlitchedObject : MonoBehaviour
{
	[Header("Startowe Cechy")]
	public List<GlitchComponentType> startingComponents;
	public List<GlitchComponentType> finalState = new List<GlitchComponentType>();

	[Header("Is important")]
	public bool isImportant;

	[Header("Audio Glitch")] // --- NOWA SEKCJA ---
	public AudioClip glitchLoopSound;
	[Range(0f, 1f)] public float glitchVolume = 0.5f;
	public float soundDistance = 10f; // Jak daleko słychać dźwięk
	public Vector2 pitchRange = new Vector2(0.8f, 1.2f); // Losowość (min, max)
	
	[Header("Referencje")]
	public Collider myCollider;
	public Rigidbody myRigidbody;
	public Renderer myRenderer;
	public Pushable myPushable;
	public Movable myMovable;
	public Rotatable myRotatable;
	public Enlarge myEnlarge;
	public Shrink myShrink;
	public Material nakedMaterial;
	public TurretShooter myTurretShooter;

	public HashSet<GlitchComponentType> activeComponents = new HashSet<GlitchComponentType>();

	private Stack<Material> materialHistory = new Stack<Material>();
	
	public List<GlitchComponentType> originalState = new List<GlitchComponentType>();
	private MaterialPropertyBlock _propBlock;
    private int hoverPropertyID;
	
	// NOWE: Zapamiętujemy czy gracz patrzy na obiekt
	private bool _isHoveredByPlayer = false;
	private Outline myOutline;

	private AudioSource _glitchAudioSource;
	private Coroutine _glitchCoroutine;
	
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

		if (glitchLoopSound != null)
		{
			_glitchAudioSource = gameObject.AddComponent<AudioSource>();
			_glitchAudioSource.clip = glitchLoopSound;
			_glitchAudioSource.spatialBlend = 1.0f; // 1.0 = Pełne 3D (słychać kierunek i odległość)
			_glitchAudioSource.minDistance = 1f;
			_glitchAudioSource.maxDistance = soundDistance;
			_glitchAudioSource.rolloffMode = AudioRolloffMode.Linear; // Liniowe wyciszanie wraz z dystansem
			_glitchAudioSource.volume = glitchVolume * 0.2f;
			_glitchAudioSource.playOnAwake = false;
			_glitchAudioSource.loop = false; // Sami obsługujemy pętlę, żeby zmieniać pitch
		}
		
		myOutline = GetComponent<Outline>();
		if (myOutline == null)
		{
			myOutline = gameObject.AddComponent<Outline>();
		}
		
		myOutline.OutlineMode = Outline.Mode.OutlineVisible;
		myOutline.OutlineColor = Color.green;
		myOutline.OutlineWidth = 5f;
        
		// Na start wyłączamy obrys
		myOutline.enabled = false;

		foreach (var comp in startingComponents)
		{
			originalState.Add(comp);
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
			myCollider.gameObject.layer = LayerMask.NameToLayer(hasCollider ? "glitchedObject" : "ghostObject");
		}

		if (myRenderer != null)
		{
			myRenderer.enabled = activeComponents.Contains(GlitchComponentType.Visibility);
			UpdateVisuals();
		}

		bool isMovable = activeComponents.Contains(GlitchComponentType.Movable);
		bool isPushable = activeComponents.Contains(GlitchComponentType.Pushable);
		bool hasGravity = activeComponents.Contains(GlitchComponentType.Gravity);
		bool isShooting = activeComponents.Contains(GlitchComponentType.Shooting);

        if (myMovable != null) myMovable.enabled = isMovable;
		if (myPushable != null) myPushable.enabled = isPushable;
		if (myTurretShooter != null) myTurretShooter.enabled = isShooting;

		if (myRotatable != null) myRotatable.enabled = activeComponents.Contains(GlitchComponentType.Rotatable);

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
		List<GlitchComponentType> tasksToComplete = new List<GlitchComponentType>();

		foreach (var req in finalState)
		{
			// Jeśli komponent nie znajdował się na liście startowej, to znaczy, że trzeba go zdobyć.
			if (!startingComponents.Contains(req))
			{
				tasksToComplete.Add(req);
			}
		}

		// Zabezpieczenie: Jeśli nie ma nic do naprawienia (obiekt od początku spełniał wymogi), to 100%
		if (tasksToComplete.Count == 0) return 1f;

		int completedTasks = 0;
		foreach (var task in tasksToComplete)
		{
			// Czy obiekt ma teraz ten wymagany komponent?
			if (activeComponents.Contains(task))
			{
				completedTasks++;
			}
		}

		return (float)completedTasks / tasksToComplete.Count;
	}
    
	public void SetHoverOutline(bool show)
	{
		if (myOutline != null)
		{
			myOutline.enabled = show;
		}
	}

    // Wewnętrzna logika decydująca o świeceniu
    private void UpdateHighlightState()
    {
	    // Sprawdzamy warunek: Ważny i Nienaprawiony
	    bool isGlitching = isImportant && checkFixedState() < 0.99f;

	    // 1. OBSŁUGA VISUAL (MATRIX)
	    if (myRenderer != null)
	    {
		    myRenderer.GetPropertyBlock(_propBlock);
		    _propBlock.SetInt(hoverPropertyID, isGlitching ? 1 : 0);
		    myRenderer.SetPropertyBlock(_propBlock);
	    }

	    // 2. OBSŁUGA AUDIO (LOOPER)
	    if (_glitchAudioSource != null)
	    {
		    if (isGlitching)
		    {
			    // Jeśli powinno grać, a nie gra -> startujemy korutynę
			    if (_glitchCoroutine == null)
			    {
				    _glitchCoroutine = StartCoroutine(GlitchSoundLoop());
			    }
		    }
		    else
		    {
			    // Jeśli nie powinno grać -> zatrzymujemy
			    if (_glitchCoroutine != null)
			    {
				    StopCoroutine(_glitchCoroutine);
				    _glitchCoroutine = null;
				    _glitchAudioSource.Stop();
			    }
		    }
	    }
    }

    // --- KORUTYNA DO ZMIANY PITCHA ---
    IEnumerator GlitchSoundLoop()
    {
	    while (true)
	    {
		    // 1. Losujemy pitch
		    float randomPitch = Random.Range(pitchRange.x, pitchRange.y);
		    _glitchAudioSource.pitch = randomPitch;
            
		    // 2. Gramy dźwięk
		    _glitchAudioSource.Play();

		    // 3. Czekamy aż się skończy (długość klipu / prędkość odtwarzania)
		    // Dzięki temu nie ma przerw ani nakładania się
		    float waitTime = glitchLoopSound.length / Mathf.Abs(randomPitch);
            
		    // Odejmujemy odrobinę (np. 0.05s), żeby pętla była bardziej "seamless" (bez przerw)
		    yield return new WaitForSeconds(waitTime - 0.05f);
	    }
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