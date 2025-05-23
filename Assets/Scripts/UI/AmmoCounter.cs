﻿using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    [RequireComponent(typeof(FillBarColorChange))]
    public class AmmoCounter : MonoBehaviour
    {
        [Tooltip("CanvasGroup to fade the ammo UI")]
        public CanvasGroup CanvasGroup;

        [Tooltip("Image for the weapon icon")] public Image WeaponImage;

        [Tooltip("Image component for the background")]
        public Image AmmoBackgroundImage;

        [Tooltip("Image component to display fill ratio")]
        public Image AmmoFillImage;

        [Tooltip("Text for Weapon index")]
        public TextMeshProUGUI WeaponIndexText;

        [Tooltip("Text for Bullet Counter")]
        public TextMeshProUGUI BulletCounter;

        [Tooltip("Reload Text for Weapons with physical bullets")]
        public RectTransform Reload;

        [Header("Selection")]
        [Range(0, 1)]
        [Tooltip("Opacity when weapon not selected")]
        public float UnselectedOpacity = 0.5f;

        [Tooltip("Scale when weapon not selected")]
        public Vector3 UnselectedScale = Vector3.one * 0.8f;

        [Tooltip("Root for the control keys")] public GameObject ControlKeysRoot;

        [Header("Feedback")]
        [Tooltip("Component to animate the color when empty or full")]
        public FillBarColorChange FillBarColorChange;

        [Tooltip("Sharpness for the fill ratio movements")]
        public float AmmoFillMovementSharpness = 20f;

        public int WeaponCounterIndex { get; set; }

        PlayerWeaponsManager playerWeaponsManager;
        WeaponController weapon;

        void Awake()
        {
            EventManager.AddListener<AmmoPickupEvent>(OnAmmoPickup);
        }

        void OnAmmoPickup(AmmoPickupEvent evt)
        {
            if (evt.Weapon == weapon)
                BulletCounter.text = weapon.GetCarriedPhysicalBullets().ToString();
        }

        public void Initialize(WeaponController weapon, int weaponIndex)
        {
            this.weapon = weapon;
            WeaponCounterIndex = weaponIndex;
            WeaponImage.sprite = weapon.WeaponIcon;
            if (!weapon.hasPhysicalBullets)
                BulletCounter.transform.parent.gameObject.SetActive(false);
            else
                BulletCounter.text = weapon.GetCarriedPhysicalBullets().ToString();

            Reload.gameObject.SetActive(false);
            playerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, AmmoCounter>(playerWeaponsManager, this);

            WeaponIndexText.text = (WeaponCounterIndex + 1).ToString();

            FillBarColorChange.Initialize(1f, this.weapon.GetAmmoNeededToShoot());
        }

        void Update()
        {
            float currenFillRatio = weapon.CurrentAmmoRatio;
            AmmoFillImage.fillAmount = Mathf.Lerp(AmmoFillImage.fillAmount, currenFillRatio,
                Time.deltaTime * AmmoFillMovementSharpness);

            BulletCounter.text = weapon.GetCarriedPhysicalBullets().ToString();

            bool isActiveWeapon = weapon == playerWeaponsManager.GetActiveWeapon();

            CanvasGroup.alpha = Mathf.Lerp(CanvasGroup.alpha, isActiveWeapon ? 1f : UnselectedOpacity,
                Time.deltaTime * 10);
            transform.localScale = Vector3.Lerp(transform.localScale, isActiveWeapon ? Vector3.one : UnselectedScale,
                Time.deltaTime * 10);
            ControlKeysRoot.SetActive(!isActiveWeapon);

            FillBarColorChange.UpdateVisual(currenFillRatio);

            Reload.gameObject.SetActive(weapon.GetCarriedPhysicalBullets() > 0 && weapon.GetCurrentAmmo() == 0 && weapon.IsWeaponActive);
        }

        void Destroy()
        {
            EventManager.RemoveListener<AmmoPickupEvent>(OnAmmoPickup);
        }
    }
}