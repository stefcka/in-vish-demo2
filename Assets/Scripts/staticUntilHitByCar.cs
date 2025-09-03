using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class staticUntilHitByCar : MonoBehaviour
{
    [Tooltip("Minimum speed car has to be to knock prop over")]
    public float minSpeed = 1f;
    public ParticleSystem rockDestroy;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided");
        GameObject other = collision.gameObject;
        float relSpeed = collision.relativeVelocity.magnitude;

        // Check for Prop or Enemy tags
        if ((other.CompareTag("Prop") || other.CompareTag("Enemy")) && relSpeed > minSpeed)
        {
            Rigidbody hitRb = other.GetComponent<Rigidbody>();
            if (hitRb)
            {
                hitRb.isKinematic = false;
                hitRb.interpolation = RigidbodyInterpolation.Interpolate;
                hitRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // Apply a small, natural impulse based on incoming velocity so it doesn't teleport
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                hitRb.AddForce(pushDir * relSpeed * 0.5f, ForceMode.Impulse);
            }
            else
            {
                return;
            }

            // also free any child rigidbodies (common in stacked props)
            for (int i = 0; i < other.transform.childCount; i++)
            {
                var child = other.transform.GetChild(i).gameObject;
                var childRb = child.GetComponent<Rigidbody>();
                if (childRb)
                {
                    childRb.isKinematic = false;
                    childRb.interpolation = RigidbodyInterpolation.Interpolate;
                    childRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    childRb.AddForce((child.transform.position - transform.position).normalized * relSpeed * 0.4f, ForceMode.Impulse);
                }
            }
        }
        else
        {
            Debug.Log("collision relative velocity: " + relSpeed);
            Debug.Log("collision object name: " + other.name);
        }

        if (other.CompareTag("RamToDestroy"))
        {
            if (relSpeed > minSpeed / 2f)
            {
                Destroy(other);
                ParticleSystem NewRockDestroyParticle = Instantiate(rockDestroy, other.transform.position, Quaternion.identity);
                NewRockDestroyParticle.transform.localScale = new Vector3(10, 10, 10);
                Destroy(NewRockDestroyParticle.gameObject, 10f);
            }
        }
    }
}
