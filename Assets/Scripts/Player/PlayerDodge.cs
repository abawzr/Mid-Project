using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDodge : MonoBehaviour
{
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDistance = 5f;
    [SerializeField] private float dodgeDuration = 0.5f;
    [SerializeField] private float dodgeCooldown = 1f;
    [SerializeField] private float invincibilityDuration = 0.5f;

    private Rigidbody _rigidBody;
    private Animator _animator;
    private PlayerHealth _playerHealth;
    private bool _isDodging = false;
    private bool _canDodge = true;
    private float _dodgeTimer = 0f;
    private Vector3 _dodgeDirection;

    public bool IsDodging => _isDodging;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (_isDodging)
        {
            _dodgeTimer += Time.deltaTime;

            if (_dodgeTimer >= dodgeDuration)
            {
                EndDodge();
            }
        }
    }

    private void FixedUpdate()
    {
        if (_isDodging)
        {
            // Apply dodge movement
            _rigidBody.linearVelocity = new Vector3(_dodgeDirection.x * dodgeDistance / dodgeDuration, _rigidBody.linearVelocity.y, _dodgeDirection.z * dodgeDistance / dodgeDuration);
        }
    }

    private void StartDodge()
    {
        _isDodging = true;
        _canDodge = false;
        _dodgeTimer = 0f;

        // Get dodge direction (current movement direction or backward if not moving)
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            // Dodge in the direction player is facing
            _dodgeDirection = transform.forward;
        }
        else
        {
            _dodgeDirection = -transform.forward; // Backward dodge as fallback
        }

        // Trigger dodge animation
        if (_animator != null)
        {
            _animator.SetTrigger("Dodge");
        }

        // Enable invincibility
        if (_playerHealth != null)
        {
            _playerHealth.SetInvincible(true);
        }
    }

    private void EndDodge()
    {
        _isDodging = false;

        // Disable invincibility
        if (_playerHealth != null)
        {
            Invoke(nameof(DisableInvincibility), invincibilityDuration);
        }

        // Start cooldown
        Invoke(nameof(ResetCooldown), dodgeCooldown);
    }

    private void DisableInvincibility()
    {
        if (_playerHealth != null)
        {
            _playerHealth.SetInvincible(false);
        }
    }

    private void ResetCooldown()
    {
        _canDodge = true;
    }

    public void OnDodge(InputValue value)
    {
        if (value.isPressed && _canDodge && !_isDodging)
        {
            StartDodge();
        }
    }
}
