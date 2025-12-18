using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float jumpPower;
    [SerializeField] private float groundSphereOffset;
    [SerializeField] private float groundSphereRadius;
    [SerializeField] private LayerMask excludePlayerLayerMask;

    private Rigidbody _rigidBody;
    private Animator _animator;
    private bool _isGrounded;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.up * groundSphereOffset, groundSphereRadius);
    }

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckGround();

        UpdateAnimation();
    }

    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(transform.position + transform.up * groundSphereOffset, groundSphereRadius, excludePlayerLayerMask);
    }

    private void UpdateAnimation()
    {
        bool isInAir = !_isGrounded || _rigidBody.linearVelocity.y > 0.1f;
        _animator.SetBool("IsInAir", isInAir);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && _isGrounded)
        {
            _rigidBody.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }
    }
}
