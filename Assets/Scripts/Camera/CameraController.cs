using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform target;
    public float sensitivity = 1f;
    public Vector3 offset;
    public float HorizontalAngle {
        get {
            Vector3 projected = Vector3.ProjectOnPlane (-offset, Vector3.up);
            return Mathf.Sign(projected.x) * Vector3.Angle (Vector3.forward, projected);
        }
    }

    public KeyCode recenterKey, joystickRecenterKey;

    Quaternion rotationOffset;

    void Start () {
        rotationOffset = Quaternion.FromToRotation (-offset, transform.forward);
    }

    // Update is called once per frame
    void Update () {
        Quaternion rotation = Quaternion.AngleAxis (Input.GetAxis ("CameraHorizontal") * sensitivity, Vector3.up)
                            * Quaternion.AngleAxis (Input.GetAxis ("CameraVertical")   * sensitivity, transform.right);

        offset = rotation * offset;
        float verticalAngle = Vector3.Angle (offset, Vector3.up);
        if (verticalAngle < 30f)
            offset = Quaternion.AngleAxis (verticalAngle - 30f, transform.right) * offset;
        else if (verticalAngle > 80f)
            offset = Quaternion.AngleAxis (verticalAngle - 80f, transform.right) * offset;

        if (Input.GetKey (recenterKey) || Input.GetKey (joystickRecenterKey))
            offset = Quaternion.AngleAxis(-HorizontalAngle + target.eulerAngles.y, Vector3.up) * offset;

        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation (-offset) * rotationOffset;
    }
}
