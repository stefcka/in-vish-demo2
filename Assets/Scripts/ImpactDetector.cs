using UnityEngine;

namespace Ezereal
{
    [RequireComponent(typeof(Rigidbody))]
    public class ImpactDetector : MonoBehaviour
    {
        [SerializeField] private EzerealCarController carController;

        private void Awake()
        {
            if (carController == null)
                carController = GetComponentInChildren<EzerealCarController>();
            if (carController == null)
                Debug.LogError("ImpactDetector needs a reference to EzerealCarController!");
        }

        private void OnCollisionEnter(Collision collision)
        {
            carController.HandleImpact(collision);
        }
    }
}
