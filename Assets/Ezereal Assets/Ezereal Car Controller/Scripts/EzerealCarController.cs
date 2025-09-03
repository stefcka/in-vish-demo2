using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

namespace Ezereal
{

    public class EzerealCarController : MonoBehaviour // This is the main system responsible for car control.
    {
        // --- Roll Stability Mod ---
        [Header("Roll Stability")]
        [SerializeField] private float rollStability = 5f;
        [SerializeField] private float rollDamping = 2f;
        // ---------------------------

        [Header("Smooth Impact")]
        [SerializeField] private float impactRetention = 0.80f;      // fraction of pre-impact velocity to keep (0..1)
        [SerializeField] private float impactLerpDuration = 0.25f;  // seconds to blend to retained velocity
        [SerializeField] private float minImpactSpeedForSmoothing = 1.5f; // only apply smoothing if collision speed > this
        [SerializeField] private bool useTempDrag = true;
        [SerializeField] private float tempDrag = 1.5f;
        [SerializeField] private float tempDragDuration = 0.25f;

        [Header("Ezereal References")]
        [SerializeField] EzerealLightController ezerealLightController;
        [SerializeField] EzerealSoundController ezerealSoundController;
        [SerializeField] EzerealWheelFrictionController ezerealWheelFrictionController;

        [Header("References")]
        public Rigidbody vehicleRB;
        public WheelCollider frontLeftWheelCollider;
        public WheelCollider frontRightWheelCollider;
        public WheelCollider rearLeftWheelCollider;
        public WheelCollider rearRightWheelCollider;
        WheelCollider[] wheels;

        [SerializeField] Transform frontLeftWheelMesh;
        [SerializeField] Transform frontRightWheelMesh;
        [SerializeField] Transform rearLeftWheelMesh;
        [SerializeField] Transform rearRightWheelMesh;

        [Header("Tire Smoke")]
        [SerializeField] private ParticleSystem frontLeftSmoke;
        [SerializeField] private ParticleSystem frontRightSmoke;
        [SerializeField] private ParticleSystem rearLeftSmoke;
        [SerializeField] private ParticleSystem rearRightSmoke;
        // Slip threshold to start emitting smoke:
        [SerializeField] public float slipThreshold = 0.5f;

        [Header("Engine Sound")]
        [SerializeField] private AudioSource engineAudio;        // your looped engine clip
        [SerializeField] private float minEnginePitch = 0.8f;    // at zero speed
        [SerializeField] private float maxEnginePitch = 2.0f;    // at top speed
        [SerializeField] private float minEngineVolume = 0.2f;   // when stopped
        [SerializeField] private float maxEngineVolume = 1.0f;   // at top speed

        [Header("Collision Thud")]
        [SerializeField] private AudioSource impactSource;   // non-looping AudioSource
        [SerializeField] private AudioClip thudClip;         // your thud sound
        [SerializeField] private float minThudSpeed = 2f;    // below this no sound
        [SerializeField] private float maxThudSpeed = 10f;   // above this full volume

        [Header("Collision Effects")]
        [SerializeField] private ParticleSystem impactEffectPrefab;

        [Header("Car Flipping")]
        [SerializeField] private KeyCode flipCarKey;
        [SerializeField] private float minFlipCarStationarySpeed = 2;
        private bool flipCar = false;

        [SerializeField] Transform steeringWheel;

        [SerializeField] TMP_Text currentGearTMP_UI;
        [SerializeField] TMP_Text currentGearTMP_Dashboard;

        [SerializeField] TMP_Text currentSpeedTMP_UI;
        [SerializeField] TMP_Text currentSpeedTMP_Dashboard;
        [SerializeField] Slider accelerationSlider;

        [Header("Settings")]
        public bool isStarted = true;
        public float maxForwardSpeed = 100f;
        public float maxReverseSpeed = 30f;
        public float horsePower = 1000f;
        public float brakePower = 2000f;    // unused in auto
        public float handbrakeForce = 3000f;
        public float maxSteerAngle = 30f;
        public float steeringSpeed = 5f;
        public float stopThreshold = 1f;
        public float decelerationSpeed = 0.5f;
        public float maxSteeringWheelRotation = 360f;

        [Header("Drive Type")]
        public DriveTypes driveType = DriveTypes.RWD;

        [Header("Gearbox")]
        public AutomaticGears currentGear = AutomaticGears.Drive;

