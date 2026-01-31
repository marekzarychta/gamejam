using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public Checkpoint checkpoint;
	
	private List<GlitchedObject> sceneObjects = new List<GlitchedObject>();

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}
	
	void Start()
	{
		GlitchedObject[] foundObjects = FindObjectsOfType<GlitchedObject>();
		sceneObjects.AddRange(foundObjects);

		Debug.Log($"Znaleziono {sceneObjects.Count} zglitchowanych obiekt�w.");
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			ValidateGameEnd();
		}
	}

	public void Respawn()
	{
		checkpoint.MoveToCheckpoint();
	}

	public float ValidateGameEnd()
	{
		float totalPercentage = 0f;
		int importantCount = 0;

		foreach (var obj in sceneObjects)
		{
			// Interesują nas tylko obiekty oznaczone jako ważne dla ukończenia poziomu
			if (obj.isImportant) 
			{ 
				float objProgress = obj.checkFixedState(); // Zwraca 0.0 - 1.0
				totalPercentage += objProgress;
				importantCount++;

				// Opcjonalnie: Raportuj stan każdego obiektu
				// Debug.Log($"Obiekt {obj.name}: {objProgress * 100:F0}%");
			}
		}

		if (importantCount > 0)
		{
			// Obliczamy średnią arytmetyczną
			float gameProgress = totalPercentage / importantCount;
            
			Debug.Log($"=== STATUS SYSTEMU: {gameProgress * 100:F1}% ===");

			// Sprawdzamy czy gra jest ukończona (z małym marginesem błędu dla floatów)
			if (gameProgress >= 0.99f)
			{
				Debug.Log("SYSTEM STABILNY. POZIOM UKOŃCZONY!");
			} 
			else
			{
				Debug.Log($"Wymagana dalsza naprawa. Brakuje {(1f - gameProgress) * 100:F1}%");
			}

			return gameProgress;
		}
		else
		{
			Debug.Log("Brak ważnych obiektów w scenie. Poziom automatycznie zaliczony?");
		}

		return 0f;
	}
}