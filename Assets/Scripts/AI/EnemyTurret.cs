using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyTurret : MonoBehaviour
    {
        public enum AIState
        {
            Idle,
            Attack,
        }

        public Transform TurretPivot;
        public Transform TurretAimPoint;
        public Animator Animator;
        public float AimRotationSharpness = 5f;
        public float LookAtRotationSharpness = 2.5f;
        public float DetectionFireDelay = 1f;
        public float AimingTransitionBlendTime = 1f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        public AIState AiState { get; private set; }

        EnemyController enemyController;
        Health health;
        Quaternion rotationWeaponForwardToPivot;
        float timeStartedDetection;
        float timeLostDetection;
        Quaternion previousPivotAimingRotation;
        Quaternion pivotAimingRotation;

        const string animOnDamagedParameter = "OnDamaged";
        const string animIsActiveParameter = "IsActive";

        void Start()
        {
            health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyTurret>(health, this, gameObject);
            health.OnDamaged += OnDamaged;

            enemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyTurret>(enemyController, this,
                gameObject);

            enemyController.onDetectedTarget += OnDetectedTarget;
            enemyController.onLostTarget += OnLostTarget;

            // Remember the rotation offset between the pivot's forward and the weapon's forward
            rotationWeaponForwardToPivot =
                Quaternion.Inverse(enemyController.GetCurrentWeapon().WeaponMuzzle.rotation) * TurretPivot.rotation;

            // Start with idle
            AiState = AIState.Idle;

            timeStartedDetection = Mathf.NegativeInfinity;
            previousPivotAimingRotation = TurretPivot.rotation;
        }

        void Update()
        {
            UpdateCurrentAiState();
        }

        void LateUpdate()
        {
            UpdateTurretAiming();
        }

        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (AiState)
            {
                case AIState.Attack:
                    bool mustShoot = Time.time > timeStartedDetection + DetectionFireDelay;
                    // Calculate the desired rotation of our turret (aim at target)
                    Vector3 directionToTarget =
                        (enemyController.KnownDetectedTarget.transform.position - TurretAimPoint.position).normalized;
                    Quaternion offsettedTargetRotation =
                        Quaternion.LookRotation(directionToTarget) * rotationWeaponForwardToPivot;
                    pivotAimingRotation = Quaternion.Slerp(previousPivotAimingRotation, offsettedTargetRotation,
                        (mustShoot ? AimRotationSharpness : LookAtRotationSharpness) * Time.deltaTime);

                    // shoot
                    if (mustShoot)
                    {
                        Vector3 correctedDirectionToTarget =
                            (pivotAimingRotation * Quaternion.Inverse(rotationWeaponForwardToPivot)) *
                            Vector3.forward;

                        enemyController.TryAtack(TurretAimPoint.position + correctedDirectionToTarget);
                    }

                    break;
            }
        }

        void UpdateTurretAiming()
        {
            switch (AiState)
            {
                case AIState.Attack:
                    TurretPivot.rotation = pivotAimingRotation;
                    break;
                default:
                    // Use the turret rotation of the animation
                    TurretPivot.rotation = Quaternion.Slerp(pivotAimingRotation, TurretPivot.rotation,
                        (Time.time - timeLostDetection) / AimingTransitionBlendTime);
                    break;
            }

            previousPivotAimingRotation = TurretPivot.rotation;
        }

        void OnDamaged(float dmg, GameObject source)
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }

            Animator.SetTrigger(animOnDamagedParameter);
        }

        void OnDetectedTarget()
        {
            if (AiState == AIState.Idle)
            {
                AiState = AIState.Attack;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Play();
            }

            if (OnDetectSfx)
            {
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            }

            Animator.SetBool(animIsActiveParameter, true);
            timeStartedDetection = Time.time;
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Attack)
            {
                AiState = AIState.Idle;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Stop();
            }

            Animator.SetBool(animIsActiveParameter, false);
            timeLostDetection = Time.time;
        }
    }
}