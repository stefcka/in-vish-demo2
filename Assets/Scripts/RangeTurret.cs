using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class RangeTurret : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Where projectiles will spawn from")]
    public Transform firePoint;
    [Tooltip("Projectile prefab, must have a Projectile script with Initialize(Transform)")]
    public GameObject projectilePrefab;
    [Tooltip("The target to look at and fire upon (raycast target)")]
    public Transform target;

    [Tooltip("If raycast hits a collider tagged “Car”, shoot toward this one")]
    public Transform shootTarget;

    [Header("Look Settings")]
    [Tooltip("Max distance at which turret will track and fire")]
    public float lookRange = 10f;
    [Tooltip("How fast to rotate towards the target")]
    public float rotationSpeed = 5f;

    [Header("Fire Settings")]
    [Tooltip("Shots per second (1 / interval)")]
    public float fireInterval = 1f;

    private Coroutine fireRoutine;

    private void Reset()
    {
        firePoint = transform;
    }

    private void Start()
    {
        if (projectilePrefab == null)
            Debug.LogError($"[{name}] No projectilePrefab assigned!", this);

        if (target == null)
            Debug.LogWarning($"[{name}] No target assigned — turret won't track or fire.", this);

        if (shootTarget == null)
            Debug.LogWarning($"[{name}] No shootTarget assigned — will use 'target' for firing.", this);

        fireRoutine = StartCoroutine(FireLoop());
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        float dist = dir.magnitude;

        // only rotate if in range and raycast hits a collider tagged "Car"
        if (dist <= lookRange &&
            Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, lookRange) &&
            hit.collider.CompareTag("Car"))
        {
            Quaternion desired = Quaternion.LookRotation(-dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator FireLoop()
    {
        WaitForSeconds delay = new WaitForSeconds(fireInterval);
        while (true)
        {
            yield return delay;
            TryFire();
        }
    }

    private void TryFire()
    {
        if (projectilePrefab == null || target == null) return;

        Vector3 dir = target.position - firePoint.position;
        float dist = dir.magnitude;
        if (dist > lookRange) return;

        // only fire if raycast hits a collider tagged "Car"
        if (!Physics.Raycast(firePoint.position, dir.normalized, out RaycastHit hit, lookRange)) return;
        if (!hit.collider.CompareTag("Car")) return;

        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        var proj = projGO.GetComponent<Projectile>();
        if (proj != null)
        {
            Transform aim = shootTarget != null ? shootTarget : target;
            proj.Initialize(aim);
        }
        else
        {
            Debug.LogWarning($"[{name}] Spawned prefab has no Projectile script.", this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Gizmos.DrawWireSphere(origin, lookRange);
    }
}