        [Header("Transmission")]
        [SerializeField] private bool automaticTransmission = false;

        [Header("Debug Info")]
        public bool stationary = true;
        [SerializeField] float currentSpeed = 0f;
        [SerializeField] float currentAccelerationValue = 0f;
        [SerializeField] float currentBrakeValue = 0f;
        [SerializeField] float currentHandbrakeValue = 0f;
        [SerializeField] float currentSteerAngle = 0f;
        [SerializeField] float targetSteerAngle = 0f;
        [SerializeField] float FrontLeftWheelRPM = 0f;
        [SerializeField] float FrontRightWheelRPM = 0f;
        [SerializeField] float RearLeftWheelRPM = 0f;
        [SerializeField] float RearRightWheelRPM = 0f;
        [SerializeField] float speedFactor = 0f;

        // Raw inputs to know what you’re still holding each frame:
        private float rawAccelInput = 0f;
        private float rawBrakeInput = 0f;

        // impact smoothing state
        private Vector3 lastFixedVelocity;
        private Coroutine impactCoroutine;
        private float originalDrag;

        private float preImpactSpeed = 0f;
        private bool isImpactRecovering = false;


        private void Awake()
        {
            wheels = new[]
            {
                frontLeftWheelCollider,
                frontRightWheelCollider,
                rearLeftWheelCollider,
                rearRightWheelCollider,
            };

            if (ezerealLightController == null) Debug.LogWarning("EzerealLightController missing.");
            if (ezerealSoundController == null) Debug.LogWarning("EzerealSoundController missing.");
            if (ezerealWheelFrictionController == null) Debug.LogWarning("EzerealWheelFrictionController missing.");
            if (vehicleRB == null) Debug.LogError("VehicleRB missing!");

            if (isStarted)
            {
                Debug.Log("Car is started.");
                ezerealLightController?.MiscLightsOn();
                ezerealSoundController?.TurnOnEngineSound();
            }
        }

        private void Start()
        {
            if (vehicleRB != null)
            {
                // Lower center of mass for less roll (tweak to taste)
                Vector3 com = vehicleRB.centerOfMass;
                com.y -= 0.5f;
                vehicleRB.centerOfMass = com;

                // make motion smoother visually
                vehicleRB.interpolation = RigidbodyInterpolation.Interpolate;
                vehicleRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                originalDrag = vehicleRB.drag;
            }
        }


        /// <summary>
        /// Called by child CollisionForwarder scripts whenever *any* part collides.
        /// </summary>
        public void HandleImpact(Collision collision)
        {   
            // 1) spawn particle at first contact
            if (impactEffectPrefab != null && collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                // orient effect so “up” is aligned with surface normal
                Quaternion rot = Quaternion.LookRotation(contact.normal);
                // instantiate and auto-play
                ParticleSystem ps = Instantiate(
                    impactEffectPrefab,
                    contact.point,
                    rot
                );
                // destroy after it’s done
                Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }

            // 2) play thud sound
            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed >= minThudSpeed && impactSource != null && thudClip != null)
            {
                float volume = Mathf.InverseLerp(minThudSpeed, maxThudSpeed, impactSpeed);
                impactSource.PlayOneShot(thudClip, volume);
            }

            // --- NEW: smooth car reaction ---
            if (vehicleRB == null) return;

            if (impactSpeed < minImpactSpeedForSmoothing)
                return; // small bumps we ignore

            // record pre-impact scalar speed
            preImpactSpeed = lastFixedVelocity.magnitude;
            isImpactRecovering = true;

            // compute target retained velocity (keep some of pre-impact momentum)
            Vector3 preVel = lastFixedVelocity;
            Vector3 desiredVel = preVel * impactRetention;

            // start smoothing coroutine (it will clamp to preImpactSpeed)
            if (impactCoroutine != null) StopCoroutine(impactCoroutine);
            impactCoroutine = StartCoroutine(SmoothRestoreVelocity(desiredVel, impactLerpDuration));


            if (useTempDrag)
            {
                StartCoroutine(TemporarilyIncreaseDrag(tempDrag, tempDragDuration));
            }
        }

