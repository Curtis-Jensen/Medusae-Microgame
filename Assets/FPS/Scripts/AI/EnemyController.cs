using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        #region struct RenderIndexData
        [System.Serializable]
        public struct RendererIndexData
        {
            public Renderer Renderer;
            public int MaterialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                Renderer = renderer;
                MaterialIndex = index;
            }
        }
        #endregion

        #region Global🌎Variables
        [Header("Parameters")]
        [Tooltip("The Y height at which the enemy will be automatically killed (if it falls off of the level)")]
        public float selfDestructYHeight = -20f;

        [Tooltip("The distance at which the enemy considers that it has reached its current path destination point")]
        public float pathReachingRadius = 2f;

        [Tooltip("The speed at which the enemy rotates")]
        public float orientationSpeed = 10f;

        [Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
        public float deathDuration = 0f;


        [Header("Weapons Parameters")] [Tooltip("Allow weapon swapping for this enemy")]
        public bool swapToNextWeapon = false;

        [Tooltip("Time delay between a weapon swap and the next attack")]
        public float delayAfterWeaponSwap = 0f;

        [Header("Eye color")] [Tooltip("Material for the eye color")]
        public Material eyeColorMaterial;

        [Tooltip("The default color of the bot's eye")] [ColorUsageAttribute(true, true)]
        public Color defaultEyeColor;

        [Tooltip("The attack color of the bot's eye")] [ColorUsageAttribute(true, true)]
        public Color attackEyeColor;

        [Header("Flash on hit")] [Tooltip("The material used for the body of the hoverbot")]
        public Material bodyMaterial;

        [Tooltip("The gradient representing the color of the flash on hit")] [GradientUsageAttribute(true)]
        public Gradient onHitBodyGradient;

        [Tooltip("The duration of the flash on hit")]
        public float flashOnHitDuration = 0.5f;

        [Header("Sounds")] [Tooltip("Sound played when recieving damages")]
        public AudioClip damageTick;

        [Header("VFX")] [Tooltip("The VFX prefab spawned when the enemy dies")]
        public GameObject deathVfx;

        [Tooltip("The point at which the death VFX is spawned")]
        public Transform deathVfxSpawnPoint;

        [Header("Loot")] [Tooltip("The object this enemy can drop when dying")]
        public GameObject lootPrefab;

        [Tooltip("The chance the object has to drop")] [Range(0, 1)]
        public float dropRate = 1f;

        [Header("Debug Display")] [Tooltip("Color of the sphere gizmo representing the path reaching range")]
        public Color pathReachingRangeColor = Color.yellow;

        [Tooltip("Color of the sphere gizmo representing the attack range")]
        public Color attackRangeColor = Color.red;

        [Tooltip("Color of the sphere gizmo representing the detection range")]
        public Color detectionRangeColor = Color.blue;

        public UnityAction onAttack;
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;
        public UnityAction onDamaged;

        List<RendererIndexData> bodyRenderers = new List<RendererIndexData>();
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;
        float lastTimeDamaged = float.NegativeInfinity;

        RendererIndexData eyeRendererData;
        MaterialPropertyBlock eyeColorMaterialPropertyBlock;

        public PatrolPath patrolPath { get; set; }
        //This used to be below all these other things but I moved it above what is accessing it
        public DetectionModule detectionModule { get; private set; }
        public GameObject knownDetectedTarget => detectionModule.KnownDetectedTarget;
        public bool isTargetInAttackRange => detectionModule.IsTargetInAttackRange;
        public bool isSeeingTarget => detectionModule.IsSeeingTarget;
        public bool hadKnownTarget => detectionModule.HadKnownTarget;
        public NavMeshAgent navMeshAgent { get; private set; }

        int pathDestinationNodeIndex;
        EnemyManager enemyManager;
        ActorsManager actorsManager;
        Health health;
        Actor actor;
        Collider[] selfColliders;
        GameFlowManager gameFlowManager;
        bool wasDamagedThisFrame;
        float lastTimeWeaponSwapped = Mathf.NegativeInfinity;
        int currentWeaponIndex;
        WeaponController currentWeapon;
        WeaponController[] weapons;
        NavigationModule navigationModule;
        #endregion

        #region Unity Monobehavior Methods
        /* Makes sure a bunch of different components are present on the enemy game object.
         */
        void Start()
        {
            enemyManager = FindObjectOfType<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(enemyManager, this);

            actorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(actorsManager, this);

            enemyManager.RegisterEnemy(this);

            health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(health, this, gameObject);

            actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(actor, this, gameObject);

            navMeshAgent = GetComponent<NavMeshAgent>();
            selfColliders = GetComponentsInChildren<Collider>();

            gameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, EnemyController>(gameFlowManager, this);

            // Subscribe to damage & death actions
            health.OnDie += OnDie;
            health.OnDamaged += OnDamaged;

            // Find and initialize all weapons
            FindAndInitializeAllWeapons();
            var weapon = GetCurrentWeapon();
            weapon.ShowWeapon(true);

            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(detectionModules.Length, this,
                gameObject);
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
                this, gameObject);
            // Initialize detection module
            detectionModule = detectionModules[0];
            detectionModule.onDetectedTarget += OnDetectedTarget;
            detectionModule.onLostTarget += OnLostTarget;
            onAttack += detectionModule.OnAttack;

            var navigationModules = GetComponentsInChildren<NavigationModule>();
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
                this, gameObject);
            // Override navmesh agent data
            if (navigationModules.Length > 0)
            {
                navigationModule = navigationModules[0];
                navMeshAgent.speed = navigationModule.MoveSpeed;
                navMeshAgent.angularSpeed = navigationModule.AngularSpeed;
                navMeshAgent.acceleration = navigationModule.Acceleration;
            }

            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == eyeColorMaterial)
                    {
                        eyeRendererData = new RendererIndexData(renderer, i);
                    }

                    if (renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderers.Add(new RendererIndexData(renderer, i));
                    }
                }
            }

            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

            // Check if we have an eye renderer for this enemy
            if (eyeRendererData.Renderer != null)
            {
                eyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                eyeRendererData.Renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                    eyeRendererData.MaterialIndex);
            }
        }

        void Update()
        {
            EnsureIsWithinLevelBounds();
            detectionModule.HandleTargetDetection(actor, selfColliders);

            Color currentColor = onHitBodyGradient.Evaluate((Time.time - lastTimeDamaged) / flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in bodyRenderers)
            {
                data.Renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.MaterialIndex);
            }

            wasDamagedThisFrame = false;
        }

        void OnDrawGizmosSelected()
        {
            // Path reaching range
            Gizmos.color = pathReachingRangeColor;
            Gizmos.DrawWireSphere(transform.position, pathReachingRadius);

            if (detectionModule != null)
            {
                // Detection range
                Gizmos.color = detectionRangeColor;
                Gizmos.DrawWireSphere(transform.position, detectionModule.DetectionRange);

                // Attack range
                Gizmos.color = attackRangeColor;
                Gizmos.DrawWireSphere(transform.position, detectionModule.AttackRange);
            }
        }
        #endregion

        #region🚦Pathfinding
        void EnsureIsWithinLevelBounds()
        {
            // at every frame, this tests for conditions to kill the enemy
            if (transform.position.y < selfDestructYHeight)
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnLostTarget()
        {
            onLostTarget.Invoke();

            // Set the eye attack color and property block if the eye renderer is set
            if (eyeRendererData.Renderer != null)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                eyeRendererData.Renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                    eyeRendererData.MaterialIndex);
            }
        }

        void OnDetectedTarget()
        {
            onDetectedTarget.Invoke();

            // Set the eye default color and property block if the eye renderer is set
            if (eyeRendererData.Renderer != null)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", attackEyeColor);
                eyeRendererData.Renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                    eyeRendererData.MaterialIndex);
            }
        }

        public void OrientTowards(Vector3 lookPosition)
        {
            Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude != 0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * orientationSpeed);
            }
        }

        bool IsPathValid()
        {
            return patrolPath && patrolPath.PathNodes.Count > 0;
        }

        public void ResetPathDestination()
        {
            pathDestinationNodeIndex = 0;
        }

        public void SetPathDestinationToClosestNode()
        {
            if (IsPathValid())
            {
                int closestPathNodeIndex = 0;
                for (int i = 0; i < patrolPath.PathNodes.Count; i++)
                {
                    float distanceToPathNode = patrolPath.GetDistanceToNode(transform.position, i);
                    if (distanceToPathNode < patrolPath.GetDistanceToNode(transform.position, closestPathNodeIndex))
                    {
                        closestPathNodeIndex = i;
                    }
                }

                pathDestinationNodeIndex = closestPathNodeIndex;
            }
            else
            {
                pathDestinationNodeIndex = 0;
            }
        }

        public Vector3 GetDestinationOnPath()
        {
            if (IsPathValid())
            {
                return patrolPath.GetPositionOfPathNode(pathDestinationNodeIndex);
            }
            else
            {
                return transform.position;
            }
        }

        public void SetNavDestination(Vector3 destination)
        {
            if (navMeshAgent)
            {
                navMeshAgent.SetDestination(destination);
            }
        }

        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathValid())
            {
                // Check if reached the path destination
                if ((transform.position - GetDestinationOnPath()).magnitude <= pathReachingRadius)
                {
                    // increment path destination index
                    pathDestinationNodeIndex =
                        inverseOrder ? (pathDestinationNodeIndex - 1) : (pathDestinationNodeIndex + 1);
                    if (pathDestinationNodeIndex < 0)
                    {
                        pathDestinationNodeIndex += patrolPath.PathNodes.Count;
                    }

                    if (pathDestinationNodeIndex >= patrolPath.PathNodes.Count)
                    {
                        pathDestinationNodeIndex -= patrolPath.PathNodes.Count;
                    }
                }
            }
        }
        #endregion

        #region➕Health
        void OnDamaged(float damage, GameObject damageSource)
        {
            // test if the damage source is the player
            if (damageSource && !damageSource.GetComponent<EnemyController>())
            {
                // pursue the player
                detectionModule.OnDamaged(damageSource);
                
                onDamaged?.Invoke();
                lastTimeDamaged = Time.time;
            
                // play the damage tick sound
                if (damageTick && !wasDamagedThisFrame)
                    AudioUtility.CreateSFX(damageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);
            
                wasDamagedThisFrame = true;
            }
        }

        void OnDie()
        {
            // spawn a particle system when dying
            var vfx = Instantiate(deathVfx, deathVfxSpawnPoint.position, Quaternion.identity);
            Destroy(vfx, 5f);

            // tells the game flow manager to handle the enemy destuction
            enemyManager.UnregisterEnemy(this);

            // loot an object
            if (TryDropItem())
            {
                Instantiate(lootPrefab, transform.position, Quaternion.identity);
            }

            // this will call the OnDestroy function
            Destroy(gameObject, deathDuration);
        }
        #endregion

        public void OrientWeaponsTowards(Vector3 lookPosition)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                // orient weapon towards player
                Vector3 weaponForward = (lookPosition - weapons[i].WeaponRoot.transform.position).normalized;
                weapons[i].transform.forward = weaponForward;
            }
        }

        public bool TryAtack(Vector3 enemyPosition)
        {
            if (gameFlowManager.GameIsEnding)
                return false;

            OrientWeaponsTowards(enemyPosition);

            if ((lastTimeWeaponSwapped + delayAfterWeaponSwap) >= Time.time)
                return false;

            // Shoot the weapon
            bool didFire = GetCurrentWeapon().HandleShootInputs(inputDown: false, inputHeld: true, inputUp: false);

            if (didFire && onAttack != null)
            {
                onAttack.Invoke();

                if (swapToNextWeapon && weapons.Length > 1)
                {
                    int nextWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
                    SetCurrentWeapon(nextWeaponIndex);
                }
            }

            return didFire;
        }

        /* If there is no loot to drop, don't try to drop it
         * 
         * What's up with this drop rate of 0 or 1 stuff?  Seems a bit redundant.
         */
        public bool TryDropItem()
        {
            if (dropRate == 0 || lootPrefab == null)
                return false;
            else if (dropRate == 1)
                return true;
            else
                return (Random.value <= dropRate);
        }

        /* 1 Check if we already found and initialized the weapons
         */
        void FindAndInitializeAllWeapons()
        {
            if (weapons != null) return;//1

            weapons = GetComponentsInChildren<WeaponController>();
                DebugUtility.HandleErrorIfNoComponentFound<WeaponController, EnemyController>(weapons.Length, this,
                    gameObject);

                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i].Owner = gameObject;
                }
           
        }

        public WeaponController GetCurrentWeapon()
        {
            FindAndInitializeAllWeapons();
            // Check if no weapon is currently selected
            if (currentWeapon == null)
            {
                // Set the first weapon of the weapons list as the current weapon
                SetCurrentWeapon(0);
            }

            DebugUtility.HandleErrorIfNullGetComponent<WeaponController, EnemyController>(currentWeapon, this,
                gameObject);

            return currentWeapon;
        }

        void SetCurrentWeapon(int index)
        {
            currentWeaponIndex = index;
            currentWeapon = weapons[currentWeaponIndex];
            if (swapToNextWeapon)
            {
                lastTimeWeaponSwapped = Time.time;
            }
            else
            {
                lastTimeWeaponSwapped = Mathf.NegativeInfinity;
            }
        }
    }
}