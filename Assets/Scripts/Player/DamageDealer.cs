using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private LayerMask targetLayers;

    private Collider _weaponCollider;
    private HashSet<Collider> _enemiesReceivedHit = new HashSet<Collider>();

    private void Awake()
    {
        _weaponCollider = GetComponent<Collider>();

        _weaponCollider.isTrigger = true;
        _weaponCollider.enabled = false;
    }

    private void OnEnable()
    {
        PlayerAttack.OnPlayerAttack += EnableHitDetection;
    }

    private void OnDisable()
    {
        PlayerAttack.OnPlayerAttack -= EnableHitDetection;
    }

    private void EnableHitDetection(bool isAttacking)
    {
        _weaponCollider.enabled = isAttacking;

        if (isAttacking)
        {
            _enemiesReceivedHit.Clear();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_enemiesReceivedHit.Contains(other)) return;

        if (((1 << other.gameObject.layer) & targetLayers) == 0)
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null && !enemyHealth.IsDead)
        {
            enemyHealth.TakeDamage(damage);

            _enemiesReceivedHit.Add(other);
        }
    }
}
