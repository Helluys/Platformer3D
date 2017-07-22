using UnityEngine;

public class MovingPlatform : MonoBehaviour {
    public Vector3 Velocity { get; private set; }
    Vector3 previousPosition;

    private void Start () {
        previousPosition = transform.position;
    }

    private void FixedUpdate () {
        Velocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = transform.position;
    }
}
