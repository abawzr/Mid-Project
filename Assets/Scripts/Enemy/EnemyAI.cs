using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("States")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float patrolRadius = 10f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Patrol")]
    [SerializeField] private float patrolWaitTime = 2f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private Transform _playerTransform;
    private EnemyState _currentState = EnemyState.Patrol;
    private Vector3 _patrolPoint;
    private float _lastAttackTime;
    private float _patrolWaitTimer;
    private Vector3 _startPosition;

    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Vector3 center = Application.isPlaying ? _startPosition : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _startPosition = transform.position;
    }

    private void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetNewPatrolPoint();
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        switch (_currentState)
        {
            case EnemyState.Patrol:
                _agent.speed = patrolSpeed;
                Patrol();

                if (distanceToPlayer <= detectionRange)
                {
                    _currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                _agent.speed = chaseSpeed;
                Chase();

                if (distanceToPlayer <= attackRange)
                {
                    _currentState = EnemyState.Attack;
                }
                else if (distanceToPlayer > detectionRange * 1.5f)
                {
                    _currentState = EnemyState.Patrol;
                    SetNewPatrolPoint();
                }
                break;

            case EnemyState.Attack:
                _agent.speed = 0f;
                Attack();

                if (distanceToPlayer > attackRange)
                {
                    _currentState = EnemyState.Chase;
                }
                break;
        }

        UpdateAnimation();
    }

    private void Patrol()
    {
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _patrolWaitTimer += Time.deltaTime;

            if (_patrolWaitTimer >= patrolWaitTime)
            {
                SetNewPatrolPoint();
                _patrolWaitTimer = 0f;
            }
        }
        else
        {
            _agent.SetDestination(_patrolPoint);
        }
    }

    private void Chase()
    {
        _agent.SetDestination(_playerTransform.position);
    }

    private void Attack()
    {
        _agent.SetDestination(transform.position);

        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Time.time - _lastAttackTime >= attackCooldown)
        {
            PerformAttackAnimation();
            _lastAttackTime = Time.time;
        }
    }

    private void PerformAttackAnimation()
    {
        int randomAnimation = Random.Range(1, 4);
        _animator.SetTrigger($"Attack{1}");
    }

    private void SetNewPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 randomPoint = _startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            _patrolPoint = hit.position;
            _agent.SetDestination(_patrolPoint);
        }
    }

    private void UpdateAnimation()
    {
        float speed = _agent.velocity.magnitude / chaseSpeed;
        _animator.SetFloat("Speed", Mathf.Clamp01(speed));
    }

    public void DealDamageToPlayer()
    {
        PlayerHealth playerHealth = _playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
}
