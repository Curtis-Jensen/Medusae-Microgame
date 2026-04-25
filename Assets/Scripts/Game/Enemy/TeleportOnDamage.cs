using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Game
{
    public class TeleportOnDamage : MonoBehaviour
    {
        [SerializeField] private float teleportDistance = 5f;

        private Health health;

        private void Start()
        {
            health = GetComponent<Health>();
            if (health != null)
            {
                health.OnDamaged += Teleport;
            }
        }

        private void Teleport(float damageAmount, GameObject damageSource)
        {
            Vector3 randomOffset = Random.insideUnitSphere * teleportDistance;
            transform.position += randomOffset;
        }
    }
}
