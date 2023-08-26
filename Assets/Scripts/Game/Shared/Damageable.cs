using UnityEngine;

namespace Unity.FPS.Game
{
    public class Damageable : MonoBehaviour
    {
        [Tooltip("Multiplier to apply to the received damage")]
        public float DamageMultiplier = 1f;

        [Tooltip("Multiplier to apply to self damage")]
        [Range(0, 1)]
        public float SensibilityToSelfdamage = 0.5f;

        public Health Health { get; private set; }

        void Awake()
        {
            // Find the health component either at the same level, or higher in the hierarchy
            Health = GetComponent<Health>();
            if (!Health)
                Health = GetComponentInParent<Health>();
        }

        /* 0 If there is no health component, return
         * 
         * 1 skip the crit multiplier if it's from an explosion
         * 
         * 2 potentially reduce damages if inflicted by self
         * 
         * 3 apply the damages
         */
        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if (!Health) return; // 0 

            var totalDamage = damage;

            if (!isExplosionDamage)
                totalDamage *= DamageMultiplier; // 1

            if (Health.gameObject == damageSource)
                totalDamage *= SensibilityToSelfdamage; // 2

            Health.TakeDamage(totalDamage, damageSource); // 3
        }
    }
}