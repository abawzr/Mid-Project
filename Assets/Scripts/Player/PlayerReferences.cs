using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerReferences : MonoBehaviour
{
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerHealth _playerHealth;
    private PlayerAttack _playerAttack;
    private PlayerDodge _playerDodge;

    public PlayerInput Input => _playerInput;
    public PlayerMovement Movement => _playerMovement;
    public PlayerHealth Health => _playerHealth;
    public PlayerAttack Attack => _playerAttack;
    public PlayerDodge Dodge => _playerDodge;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerHealth = GetComponent<PlayerHealth>();
        _playerAttack = GetComponent<PlayerAttack>();
        _playerDodge = GetComponent<PlayerDodge>();
    }

    public void SetReferencesEnabled(bool enabled)
    {
        _playerInput.enabled = enabled;
        _playerMovement.enabled = enabled;
        _playerHealth.enabled = enabled;
        _playerAttack.enabled = enabled;
        _playerDodge.enabled = enabled;
    }
}
