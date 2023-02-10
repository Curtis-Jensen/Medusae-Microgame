using UnityEngine;

namespace Unity.FPS.Game
{
    public abstract class WeaponController : MonoBehaviour
    {
        #region🌎Variables
        [Header("Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        public GameObject WeaponRoot;

        [Tooltip("Sound played when changing to this weapon")]
        public AudioClip ChangeWeaponSfx;

        [Tooltip("Default data for the crosshair")]
        public CrosshairData CrosshairDataDefault;

        [Tooltip("Data for the crosshair when targeting an enemy")]
        public CrosshairData CrosshairDataTargetInSight;

        [HideInInspector]
        public bool meleeWeapon = false;  // Used to check if behaviors should follow a melee weapon or a gun.  Set to false by default.  Set to true within melee child classes Start().

        public GameObject owner { get; set; }
        public GameObject sourcePrefab { get; set; }
        public bool IsWeaponActive { get; private set; }
        public bool IsCharging { get; protected set; }

        protected AudioSource weaponAudioSource;
        #endregion

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
    }
}