using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpHeight = 1.8f;      // metres
    [SerializeField] float fallMultiplier = 2.2f;
    [SerializeField] float coyoteTime = 0.1f, jumpBuffer = 0.12f;

    Rigidbody rb;
    InputAction moveAction, jumpAction;
    Vector3 move;                                  // from Update
    bool isGrounded, jumpQueued, rising;
    float lastGroundedTime = -1f, lastPressedTime = -1f;
    Vector3 groundPoint, groundNormal;             // last ground contact, for the gizmo

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.sleepThreshold = 0f;                    // a sleeping body sends no OnCollisionStay
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        move = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

        // coyote time and jump buffer: two timestamps
        if (isGrounded && !jumpQueued && !rising)
            lastGroundedTime = Time.time;
        if (jumpAction.WasPressedThisFrame())
            lastPressedTime = Time.time;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime <= jumpBuffer;
        if (canJump && wantJump)
        {
            jumpQueued = true;
            lastGroundedTime = lastPressedTime = -1f;  // used up
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

        if (jumpQueued)
        {
            // zero any fall first, so a coyote jump is full height
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
            jumpQueued = false;
            rising = true;
        }
        else if (rb.linearVelocity.y <= 0f)
            rising = false;

        if (rb.linearVelocity.y < 0f)              // falling: pull harder
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);

        isGrounded = false;                        // this step's contacts set it again
    }

    void OnCollisionEnter(Collision c) => OnCollisionStay(c);

    void OnCollisionStay(Collision c)
    {
        for (int i = 0; i < c.contactCount; i++)
        {
            ContactPoint p = c.GetContact(i);
            if (Vector3.Dot(p.normal, Vector3.up) > 0.5f)   // floor, not wall
            {
                isGrounded = true;
                groundPoint = p.point;
                groundNormal = p.normal;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(groundPoint, groundNormal);
    }
}
