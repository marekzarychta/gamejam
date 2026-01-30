using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private List<GlitchedObject> sceneObjects = new List<GlitchedObject>();

	void Start()
	{
		GlitchedObject[] foundObjects = FindObjectsOfType<GlitchedObject>();
		sceneObjects.AddRange(foundObjects);

		Debug.Log($"Znaleziono {sceneObjects.Count} zglitchowanych obiektów.");
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			ValidateGameEnd();
		}
	}

	void ValidateGameEnd()
	{
		bool allFixed = true;

		foreach (var obj in sceneObjects)
		{
			if (obj.isImportant) { 
				if (!obj.checkFixedState())
				{
					allFixed = false;
					Debug.Log($"Obiekt {obj.name} jest Ÿle skonfigurowany.");
					break;
				}
			}
		}

		if (allFixed)
		{
			Debug.Log("UDALO SIE! Wszystkie obiekty naprawione.");
		} else
		{
			Debug.Log("Jeszcze nie koniec...");
		}
	}
}