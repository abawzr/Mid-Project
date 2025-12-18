using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator _animator;

    public static event Action<bool> OnPlayerAttack;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            _animator.SetTrigger("Attack");
        }
    }

    public void OnAttackAnimationStart()
    {
        OnPlayerAttack?.Invoke(true);
    }

    public void OnAttackAnimationEnd()
    {
        OnPlayerAttack?.Invoke(false);
    }
}
