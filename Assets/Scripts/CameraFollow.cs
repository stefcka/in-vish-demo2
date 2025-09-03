using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public bool followX = false;
    public bool followY = true;
    public bool followZ = false;

    private Vector3 offset;

    void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Maintain initial offset
        transform.position = target.position + offset;

        // Rotation follow
        Vector3 targetEuler = target.eulerAngles;
        Vector3 currentEuler = transform.eulerAngles;
        Vector3 newEuler = currentEuler;

        if (followX)
        {
            newEuler.x = targetEuler.x;
        }
        if (followY)
        {
            newEuler.y = targetEuler.y;
        }
        if (followZ)
        {
            newEuler.z = targetEuler.z;
        }

        transform.rotation = Quaternion.Euler(newEuler);
    }
}
