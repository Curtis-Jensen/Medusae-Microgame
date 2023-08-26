using UnityEngine;


    public class MedusaHealth : MonoBehaviour
    {
        [Tooltip("*Whatever the health is set to at start is the max health")]
        [Min(1)]
        public int health;
        [Range(0, 1)]
        public float hitVolume;
        public NavigateToPlayer medusaController;
        public AudioClip hitSound;

        int maxHealth;
        AudioSource audioSource;

        /* 1 The max health is determined by whatever it's set at at start
         * 
         * 2 Gets the audio component for when the player is hit
         */
        void Start()
        {
            maxHealth = health;
            audioSource = GetComponentInParent<AudioSource>();//2
        }

        void OnCollisionEnter(Collision striker)
        {
            if (striker.transform.CompareTag("Weapon"))
                Damage(1);
        }

        /* 1 Plays the sound of it getting hit (with the right volume)
         * 
         * 2 takes away health
         * 
         * 3 if there is no health, prep it healthwise for next time, then kill it
         */
        public void Damage(int damage)
        {
            audioSource.PlayOneShot(hitSound, hitVolume);//1

            health -= damage;//2
            if (health <= 0)
            {
                health = maxHealth;//3
                //StartCoroutine(medusaController.Die());
            }
        }
    }
