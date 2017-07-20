using UnityEngine;

public class TriggerCounter : TriggerChildCounter {

    public Vector3 direction;
    
    public int DetectorHitCount { get { return child.HitCount; } }
    public bool DetectorHit { get { return child.Hit; } }

    TriggerChildCounter child;

    private void Start () {
        child = transform.GetComponentInChildren<TriggerChildCounter> ();
    }
}
