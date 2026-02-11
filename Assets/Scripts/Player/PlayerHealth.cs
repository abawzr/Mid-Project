using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private PlayerReferences _playerReferences;
    private Animator _animator;
    private float _currentHealth;
    private bool _isInvincible = false;

    public static event Action<float, float> OnHealthChanged; // (current, max)
    public static event Action OnPlayerDeath;

    public float CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _animator = GetComponent<Animator>();
        _playerReferences = GetComponent<PlayerReferences>();
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke();

        // Disable player controls
        _playerReferences.SetReferencesEnabled(false);

        // Play death animation
        _animator.SetTrigger("Die");
    }

    [ContextMenu("Take Hit 25")] private void TestDamage25() => TakeDamage(25f);
    public void TakeDamage(float damage)
    {
        if (_currentHealth <= 0) return;

        if (_isInvincible)
        {
            return;
        }

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (_currentHealth > 0)
            _animator.SetTrigger("Hit");

        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (_currentHealth <= 0) return;

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, maxHealth); // Clamp to max health

        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void Respawn(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;

        // Re-enable controls
        _playerReferences.SetReferencesEnabled(true);

        _currentHealth = maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void SetInvincible(bool invincible)
    {
        _isInvincible = invincible;
    }
}
