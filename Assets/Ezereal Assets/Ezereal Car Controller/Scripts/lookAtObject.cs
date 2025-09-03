//if player target is in range, make this object look at target smoothly

using UnityEngine;

public class lookAtObject : MonoBehaviour
{
    public GameObject target; // assign the parent object here
    public float lookRange = 10f;
    public float rotationSpeed = 5f;

    void Update()
    {
        if (target == null) return;

        Vector3 direction = target.transform.position - transform.position;
        float distanceToTarget = direction.magnitude;

        Ray ray = new Ray(transform.position, direction.normalized);
        RaycastHit hit;

        if (distanceToTarget <= lookRange && Physics.Raycast(ray, out hit, lookRange))
        {

            Quaternion targetRotation = Quaternion.LookRotation(-direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        }
    }
}
