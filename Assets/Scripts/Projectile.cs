using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("Flight Settings")]
    [Tooltip("Initial speed (units/sec) at spawn")]
    public float speed = 10f;
    [Tooltip("Auto-destroy if it never hits anything")]
    public float maxLifetime = 5f;

    [Header("Hit FX")]
    [Tooltip("ParticleSystem prefab for hit effect")]
    public ParticleSystem hitEffectPrefab;

    private Transform target;
    private Rigidbody rb;
    private GameManager gameManager;
    private bool initialized;

    // previous position recorded each physics step (used for raycast)
    private Vector3 prevPosition;

    /// <summary>
    /// Call this immediately after Instantiate()
    /// </summary>
    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
        initialized = true;

        // compute direction and give it a one-time impulse
        Vector3 dir = (target.position - transform.position).normalized;
        rb.velocity = dir * speed;

        // align its forward to the velocity vector
        if (dir != Vector3.zero)
            rb.rotation = Quaternion.LookRotation(dir);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // projectiles shouldn’t rotate under physics torque
        rb.freezeRotation = true;
        // no gravity by default; remove if you want arcing
        rb.useGravity = false;

        // nice smoothing and less tunneling (optional)
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Always clean up
        Destroy(gameObject, maxLifetime);
    }

    void Start()
    {
        // grab your GameManager reference however you like
        var gmObj = GameObject.Find("GameManager");
        if (gmObj != null) gameManager = gmObj.GetComponent<GameManager>();
        // initialize prevPosition
        prevPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Optional homing tweak: uncomment if you want constant-force homing
        /*
        if (initialized && target != null)
        {
            Vector3 homingForce = (target.position - transform.position).normalized * speed * Time.fixedDeltaTime;
            rb.AddForce(homingForce, ForceMode.VelocityChange);
        }
        */

        // record position at end of physics step for next frame's raycast
        prevPosition = transform.position;
    }

    // NOTE: Using a trigger (set your collider's isTrigger = true in Inspector).
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // Determine an accurate hit point + normal:
        Vector3 hitPoint = transform.position;
        Vector3 hitNormal = -transform.forward; // fallback normal

        // Try a raycast from prevPosition to current position to get an accurate contact
        Vector3 travel = transform.position - prevPosition;
        float travelDist = travel.magnitude;
        if (travelDist > 0.001f)
        {
            RaycastHit hit;
            if (Physics.Raycast(prevPosition, travel.normalized, out hit, travelDist + 0.05f))
            {
                hitPoint = hit.point;
                hitNormal = hit.normal;
            }
            else
            {
                // Raycast failed (maybe overlapped). Use ClosestPoint fallback
                hitPoint = other.ClosestPoint(transform.position);
                Vector3 n = (transform.position - hitPoint);
                if (n.sqrMagnitude > 0.0001f) hitNormal = n.normalized;
            }
        }
        else
        {
            // Very small travel — just use ClosestPoint
            hitPoint = other.ClosestPoint(transform.position);
            Vector3 n = (transform.position - hitPoint);
            if (n.sqrMagnitude > 0.0001f) hitNormal = n.normalized;
        }

        // Spawn hit effect at calculated point + orientation
        if (hitEffectPrefab != null)
        {
            var fx = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        // Award points if it hit a Car
        if (other.gameObject.CompareTag("Car") && gameManager != null)
        {
            gameManager.points += gameManager.pointsPerProjectileHitPlayer;
        }

        // Destroy projectile
        Destroy(gameObject);
    }
}
