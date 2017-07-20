using UnityEngine;

public class PlayerController : MonoBehaviour {
    public bool Grounded { get { return feet.Hit; } }
    TriggerChildCounter feet;

    Vector3 velocity = new Vector3 ();
    public Vector3 gravity;
    public float linearDamping = 0.1f, inputFactor = 1f;
    public float jumpHeight = 4f;
    public float runSpeed = 10f;
    public float acceleration = 1f;
    public float airControl = 0.1f;
    public float bodyRepulsion = 100f;

    public CameraController cameraController;
    Rigidbody rb;

    private void Start () {
        feet = transform.GetComponentInChildren<TriggerChildCounter> ();
        rb = GetComponent<Rigidbody> ();
    }

    private void FixedUpdate () {
        rb.WakeUp (); // Avoid stopping collision detection when not moving

        Vector3 targetVelocity = Quaternion.AngleAxis (cameraController.HorizontalAngle, Vector3.up) * (inputFactor * runSpeed * (Input.GetAxis ("Vertical") * Vector3.forward + Input.GetAxis ("Horizontal") * Vector3.right));

        velocity = Vector3.Lerp (Vector3.ProjectOnPlane (velocity, transform.up), targetVelocity, Grounded ? acceleration : airControl * acceleration) + Vector3.Project (velocity, transform.up);

        GameObject closestGround = feet.ClosestHitObject;
        transform.parent = closestGround == null ? null : closestGround.transform;

        if (!Grounded)
            velocity += gravity * Time.fixedDeltaTime;
        velocity -= linearDamping * Vector3.ProjectOnPlane (velocity, Vector3.up) * Time.fixedDeltaTime;

        if (Grounded && Input.GetAxis ("Jump") > 0.9f)
            velocity.y = Mathf.Sqrt (-2f * gravity.y * jumpHeight);

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
