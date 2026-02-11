using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;

    private PlayerReferences _playerReferences;
    private Animator _animator;
    private float _lastAttackTime;
    private bool _isAttacking = false;

    public static event Action<bool> OnPlayerAttack;

    public bool IsAttacking => _isAttacking;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerReferences = GetComponent<PlayerReferences>();
    }

    private void PerformAttack()
    {
        _animator.SetTrigger("Attack");
        OnPlayerAttack?.Invoke(true);
        _isAttacking = true;
        _lastAttackTime = Time.time;
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;
        if (_playerReferences.Dodge.IsDodging) return;
        if (!_playerReferences.Movement.IsGrounded) return;
        if (_playerReferences.Health.CurrentHealth <= 0) return;
        if (_isAttacking) return;

        // Can't attack if on cooldown
        if (Time.time - _lastAttackTime < attackCooldown) return;

        PerformAttack();
    }

    public void OnAttackAnimationEnd()
    {
        _isAttacking = false;
        OnPlayerAttack?.Invoke(false);
    }
}
