using UnityEngine;

public class TriggerParentCounter : TriggerCounter {

    public Vector3 direction;
    
    public int DetectorHitCount { get { return child.HitCount; } }
    public bool DetectorHit { get { return child.Hit; } }

    TriggerCounter child;

    private void Start () {
        child = transform.GetComponentInChildren<TriggerCounter> ();
    }
}
