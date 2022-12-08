using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class FeedbackFlashHUD : MonoBehaviour
    {
        [Header("References")] [Tooltip("Image component of the flash")]
        public Image FlashImage;

        [Tooltip("CanvasGroup to fade the damage flash, used when recieving damage end healing")]
        public CanvasGroup FlashCanvasGroup;

        [Tooltip("CanvasGroup to fade the critical health vignette")]
        public CanvasGroup VignetteCanvasGroup;

        [Header("Damage")] [Tooltip("Color of the damage flash")]
        public Color DamageFlashColor;

        [Tooltip("Duration of the damage flash")]
        public float DamageFlashDuration;

        [Tooltip("Max alpha of the damage flash")]
        public float DamageFlashMaxAlpha = 1f;

        [Header("Critical health")] [Tooltip("Max alpha of the critical vignette")]
        public float CriticaHealthVignetteMaxAlpha = .8f;

        [Tooltip("Frequency at which the vignette will pulse when at critical health")]
        public float PulsatingVignetteFrequency = 4f;

        [Header("Heal")] [Tooltip("Color of the heal flash")]
        public Color HealFlashColor;

        [Tooltip("Duration of the heal flash")]
        public float HealFlashDuration;

        [Tooltip("Max alpha of the heal flash")]
        public float HealFlashMaxAlpha = 1f;

        bool flashActive;
        float lastTimeFlashStarted = Mathf.NegativeInfinity;
        Health playerHealth;
        GameFlowManager gameFlowManager;

        void Start()
        {
            // Subscribe to player damage events
            PlayerCharacterController playerCharacterController = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, FeedbackFlashHUD>(
                playerCharacterController, this);

            playerHealth = playerCharacterController.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, FeedbackFlashHUD>(playerHealth, this,
                playerCharacterController.gameObject);

            gameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, FeedbackFlashHUD>(gameFlowManager, this);

            playerHealth.onDamaged += OnTakeDamage;
            playerHealth.onHealed += OnHealed;
        }

        void Update()
        {
            if (playerHealth.IsCritical())
                CriticalHealthFlash();
            else
                VignetteCanvasGroup.gameObject.SetActive(false);

            if (flashActive)
                Flash();
        }

        void Flash()
        {
            float normalizedTimeSinceDamage = (Time.time - lastTimeFlashStarted) / DamageFlashDuration;

            if (normalizedTimeSinceDamage < 1f)
            {
                //Gradually weaken the flash amount
                float flashAmount = DamageFlashMaxAlpha * (1f - normalizedTimeSinceDamage);
                FlashCanvasGroup.alpha = flashAmount;
            }
            else
            {
                FlashCanvasGroup.gameObject.SetActive(false);
                flashActive = false;
            }
        }

        void CriticalHealthFlash()
        {
            VignetteCanvasGroup.gameObject.SetActive(true);
            float vignetteAlpha =
                (1 - (playerHealth.CurrentHealth / playerHealth.MaxHealth /
                      playerHealth.CriticalHealthRatio)) * CriticaHealthVignetteMaxAlpha;

            if (gameFlowManager.GameIsEnding)
                VignetteCanvasGroup.alpha = vignetteAlpha;
            else
                VignetteCanvasGroup.alpha =
                    ((Mathf.Sin(Time.time * PulsatingVignetteFrequency) / 2) + 0.5f) * vignetteAlpha;
        }

        void ResetFlash()
        {
            lastTimeFlashStarted = Time.time;
            flashActive = true;
            FlashCanvasGroup.alpha = 0f;
            FlashCanvasGroup.gameObject.SetActive(true);
        }

        void OnTakeDamage(float damage, GameObject damageSource)
        {
            //If the damage is not large enough then assume it is gradual static damage and don't flash
            if (damage < 3) return;

            ResetFlash();
            FlashImage.color = DamageFlashColor;
        }

        void OnHealed(float amount)
        {
            ResetFlash();
            FlashImage.color = HealFlashColor;
        }
    }
}