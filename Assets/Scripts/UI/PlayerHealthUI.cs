using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private float smoothSpeed;

    private Slider _healthBar;
    private float _targetHealth;

    private void Awake()
    {
        _healthBar = GetComponent<Slider>();
        _healthBar.minValue = 0;
    }

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void Update()
    {
        if (Mathf.Abs(_healthBar.value - _targetHealth) > 0.01f)
            _healthBar.value = Mathf.Lerp(_healthBar.value, _targetHealth, smoothSpeed * Time.deltaTime);
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (_healthBar.maxValue != maxHealth) _healthBar.maxValue = maxHealth;

        _targetHealth = currentHealth;
    }
}
