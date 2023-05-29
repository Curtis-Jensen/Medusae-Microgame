using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(NpcController))]
    public class EnemyMobile : MonoBehaviour
    {
        public enum AIState
        {
            Patrol,
            Follow,
            Attack,
        }

        public Animator Animator;

        [Tooltip("Fraction of the enemy's attack range at which it will stop moving towards target while attacking")]
        [Range(0f, 1f)]
        public float AttackStopDistanceRatio = 0.5f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        [Header("Sound")] public AudioClip MovementSound;
        public MinMaxFloat PitchDistortionMovementSpeed;

        public AIState AiState { get; private set; }
        NpcController npcController;
        AudioSource audioSource;

        const string animMoveSpeedParameter = "MoveSpeed";
        const string animAttackParameter = "Attack";
        const string animAlertedParameter = "Alerted";
        const string animOnDamagedParameter = "OnDamaged";

        void Start()
        {
            npcController = GetComponent<NpcController>();
            DebugUtility.HandleErrorIfNullGetComponent<NpcController, EnemyMobile>(npcController, this,
                gameObject);

            npcController.onAttack += OnAttack;
            npcController.onDetectedTarget += OnDetectedTarget;
            npcController.onLostTarget += OnLostTarget;
            npcController.SetPathDestinationToClosestNode();
            npcController.onDamaged += OnDamaged;

            // Start patrolling
            AiState = AIState.Patrol;

            // adding a audio source to play the movement sound on it
            audioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, EnemyMobile>(audioSource, this, gameObject);
            audioSource.clip = MovementSound;
            audioSource.Play();
        }

        void Update()
        {
            UpdateAiStateTransitions();
            UpdateCurrentAiState();
            Debug.Log(npcController.NavMeshAgent);
            float moveSpeed = npcController.NavMeshAgent.velocity.magnitude;

            // Update animator speed parameter
            Animator.SetFloat(animMoveSpeedParameter, moveSpeed);

            // Changing the pitch of the movement sound depending on the movement speed
            audioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max,
                moveSpeed / npcController.NavMeshAgent.speed);
        }

        /// <summary>
        /// Handles state transitions for an AI system, 
        /// transitioning between "Follow" and "Attack" states based on target visibility and range.
        /// </summary>
        void UpdateAiStateTransitions()
        {
            // Handle transitions 
            switch (AiState)
            {
                case AIState.Follow:
                    // Transition to attack when there is a line of sight to the target
                    if (npcController.IsSeeingTarget && npcController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Attack;
                        npcController.SetNavDestination(transform.position);
                    }

                    break;
                case AIState.Attack:
                    // Transition to follow when no longer a target in attack range
                    if (!npcController.IsTargetInAttackRange)
                        AiState = AIState.Follow;

                    break;
            }
        }

        /// <summary>
        /// Updates the current state of an AI system, 
        /// executing specific logic based on the current state such as patrol movement, 
        /// following a detected target while orienting towards it, 
        /// and attacking the target while maintaining a certain distance.
        /// </summary>
        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (AiState)
            {
                case AIState.Patrol:
                    npcController.UpdatePathDestination();
                    npcController.SetNavDestination(npcController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    npcController.SetNavDestination(npcController.KnownDetectedTarget.transform.position);
                    npcController.OrientTowards(npcController.KnownDetectedTarget.transform.position);
                    npcController.OrientWeaponsTowards(npcController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    if (Vector3.Distance(npcController.KnownDetectedTarget.transform.position,
                            npcController.DetectionModule.DetectionSourcePoint.position)
                        >= (AttackStopDistanceRatio * npcController.DetectionModule.AttackRange))
                        npcController.SetNavDestination(npcController.KnownDetectedTarget.transform.position);
                    else
                        npcController.SetNavDestination(transform.position);

                    npcController.OrientTowards(npcController.KnownDetectedTarget.transform.position);
                    npcController.TryAtack(npcController.KnownDetectedTarget.transform.position);
                    break;
            }
        }

        /// <summary>
        /// Triggers the attack animation within the associated Animator component.
        /// </summary>
        void OnAttack()
        {
            Animator.SetTrigger(animAttackParameter);
        }

        /// <summary>
        /// Handles actions when a target is detected by the AI.
        /// It changes the AI state from "Patrol" to "Follow",
        /// plays visual effects, triggers sound effects,
        /// and sets an alerted parameter in the associated Animator component.
        /// </summary>
        void OnDetectedTarget()
        {
            if (AiState == AIState.Patrol)
                AiState = AIState.Follow;

            for (int i = 0; i < OnDetectVfx.Length; i++)
                OnDetectVfx[i].Play();

            if (OnDetectSfx)
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);

            Animator.SetBool(animAlertedParameter, true);
        }

        /// <summary>
        /// Handles actions when the AI loses its target.
        /// If the AI state is either "Follow" or "Attack",
        /// it transitions the AI state to "Patrol".
        /// It stops any ongoing visual effects,
        /// sets the alerted parameter in the associated Animator component to false.
        /// </summary>
        void OnLostTarget()
        {
            if (AiState == AIState.Follow || AiState == AIState.Attack)
                AiState = AIState.Patrol;

            for (int i = 0; i < OnDetectVfx.Length; i++)
                OnDetectVfx[i].Stop();

            Animator.SetBool(animAlertedParameter, false);
        }

        /// <summary>
        /// Performs actions when the AI is damaged.
        /// It plays a random hit spark visual effect from an array,
        /// and triggers the "OnDamaged" animation within the associated Animator component.
        /// </summary>
        void OnDamaged()
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }

            Animator.SetTrigger(animOnDamagedParameter);
        }
    }
}