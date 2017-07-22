using UnityEngine;

public class PlayerController : MonoBehaviour {
    public bool Grounded { get { return feet.Hit; } }

    Vector3 velocity = new Vector3 ();
    public Vector3 gravity;
    public float linearDamping = 0.1f, inputFactor = 1f;
    public float jumpHeight = 4f;
    public float runSpeed = 10f;
    public float acceleration = 1f;
    public float airControl = 0.1f;
    public float bodyRepulsion = 100f;

    public CameraController cameraController;
    public KeyCode jumpKey, joystickJumpKey;

    TriggerCounter feet, front, back, left, right;
    Rigidbody rb;

    private void Start () {
        // Fetch all triggers
        feet = transform.Find ("Feet").GetComponent<TriggerCounter> ();
        front = transform.Find ("Front").GetComponent<TriggerCounter> ();
        back = transform.Find ("Back").GetComponent<TriggerCounter> ();
        left = transform.Find ("Left").GetComponent<TriggerCounter> ();
        right = transform.Find ("Right").GetComponent<TriggerCounter> ();

        rb = GetComponent<Rigidbody> ();
    }

    private void Update () {
        // Jump !
        if (Grounded && (Input.GetKeyDown (jumpKey) || Input.GetKeyDown (joystickJumpKey)))
            velocity.y = Mathf.Sqrt (-2f * gravity.y * jumpHeight);
    }

    private void FixedUpdate () {
        // Avoid stopping collision detection when not moving
        rb.WakeUp ();

        // Apply input to target velocity
        Vector3 targetVelocity = (inputFactor * runSpeed * (Input.GetAxis ("Vertical") * Vector3.forward + Input.GetAxis ("Horizontal") * Vector3.right));
        targetVelocity = Quaternion.AngleAxis (cameraController.HorizontalAngle, Vector3.up) * targetVelocity;

        GameObject closestGround = feet.ClosestHitObject;
        // Add moving platform's velocity when we leave it
        if (transform.parent != null && (closestGround == null || closestGround.transform != transform.parent)) {
            MovingPlatform platform = transform.parent.GetComponent<MovingPlatform> ();
            if (platform != null)
                velocity += platform.Velocity;
        }
        // Set parent to the platform we are on (for moving with mobile platforms)
        transform.parent = closestGround == null ? null : closestGround.transform;

        // Interpolate velocity towards target velocity on horizontal plane, vertical axis is untouched
        velocity = Vector3.Lerp (Vector3.ProjectOnPlane (velocity, transform.up), targetVelocity, Grounded ? acceleration : airControl * acceleration) + Vector3.Project (velocity, transform.up);

        // Apply gravity and linera damping
        if (!Grounded)
            velocity += gravity * Time.fixedDeltaTime;
        velocity -= linearDamping * velocity * Time.fixedDeltaTime;
        
        // Apply velocity to position, and rotate towards movement direction
        transform.position += velocity * Time.fixedDeltaTime;
        if (targetVelocity.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (targetVelocity), 0.1f);
    }

    private void OnCollisionExit (Collision collision) {
        ExtractBody (collision);
    }

    private void OnCollisionStay (Collision collision) {
        ExtractBody (collision);
    }

    /// <summary>
    /// Moves the body out of the given collision
    /// </summary>
    /// <param name="collision">The collision we want to get out of</param>
    private void ExtractBody (Collision collision) {
        Vector3 separation = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts) {
            Vector3 contactSeparation = -contact.separation * contact.normal;

            if (Mathf.Abs (contactSeparation.x) > Mathf.Abs (separation.x))
                separation.x = contactSeparation.x;

            if (Mathf.Abs (contactSeparation.y) > Mathf.Abs (separation.y))
                separation.y = contactSeparation.y;

            if (Mathf.Abs (contactSeparation.z) > Mathf.Abs (separation.z))
                separation.z = contactSeparation.z;
        }

        if (Vector3.Dot (velocity, separation) < 0f)
            velocity = Vector3.ProjectOnPlane (velocity, separation);
        transform.position += separation;
    }
}
