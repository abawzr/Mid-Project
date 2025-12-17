using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody _rigidBody;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private bool _isRunning;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Calculate movement direction relative to camera
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction
        _moveDirection = cameraForward * _moveInput.y + cameraRight * _moveInput.x;

        // Apply movement
        _moveDirection *= _isRunning ? moveSpeed * sprintMultiplier : moveSpeed;

        _rigidBody.linearVelocity = _moveDirection;

        // Rotate player to face movement direction
        if (_moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        _isRunning = value.isPressed;
    }
}
