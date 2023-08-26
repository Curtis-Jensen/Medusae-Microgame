using UnityEngine;
using UnityEngine.Events;


    public class Health : MonoBehaviour
    {
        [Tooltip("Maximum amount of health")] public float maxHealth = 100f;

        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        [Range(0, 1)]
        public float CriticalHealthRatio = 0.3f;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction OnDie;

        [Tooltip("What amount of health the player starts with")]
        public float currentHealth = 100f;
        public bool Invincible { get; set; }
        public bool CanPickup() => currentHealth < maxHealth;

        public float GetRatio() => currentHealth / maxHealth;
        public bool IsCritical() => GetRatio() <= CriticalHealthRatio;

        bool isDead;

        public void Heal(float healAmount)
        {
            float healthBefore = currentHealth;
            currentHealth += healAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            // call OnHeal action
            float trueHealAmount = currentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        /// <summary>
        /// Inflict damage, and say what damaged it
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="damageSource"></param>
        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible)
                return;

            float healthBefore = currentHealth;
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            // call OnDamage action
            float trueDamageAmount = healthBefore - currentHealth;
            if (trueDamageAmount > 0f)
                OnDamaged?.Invoke(trueDamageAmount, damageSource);

            HandleDeath();
        }

        public void Kill()
        {
            currentHealth = 0f;

            // call OnDamage action
            OnDamaged?.Invoke(maxHealth, null);

            HandleDeath();
        }

        void HandleDeath()
        {
            if (isDead)
                return;

            // call OnDie action
            if (currentHealth <= 0f)
            {
                isDead = true;
                OnDie?.Invoke();
            }
        }
    }
