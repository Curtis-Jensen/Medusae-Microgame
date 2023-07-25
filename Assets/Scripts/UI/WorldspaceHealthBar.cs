using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WorldspaceHealthBar : MonoBehaviour
    {
        [Tooltip("Health component to track")] public Health Health;

        [Tooltip("Image component displaying health left")]
        public Image HealthBarImage;

        [Tooltip("The floating healthbar pivot transform")]
        public Transform HealthBarPivot;

        [Tooltip("Whether the health bar is visible when at full health or not.  (Also, disabling this on the hoverbot deletes the healthbar...  Somehow??)")]
        public bool HideFullHealthBar = true;

        void Update()
        {
            // update health bar value
            HealthBarImage.fillAmount = Health.currentHealth / Health.maxHealth;

            // rotate health bar to face the camera/player
            HealthBarPivot.LookAt(Camera.main.transform.position);

            // hide health bar if needed
            if (HideFullHealthBar)
                HealthBarPivot.gameObject.SetActive(HealthBarImage.fillAmount != 1);
        }
    }
}