        void HandleTireSmoke()
        {
            var wheelData = new (WheelCollider col, ParticleSystem ps)[]
            {
                (frontLeftWheelCollider, frontLeftSmoke),
                (frontRightWheelCollider, frontRightSmoke),
                (rearLeftWheelCollider, rearLeftSmoke),
                (rearRightWheelCollider, rearRightSmoke)
            };

            foreach (var (col, ps) in wheelData)
            {
                if (ps == null) continue;

                var emission = ps.emission;              // grab the module
                if (col.GetGroundHit(out WheelHit hit))
                {
                    float slip = Mathf.Abs(hit.sidewaysSlip);
                    if (slip > slipThreshold)
                    {
                        Debug.Log("creating smoke");
                        // scale 0→50 particles/sec as slip goes from threshold→1
                        float rate = Mathf.Lerp(0f, 75f, (slip - slipThreshold) / (1f - slipThreshold));
                        emission.rateOverTime = new ParticleSystem.MinMaxCurve(rate);
                    }
                    else
                    {
                        emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f);
                    }
                }
                else
                {
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f);
                }
            }
        }

        void OnStartCar()
        {
            isStarted = !isStarted;
            if (isStarted)
            {
                Debug.Log("Car started.");
                ezerealLightController?.MiscLightsOn();
                ezerealSoundController?.TurnOnEngineSound();
            }
            else
            {
                Debug.Log("Car turned off");
                ezerealLightController?.AllLightsOff();
                ezerealSoundController?.TurnOffEngineSound();
                frontLeftWheelCollider.motorTorque = 0;
                frontRightWheelCollider.motorTorque = 0;
                rearLeftWheelCollider.motorTorque = 0;
                rearRightWheelCollider.motorTorque = 0;
            }
        }

        void OnAccelerate(InputValue accelValue)
        {
            rawAccelInput = accelValue.Get<float>();

            if (automaticTransmission)
            {
                if (rawAccelInput > 0f)
                {
                    // Reverse→Drive brake-to-stop logic
                    if (currentGear == AutomaticGears.Reverse)
                    {
                        if (Mathf.Abs(currentSpeed) > stopThreshold)
                        {
                            currentBrakeValue = rawAccelInput;
                            currentAccelerationValue = 0f;
                            ezerealLightController?.BrakeLightsOn();
                        }
                        else
                        {
                            currentBrakeValue = 0f;
                            ezerealLightController?.BrakeLightsOff();
                            currentGear = AutomaticGears.Drive;
                            UpdateGearText("D");
                            currentAccelerationValue = rawAccelInput;
                        }
                    }
                    else
                    {
                        // Already in Drive or Neutral
                        if (currentGear != AutomaticGears.Drive)
                        {
                            currentGear = AutomaticGears.Drive;
                            UpdateGearText("D");
                            ezerealLightController?.ReverseLightsOff();
                        }
                        currentBrakeValue = 0f;
                        currentAccelerationValue = rawAccelInput;
                    }
                }
                else
                {
                    currentAccelerationValue = 0f;
                    currentBrakeValue = 0f;
                    ezerealLightController?.BrakeLightsOff();
                }
            }
            else
            {
                // Manual mode
                currentAccelerationValue = rawAccelInput;
            }
        }

        void OnBrake(InputValue brakeValue)
        {
            rawBrakeInput = brakeValue.Get<float>();

            if (automaticTransmission)
            {
                if (rawBrakeInput > 0f)
                {
                    // Drive→Reverse brake-to-stop logic
                    if (currentGear == AutomaticGears.Drive)
                    {
                        if (Mathf.Abs(currentSpeed) > stopThreshold)
                        {
                            currentBrakeValue = rawBrakeInput;
                            currentAccelerationValue = 0f;
                            ezerealLightController?.BrakeLightsOn();
                        }
                        else
                        {
                            currentBrakeValue = 0f;
                            ezerealLightController?.BrakeLightsOff();
                            currentGear = AutomaticGears.Reverse;
                            UpdateGearText("R");
                            ezerealLightController?.ReverseLightsOn();
                            currentAccelerationValue = rawBrakeInput;
                        }
                    }
                    else
                    {
                        // Already in Reverse or Neutral
                        if (currentGear != AutomaticGears.Reverse)
                        {
                            currentGear = AutomaticGears.Reverse;
                            UpdateGearText("R");
                            ezerealLightController?.ReverseLightsOn();
                        }
                        currentBrakeValue = 0f;
                        currentAccelerationValue = rawBrakeInput;
                    }
                }
                else
                {
                    currentAccelerationValue = 0f;
                    currentBrakeValue = 0f;
                    ezerealLightController?.BrakeLightsOff();
                }
            }
            else
            {
                // Manual mode
                currentBrakeValue = rawBrakeInput;
                if (isStarted && ezerealLightController != null)
                {
                    if (currentBrakeValue > 0f) ezerealLightController.BrakeLightsOn();
                    else ezerealLightController.BrakeLightsOff();
                }
            }
        }

        void Acceleration()
        {
            if (!isStarted) return;

            if (currentGear == AutomaticGears.Drive)
            {
                speedFactor = Mathf.InverseLerp(0, maxForwardSpeed, currentSpeed);
                float torque = Mathf.Lerp(horsePower, 0, speedFactor);

                if (currentAccelerationValue > 0f && currentSpeed < maxForwardSpeed)
                {
                    ApplyDriveTorque(torque * currentAccelerationValue);
                }
                else
                {
                    ClearDriveTorque();
                }
            }
            else if (currentGear == AutomaticGears.Reverse)
            {
                if (currentAccelerationValue > 0f && currentSpeed > -maxReverseSpeed)
                {
                    ApplyReverseTorque(horsePower * currentAccelerationValue);
                }
                else
                {
                    ClearDriveTorque();
                }
            }

            UpdateAccelerationSlider();
        }

        void Braking()
        {
            // Only used in manual mode
            frontLeftWheelCollider.brakeTorque = currentBrakeValue * brakePower;
            frontRightWheelCollider.brakeTorque = currentBrakeValue * brakePower;
        }

        void OnHandbrake(InputValue handbrakeValue)
        {
            currentHandbrakeValue = handbrakeValue.Get<float>();
            if (!isStarted) return;

            if (currentHandbrakeValue > 0)
            {
                ezerealWheelFrictionController?.StartDrifting(currentHandbrakeValue);
                ezerealLightController?.HandbrakeLightOn();
            }
            else
            {
                ezerealWheelFrictionController?.StopDrifting();
                ezerealLightController?.HandbrakeLightOff();
            }
        }

        void Handbraking()
        {
            if (currentHandbrakeValue > 0f)
            {
                rearLeftWheelCollider.motorTorque = 0;
                rearRightWheelCollider.motorTorque = 0;
                rearLeftWheelCollider.brakeTorque = currentHandbrakeValue * handbrakeForce;
                rearRightWheelCollider.brakeTorque = currentHandbrakeValue * handbrakeForce;
            }
            else
            {
                rearLeftWheelCollider.brakeTorque = 0;
                rearRightWheelCollider.brakeTorque = 0;
            }
        }

        void OnSteer(InputValue turnValue)
        {
            targetSteerAngle = turnValue.Get<float>() * maxSteerAngle;
        }

        void Steering()
        {
            float f = Mathf.InverseLerp(20, maxForwardSpeed, currentSpeed);
            float angle = targetSteerAngle * (1 - f);
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, angle, Time.deltaTime * steeringSpeed);

            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle;

            UpdateWheel(frontLeftWheelCollider, frontLeftWheelMesh);
            UpdateWheel(frontRightWheelCollider, frontRightWheelMesh);
            UpdateWheel(rearLeftWheelCollider, rearLeftWheelMesh);
            UpdateWheel(rearRightWheelCollider, rearRightWheelMesh);
        }

        void Slowdown()
        {
            if (vehicleRB == null) return;

            if (currentAccelerationValue == 0 && currentBrakeValue == 0 && currentHandbrakeValue == 0)
                vehicleRB.velocity = Vector3.Lerp(vehicleRB.velocity, Vector3.zero, Time.deltaTime * decelerationSpeed);
        }

        private void Update()
        {
            if (Input.GetKeyDown(flipCarKey) && vehicleRB.velocity.magnitude <= minFlipCarStationarySpeed)
            {
                flipCar = true;
            }
        }

        private void FixedUpdate()
        {
            if (flipCar)
            {
                vehicleRB.gameObject.transform.eulerAngles = new Vector3(vehicleRB.gameObject.transform.localEulerAngles.x, vehicleRB.gameObject.transform.localEulerAngles.y, 0);
                flipCar = false;
            }

            // --- Roll Stability Mod ---
            // Apply torque to keep car upright (spring + damper)
            if (vehicleRB != null)
            {
                float zTilt = Mathf.DeltaAngle(transform.localEulerAngles.z, 0f);
                // torque around forward vector keeps roll corrected
                Vector3 correctiveTorque = transform.forward * (-zTilt * rollStability)
                                          - transform.forward * (vehicleRB.angularVelocity.z * rollDamping);
                vehicleRB.AddTorque(correctiveTorque, ForceMode.Acceleration);
            }
            // ---------------------------

            if (automaticTransmission)
            {
                bool holdF = rawAccelInput > 0f;
                bool holdB = rawBrakeInput > 0f;

                // Handle backward hold (S)
                if (holdB)
                {
                    if (currentGear == AutomaticGears.Drive)
                    {
                        if (Mathf.Abs(currentSpeed) > stopThreshold)
                        {
                            currentBrakeValue = rawBrakeInput;
                            currentAccelerationValue = 0f;
                            ezerealLightController?.BrakeLightsOn();
                        }
                        else
                        {
                            currentGear = AutomaticGears.Reverse;
                            UpdateGearText("R");
                            ezerealLightController?.ReverseLightsOn();
                        }
                    }
                    if (currentGear == AutomaticGears.Reverse)
                    {
                        ezerealLightController?.BrakeLightsOff();
                        currentAccelerationValue = rawBrakeInput;
                        currentBrakeValue = 0f;
                    }
                }
                // Handle forward hold (W)
                else if (holdF)
                {
                    if (currentGear == AutomaticGears.Reverse)
                    {
                        if (Mathf.Abs(currentSpeed) > stopThreshold)
                        {
                            currentBrakeValue = rawAccelInput;
                            currentAccelerationValue = 0f;
                            ezerealLightController?.BrakeLightsOn();
                        }
                        else
                        {
                            currentBrakeValue = 0f;
                            currentGear = AutomaticGears.Drive;
                            UpdateGearText("D");
                            ezerealLightController?.ReverseLightsOff();
                        }
                    }
                    if (currentGear == AutomaticGears.Drive)
                    {
                        ezerealLightController?.BrakeLightsOff();
                        currentAccelerationValue = rawAccelInput;
                        currentBrakeValue = 0f;
                    }
                }
                else
                {
                    // Neither held
                    currentAccelerationValue = 0f;
                    currentBrakeValue = 0f;
                    ezerealLightController?.BrakeLightsOff();
                }
            }

            // Your existing pipeline
            Acceleration();
            Braking();
            Handbraking();
            HandleTireSmoke();
            Steering();
            Slowdown();
            RotateSteeringWheel();

            stationary =
                Mathf.Abs(frontLeftWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(frontRightWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(rearLeftWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(rearRightWheelCollider.rpm) < stopThreshold;

            if (vehicleRB != null)
            {
                currentSpeed = Vector3.Dot(vehicleRB.transform.forward, vehicleRB.velocity);
                currentSpeed *= 3.6f;
                UpdateSpeedText(currentSpeed);
            }

            FrontLeftWheelRPM = frontLeftWheelCollider.rpm;
            FrontRightWheelRPM = frontRightWheelCollider.rpm;
            RearLeftWheelRPM = rearLeftWheelCollider.rpm;
            RearRightWheelRPM = rearRightWheelCollider.rpm;

            // capture velocity for use on next impact
            if (vehicleRB != null)
                lastFixedVelocity = vehicleRB.velocity;
        }

        private void UpdateWheel(WheelCollider col, Transform mesh)
        {
            col.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.SetPositionAndRotation(pos, rot);
        }

        void RotateSteeringWheel()
        {
            float x = steeringWheel.localEulerAngles.x;
            float norm = Mathf.Clamp(frontLeftWheelCollider.steerAngle, -maxSteerAngle, maxSteerAngle);
            float z = Mathf.Lerp(maxSteeringWheelRotation, -maxSteeringWheelRotation, (norm + maxSteerAngle) / (2 * maxSteerAngle));
            steeringWheel.localRotation = Quaternion.Euler(x, 0, z);
        }

        void UpdateGearText(string gear)
        {
            /*currentGearTMP_UI.text = gear;
            currentGearTMP_Dashboard.text = gear;*/
        }

        void UpdateSpeedText(float speed)
        {
            speed = Mathf.Abs(speed);
            //currentSpeedTMP_UI.text = speed.ToString("F0");
            currentSpeedTMP_Dashboard.text = speed.ToString("F0");
        }

        void UpdateAccelerationSlider()
        {
            if (currentGear == AutomaticGears.Drive || currentGear == AutomaticGears.Reverse)
                accelerationSlider.value = Mathf.Lerp(accelerationSlider.value, currentAccelerationValue, Time.deltaTime * 15f);
            else
                accelerationSlider.value = 0;
        }

        public bool InAir()
        {
            foreach (var w in wheels)
                if (w.GetGroundHit(out _))
                    return false;
            return true;
        }

        // Helper methods for torque
        private void ApplyDriveTorque(float amount)
        {
            switch (driveType)
            {
                case DriveTypes.RWD:
                    rearLeftWheelCollider.motorTorque = amount;
                    rearRightWheelCollider.motorTorque = amount;
                    break;
                case DriveTypes.FWD:
                    frontLeftWheelCollider.motorTorque = amount;
                    frontRightWheelCollider.motorTorque = amount;
                    break;
                case DriveTypes.AWD:
                    frontLeftWheelCollider.motorTorque = amount;
                    frontRightWheelCollider.motorTorque = amount;
                    rearLeftWheelCollider.motorTorque = amount;
                    rearRightWheelCollider.motorTorque = amount;
                    break;
            }
        }

        private void ApplyReverseTorque(float amount)
        {
            switch (driveType)
            {
                case DriveTypes.RWD:
                    rearLeftWheelCollider.motorTorque = -amount;
                    rearRightWheelCollider.motorTorque = -amount;
                    break;
                case DriveTypes.FWD:
                    frontLeftWheelCollider.motorTorque = -amount;
                    frontRightWheelCollider.motorTorque = -amount;
                    break;
                case DriveTypes.AWD:
                    frontLeftWheelCollider.motorTorque = -amount;
                    frontRightWheelCollider.motorTorque = -amount;
                    rearLeftWheelCollider.motorTorque = -amount;
                    rearRightWheelCollider.motorTorque = -amount;
                    break;
            }
        }

        private void ClearDriveTorque()
        {
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;
        }

        // --- Impact smoothing coroutines ---
        private IEnumerator SmoothRestoreVelocity(Vector3 targetVelocity, float duration)
        {
            if (vehicleRB == null) yield break;

            float t = 0f;
            Vector3 startVel = vehicleRB.velocity;
            Vector3 startVelFlat = new Vector3(startVel.x, 0f, startVel.z);
            Vector3 targetVelFlat = new Vector3(targetVelocity.x, 0f, targetVelocity.z);

            float y = startVel.y;

            // Ensure the target magnitude is not greater than preImpactSpeed
            float targetMag = Mathf.Min(targetVelFlat.magnitude, preImpactSpeed);
            if (targetMag > 0f)
                targetVelFlat = targetVelFlat.normalized * targetMag;

            while (t < duration)
            {
                float alpha = t / duration;
                Vector3 blended = Vector3.Lerp(startVelFlat, targetVelFlat, alpha);

                // clamp blended magnitude so we don't overshoot pre-impact speed
                float blendedMag = blended.magnitude;
                if (blendedMag > preImpactSpeed)
                    blended = blended.normalized * preImpactSpeed;

                vehicleRB.velocity = new Vector3(blended.x, y, blended.z);
                t += Time.deltaTime;
                yield return null;
            }

            // final clamp and set
            Vector3 final = new Vector3(targetVelFlat.x, y, targetVelFlat.z);
            if (final.magnitude > preImpactSpeed)
                final = final.normalized * preImpactSpeed;

            vehicleRB.velocity = final;
            impactCoroutine = null;
            isImpactRecovering = false;
        }


        private IEnumerator TemporarilyIncreaseDrag(float toDrag, float duration)
        {
            if (vehicleRB == null) yield break;
            float start = vehicleRB.drag;
            float t = 0f;
            while (t < duration)
            {
                vehicleRB.drag = Mathf.Lerp(start, toDrag, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            // blend back to original smoothly
            t = 0f;
            while (t < duration)
            {
                vehicleRB.drag = Mathf.Lerp(toDrag, originalDrag, t / duration);
                t += Time.deltaTime;
                yield return null;
            }

            vehicleRB.drag = originalDrag;
        }
    }
}
