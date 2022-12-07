using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    //Contains properties and behaviors that only the standard projectile has
    public class ProjectileStandard : ProjectileBase
    {
        [Header("General")] [Tooltip("Radius of this projectile's collision detection")]
        public float Radius = 0.01f;

        [Tooltip("Transform representing the root of the projectile (used for accurate collision detection)")]
        public Transform Root;

        [Tooltip("Transform representing the tip of the projectile (used for accurate collision detection)")]
        public Transform Tip;

        [Tooltip("LifeTime of the projectile")]
        public float MaxLifeTime = 5f;

        [Tooltip("VFX prefab to spawn upon impact")]
        public GameObject ImpactVfx;

        [Tooltip("LifeTime of the VFX before being destroyed")]
        public float ImpactVfxLifetime = 5f;

        [Tooltip("Offset along the hit normal where the VFX will be spawned")]
        public float ImpactVfxSpawnOffset = 0.1f;

        [Tooltip("Clip to play on impact")] 
        public AudioClip ImpactSfxClip;

        [Tooltip("Layers this projectile can collide with")]
        public LayerMask HittableLayers = -1;

        [Header("Movement")] [Tooltip("Speed of the projectile")]
        public float Speed = 20f;

        [Tooltip("Downward acceleration from gravity")]
        public float GravityDownAcceleration = 0f;

        [Tooltip("Distance over which the projectile will correct its course to fit the intended trajectory (used to drift projectiles towards center of screen in First Person view). At values under 0, there is no correction")]
        public float TrajectoryCorrectionDistance = -1;

        [Tooltip("Determines if the projectile inherits the velocity that the weapon's muzzle had when firing")]
        public bool InheritWeaponVelocity = false;

        [Header("Damage")] [Tooltip("Damage of the projectile")]
        public float Damage = 40f;

        [Tooltip("Area of damage. Keep empty if you don<t want area damage")]
        public DamageArea AreaOfDamage;

        [Header("Debug")] [Tooltip("Color of the projectile radius debug view")]
        public Color RadiusColor = Color.cyan * 0.2f;

        ProjectileBase projectileBase;
        Vector3 lastRootPosition;
        Vector3 velocity;
        bool hasTrajectoryOverride;
        Vector3 trajectoryCorrectionVector;
        Vector3 consumedTrajectoryCorrectionVector;
        List<Collider> ignoredColliders;

        const QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileStandard>(projectileBase, this,
                gameObject);

            projectileBase.onShoot += OnShoot;

            Destroy(gameObject, MaxLifeTime);
        }

        void OnShoot()
        {
            lastRootPosition = Root.position;
            velocity = transform.forward * Speed; // Determines the direction and speed the projectile will go
            ignoredColliders = new List<Collider>();
            transform.position += projectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            // Ignore colliders of owner
            Collider[] ownerColliders = projectileBase.Owner.GetComponentsInChildren<Collider>();
            ignoredColliders.AddRange(ownerColliders);

            // Handle case of player shooting (make projectiles not go through walls, and remember center-of-screen trajectory)
            PlayerWeaponsManager playerWeaponsManager = projectileBase.Owner.GetComponent<PlayerWeaponsManager>();
            if (!playerWeaponsManager) return;

            hasTrajectoryOverride = true;

            Vector3 cameraToMuzzle = (projectileBase.InitialPosition -
                                        playerWeaponsManager.WeaponCamera.transform.position);

            trajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle,
                playerWeaponsManager.WeaponCamera.transform.forward);
            if (TrajectoryCorrectionDistance == 0)
            {
                transform.position += trajectoryCorrectionVector;
                consumedTrajectoryCorrectionVector = trajectoryCorrectionVector;
            }
            else if (TrajectoryCorrectionDistance < 0)
                hasTrajectoryOverride = false;

            if (Physics.Raycast(playerWeaponsManager.WeaponCamera.transform.position, cameraToMuzzle.normalized,
                out RaycastHit hit, cameraToMuzzle.magnitude, HittableLayers, triggerInteraction))
                if (IsHitValid(hit))
                    OnHit(hit.point, hit.normal, hit.collider);
        }

        void Update()
        {
            // Move
            transform.position += velocity * Time.deltaTime;
            if (InheritWeaponVelocity)
                transform.position += projectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            // Drift towards trajectory override (this is so that projectiles can be centered 
            // with the camera center even though the actual weapon is offset)
            if (hasTrajectoryOverride && consumedTrajectoryCorrectionVector.sqrMagnitude <
                trajectoryCorrectionVector.sqrMagnitude)
            {
                Vector3 correctionLeft = trajectoryCorrectionVector - consumedTrajectoryCorrectionVector;
                float distanceThisFrame = (Root.position - lastRootPosition).magnitude;
                Vector3 correctionThisFrame =
                    (distanceThisFrame / TrajectoryCorrectionDistance) * trajectoryCorrectionVector;
                correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
                consumedTrajectoryCorrectionVector += correctionThisFrame;

                // Detect end of correction
                if (consumedTrajectoryCorrectionVector.sqrMagnitude == trajectoryCorrectionVector.sqrMagnitude)
                    hasTrajectoryOverride = false;

                transform.position += correctionThisFrame;
            }

            // Orient towards velocity
            transform.forward = velocity.normalized;

            // Gravity
            if (GravityDownAcceleration > 0)
                // add gravity to the projectile velocity for ballistic effect
                velocity += Vector3.down * GravityDownAcceleration * Time.deltaTime;

            // Hit detection
            {
                RaycastHit closestHit = new RaycastHit();
                closestHit.distance = Mathf.Infinity;
                bool foundHit = false;

                // Sphere cast
                Vector3 displacementSinceLastFrame = Tip.position - lastRootPosition;
                RaycastHit[] hits = Physics.SphereCastAll(lastRootPosition, Radius,
                    displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, HittableLayers,
                    triggerInteraction);
                foreach (var hit in hits)
                {
                    if (IsHitValid(hit) && hit.distance < closestHit.distance)
                    {
                        foundHit = true;
                        closestHit = hit;
                    }
                }

                if (foundHit)
                {
                    // Handle case of casting while already inside a collider
                    if (closestHit.distance <= 0f)
                    {
                        closestHit.point = Root.position;
                        closestHit.normal = -transform.forward;
                    }

                    OnHit(closestHit.point, closestHit.normal, closestHit.collider);
                }
            }

            lastRootPosition = Root.position;
        }

        bool IsHitValid(RaycastHit hit)
        {
            // ignore hits with an ignore component
            if (hit.collider.GetComponent<IgnoreHitDetection>())
            return false;

            // ignore hits with triggers that don't have a Damageable component
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null)
            return false;

            // ignore hits with specific ignored colliders (self colliders, by default)
            if (ignoredColliders != null && ignoredColliders.Contains(hit.collider))
                return false;

            return true;
        }

        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            // damage
            if (AreaOfDamage)
            AreaOfDamage.InflictDamageInArea(Damage, point, HittableLayers, triggerInteraction, projectileBase.Owner);
            else
            {
                // point damage
                Damageable damageable = collider.GetComponent<Damageable>();
                if (damageable)
                    damageable.InflictDamage(Damage, false, projectileBase.Owner);
            }

            // impact vfx
            if (ImpactVfx)
            {
                GameObject impactVfxInstance = Instantiate(ImpactVfx, point + (normal * ImpactVfxSpawnOffset),
                    Quaternion.LookRotation(normal));
                if (ImpactVfxLifetime > 0)
                    Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
            }

            // impact sfx
            if (ImpactSfxClip)
                AudioUtility.CreateSFX(ImpactSfxClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);

            // Self Destruct
            Destroy(this.gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = RadiusColor;
            Gizmos.DrawSphere(transform.position, Radius);
        }
    }
}