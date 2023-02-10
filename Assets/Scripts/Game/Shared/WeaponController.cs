using UnityEngine;

namespace Unity.FPS.Game
{
    public abstract class WeaponController : MonoBehaviour
    {
        #region🌎Variables
        [Header("Weapon Information")]
        [Tooltip("The image that will be displayed in the UI for this weapon")]
        public Sprite WeaponIcon;

        [Header("Weapon Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        public GameObject WeaponRoot;

        [Tooltip("Sound played when changing to this weapon")]
        public AudioClip ChangeWeaponSfx;

        [Tooltip("Default data for the crosshair")]
        public CrosshairData CrosshairDataDefault;

        [Tooltip("Data for the crosshair when targeting an enemy")]
        public CrosshairData CrosshairDataTargetInSight;

        [Header("Weapon Shoot Parameters")]
        [Tooltip("The type of weapon wil affect how it shoots")]
        public WeaponShootType ShootType;

        [Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
        [Range(0f, 1f)]
        public float AimZoomRatio = 1f;

        [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
        public Vector3 AimOffset;

        [Header("Weapon Ammo Parameters")]
        [Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
        public bool hasPhysicalBullets = false;

        [Tooltip("Maximum amount of ammo in the gun")]
        public int MaxAmmo = 8;

        [Tooltip("Number of bullets in a clip")]
        public int ClipSize = 30;

        [Tooltip("Initial ammo used when starting to charge")]
        public float AmmoUsedOnStartCharge = 1f;

        [Tooltip("Amount of bullets per shot")]
        public int BulletsPerShot = 1;

        [HideInInspector]
        public bool meleeWeapon = false;  // Used to check if behaviors should follow a melee weapon or a gun.  Set to false by default.  Set to true within melee child classes Start().

        public float CurrentAmmoRatio { get; set; }
        public GameObject owner { get; set; }
        public GameObject sourcePrefab { get; set; }
        public bool IsWeaponActive { get; private set; }
        public bool IsCharging { get; protected set; }
        protected int carriedPhysicalBullets;
        protected float currentAmmo;

        protected AudioSource weaponAudioSource;
        #endregion

        void Awake()
        {
            currentAmmo = MaxAmmo;
            carriedPhysicalBullets = hasPhysicalBullets ? ClipSize : 0;
        }

        public void ShowWeapon(bool show)
        {
            WeaponRoot.SetActive(show);

            if (show && ChangeWeaponSfx)
            {
                weaponAudioSource.PlayOneShot(ChangeWeaponSfx);
            }

            IsWeaponActive = show;
        }

        public abstract bool HandleAttackInputs(bool inputDown, bool inputHeld, bool inputUp);
        public abstract float AttackAnimation();
        public int GetCarriedPhysicalBullets() => carriedPhysicalBullets;
        public int GetCurrentAmmo() => Mathf.FloorToInt(currentAmmo);
        public float GetAmmoNeededToShoot() =>
            (ShootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
            (MaxAmmo * BulletsPerShot);
    }
}