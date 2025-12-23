using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPoint = Vector3.zero;

    private PlayerHealth playerHealth;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            PlayerHealth.OnPlayerDeath += HandleDeath;
        }
    }

    private void HandleDeath()
    {
        Invoke(nameof(Respawn), 2f);
    }

    private void Respawn()
    {
        if (playerHealth != null)
        {
            playerHealth.Respawn(spawnPoint);
        }
    }
}
