using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ChargedProjectileEffectsHandler : MonoBehaviour
    {
        [Tooltip("Object that will be affected by charging scale & color changes")]
        public GameObject ChargingObject;

        [Tooltip("Scale of the charged object based on charge")]
        public MinMaxVector3 Scale;

        [Tooltip("Color of the charged object based on charge")]
        public MinMaxColor Color;

        MeshRenderer[] affectedRenderers;
        ProjectileBase projectileBase;

        /* 1 I don't think this is instantiation like a game object.  
         * I think it's just applying the material
         */
        void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ChargedProjectileEffectsHandler>(
                projectileBase, this, gameObject);

            projectileBase.OnShoot += OnShoot;

            affectedRenderers = ChargingObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var ren in affectedRenderers)
            {
                ren.sharedMaterial = Instantiate(ren.sharedMaterial);//1
            }
        }

        void OnShoot()
        {
            ChargingObject.transform.localScale = Scale.GetValueFromRatio(projectileBase.InitialCharge);

            foreach (var ren in affectedRenderers)
            {
                ren.sharedMaterial.SetColor("_Color", Color.GetValueFromRatio(projectileBase.InitialCharge));
            }
        }
    }
}