using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Unity.FPS.Game
{
    public class MedusaController : MonoBehaviour
    {
        #region Variables
        public float lookRadius = 10f;
        [Range(0, 1)]
        public float ammoChance;
        public float speed;
        [Tooltip("Used to determine how high the player can look before they are determined to be looking above the medusae")]
        public float gazeHeight;
        public AudioClip[] deathScreams;
        public GameObject ammoCrate;
        [Tooltip("Default material for when emitting light")]
        public Material glowingMaterial;
        [Tooltip("Appearance when killed.")]
        public Material dimMaterial;

        NavMeshAgent agent;
        Transform target; //Whatever is the intended destination of the enemy at the moment
        Transform player; //Player 1, used for targeting
        Transform playerFront; //The front of the player.  Used for targeting
        AudioSource audioSource;
        Collider collider;
        GameObject[] lifeLights; //The light on the enemy's chest
        [Tooltip("The number of enemies alive in the wave.")]
        public static int enemyNum = 1;
        private Renderer[] ownRenderers;
        #endregion

        /* Just getting a bunch of components
         * 
         * The life lights are actually the game objects that house the lights,
         * so a bit more work needs to be done to turn those off.
         */
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            audioSource = GetComponent<AudioSource>();
            ownRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            collider = GetComponentInChildren<Collider>();

            var lifeLightsCoreLights = GetComponentsInChildren<Light>();
            lifeLights = new GameObject[lifeLightsCoreLights.Length];

            for (int i = 0; i < lifeLightsCoreLights.Length; i++)
            {
                lifeLights[i] = lifeLightsCoreLights[i].gameObject;
            }

            player = GameObject.Find("Player").transform;
            playerFront = GameObject.Find("Player Front").transform;
            target = player;
        }

        /* This function is called whenever the game object is activated, so 
         * it is used to reactivate it's components.
         */
        void OnEnable()
        {
            ToggleComponents(true);
        }

        /* This function is used a bit on death and life to turn on and off certain game components,
         * such as the colider, light, and the material
         */
        void ToggleComponents(bool lifeDirection)
        {
            collider.enabled = lifeDirection;
            Material currentMaterial;

            if (lifeDirection)
            {
                currentMaterial = glowingMaterial;
                agent.speed = speed;
            }
            else
            {
                currentMaterial = dimMaterial;
                agent.speed = -speed;
            }

            foreach (Renderer renderer in ownRenderers)
                renderer.sharedMaterial = currentMaterial;

            foreach (GameObject light in lifeLights)
                light.SetActive(lifeDirection);
        }

        /* If the target is in range, set it as the destination.
         * 
         * If the player is looking too far above the enemies (determined with the looking forward object),
         * make them go to the player's position, if not make them go to the front of the player.
         */
        void Update()
        {
            PathFind();
        }

        void PathFind()
        {
            float distance = Vector3.Distance(target.position, transform.position);

            if (distance >= lookRadius) return;

            agent.SetDestination(target.position);

            if (playerFront.position.y > gazeHeight) target = player;
            else target = playerFront;

            LookAtPlayer(distance);
        }

        void LookAtPlayer(float distance)
        {
            if (distance > 4) return;

            var playerPosition = player.transform.position;
            gameObject.transform.LookAt(
                new Vector3(playerPosition.x, gameObject.transform.position.y, playerPosition.z));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, lookRadius);
        }

        /* Anything before the yield happens while the groan is playing,
         * anything after the yield happens after the groan is done.
         * 
         * Here, when the enemy dies, their collider is immediately deactivated, but it is reacctivated
         * when the rest of the enemy is.
         * 
         * 3 Play the dying sound
         * 
         * 4 Wait for it to be done playing
         * 
         * 5 Tally the death
         * 
         * 6 Make the enemy disappear
         */
        /// <summary>
        /// Where the enemy dying is handled.
        /// </summary>
        /// <returns></returns>
        public IEnumerator Die()
        {
            AudioClip deathScream = deathScreams[Random.Range(0, deathScreams.Length)];
            audioSource.PlayOneShot(deathScream);//3
            ToggleComponents(false);
            yield return new WaitForSeconds(deathScream.length);//4
            enemyNum--;//5

            if (Random.value < ammoChance) Instantiate(ammoCrate, transform.position, transform.rotation);
            gameObject.SetActive(false);//6
        }
    }
}