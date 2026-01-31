using UnityEngine;
using System.Collections.Generic;

public enum GlitchComponentType
{
	Collider,
	Gravity,
	Visibility,
	Pushable,
	Movable
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

	void Start()
	{
		foreach (var comp in startingComponents)
		{
			activeComponents.Add(comp);
		}
		UpdatePhysicalState();
	}

	public void AddComponent(GlitchComponentType type)
	{
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

	void UpdatePhysicalState()
	{
		// COLLIDER
		if (myCollider != null)
		{
			bool hasCollider = activeComponents.Contains(GlitchComponentType.Collider);
			gameObject.layer = LayerMask.NameToLayer(hasCollider ? "glitchedObject" : "ghostObject");
		}

		if (myRenderer != null)
			myRenderer.enabled = activeComponents.Contains(GlitchComponentType.Visibility);

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
}