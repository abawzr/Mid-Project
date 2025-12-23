using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;

    private Animator _animator;
    private PlayerHealth _playerHealth;
    private PlayerDodge _playerDodge;
    private float _lastAttackTime;
    private bool _isAttacking = false;

    public static event Action<bool> OnPlayerAttack;

    public bool IsAttacking => _isAttacking;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerHealth = GetComponent<PlayerHealth>();
        _playerDodge = GetComponent<PlayerDodge>();
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;

        // Can't attack when dodging
        if (_playerDodge != null && _playerDodge.IsDodging)
        {
            return;
        }

        // Can't attack if dead
        if (_playerHealth != null && _playerHealth.CurrentHealth <= 0)
        {
            return;
        }

        // Can't attack if already attacking
        if (_isAttacking)
        {
            return;
        }

        // Can't attack if on cooldown
        if (Time.time - _lastAttackTime < attackCooldown)
        {
            return;
        }

        PerformAttack();
    }

    private void PerformAttack()
    {
        _animator.SetTrigger("Attack");
        _lastAttackTime = Time.time;
    }

    public void OnAttackAnimationStart()
    {
        _isAttacking = true;
        OnPlayerAttack?.Invoke(true);
    }

    public void OnAttackAnimationEnd()
    {
        _isAttacking = false;
        OnPlayerAttack?.Invoke(false);
    }
}
