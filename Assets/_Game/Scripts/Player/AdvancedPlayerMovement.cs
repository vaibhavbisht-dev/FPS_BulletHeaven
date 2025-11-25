using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class AdvancedPlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerInputManager inputManager;
    public Transform orientation;

    [Header("Ground Movement")]
    public float groundSpeed = 12f;
    public float groundAccel = 100f;
    public float friction = 6f;

    [Header("Air Movement (Strafing)")]
    [Tooltip("Higher values allow faster turning in air")]
    public float airAccel = 200f;
    [Tooltip("Max speed you can accelerate to while in air (keep low for bunny hopping balance)")]
    public float airMaxSpeed = 2f;
    [Tooltip("Allows steering in the air without needing to perfect-strafe. Set to 0 for classic Quake difficulty.")]
    [Range(0f, 150f)]
    public float airControl = 25f; // CPMA style control

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float gravityMultiplier = 2.5f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool _isGrounded;
    [SerializeField] private float _currentSpeed;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Vector3 groundNormal = Vector3.up;

    // Unity 6 Physics Wrapper
    private Vector3 Velocity
    {
#if UNITY_6000_0_OR_NEWER
        get => rb.linearVelocity;
        set => rb.linearVelocity = value;
#else
        get => rb.velocity;
        set => rb.velocity = value;
#endif
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.useGravity = false;
        rb.freezeRotation = true;

        // Zero out friction material so we handle it in code
        if (col.material == null)
        {
            col.material = new PhysicsMaterial { dynamicFriction = 0f, staticFriction = 0f, frictionCombine = PhysicsMaterialCombine.Minimum };
        }
    }

    private void Update()
    {
        if (_isGrounded && inputManager.JumpTriggered)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (_isGrounded)
        {
            GroundMove();
        }
        else
        {
            AirMove();
        }

        ApplyGravity();

        // Debug
        _currentSpeed = new Vector3(Velocity.x, 0, Velocity.z).magnitude;
    }

    // --- MOVEMENT LOGIC ---

    private void GroundMove()
    {
        ApplyFriction();

        Vector3 wishDir = GetWishDirection();

        // If no input, stop calculating
        if (wishDir.sqrMagnitude == 0) return;

        Accelerate(wishDir, groundSpeed, groundAccel);
    }

    private void AirMove()
    {
        Vector3 wishDir = GetWishDirection();

        // 1. CPMA Air Control: Allows steering in the air if moving fast
        // Only applies if we are holding input and trying to move
        float speed = new Vector3(Velocity.x, 0, Velocity.z).magnitude;
        if (airControl > 0f && wishDir.sqrMagnitude > 0f)
        {
            ApplyAirControl(wishDir, speed);
        }

        // 2. Standard Air Acceleration (Strafing)
        // This is what allows you to gain speed by looking sideways + A/D
        Accelerate(wishDir, airMaxSpeed, airAccel);
    }

    private void ApplyAirControl(Vector3 targetDir, float currentSpeed)
    {
        // Only apply air control if we are moving primarily forward relative to input
        // This prevents losing speed when strafing perfectly
        if (Mathf.Abs(inputManager.MoveInput.y) < 0.001f || Mathf.Abs(currentSpeed) < 0.001f) return;

        float zSpeed = Velocity.y;
        Vector3 velFlat = Velocity;
        velFlat.y = 0;

        float speed = velFlat.magnitude;
        velFlat.Normalize();

        float dot = Vector3.Dot(velFlat, targetDir);
        float k = 32f; // Constant for CPMA feel
        k *= airControl * dot * dot * Time.fixedDeltaTime;

        // Change direction smoothly
        if (dot > 0)
        {
            Vector3 newVel = velFlat * speed + targetDir * k;
            newVel.Normalize();
            newVel *= speed; // Maintain speed, just change direction

            newVel.y = zSpeed; // Restore vertical
            Velocity = newVel;
        }
    }

    private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
    {
        float currentSpeedInWishDir = Vector3.Dot(Velocity, targetDir);
        float addSpeed = targetSpeed - currentSpeedInWishDir;

        if (addSpeed <= 0) return;

        float accelSpeed = accel * Time.fixedDeltaTime * targetSpeed;
        if (accelSpeed > addSpeed) accelSpeed = addSpeed;

        rb.AddForce(targetDir * accelSpeed, ForceMode.VelocityChange);
    }

    private void ApplyFriction()
    {
        if (inputManager.IsJumping) return; // No friction while queueing a jump

        Vector3 vel = Velocity;
        Vector3 velFlat = new Vector3(vel.x, 0, vel.z);
        float speed = velFlat.magnitude;
        float drop = 0f;

        if (speed > 0)
        {
            float control = (speed < groundSpeed) ? groundSpeed : speed;
            drop = control * friction * Time.fixedDeltaTime;
        }

        float newSpeed = speed - drop;
        if (newSpeed < 0) newSpeed = 0;

        if (speed > 0)
        {
            newSpeed /= speed;
            // Apply new speed to X/Z, keep Y untouched
            vel.x = velFlat.x * newSpeed;
            vel.z = velFlat.z * newSpeed;
            Velocity = vel;
        }
    }

    // --- HELPERS ---

    private Vector3 GetWishDirection()
    {
        Vector2 input = inputManager.MoveInput;
        if (input.sqrMagnitude == 0) return Vector3.zero;

        Vector3 dir = orientation.forward * input.y + orientation.right * input.x;

        // If on ground, project along slope. If in air, keep it flat.
        if (_isGrounded)
        {
            dir = Vector3.ProjectOnPlane(dir, groundNormal).normalized;
        }
        else
        {
            // Flatten air input so looking up doesn't slow you down
            dir = new Vector3(dir.x, 0, dir.z).normalized;
        }
        return dir;
    }

    private void Jump()
    {
        Vector3 vel = Velocity;
        vel.y = 0;
        Velocity = vel;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ApplyGravity()
    {
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * (col.height * 0.5f);
        float radius = col.radius * 0.9f;
        float dist = (col.height * 0.5f) - radius + groundCheckDistance;

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, dist, groundLayer))
        {
            _isGrounded = true;
            groundNormal = hit.normal;

            // Slope magnetism
            if (hit.distance > 0.05f && !inputManager.IsJumping)
            {
                rb.AddForce(Vector3.down * 10f, ForceMode.Force);
            }
        }
        else
        {
            _isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 origin = transform.position + Vector3.up * (col.height * 0.5f);
            float radius = col.radius * 0.9f;
            float dist = (col.height * 0.5f) - radius + groundCheckDistance;
            Gizmos.DrawWireSphere(origin + Vector3.down * dist, radius);
        }
    }
}
