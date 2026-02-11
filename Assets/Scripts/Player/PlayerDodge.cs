using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDodge : MonoBehaviour
{
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDistance = 5f;
    [SerializeField] private float dodgeDuration = 0.5f;
    [SerializeField] private float dodgeCooldown = 1f;
    [SerializeField] private float invincibilityDuration = 0.5f;

    private CharacterController _characterController;
    private Animator _animator;
    private PlayerReferences _playerReferences;
    private bool _isDodging = false;
    private bool _canDodge = true;
    private float _dodgeTimer = 0f;
    private Vector3 _dodgeDirection;

    public bool IsDodging => _isDodging;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _playerReferences = GetComponent<PlayerReferences>();
    }

    private void Update()
    {
        if (_isDodging)
        {
            _characterController.Move(new Vector3(
                _dodgeDirection.x * dodgeDistance / dodgeDuration,
                0,
                _dodgeDirection.z * dodgeDistance / dodgeDuration
            ) * Time.deltaTime);

            _dodgeTimer += Time.deltaTime;

            if (_dodgeTimer >= dodgeDuration)
            {
                EndDodge();
            }
        }
    }

    private void StartDodge()
    {
        _isDodging = true;
        _canDodge = false;
        _dodgeTimer = 0f;

        // Get dodge direction (current movement direction or backward if not moving)
        if (_playerReferences.Movement != null)
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
        if (_playerReferences.Health != null)
        {
            _playerReferences.Health.SetInvincible(true);
        }
    }

    private void EndDodge()
    {
        _isDodging = false;

        // Disable invincibility
        if (_playerReferences.Health != null)
        {
            Invoke(nameof(DisableInvincibility), invincibilityDuration);
        }

        // Start cooldown
        Invoke(nameof(ResetCooldown), dodgeCooldown);
    }

    private void DisableInvincibility()
    {
        if (_playerReferences.Health != null)
        {
            _playerReferences.Health.SetInvincible(false);
        }
    }

    private void ResetCooldown()
    {
        _canDodge = true;
    }

    public void OnDodge(InputValue value)
    {
        if (!value.isPressed) return;
        if (!_canDodge) return;
        if (_isDodging) return;
        if (!_playerReferences.Movement.IsGrounded) return;
        if (_playerReferences.Attack.IsAttacking) return;

        StartDodge();
    }
}
