//vish
//place on directional sun light
//makes sun move to simulate time changing and lighting changes

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeOfDay : MonoBehaviour
{
    public float timeSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Time.deltaTime * timeSpeed,0,0, Space.Self);
    }
}
