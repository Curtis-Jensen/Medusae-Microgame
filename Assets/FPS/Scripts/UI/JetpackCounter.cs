using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class JetpackCounter : MonoBehaviour
    {
        [Tooltip("Image component representing jetpack fuel")]
        public Image JetpackFillImage;

        [Tooltip("Canvas group that contains the whole UI for the jetack")]
        public CanvasGroup MainCanvasGroup;

        [Tooltip("Component to animate the color when empty or full")]
        public FillBarColorChange FillBarColorChange;

        Jetpack jetpack;

        void Awake()
        {
            jetpack = FindObjectOfType<Jetpack>();
            DebugUtility.HandleErrorIfNullFindObject<Jetpack, JetpackCounter>(jetpack, this);

            FillBarColorChange.Initialize(1f, 0f);
        }

        void Update()
        {
            MainCanvasGroup.gameObject.SetActive(jetpack.IsJetpackUnlocked);

            if (jetpack.IsJetpackUnlocked)
            {
                JetpackFillImage.fillAmount = jetpack.CurrentFillRatio;
                FillBarColorChange.UpdateVisual(jetpack.CurrentFillRatio);
            }
        }
    }
}