// Tyler Birch
// 6/20/25
// Translate mouse clicks to shots
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseShooting : MonoBehaviour
{
    [Tooltip("By default you will shoot any time you click. To disable shooting set this as false.")]
    public bool canShoot = true; // having a check in ShotManager may be a better idea

    // Update is called once per frame
    void Update()
    {
        if (canShoot)
        {
            // Player 1
            if (Input.GetMouseButtonDown(0))
            {
                ShotManager.Instance.Shoot(1, (int)Input.mousePosition.x, (int)Input.mousePosition.y);
            }

            // Player 2
            if (Input.GetMouseButtonDown(1))
            {
                ShotManager.Instance.Shoot(2, (int)Input.mousePosition.x, (int)Input.mousePosition.y);
            }
        }  
    }
}
