using UnityEngine;


    public class EnemyMelee : MonoBehaviour
    {
        public float clawDamage;
        public AudioClip hitSound;
        public float pushForce;

        AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponentInParent<AudioSource>();
        }

        /* Here's where melee damage comes in.
         * 
         * 1 Deal damage
         * 
         * 2 When doing the claw attack it should push the player away so
         * they have some time to breathe or are put out of position
         */
        protected void OnCollisionEnter(Collision striker)
        {
            if (striker.transform.CompareTag("Player"))
            {
                var health = striker.gameObject.GetComponentInChildren<Health>();
                var playerRigidbody = striker.gameObject.GetComponent<Rigidbody>();

                health.TakeDamage(clawDamage, damageSource: gameObject);//1
                audioSource.PlayOneShot(hitSound);

                var relativeX = striker.transform.position.x - transform.position.x;//2
                var relativeZ = striker.transform.position.z - transform.position.z;

                Vector3 pushDirection = new Vector3(relativeX, 0, relativeZ);
                playerRigidbody.AddForce(pushDirection * pushForce);
            }
        }
    }
