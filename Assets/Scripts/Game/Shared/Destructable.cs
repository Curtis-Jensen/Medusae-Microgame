using UnityEngine;


    public class Destructable : MonoBehaviour
    {
        Health health;

        void Start()
        {
            health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(health, this, gameObject);

            // Subscribe to damage & death actions
            health.OnDie += OnDie;
            health.OnDamaged += OnDamaged;
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            // TODO: damage reaction
        }

        void OnDie()
        {
            // this will call the OnDestroy function
            Destroy(gameObject);
        }
    }
