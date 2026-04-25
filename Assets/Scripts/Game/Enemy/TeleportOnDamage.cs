using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Game
{
    public class TeleportOnDamage : MonoBehaviour
    {
        [SerializeField] private float teleportDistance = 5f;
        [SerializeField] private bool randomHeight = true;
        [SerializeField] private float maxHeightOffset = 2f;

        private Health health;

        private void Start()
        {
            health = GetComponent<Health>();
            if (health != null)
            {
                health.OnDamaged += OnDamageTaken;
            }
            else
            {
                Debug.LogWarning("TeleportOnDamage: Health component not found on this GameObject", gameObject);
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDamaged -= OnDamageTaken;
            }
        }

        private void OnDamageTaken(float damageAmount, GameObject damageSource)
        {
            Teleport();
        }

        private void Teleport()
        {
            Vector3 randomOffset = Random.insideUnitSphere * teleportDistance;
            
            if (!randomHeight)
            {
                randomOffset.y = 0f;
            }
            else
            {
                randomOffset.y = Random.Range(-maxHeightOffset, maxHeightOffset);
            }

            transform.position += randomOffset;
        }
    }
}
