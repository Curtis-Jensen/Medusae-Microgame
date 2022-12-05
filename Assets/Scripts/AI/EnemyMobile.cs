using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
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
        EnemyController enemyController;
        AudioSource audioSource;

        const string animMoveSpeedParameter = "MoveSpeed";
        const string animAttackParameter = "Attack";
        const string animAlertedParameter = "Alerted";
        const string animOnDamagedParameter = "OnDamaged";

        void Start()
        {
            enemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyMobile>(enemyController, this,
                gameObject);

            enemyController.onAttack += OnAttack;
            enemyController.onDetectedTarget += OnDetectedTarget;
            enemyController.onLostTarget += OnLostTarget;
            enemyController.SetPathDestinationToClosestNode();
            enemyController.onDamaged += OnDamaged;

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

            float moveSpeed = enemyController.NavMeshAgent.velocity.magnitude;

            // Update animator speed parameter
            Animator.SetFloat(animMoveSpeedParameter, moveSpeed);

            // changing the pitch of the movement sound depending on the movement speed
            audioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max,
                moveSpeed / enemyController.NavMeshAgent.speed);
        }

        void UpdateAiStateTransitions()
        {
            // Handle transitions 
            switch (AiState)
            {
                case AIState.Follow:
                    // Transition to attack when there is a line of sight to the target
                    if (enemyController.IsSeeingTarget && enemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Attack;
                        enemyController.SetNavDestination(transform.position);
                    }

                    break;
                case AIState.Attack:
                    // Transition to follow when no longer a target in attack range
                    if (!enemyController.IsTargetInAttackRange)
                    AiState = AIState.Follow;

                    break;
            }
        }

        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (AiState)
            {
                case AIState.Patrol:
                    enemyController.UpdatePathDestination();
                    enemyController.SetNavDestination(enemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    enemyController.SetNavDestination(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientTowards(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsTowards(enemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    if (Vector3.Distance(enemyController.KnownDetectedTarget.transform.position,
                            enemyController.DetectionModule.DetectionSourcePoint.position)
                        >= (AttackStopDistanceRatio * enemyController.DetectionModule.AttackRange))
                    enemyController.SetNavDestination(enemyController.KnownDetectedTarget.transform.position);
                    else
                    enemyController.SetNavDestination(transform.position);

                    enemyController.OrientTowards(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.TryAtack(enemyController.KnownDetectedTarget.transform.position);
                    break;
            }
        }

        void OnAttack()
        {
            Animator.SetTrigger(animAttackParameter);
        }

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

        void OnLostTarget()
        {
            if (AiState == AIState.Follow || AiState == AIState.Attack)
                AiState = AIState.Patrol;

            for (int i = 0; i < OnDetectVfx.Length; i++)
                OnDetectVfx[i].Stop();

            Animator.SetBool(animAlertedParameter, false);
        }

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