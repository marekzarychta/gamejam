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

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalLookRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        // Ground check
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && velocity.y < 0)
        {
            velocity.y = 0f;
        }

        // Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

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
}
