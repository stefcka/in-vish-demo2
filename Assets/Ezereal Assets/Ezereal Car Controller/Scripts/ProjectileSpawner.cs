using System.Collections;
using UnityEngine;

public class projectileSpawner : MonoBehaviour
{
    public GameObject target;             // assign the target here
    public float lookRange = 10f;
    public float rotationSpeed = 5f;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireInterval = 1f;

    float fireTimer = 0f;
    bool isActive = false;                // delay flag

    IEnumerator Start()
    {
        // wait 1 second before enabling the turret
        yield return new WaitForSeconds(3.25f);
        isActive = true;
    }

    void Update()
    {
        if (!isActive || target == null) return;

        fireTimer -= Time.deltaTime;

        // direction *toward* the target
        Vector3 dir = (target.transform.position - transform.position);
        float dist = dir.magnitude;

        Ray ray = new Ray(transform.position, dir.normalized);
        RaycastHit hit;

        if (dist <= lookRange &&
            Physics.Raycast(ray, out hit, lookRange) &&
            hit.transform.gameObject == target)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );

            if (fireTimer <= 0f && projectilePrefab != null && firePoint != null)
            {
                Instantiate(projectilePrefab,
                            firePoint.position,
                            firePoint.rotation);
                fireTimer = fireInterval;
            }
        }
    }
}
