using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    //Destroy this object after a certain amount of time
    public float lifetime = 2.0f;

    private float timer;

    void Start()
    {
        timer = lifetime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
