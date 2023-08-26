using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying weapon ammo")]
        public RectTransform AmmoPanel;

        [Tooltip("Prefab for displaying weapon ammo")]
        public GameObject AmmoCounterPrefab;

        PlayerWeaponsManager playerWeaponsManager;
        List<AmmoCounter> ammoCounters = new List<AmmoCounter>();

        void Start()
        {
            playerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, WeaponHUDManager>(playerWeaponsManager,
                this);

            WeaponController activeWeapon = playerWeaponsManager.GetActiveWeapon();
            if (activeWeapon)
            {
                AddWeapon(activeWeapon, playerWeaponsManager.ActiveWeaponIndex);
                ChangeWeapon(activeWeapon);
            }

            playerWeaponsManager.OnAddedWeapon += AddWeapon;
            playerWeaponsManager.OnRemovedWeapon += RemoveWeapon;
            playerWeaponsManager.OnSwitchedToWeapon += ChangeWeapon;
        }

        void AddWeapon(WeaponController newWeapon, int weaponIndex)
        {
            GameObject ammoCounterInstance = Instantiate(AmmoCounterPrefab, AmmoPanel);
            AmmoCounter newAmmoCounter = ammoCounterInstance.GetComponent<AmmoCounter>();
            DebugUtility.HandleErrorIfNullGetComponent<AmmoCounter, WeaponHUDManager>(newAmmoCounter, this,
                ammoCounterInstance.gameObject);

            newAmmoCounter.Initialize(newWeapon, weaponIndex);

            ammoCounters.Add(newAmmoCounter);
        }

        /// <summary>
        /// Makes sure that weapons are indexed correctly when removing a weapon?
        /// </summary>
        /// <param name="newWeapon"></param>
        /// <param name="weaponIndex"></param>
        void RemoveWeapon(WeaponController newWeapon, int weaponIndex)
        {
            int foundCounterIndex = -1;
            for (int i = 0; i < ammoCounters.Count; i++)
            {
                if (ammoCounters[i].WeaponCounterIndex == weaponIndex)
                {
                    foundCounterIndex = i;
                    Destroy(ammoCounters[i].gameObject);
                }
            }

            if (foundCounterIndex >= 0)
                ammoCounters.RemoveAt(foundCounterIndex);
        }

        void ChangeWeapon(WeaponController weapon)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(AmmoPanel);
        }
    }
}