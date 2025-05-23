﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    #region 🌎 Enums
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
    }

    [System.Serializable]
    public struct CrosshairData
    {
        [Tooltip("The image that will be used for this weapon's crosshair")]
        public Sprite CrosshairSprite;

        [Tooltip("The size of the crosshair image")]
        public int CrosshairSize;

        [Tooltip("The color of the crosshair image")]
        public Color CrosshairColor;
    }
    #endregion

    [RequireComponent(typeof(AudioSource))]
    public class GunController : WeaponController
    {
        #region 🌎 Variables
        [Header("Information")]
        [Tooltip("Tip of the weapon, where the projectiles are shot")]
        public Transform WeaponMuzzle;

        [Header("Shoot Parameters")]

        [Tooltip("The projectile prefab")] public ProjectileBase ProjectilePrefab;

        [Tooltip("Minimum duration between two shots")]
        public float DelayBetweenShots = 0.5f;

        [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
        public float BulletSpreadAngle = 0f;

        [Tooltip("Force that will push back the weapon after each shot")]
        [Range(0f, 2f)]
        public float RecoilForce = 1;

        [Header("Ammo Parameters")]
        [Tooltip("Bullet Shell Casing")]
        public GameObject ShellCasing;
        [Tooltip("Weapon Ejection Port for physical ammo")]
        public Transform EjectionPort;
        [Tooltip("Force applied on the shell")]
        [Range(0.0f, 5.0f)] public float ShellCasingEjectionForce = 2.0f;
        [Tooltip("Maximum number of shell that can be spawned before reuse")]
        [Range(1, 30)] public int ShellPoolSize = 1;
        [Tooltip("Amount of ammo reloaded per second")]
        public float AmmoReloadRate = 1f;

        [Tooltip("Delay after the last shot before starting to reload")]
        public float AmmoReloadDelay = 2f;

        [Header("Charging parameters (charging weapons only)")]
        [Tooltip("Trigger a shot when maximum charge is reached")]
        public bool AutomaticReleaseOnCharged;

        [Tooltip("Duration to reach maximum charge")]
        public float MaxChargeDuration = 2f;

        [Tooltip("Additional ammo used when charge reaches its maximum")]
        public float AmmoUsageRateWhileCharging = 1f;

        [Header("Audio & Visual")]
        [Tooltip("Optional weapon animator for OnShoot animations")]
        public Animator WeaponAnimator;

        [Tooltip("Prefab of the muzzle flash")]
        public GameObject MuzzleFlashPrefab;

        [Tooltip("Unparent the muzzle flash instance on spawn")]
        public bool UnparentMuzzleFlash;

        [Tooltip("sound played when shooting")]
        public AudioClip ShootSfx;

        [Tooltip("Continuous Shooting Sound")] public bool UseContinuousShootSound = false;
        public AudioClip ContinuousShootStartSfx;
        public AudioClip ContinuousShootLoopSfx;
        public AudioClip ContinuousShootEndSfx;
        AudioSource continuousShootAudioSource = null;
        bool wantsToShoot = false;

        public UnityAction OnShoot;
        public event Action OnShootProcessed;

        float lastTimeShot = Mathf.NegativeInfinity;
        public float LastChargeTriggerTimestamp { get; private set; }
        Vector3 lastMuzzlePosition;

        public bool IsCooling { get; private set; }
        public float CurrentCharge { get; private set; }
        public Vector3 MuzzleWorldVelocity { get; private set; }

        const string animAttackParameter = "Attack";

        private Queue<Rigidbody> physicalAmmoPool;
        #endregion

        #region Monobehavior Methods
        void Awake()
        {
            lastMuzzlePosition = WeaponMuzzle.position;

            weaponAudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, GunController>(weaponAudioSource, this,
                gameObject);

            if (UseContinuousShootSound)
            {
                continuousShootAudioSource = gameObject.AddComponent<AudioSource>();
                continuousShootAudioSource.playOnAwake = false;
                continuousShootAudioSource.clip = ContinuousShootLoopSfx;
                continuousShootAudioSource.outputAudioMixerGroup =
                    AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
                continuousShootAudioSource.loop = true;
            }

            if (hasPhysicalBullets)
            {
                physicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

                for (int i = 0; i < ShellPoolSize; i++)
                {
                    GameObject shell = Instantiate(ShellCasing, transform);
                    shell.SetActive(false);
                    physicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
                }
            }
        }

        void Update()
        {
            UpdateAmmo();
            UpdateCharge();
            UpdateContinuousShootSound();

            if (Time.deltaTime > 0)
            {
                MuzzleWorldVelocity = (WeaponMuzzle.position - lastMuzzlePosition) / Time.deltaTime;
                lastMuzzlePosition = WeaponMuzzle.position;
            }
        }
        #endregion

        #region Other Methods
        public void AddCarriablePhysicalBullets(int count) => carriedPhysicalBullets = Mathf.Max(carriedPhysicalBullets + count, MaxAmmo);

        void ShootShell()
        {
            Rigidbody nextShell = physicalAmmoPool.Dequeue();

            nextShell.transform.position = EjectionPort.transform.position;
            nextShell.transform.rotation = EjectionPort.transform.rotation;
            nextShell.gameObject.SetActive(true);
            nextShell.transform.SetParent(null);
            nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
            nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

            physicalAmmoPool.Enqueue(nextShell);
        }

        void UpdateAmmo()
        {
            if (lastTimeShot + AmmoReloadDelay < Time.time && currentAmmo < MaxAmmo && !IsCharging)
            {
                // reloads weapon over time
                currentAmmo += AmmoReloadRate * Time.deltaTime;

                IsCooling = true;
            }
            else
                IsCooling = false;

            if (MaxAmmo == Mathf.Infinity)
                CurrentAmmoRatio = 1f;
            else
                CurrentAmmoRatio = currentAmmo / MaxAmmo;
        }

        void UpdateCharge()
        {
            if (IsCharging)
            {
                if (CurrentCharge < 1f)
                {
                    float chargeLeft = 1f - CurrentCharge;

                    // Calculate how much charge ratio to add this frame
                    float chargeAdded = 0f;
                    if (MaxChargeDuration <= 0f)
                    {
                        chargeAdded = chargeLeft;
                    }
                    else
                    {
                        chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
                    }

                    chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                    // See if we can actually add this charge
                    float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
                    if (ammoThisChargeWouldRequire <= currentAmmo)
                    {
                        // Use ammo based on charge added
                        UseAmmo(ammoThisChargeWouldRequire);

                        // set current charge ratio
                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
                    }
                }
            }
        }

        void UpdateContinuousShootSound()
        {
            if (!UseContinuousShootSound) return;

            if (wantsToShoot && currentAmmo >= 1f && !continuousShootAudioSource.isPlaying)
            {
                weaponAudioSource.PlayOneShot(ShootSfx);
                weaponAudioSource.PlayOneShot(ContinuousShootStartSfx);
                continuousShootAudioSource.Play();
            }
            else if (continuousShootAudioSource.isPlaying)
            {
                weaponAudioSource.PlayOneShot(ContinuousShootEndSfx);
                continuousShootAudioSource.Stop();
            }
        }

        public void UseAmmo(float amount)
        {
            currentAmmo = Mathf.Clamp(currentAmmo - amount, 0f, MaxAmmo);
            carriedPhysicalBullets -= Mathf.RoundToInt(amount);
            carriedPhysicalBullets = Mathf.Clamp(carriedPhysicalBullets, 0, MaxAmmo);
            lastTimeShot = Time.time;
        }

        /// <summary>
        /// Determine what kind of shooting should occur based off inputs and weapon type.
        /// </summary>
        /// <param name="inputDown"></param>
        /// <param name="inputHeld"></param>
        /// <param name="inputUp"></param>
        /// <returns></returns>
        public override bool HandleAttackInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            wantsToShoot = inputDown || inputHeld;
            switch (ShootType)
            {
                case WeaponShootType.Manual:
                    if (inputDown)
                        return TryShoot();

                    return false;

                case WeaponShootType.Automatic:
                    if (inputHeld)
                        return TryShoot();

                    return false;

                case WeaponShootType.Charge:
                    if (inputHeld)
                        TryBeginCharge();

                    // Check if we released charge or if the weapon shoot autmatically when it's fully charged
                    if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
                        return TryReleaseCharge();

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Check if shooting is allowed
        /// </summary>
        /// <returns></returns>
        bool TryShoot()
        {
            if (currentAmmo >= 1f
                && lastTimeShot + DelayBetweenShots < Time.time)
            {
                HandleShoot();
                currentAmmo -= 1f;

                return true;
            }

            return false;
        }

        bool TryBeginCharge()
        {
            if (!IsCharging
                && currentAmmo >= AmmoUsedOnStartCharge
                && Mathf.FloorToInt((currentAmmo - AmmoUsedOnStartCharge) * BulletsPerShot) > 0
                && lastTimeShot + DelayBetweenShots < Time.time)
            {
                UseAmmo(AmmoUsedOnStartCharge);

                LastChargeTriggerTimestamp = Time.time;
                IsCharging = true;

                return true;
            }

            return false;
        }

        bool TryReleaseCharge()
        {
            if (IsCharging)
            {
                HandleShoot();

                CurrentCharge = 0f;
                IsCharging = false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles bullet instantiation, muzzle flash, and sound effects
        /// </summary>
        void HandleShoot()
        {
            int bulletsPerShotFinal = ShootType == WeaponShootType.Charge
                ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
                : BulletsPerShot;

            for (int i = 0; i < bulletsPerShotFinal; i++)
            {
                Vector3 shotDirection = GetShotDirectionWithinSpread(WeaponMuzzle);
                ProjectileBase newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position,
                    Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(this);
            }

            // muzzle flash
            if (MuzzleFlashPrefab != null)
            {
                GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position,
                    WeaponMuzzle.rotation, WeaponMuzzle.transform);
                // Unparent the muzzleFlashInstance
                if (UnparentMuzzleFlash)
                    muzzleFlashInstance.transform.SetParent(null);

                Destroy(muzzleFlashInstance, 2f);
            }

            if (hasPhysicalBullets)
            {
                ShootShell();
                carriedPhysicalBullets--;
            }

            lastTimeShot = Time.time;

            // play shoot SFX
            if (ShootSfx && !UseContinuousShootSound)
                weaponAudioSource.PlayOneShot(ShootSfx);

            // Trigger attack animation if there is any
            if (WeaponAnimator)
                WeaponAnimator.SetTrigger(animAttackParameter);

            OnShoot?.Invoke();
            OnShootProcessed?.Invoke();
        }

        /// <summary>
        /// Gives a random vector within a range to simulate bullet spread
        /// </summary>
        /// <param name="shootTransform"></param>
        /// <returns></returns>
        public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
        {
            float spreadAngleRatio = BulletSpreadAngle / 180f;
            Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
                spreadAngleRatio);

            return spreadWorldDirection;
        }

        public override float AttackAnimation()
        {
            return RecoilForce;
        }
        #endregion
    }
}