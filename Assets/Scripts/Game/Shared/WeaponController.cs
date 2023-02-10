using UnityEngine;

namespace Unity.FPS.Game
{
    public class WeaponController : MonoBehaviour
    {
        #region🌎Variables
        [Header("Internal References")]
        [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
        public GameObject WeaponRoot;

        [Tooltip("Sound played when changing to this weapon")]
        public AudioClip ChangeWeaponSfx;

        [HideInInspector]
        public bool meleeWeapon = false;  // Used to check if behaviors should follow a melee weapon or a gun.  Set to false by default.  Set to true within melee child classes Start().

        public GameObject owner { get; set; }
        public GameObject sourcePrefab { get; set; }
        public bool IsWeaponActive { get; private set; }

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

        public void Reloading()
        {
            // Nothing goes in here for the sword, only the gun, at least for now
        }
    }
}