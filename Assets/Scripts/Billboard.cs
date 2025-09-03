using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);


            transform.rotation = targetRotation;
        }
    }
}