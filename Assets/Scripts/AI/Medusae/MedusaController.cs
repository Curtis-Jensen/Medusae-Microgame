using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.FPS.Game
{
    public class MedusaController : MonoBehaviour
    {
        #region Variables
        public float lookRadius = 10f;
        [Tooltip("Used to determine how high the player can look before they are determined to be looking above the medusae")]
        public float gazeHeight;

        NavMeshAgent agent;
        Transform target; //Whatever is the intended destination of the enemy at the moment
        Transform player; //Player 1, used for targeting
        Transform playerFront; //The front of the player.  Used for targeting
        [Tooltip("The number of enemies alive in the wave.")]
        public static int enemyNum = 1;
        #endregion

        /* Just getting a bunch of components
         * 
         * The life lights are actually the game objects that house the lights,
         * so a bit more work needs to be done to turn those off.
         */
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            player = GameObject.Find("Player").transform;
            playerFront = GameObject.Find("Player Front").transform;
            target = player;
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

            Debug.Log("The player is not too far away");

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
    }
}