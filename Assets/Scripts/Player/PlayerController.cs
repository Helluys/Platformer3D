using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public bool Grounded { get { return feet.Hit; } }
    public bool Walled {
        get {
            foreach (TriggerCounter trigger in triggers.Values)
                if (trigger != feet && trigger.Hit)
                    return true;
            return false;
        }
    }

    Vector3 velocity = new Vector3 ();
    public Vector3 gravity;
    public float linearDamping = 0.1f, inputFactor = 1f;
    public float jumpHeight = 4f;
    public float runSpeed = 10f;
    public float acceleration = 1f;
    public float airControl = 0.1f;
    public float bodyRepulsion = 100f;
    public float rotationSpeed = 0.1f;
    public float walledFallingSpeed = 5f;

    public CameraController cameraController;
    public KeyCode jumpKey, joystickJumpKey;

    Dictionary<string, TriggerCounter> triggers = new Dictionary<string, TriggerCounter> ();
    TriggerCounter feet;
    Rigidbody rb;

    private float lastJumpTime;
    private const float jumpDelay = 0.3f;

    private void Start () {
        // Fetch all triggers
        foreach (Transform child in transform) {
            TriggerCounter trigger = child.GetComponent<TriggerCounter> ();
            if (trigger) {
                triggers.Add (child.name, trigger);
                if (child.name == "Feet")
                    feet = trigger;
            }
        }

        rb = GetComponent<Rigidbody> ();
    }

    private void Update () {
        // Jumping
        if (Input.GetKeyDown (jumpKey) || Input.GetKeyDown (joystickJumpKey) && CanJump ()) {
            // Determine jump direction based on walls and ground contacts
            bool jump = false;
            Vector3 jumpDirection = Vector3.up;
            if (Grounded) {
                jump = true;
                jumpDirection = Vector3.up;
            } else {
                foreach (TriggerCounter trigger in triggers.Values) {
                    if (trigger.Hit) {
                        jump = true;
                        jumpDirection -= trigger.Direction;
                    }
                }
            }

            if (jump) { // Apply jump formula based on jump height, gravity and jump direction
                velocity += Mathf.Sqrt (-2f * gravity.y * jumpHeight) * jumpDirection.normalized * (Vector3.ProjectOnPlane (jumpDirection, Vector3.up).magnitude > 0.1f ? 1.5f : 1f) - velocity.y * Vector3.up;
                lastJumpTime = Time.time;
            }
        }
    }

    private void FixedUpdate () {
        // Avoid stopping collision detection when not moving
        rb.WakeUp ();

        // Apply input to target velocity
        Vector3 targetVelocity = runSpeed * (Input.GetAxis ("Vertical") * Vector3.forward + Input.GetAxis ("Horizontal") * Vector3.right);
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
        velocity += Vector3.ProjectOnPlane (targetVelocity - velocity, Vector3.up) * (Grounded ? acceleration : airControl * acceleration) * Time.fixedDeltaTime;

        // Apply gravity and linear damping
        if (!Grounded) {
            if (Walled && CanJump ())
                velocity.y = Mathf.Clamp (velocity.y + 0.5f * gravity.y, -walledFallingSpeed, walledFallingSpeed);
            else
                velocity += gravity * Time.fixedDeltaTime;
        }
        velocity -= linearDamping * velocity * Time.fixedDeltaTime;

        // Apply velocity to position, and rotate towards movement direction
        transform.position += velocity * Time.fixedDeltaTime;
        if (targetVelocity.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (targetVelocity), rotationSpeed);
    }

    private bool CanJump () {
        return lastJumpTime < Time.fixedTime - jumpDelay;
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
