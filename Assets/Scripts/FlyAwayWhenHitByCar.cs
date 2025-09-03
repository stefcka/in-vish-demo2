using UnityEngine;

public class FlyAwayWhenHitByCar : MonoBehaviour
{
    [Tooltip("Minimum car speed required to trigger the effect")]
    public float minCarSpeedThreshold = 5f;

    [Tooltip("Base speed at which objects will be launched")]
    public float minLaunchSpeed = 5f;

    [Tooltip("Additional launch speed multiplier based on collision speed above the threshold")]
    public float speedPerUnitCollisionSpeed = 1f;

    [Tooltip("Launch angle in degrees (0 = parallel to ground, 90 = straight up)")]
    [Range(0f, 90f)] public float launchAngle = 30f;

    [Tooltip("If true, object only launches once")]
    public bool onlyFlyAwayOnFirstHit = true;

    public bool enemyTakesDamageOnHit = true;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null || (!collision.gameObject.CompareTag("Enemy") && !collision.gameObject.GetComponent<canFlyAwayOnHit>()))
            return;

        float collisionSpeed = collision.relativeVelocity.magnitude;
        if (collisionSpeed < minCarSpeedThreshold)
            return;

        var agent = collision.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        var walker = collision.gameObject.GetComponent<RandomNavMeshWalker>();
        if (walker) walker.enabled = false;

        var randomMover = collision.gameObject.GetComponent<RandomMovement>();
        if(randomMover) randomMover.enabled = false;

        rb.isKinematic = false;

        // Calculate launch velocity
        Vector3 forwardDir = transform.forward;
        Vector3 launchDir = Quaternion.AngleAxis(launchAngle, transform.right) * forwardDir;

        float extraSpeed = (collisionSpeed - minCarSpeedThreshold) * speedPerUnitCollisionSpeed;
        float launchSpeed = minLaunchSpeed + extraSpeed;

        if(enemyTakesDamageOnHit)
        {
            var health = collision.gameObject.GetComponent<health>();
            if (health) collision.gameObject.GetComponent<health>().reduceHealthAndDamageModel();
        }

        rb.velocity = launchDir.normalized * launchSpeed;

        if (onlyFlyAwayOnFirstHit)
            enabled = false;
    }
}
