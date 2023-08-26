using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class FollowPlayer : MonoBehaviour
    {
        Transform playerTransform;
        Vector3 originalOffset;

        void Start()
        {
            ActorsManager actorsManager = FindObjectOfType<ActorsManager>();
            if (actorsManager != null)
                playerTransform = actorsManager.Player.transform;
            else
            {
                enabled = false;
                return;
            }

            originalOffset = transform.position - playerTransform.position;
        }

        void LateUpdate()
        {
            transform.position = playerTransform.position + originalOffset;
        }
    }
}