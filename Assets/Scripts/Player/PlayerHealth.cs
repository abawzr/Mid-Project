using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private float _currentHealth;

    public static event Action<float, float> OnHealthChanged; // (current, max)
    public static event Action OnPlayerDeath;

    public float CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke();

        // Disable player controls
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerAttack>().enabled = false;
        GetComponent<PlayerJump>().enabled = false;

        // Play death animation
        // GetComponent<Animator>().SetTrigger("Death");
    }

    public void TakeDamage(float damage)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0); // Clamp to 0

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
        _currentHealth = maxHealth;
        transform.position = spawnPosition;

        // Re-enable controls
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<PlayerAttack>().enabled = true;
        GetComponent<PlayerJump>().enabled = true;

        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }
}
