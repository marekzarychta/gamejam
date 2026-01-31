using System;
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

	private CharacterController controller;
	private Vector3 velocity;
	private float verticalLookRotation = 0f;

	// Zmienna stanu tabletu
	private bool isTabletVisible = false;
	private float speedMultiplier = 1f;

	void Start()
	{
		controller = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;

		isTabletVisible = false;
		if (tablet != null) tablet.SetState(isTabletVisible);
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
		}

		// Gravity
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
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