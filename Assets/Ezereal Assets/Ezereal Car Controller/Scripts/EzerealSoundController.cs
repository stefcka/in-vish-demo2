using UnityEngine;

namespace Ezereal
{
    public class EzerealSoundController : MonoBehaviour // This system plays tire and engine sounds.
    {
        [Header("References")]
        [SerializeField] bool useSounds = false;
        [SerializeField] EzerealCarController ezerealCarController;
        [SerializeField] AudioSource tireAudio;
        [SerializeField] AudioSource engineAudio;

        [Header("Settings")]
        public float maxVolume = 0.5f; // Maximum volume for high speeds

        [Header("Debug")]
        [SerializeField] bool alreadyPlaying;

        void Start()
        {
            if (useSounds)
            {
                alreadyPlaying = false;

                if (ezerealCarController == null || ezerealCarController.vehicleRB == null || tireAudio == null || engineAudio == null)
                {
                    Debug.LogWarning("ezerealSoundController is missing some references. Ignore or attach them if you want to have sound controls.");


                }

                if (tireAudio != null)
                {
                    tireAudio.volume = 0f; // Start with zero volume
                    tireAudio.Stop();
                }
            }
        }

        public void TurnOnEngineSound()
        {
            if (useSounds)
            {
                if (engineAudio != null)
                {
                    engineAudio.Play();
                }
            }
        }

        public void TurnOffEngineSound()
        {
            if (useSounds)
            {
                if (engineAudio != null)
                {
                    engineAudio.Stop();
                }
            }
        }

        void Update()
        {
            if (!useSounds
                || ezerealCarController == null
                || ezerealCarController.vehicleRB == null
                || tireAudio == null
                || engineAudio == null)
                return;

            // 1) read speed (in kph)
            float speed = ezerealCarController.vehicleRB
                                  .velocity.magnitude * 3.6f;

            // 2) normalize 0→1 over your configured top speed
            float speedFactor = Mathf.InverseLerp(
                0f,
                ezerealCarController.maxForwardSpeed,
                speed
            );

            // —— ENGINE SOUND ——
            // only audible if you're actually “started” and moving a bit
            if (speed > 1f)
            {
                if (!engineAudio.isPlaying)
                    engineAudio.Play();

                // volume ramps 0→maxVolume
                engineAudio.volume = Mathf.Lerp(
                    0f,
                    maxVolume,
                    speedFactor
                );

                // pitch 0.8→2.0 (tweak to taste)
                engineAudio.pitch = Mathf.Lerp(
                    0.8f,
                    2.0f,
                    speedFactor
                );
            }
            else
            {
                engineAudio.volume = 0f;
                if (engineAudio.isPlaying)
                    engineAudio.Stop();
            }

            // —— TIRE SKID SOUND ——
            // determine if we should be skidding/drifting
            bool shouldSkid = false;
            foreach (var col in new[] {
        ezerealCarController.frontLeftWheelCollider,
        ezerealCarController.frontRightWheelCollider,
        ezerealCarController.rearLeftWheelCollider,
        ezerealCarController.rearRightWheelCollider
    })
            {
                if (col.GetGroundHit(out var hit)
                    && Mathf.Abs(hit.sidewaysSlip) > ezerealCarController.slipThreshold)
                {
                    shouldSkid = true;
                    break;
                }
            }

            if (shouldSkid && !alreadyPlaying)
            {
                tireAudio.Play();
                alreadyPlaying = true;
            }
            else if (!shouldSkid && alreadyPlaying)
            {
                tireAudio.Stop();
                alreadyPlaying = false;
            }

            if (alreadyPlaying)
            {
                // drive volume by how hard you're slipping (or speedFactor)
                // here we use speedFactor so you hear skids at any speed
                tireAudio.volume = Mathf.Lerp(
                    0f,
                    maxVolume,
                    speedFactor
                );

                // tire pitch: modest sweep so the squeal “rises” with speed
                tireAudio.pitch = Mathf.Lerp(
                    0.8f,
                    1.5f,
                    speedFactor
                );
            }
        }

    }
}
