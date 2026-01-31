using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPPController : MonoBehaviour
{
	[Header("Movement Settings")]
	public bool groundedPlayer = true;
	public float moveSpeed = 6f;
	public float jumpHeight = 2f;
	public float gravity = -9.81f;

	[Header("Mouse Settings")]
	public Transform cameraTransform;
	public float mouseSensitivity = 200f;
	public float maxLookAngle = 80f;

	[Header("Tablet")]
	public Tablet tablet;

	[Header("Audio - Movement")]
	public List<AudioClip> footstepSounds; // Lista różnych dźwięków kroków dla różnorodności
	public AudioClip jumpSound;
	public AudioClip landSound;

	[Header("Audio Settings")]
	public float footstepInterval = 0.5f; // Jak często grać krok (w sekundach)
	public float velocityThreshold = 2f;
	
	private CharacterController controller;
	private Vector3 velocity;
	private Vector3 externalVelocity; //knockback 
	public float externalDamping = 8f;
	private float verticalLookRotation = 0f;

	// Zmienna stanu tabletu
	private bool isTabletVisible = false;
	private float speedMultiplier = 1f;

	private float footstepTimer;
	private bool wasGroundedLastFrame;
	
	void Start()
	{
		controller = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;

		isTabletVisible = false;
		if (tablet != null) tablet.SetState(isTabletVisible);
		
		wasGroundedLastFrame = true;
	}

	void Update()
	{
		HandleMovement();
		HandleMouseLook();
		TabletInput();
	}

	void HandleMovement()
	{
		// Ground check
		groundedPlayer = controller.isGrounded;

		if (groundedPlayer && !wasGroundedLastFrame)
		{
			// ... i spadaliśmy w dół (żeby nie grało przy wchodzeniu pod górkę)
			if (velocity.y < -0.1f)
			{
				AudioManager.Instance.PlaySfx(landSound, 0.2f);
			}
		}
		wasGroundedLastFrame = groundedPlayer;
		
		if (groundedPlayer && velocity.y < 0)
		{
			velocity.y = -2f;
		}

		// Input
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		Vector3 move = transform.right * x + transform.forward * z;
		controller.Move(move * (moveSpeed * speedMultiplier) * Time.deltaTime);

		// Jump
		if (Input.GetButtonDown("Jump") && groundedPlayer)
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
			// DŹWIĘK SKOKU (3D w miejscu skoku)
			AudioManager.Instance.PlaySfx(jumpSound, 0.7f);
		}

		// --- OBSŁUGA KROKÓW ---
		if (groundedPlayer && controller.velocity.magnitude > velocityThreshold)
		{
			footstepTimer -= Time.deltaTime;
			if (footstepTimer <= 0)
			{
				PlayRandomFootstep();
				footstepTimer = footstepInterval; // Reset timera
			}
		}
		else
		{
			// Reset timera jak stoimy, żeby pierwszy krok po ruszeniu był szybciej
			footstepTimer = 0.1f; 
		}

		// Gravity
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);

        //knockback
        if (externalVelocity.sqrMagnitude > 0.0001f)
        {
            controller.Move(externalVelocity * Time.deltaTime);
            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, externalDamping * Time.deltaTime);
        }
    }

	public void AddKnockback(Vector3 impulse)
	{
		impulse.y = 0f;
		externalVelocity += impulse;
    }

	void HandleMouseLook()
	{
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		verticalLookRotation -= mouseY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

		cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
		transform.Rotate(Vector3.up * mouseX);
	}

	public void SetSpeedMultiplier(float multiplier)
	{
		speedMultiplier = multiplier;
	}
	
	public void TabletInput()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			if (tablet != null)
			{
				isTabletVisible = !isTabletVisible;

				tablet.SetState(isTabletVisible);
			}
		}
	}

	private void PlayRandomFootstep()
	{
		if (footstepSounds.Count == 0) return;

		// Losujemy dźwięk z listy, żeby nie było monotonnie
		int index = UnityEngine.Random.Range(0, footstepSounds.Count);
		AudioClip clip = footstepSounds[index];
        
		// Gramy trochę ciszej i losujemy lekko pitch (wysokość), żeby brzmiało naturalniej
		AudioManager.Instance.PlaySfx(clip, UnityEngine.Random.Range(0.3f, 0.5f));
	}
	
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		/*Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic) return;

		Pushable pushableObject = hit.gameObject.GetComponent<Pushable>();
		if (pushableObject == null) return;

		if (hit.moveDirection.y < -0.3f) return;

		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

		Vector3 targetVelocity = pushDir * pushableObject.pushPower;

		pushableObject.Push(targetVelocity, transform);*/
		
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic) return;

		Pushable pushableObject = hit.gameObject.GetComponent<Pushable>();
		if (pushableObject == null) return;

		// Pchamy tylko w poziomie
		if (hit.moveDirection.y < -0.3f) return;
		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

		// Wywołujemy nową metodę bez "Transform pusher" i bez obliczania velocity
		pushableObject.Push(pushDir);
	}

}