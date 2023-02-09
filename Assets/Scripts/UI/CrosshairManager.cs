using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class CrosshairManager : MonoBehaviour
    {
        public Image CrosshairImage;
        public Sprite NullCrosshairSprite;
        public float CrosshairUpdateshrpness = 5f;

        PlayerWeaponsManager weaponsManager;
        bool wasPointingAtEnemy;
        RectTransform crosshairRectTransform;
        CrosshairData crosshairDataDefault;
        CrosshairData crosshairDataTarget;
        CrosshairData currentCrosshair;

        void Start()
        {
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, CrosshairManager>(weaponsManager, this);

            OnWeaponChanged(weaponsManager.GetActiveWeapon());

            weaponsManager.OnSwitchedToWeapon += OnWeaponChanged;
        }

        void Update()
        {
            UpdateCrosshairPointingAtEnemy(false);
            wasPointingAtEnemy = weaponsManager.IsPointingAtEnemy;
        }

        void UpdateCrosshairPointingAtEnemy(bool force)
        {
            if (crosshairDataDefault.CrosshairSprite == null)
                return;

            if ((force || !wasPointingAtEnemy) && weaponsManager.IsPointingAtEnemy)
            {
                currentCrosshair = crosshairDataTarget;
                CrosshairImage.sprite = currentCrosshair.CrosshairSprite;
                crosshairRectTransform.sizeDelta = currentCrosshair.CrosshairSize * Vector2.one;
            }
            else if ((force || wasPointingAtEnemy) && !weaponsManager.IsPointingAtEnemy)
            {
                currentCrosshair = crosshairDataDefault;
                CrosshairImage.sprite = currentCrosshair.CrosshairSprite;
                crosshairRectTransform.sizeDelta = currentCrosshair.CrosshairSize * Vector2.one;
            }

            CrosshairImage.color = Color.Lerp(CrosshairImage.color, currentCrosshair.CrosshairColor,
                Time.deltaTime * CrosshairUpdateshrpness);

            crosshairRectTransform.sizeDelta = Mathf.Lerp(crosshairRectTransform.sizeDelta.x,
                currentCrosshair.CrosshairSize,
                Time.deltaTime * CrosshairUpdateshrpness) * Vector2.one;
        }

        void OnWeaponChanged(GunController newWeapon)
        {
            if (newWeapon)
            {
                CrosshairImage.enabled = true;
                crosshairDataDefault = newWeapon.CrosshairDataDefault;
                crosshairDataTarget = newWeapon.CrosshairDataTargetInSight;
                crosshairRectTransform = CrosshairImage.GetComponent<RectTransform>();
                DebugUtility.HandleErrorIfNullGetComponent<RectTransform, CrosshairManager>(crosshairRectTransform,
                    this, CrosshairImage.gameObject);
            }
            else
            {
                if (NullCrosshairSprite)
                {
                    CrosshairImage.sprite = NullCrosshairSprite;
                }
                else
                {
                    CrosshairImage.enabled = false;
                }
            }

            UpdateCrosshairPointingAtEnemy(true);
        }
    }
}