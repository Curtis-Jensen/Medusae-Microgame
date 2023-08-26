using UnityEngine;
using UnityEngine.Events;

    //Contains properties and behaviors applicable to all projectiles
    public abstract class ProjectileBase : MonoBehaviour
    {
        public GameObject Owner { get; private set; }
        public Vector3 InitialPosition { get; private set; }
        public Vector3 InitialDirection { get; private set; }
        public Vector3 InheritedMuzzleVelocity { get; private set; }
        public float InitialCharge { get; private set; }

        public UnityAction onShoot;

        //This is called after onShoot
        public void Shoot(GunController controller)
        {
            Owner = controller.owner;
            InitialPosition = transform.position;
            InitialDirection = transform.forward;
            InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;
            InitialCharge = controller.CurrentCharge;

            onShoot?.Invoke();
        }
    }
