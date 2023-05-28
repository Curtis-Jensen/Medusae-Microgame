using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class NpcController : MonoBehaviour
    {
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

        [Header("Parameters")]
        [Tooltip("The Y height at which the enemy will be automatically killed (if it falls off of the level)")]
        public float SelfDestructYHeight = -20f;

        [Tooltip("The distance at which the enemy considers that it has reached its current path destination point")]
        public float PathReachingRadius = 2f;

        [Tooltip("The speed at which the enemy rotates")]
        public float OrientationSpeed = 10f;

        [Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
        public float DeathDuration = 0f;


        [Header("Weapons Parameters")]
        [Tooltip("Allow weapon swapping for this enemy")]
        public bool SwapToNextWeapon = false;

        [Tooltip("Time delay between a weapon swap and the next attack")]
        public float DelayAfterWeaponSwap = 0f;

        [Header("Eye color")]
        [Tooltip("Material for the eye color")]
        public Material EyeColorMaterial;

        [Tooltip("The default color of the bot's eye")]
        [ColorUsageAttribute(true, true)]
        public Color DefaultEyeColor;

        [Tooltip("The attack color of the bot's eye")]
        [ColorUsageAttribute(true, true)]
        public Color AttackEyeColor;

        [Header("Flash on hit")]
        [Tooltip("The material used for the body of the hoverbot")]
        public Material BodyMaterial;

        [Tooltip("The gradient representing the color of the flash on hit")]
        [GradientUsageAttribute(true)]
        public Gradient OnHitBodyGradient;

        [Tooltip("The duration of the flash on hit")]
        public float FlashOnHitDuration = 0.5f;

        [Header("Sounds")]
        [Tooltip("Sound played when recieving damages")]
        public AudioClip DamageTick;

        [Header("VFX")]
        [Tooltip("The VFX prefab spawned when the enemy dies")]
        public GameObject DeathVfx;

        [Tooltip("The point at which the death VFX is spawned")]
        public Transform DeathVfxSpawnPoint;

        [Header("Loot")]
        [Tooltip("The object this enemy can drop when dying")]
        public GameObject LootPrefab;

        [Tooltip("The chance the object has to drop")]
        [Range(0, 1)]
        public float DropRate = 1f;

        [Header("Debug Display")]
        [Tooltip("Color of the sphere gizmo representing the path reaching range")]
        public Color PathReachingRangeColor = Color.yellow;

        [Tooltip("Color of the sphere gizmo representing the attack range")]
        public Color AttackRangeColor = Color.red;

        [Tooltip("Color of the sphere gizmo representing the detection range")]
        public Color DetectionRangeColor = Color.blue;

        //These UnityActions are a list of methods that get called at certain times.
        //They are not implemented for the medusa yet so there are null checks
        public UnityAction onAttack;
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;
        public UnityAction onDamaged;

        List<RendererIndexData> bodyRenderers = new List<RendererIndexData>();
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;
        float lastTimeDamaged = float.NegativeInfinity;

        RendererIndexData eyeRendererData;
        MaterialPropertyBlock eyeColorMaterialPropertyBlock;

        public PatrolPath PatrolPath { get; set; }
        public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;
        public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;
        public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
        public bool HadKnownTarget => DetectionModule.HadKnownTarget;
        public NavMeshAgent NavMeshAgent { get; private set; }
        public DetectionModule DetectionModule { get; private set; }

        int pathDestinationNodeIndex;
        ActorsManager actorsManager;
        protected Health health;
        Actor actor;
        Collider[] selfColliders;
        GameFlowManager gameFlowManager;
        bool wasDamagedThisFrame;
        float lastTimeWeaponSwapped = Mathf.NegativeInfinity;
        int currentWeaponIndex;
        GunController currentWeapon;
        GunController[] weapons;
        NavigationModule navigationModule;

        protected void Start()
        {
            actorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, NpcController>(actorsManager, this);

            health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, NpcController>(health, this, gameObject);

            actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, NpcController>(actor, this, gameObject);

            NavMeshAgent = GetComponent<NavMeshAgent>();
            selfColliders = GetComponentsInChildren<Collider>();

            gameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, NpcController>(gameFlowManager, this);

            // Subscribe to damage & death actions
            health.OnDie += OnDie;
            health.OnDamaged += OnDamaged;

            // Find and initialize all weapons
            FindAndInitializeAllWeapons();
            var weapon = GetCurrentWeapon();
            weapon.ShowWeapon(true);

            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, NpcController>(detectionModules.Length, this,
                gameObject);
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, NpcController>(detectionModules.Length,
                this, gameObject);
            // Initialize detection module
            DetectionModule = detectionModules[0];
            DetectionModule.onDetectedTarget += OnDetectedTarget;
            DetectionModule.onLostTarget += OnLostTarget;
            onAttack += DetectionModule.OnAttack;

            var navigationModules = GetComponentsInChildren<NavigationModule>();
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, NpcController>(detectionModules.Length,
                this, gameObject);
            // Override navmesh agent data
            if (navigationModules.Length > 0)
            {
                navigationModule = navigationModules[0];
                NavMeshAgent.speed = navigationModule.MoveSpeed;
                NavMeshAgent.angularSpeed = navigationModule.AngularSpeed;
                NavMeshAgent.acceleration = navigationModule.Acceleration;
            }

            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == EyeColorMaterial)
                    {
                        eyeRendererData = new RendererIndexData(renderer, i);
                    }

                    if (renderer.sharedMaterials[i] == BodyMaterial)
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
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
                eyeRendererData.Renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                    eyeRendererData.MaterialIndex);
            }
        }

        void Update()
        {
            EnsureIsWithinLevelBounds();
            DetectionModule.HandleTargetDetection(actor, selfColliders);

            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - lastTimeDamaged) / FlashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in bodyRenderers)
                data.Renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.MaterialIndex);

            wasDamagedThisFrame = false;
        }

        #region🌎Navigation
        void EnsureIsWithinLevelBounds()
        {
            // at every frame, this tests for conditions to kill the enemy
            if (transform.position.y < SelfDestructYHeight)
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnLostTarget()
        {
            if (onLostTarget == null)
            {
                Debug.LogWarning("EnemyController.onLostTarget is not set.  This is likely because EnemyController has not been fully implemented.");
                return;
            }
            onLostTarget.Invoke();

            // Set the eye attack color and property block if the eye renderer is set
            if (eyeRendererData.Renderer != null)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
                eyeRendererData.Renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                    eyeRendererData.MaterialIndex);
            }
        }

        void OnDetectedTarget()
        {
            if (onDetectedTarget == null)
            {
                Debug.LogWarning("NpcController.onDetectedTarget is not set.  This is likely because EnemyController has not been fully implemented.");
                return;
            }
            onDetectedTarget.Invoke();

            // If there's a big eye set the eye default color and property block if the eye renderer is set
            if (eyeRendererData.Renderer != null)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", AttackEyeColor);
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
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * OrientationSpeed);
            }
        }

        bool IsPathValid()
        {
            return PatrolPath && PatrolPath.pathNodes.Length > 0;
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
                for (int i = 0; i < PatrolPath.pathNodes.Length; i++)
                {
                    float distanceToPathNode = PatrolPath.GetDistanceToNode(transform.position, i);
                    if (distanceToPathNode < PatrolPath.GetDistanceToNode(transform.position, closestPathNodeIndex))
                        closestPathNodeIndex = i;
                }

                pathDestinationNodeIndex = closestPathNodeIndex;
            }
            else
                pathDestinationNodeIndex = 0;
        }

        public Vector3 GetDestinationOnPath()
        {
            if (IsPathValid())
                return PatrolPath.GetPositionOfPathNode(pathDestinationNodeIndex);
            else
                return transform.position;
        }

        public void SetNavDestination(Vector3 destination)
        {
            if (NavMeshAgent)
            {
                NavMeshAgent.SetDestination(destination);
            }
        }

        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathValid())
            {
                // Check if reached the path destination
                if ((transform.position - GetDestinationOnPath()).magnitude <= PathReachingRadius)
                {
                    // increment path destination index
                    pathDestinationNodeIndex =
                        inverseOrder ? (pathDestinationNodeIndex - 1) : (pathDestinationNodeIndex + 1);
                    if (pathDestinationNodeIndex < 0)
                    {
                        pathDestinationNodeIndex += PatrolPath.pathNodes.Length;
                    }

                    if (pathDestinationNodeIndex >= PatrolPath.pathNodes.Length)
                    {
                        pathDestinationNodeIndex -= PatrolPath.pathNodes.Length;
                    }
                }
            }
        }

        public void OrientWeaponsTowards(Vector3 lookPosition)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                // orient weapon towards player
                Vector3 weaponForward = (lookPosition - weapons[i].WeaponRoot.transform.position).normalized;
                weapons[i].transform.forward = weaponForward;
            }
        }
        #endregion

        #region🔫Combat
        void OnDamaged(float damage, GameObject damageSource)
        {
            // test if the damage source is the player
            if (damageSource && !damageSource.GetComponent<NpcController>())
            {
                // pursue the player
                DetectionModule.OnDamaged(damageSource);

                onDamaged?.Invoke();
                lastTimeDamaged = Time.time;

                // play the damage tick sound
                if (DamageTick && !wasDamagedThisFrame)
                    AudioUtility.CreateSFX(DamageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);

                wasDamagedThisFrame = true;
            }
        }

        /* 1 spawn a particle system when dying inside the particle container
         * 
         * 3 loot an object
         * 
         * 4 this will call the OnDestroy function
         */
        protected void OnDie()
        {
            var particleContainter = GameObject.Find("Particle Container").transform;
            var vfx = Instantiate
                (DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity, particleContainter);//1
            Destroy(vfx, 5f);

            var level = GameObject.Find("Level").transform;
            if (TryDropItem())//3
                Instantiate(LootPrefab, transform.position, Quaternion.identity, level);

            Destroy(gameObject, DeathDuration);//4
        }

        void OnDrawGizmosSelected()
        {
            // Path reaching range
            Gizmos.color = PathReachingRangeColor;
            Gizmos.DrawWireSphere(transform.position, PathReachingRadius);

            if (DetectionModule == null) return;
            // Detection range
            Gizmos.color = DetectionRangeColor;
            Gizmos.DrawWireSphere(transform.position, DetectionModule.DetectionRange);

            // Attack range
            Gizmos.color = AttackRangeColor;
            Gizmos.DrawWireSphere(transform.position, DetectionModule.AttackRange);
        }

        public bool TryAtack(Vector3 enemyPosition)
        {
            if (gameFlowManager.GameIsEnding)
                return false;

            OrientWeaponsTowards(enemyPosition);

            if ((lastTimeWeaponSwapped + DelayAfterWeaponSwap) >= Time.time)
                return false;

            // Shoot the weapon
            bool didFire = GetCurrentWeapon().HandleAttackInputs(false, true, false);

            if (didFire && onAttack != null)
            {
                onAttack.Invoke();

                if (SwapToNextWeapon && weapons.Length > 1)
                {
                    int nextWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
                    SetCurrentWeapon(nextWeaponIndex);
                }
            }

            return didFire;
        }

        public bool TryDropItem()
        {
            if (DropRate == 0 || LootPrefab == null)
                return false;
            else if (DropRate == 1)
                return true;
            else
                return (UnityEngine.Random.value <= DropRate);
        }

        void FindAndInitializeAllWeapons()
        {
            // Check if we already found and initialized the weapons
            if (weapons == null)
            {
                weapons = GetComponentsInChildren<GunController>();
                DebugUtility.HandleErrorIfNoComponentFound<GunController, NpcController>(weapons.Length, this,
                    gameObject);

                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i].owner = gameObject;
                }
            }
        }

        public GunController GetCurrentWeapon()
        {
            FindAndInitializeAllWeapons();
            // Check if no weapon is currently selected
            if (currentWeapon == null)
                // Set the first weapon of the weapons list as the current weapon
                SetCurrentWeapon(0);

            DebugUtility.HandleErrorIfNullGetComponent<GunController, NpcController>(currentWeapon, this,
                gameObject);

            return currentWeapon;
        }

        void SetCurrentWeapon(int index)
        {
            currentWeaponIndex = index;
            currentWeapon = weapons[currentWeaponIndex];
            if (SwapToNextWeapon)
            {
                lastTimeWeaponSwapped = Time.time;
            }
            else
            {
                lastTimeWeaponSwapped = Mathf.NegativeInfinity;
            }
        }
        #endregion
    }
}