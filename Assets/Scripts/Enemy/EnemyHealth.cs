using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 50f;

    [Header("Death")]
    [SerializeField] private float deathDelay = 2f;

    private float _currentHealth;
    private Animator _animator;
    private EnemyAI _enemyAI;
    private bool _isDead = false;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => _isDead;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _animator = GetComponent<Animator>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (_animator != null && _currentHealth > 0)
        {
            _animator.SetTrigger("Hit");
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;

        _enemyAI.enabled = false;

        _animator.SetTrigger("Die");


        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        Destroy(gameObject, deathDelay);
    }
}
