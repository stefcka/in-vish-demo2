using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WheelRotator : MonoBehaviour
{
    [Tooltip("All wheel Transforms that need to spin.")]
    [SerializeField] private Transform[] wheels;

    [Tooltip("Wheel radius in Unity units (meters).")]
    [SerializeField] private float wheelRadius = 0.5f;

    private NavMeshAgent agent;
    private float wheelCircumference;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Precompute circumference for efficiency
        wheelCircumference = 2f * Mathf.PI * wheelRadius;
    }

    private void Update()
    {
        // How far we moved this frame
        float distanceThisFrame = agent.velocity.magnitude * Time.deltaTime;

        // Convert linear distance to rotational degrees
        float rotationDegrees = distanceThisFrame / wheelCircumference * 360f;

        // Spin each wheel around its local X-axis
        foreach (var wheel in wheels)
        {
            wheel.Rotate(-rotationDegrees, 0f, 0f, Space.Self);
        }
    }
}
