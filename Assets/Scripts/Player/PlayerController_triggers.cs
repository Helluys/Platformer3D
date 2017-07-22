using System.Collections.Generic;
using UnityEngine;

public class PlayerController_triggers : MonoBehaviour {
    public bool Grounded { get { return triggers["Feet"].DetectorHit; } }

    Dictionary<string, TriggerParentCounter> triggers = new Dictionary<string, TriggerParentCounter> ();

    Vector3 velocity = new Vector3 ();
    public Vector3 gravity; 
    public float linearDamping = 0.1f, inputFactor = 1f;
    public float jumpHeight = 4f;
    public float runSpeed = 10f;
    public float acceleration = 1f;
    public float airControl = 0.1f;
    public float bodyRepulsion = 100f;

    private void Start () {
        foreach (Transform child in transform) {
            TriggerParentCounter trigger = child.GetComponent<TriggerParentCounter> ();
            if (trigger != null)
                triggers.Add (child.name, trigger);
        }
    }

    private void Update () {
    }

    private void FixedUpdate () {
        Vector3 localVelocity = transform.InverseTransformVector (velocity);

        Vector3 input = inputFactor * runSpeed * (Input.GetAxis ("Vertical") * Vector3.forward + Input.GetAxis ("Horizontal") * Vector3.right);

        velocity = Vector3.Lerp (Vector3.ProjectOnPlane (localVelocity, transform.up), input * Time.deltaTime, Grounded ? acceleration : airControl * acceleration) + Vector3.Project (velocity, transform.up);

        if(!Grounded)
            velocity += gravity * Time.fixedDeltaTime;
        velocity -= linearDamping * Vector3.ProjectOnPlane (velocity, Vector3.up) * Time.fixedDeltaTime;

        if (Grounded && Input.GetAxis ("Jump") > 0.9f)
            velocity += Mathf.Sqrt (-2f * gravity.y * jumpHeight) * Vector3.up;

        foreach (TriggerParentCounter trigger in triggers.Values) {
            if (trigger.DetectorHit && Vector3.Dot (velocity, transform.TransformVector(trigger.direction)) > 0f)
                velocity -= Vector3.Project (velocity, transform.TransformVector(trigger.direction));

            if (trigger.Hit)
                transform.position -= bodyRepulsion * trigger.direction;
        }

        transform.position += velocity * Time.fixedDeltaTime;
        if (input.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation (input), 0.1f);
    }
}
