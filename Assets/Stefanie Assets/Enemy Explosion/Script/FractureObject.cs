using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif
using UnityEngine;

public class FractureObject : MonoBehaviour
{
    public GameObject explosion;
    public Vector3 explosionOffset;
    public float destroyDelay;
    public GameObject smoke;
    public int MaximumSmoke;
    public float minForce;
    public float maxForce;
    public float radius;

    public void Explode()
    {
        int smokeCounter = 0;
        if (explosion != null)
        {
            GameObject explosionFX = Instantiate(explosion, transform.position + explosionOffset, Quaternion.identity) as GameObject;
            Destroy(explosionFX, 5);
        }

        foreach (Transform t in transform)
        {
            var rb = t.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(Random.Range(minForce, maxForce), transform.position, radius);
            }
            if (smoke != null && smokeCounter < MaximumSmoke)
            {
                if (Random.Range(1, 4) == 1)
                {
                    GameObject smokeFX = Instantiate(smoke, t.transform) as GameObject;
                    smokeCounter++;
                    Destroy(smokeFX, 5);
                }
            }
            Destroy(t.gameObject, destroyDelay);
        }
    }
}