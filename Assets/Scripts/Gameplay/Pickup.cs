using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        [Tooltip("Frequency at which the item will move up and down")]
        public float VerticalBobFrequency = 1f;

        [Tooltip("Distance the item will move up and down")]
        public float BobbingAmount = 1f;

        [Tooltip("Rotation angle per second")] public float RotatingSpeed = 360f;

        [Tooltip("Sound played on pickup")] public AudioClip PickupSfx;
        [Tooltip("VFX spawned on pickup")] public GameObject PickupVfxPrefab;

        public Rigidbody PickupRigidbody { get; private set; }

        Collider col;
        Vector3 startPosition;
        bool hasPlayedFeedback;

        protected virtual void Start()
        {
            PickupRigidbody = GetComponent<Rigidbody>();
            DebugUtility.HandleErrorIfNullGetComponent<Rigidbody, Pickup>(PickupRigidbody, this, gameObject);
            col = GetComponent<Collider>();
            DebugUtility.HandleErrorIfNullGetComponent<Collider, Pickup>(col, this, gameObject);

            // ensure the physics setup is a kinematic rigidbody trigger
            PickupRigidbody.isKinematic = true;
            col.isTrigger = true;

            // Remember start position for animation
            startPosition = transform.position;
        }

        void Update()
        {
            // Handle bobbing
            float bobbingAnimationPhase = ((Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f) + 0.5f) * BobbingAmount;
            transform.position = startPosition + Vector3.up * bobbingAnimationPhase;

            // Handle rotating
            transform.Rotate(Vector3.up, RotatingSpeed * Time.deltaTime, Space.Self);
        }

        void OnTriggerEnter(Collider other)
        {
            PlayerCharacterController pickingPlayer = other.GetComponent<PlayerCharacterController>();

            if (pickingPlayer != null)
            {
                OnPicked(pickingPlayer);

                PickupEvent evt = Events.PickupEvent;
                evt.Pickup = gameObject;
                EventManager.Broadcast(evt);
            }
        }

        protected virtual void OnPicked(PlayerCharacterController playerController)
        {
            PlayPickupFeedback();
        }

        public void PlayPickupFeedback()
        {
            if (hasPlayedFeedback)
                return;

            if (PickupSfx)
                AudioUtility.CreateSFX(PickupSfx, transform.position, AudioUtility.AudioGroups.Pickup, 0f);

            if (PickupVfxPrefab)
                Instantiate(PickupVfxPrefab, transform.position, Quaternion.identity);

            hasPlayedFeedback = true;
        }
    }
}