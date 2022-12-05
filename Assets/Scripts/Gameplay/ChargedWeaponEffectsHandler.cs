using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class ChargedWeaponEffectsHandler : MonoBehaviour
    {
        [Header("Visual")] [Tooltip("Object that will be affected by charging scale & color changes")]
        public GameObject ChargingObject;

        [Tooltip("The spinning frame")] public GameObject SpinningFrame;

        [Tooltip("Scale of the charged object based on charge")]
        public MinMaxVector3 Scale;

        [Header("Particles")] [Tooltip("Particles to create when charging")]
        public GameObject DiskOrbitParticlePrefab;

        [Tooltip("Local position offset of the charge particles (relative to this transform)")]
        public Vector3 Offset;

        [Tooltip("Parent transform for the particles (Optional)")]
        public Transform ParentTransform;

        [Tooltip("Orbital velocity of the charge particles based on charge")]
        public MinMaxFloat OrbitY;

        [Tooltip("Radius of the charge particles based on charge")]
        public MinMaxVector3 Radius;

        [Tooltip("Idle spinning speed of the frame based on charge")]
        public MinMaxFloat SpinningSpeed;

        [Header("Sound")] [Tooltip("Audio clip for charge SFX")]
        public AudioClip ChargeSound;

        [Tooltip("Sound played in loop after the change is full for this weapon")]
        public AudioClip LoopChargeWeaponSfx;

        [Tooltip("Duration of the cross fade between the charge and the loop sound")]
        public float FadeLoopDuration = 0.5f;

        [Tooltip(
            "If true, the ChargeSound will be ignored and the pitch on the LoopSound will be procedural, based on the charge amount")]
        public bool UseProceduralPitchOnLoopSfx;

        [Range(1.0f, 5.0f), Tooltip("Maximum procedural Pitch value")]
        public float MaxProceduralPitchValue = 2.0f;

        public GameObject ParticleInstance { get; set; }

        ParticleSystem diskOrbitParticle;
        WeaponController weaponController;
        ParticleSystem.VelocityOverLifetimeModule velocityOverTimeModule;

        AudioSource audioSource;
        AudioSource audioSourceLoop;

        float lastChargeTriggerTimestamp;
        float chargeRatio;
        float endchargeTime;

        void Awake()
        {
            lastChargeTriggerTimestamp = 0.0f;

            // The charge effect needs it's own AudioSources, since it will play on top of the other gun sounds
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = ChargeSound;
            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup =
                AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponChargeBuildup);

            // create a second audio source, to play the sound with a delay
            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceLoop.clip = LoopChargeWeaponSfx;
            audioSourceLoop.playOnAwake = false;
            audioSourceLoop.loop = true;
            audioSourceLoop.outputAudioMixerGroup =
                AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponChargeLoop);
        }

        /* 1 Spawn the disc orbit particles for the launcher as a child of the gun
         */
        void SpawnParticleSystem()
        {
            ParticleInstance = Instantiate(DiskOrbitParticlePrefab, //1
                ParentTransform != null ? ParentTransform : transform);
            ParticleInstance.transform.localPosition += Offset;

            FindReferences();
        }

        public void FindReferences()
        {
            diskOrbitParticle = ParticleInstance.GetComponent<ParticleSystem>();
            DebugUtility.HandleErrorIfNullGetComponent<ParticleSystem, ChargedWeaponEffectsHandler>(diskOrbitParticle,
                this, ParticleInstance.gameObject);

            weaponController = GetComponent<WeaponController>();
            DebugUtility.HandleErrorIfNullGetComponent<WeaponController, ChargedWeaponEffectsHandler>(
                weaponController, this, gameObject);

            velocityOverTimeModule = diskOrbitParticle.velocityOverLifetime;
        }

        void Update()
        {
            if (ParticleInstance == null)
                SpawnParticleSystem();

            diskOrbitParticle.gameObject.SetActive(weaponController.IsWeaponActive);
            chargeRatio = weaponController.CurrentCharge;

            ChargingObject.transform.localScale = Scale.GetValueFromRatio(chargeRatio);
            if (SpinningFrame != null)
            {
                SpinningFrame.transform.localRotation *= Quaternion.Euler(0,
                    SpinningSpeed.GetValueFromRatio(chargeRatio) * Time.deltaTime, 0);
            }

            velocityOverTimeModule.orbitalY = OrbitY.GetValueFromRatio(chargeRatio);
            diskOrbitParticle.transform.localScale = Radius.GetValueFromRatio(chargeRatio * 1.1f);

            // update sound's volume and pitch 
            if (chargeRatio > 0)
            {
                if (!audioSourceLoop.isPlaying &&
                    weaponController.LastChargeTriggerTimestamp > lastChargeTriggerTimestamp)
                {
                    lastChargeTriggerTimestamp = weaponController.LastChargeTriggerTimestamp;
                    if (!UseProceduralPitchOnLoopSfx)
                    {
                        endchargeTime = Time.time + ChargeSound.length;
                        audioSource.Play();
                    }

                    audioSourceLoop.Play();
                }

                if (!UseProceduralPitchOnLoopSfx)
                {
                    float volumeRatio =
                        Mathf.Clamp01((endchargeTime - Time.time - FadeLoopDuration) / FadeLoopDuration);
                    audioSource.volume = volumeRatio;
                    audioSourceLoop.volume = 1 - volumeRatio;
                }
                else
                {
                    audioSourceLoop.pitch = Mathf.Lerp(1.0f, MaxProceduralPitchValue, chargeRatio);
                }
            }
            else
            {
                audioSource.Stop();
                audioSourceLoop.Stop();
            }
        }
    }
}