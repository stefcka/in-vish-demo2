using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    public float distance = 5f;           // Radius of the movement sphere
    public float speed = 3f;              // Movement speed
    public float smoothingAmount = 0.5f;  // Lerp smoothing factor
    public float stoppingDistance = 0.2f; // How close before picking a new target

    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        startPos = transform.position;
        PickNewTarget();
    }

    void Update()
    {
        // Move toward target
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothingAmount * Time.deltaTime * speed);

        // Check if close enough to pick a new target
        if (Vector3.Distance(transform.position, targetPos) <= stoppingDistance)
        {
            PickNewTarget();
        }
    }

    void PickNewTarget()
    {
        for (int i = 0; i < 20; i++) // Try up to 20 times to find a valid point
        {
            Vector3 randomOffset = Random.insideUnitSphere * distance;
            Vector3 candidate = startPos + randomOffset;

            // Raycast from above to check if point is inside a wall
            if (!Physics.CheckSphere(candidate, 0.5f))
            {
                // Optional: Raycast from current position to candidate to ensure no wall in between
                if (!Physics.Linecast(transform.position, candidate))
                {
                    targetPos = candidate;
                    return;
                }
            }
        }

        // If no valid point found, stay in place
        targetPos = transform.position;
    }
}
