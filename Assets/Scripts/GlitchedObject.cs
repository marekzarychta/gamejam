using UnityEngine;
using System.Collections.Generic;
// using System.Linq; // Odkomentuj, jeśli chcesz używać LINQ

public enum GlitchComponentType
{
	Collider,
	Gravity,
	Visibility,
	Pushable
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

	public List<GlitchComponentType> activeComponents = new List<GlitchComponentType>();

	void Start()
	{
		activeComponents = new List<GlitchComponentType>(startingComponents);
		UpdatePhysicalState();
	}

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

	void UpdatePhysicalState()
	{
		if (myCollider != null)
		{
			bool hasCollider = activeComponents.Contains(GlitchComponentType.Collider);
			gameObject.layer = LayerMask.NameToLayer(hasCollider ? "glitchedObject" : "ghostObject");
		}

		if (myRigidbody != null)
			myRigidbody.useGravity = activeComponents.Contains(GlitchComponentType.Gravity);

		if (myRenderer != null)
			myRenderer.enabled = activeComponents.Contains(GlitchComponentType.Visibility);
		if (myPushable != null)
			myPushable.enabled = activeComponents.Contains (GlitchComponentType.Pushable);
	}

	public bool checkFixedState()
	{
		return new HashSet<GlitchComponentType>(activeComponents).SetEquals(finalState);
	}
}