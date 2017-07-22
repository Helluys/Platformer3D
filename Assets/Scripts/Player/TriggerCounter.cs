using UnityEngine;

public class TriggerCounter : MonoBehaviour {

    public int HitCount { get; private set; }
    public bool Hit { get { return HitCount > 0; } }
    public GameObject ClosestHitObject {
        get {
            if (!Hit)
                return null;

            float minDistance = float.MaxValue;
            GameObject obj = null;
            foreach (Collider collider in Physics.OverlapBox (transform.position + box.center, box.size / 2f, transform.rotation, LayerMask.GetMask ("Terrain"))) {
                float distance = collider.ClosestPoint (transform.position + box.center).magnitude;
                if (distance < minDistance) {
                    minDistance = distance;
                    obj = collider.gameObject;
                }
            }

            return obj;
        }
    }

    BoxCollider box;

    private void Start () {
        box = GetComponent<BoxCollider> ();
    }

    private void OnTriggerEnter (Collider other) {
        HitCount++;
    }

    private void OnTriggerExit (Collider other) {
        HitCount--;
    }
}
