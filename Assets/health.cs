using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class health : MonoBehaviour
{
    public int healthValue = 2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void reduceHealthAndDamageModel()
    {
        healthValue--;
        transform.GetChild(0).gameObject.SetActive(false); //normal model
        transform.GetChild(1).gameObject.SetActive(true); //damaged model
    }
}
