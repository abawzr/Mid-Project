using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController _characterController;
    private Animator _animator;
    private PlayerReferences _playerReferences;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private float _verticalVelocity;
    private bool _isGrounded;

    public bool IsGrounded => _isGrounded;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _playerReferences = GetComponent<PlayerReferences>();
    }

    private void OnEnable()
    {
        PlayerAttack.OnPlayerAttack += ModifyMoveSpeedWhenAttacking;
    }

    private void OnDisable()
    {
        PlayerAttack.OnPlayerAttack -= ModifyMoveSpeedWhenAttacking;
    }

    private void Update()
    {
        _isGrounded = _characterController.isGrounded;

        ApplyGravity();

        ApplyMovement();

        RotatePlayer();

        UpdateAnimation();
    }

    private void ApplyMovement()
    {
        // Don't move if dodging
        if (_playerReferences.Dodge != null && _playerReferences.Dodge.IsDodging)
        {
            return;
        }

        float inputMagnitude = _moveInput.magnitude;
        inputMagnitude = Mathf.Clamp01(inputMagnitude);

        Vector2 inputDirection = _moveInput.normalized;

        // Calculate movement direction relative to camera
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction
        _moveDirection = cameraForward * inputDirection.y + cameraRight * inputDirection.x;

        // Apply movement
        Vector3 velocity = _moveDirection * (moveSpeed * inputMagnitude);
        velocity.y = _verticalVelocity;

        _characterController.Move(velocity * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;
        }

        else if (!_characterController.isGrounded)
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void RotatePlayer()
    {
        // Rotate player to face movement direction
        if (_moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimation()
    {
        _animator.SetFloat("Speed", _moveInput.magnitude);
    }

    private void ModifyMoveSpeedWhenAttacking(bool isAttacking)
    {
        if (isAttacking)
        {
            moveSpeed *= 0.1f;
        }

        else
        {
            moveSpeed /= 0.1f;
        }
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!_characterController.isGrounded) return;

        if (_playerReferences.Attack.IsAttacking) return;

        if (_playerReferences.Dodge.IsDodging) return;

        _verticalVelocity = jumpPower;
        _animator.SetTrigger("Jump");
    }
}
