using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    // Debug script, teleports the player across the map for faster testing
    public class TeleportPlayer : MonoBehaviour
    {
        public KeyCode ActivateKey = KeyCode.F12;

        PlayerCharacterController playerCharacterController;

        void Awake()
        {
            playerCharacterController = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, TeleportPlayer>(
                playerCharacterController, this);
        }

        void Update()
        {
            if (Input.GetKeyDown(ActivateKey))
            {
                playerCharacterController.transform.SetPositionAndRotation(transform.position, transform.rotation);
                Health playerHealth = playerCharacterController.GetComponent<Health>();
                if (playerHealth)
                    playerHealth.Heal(999);
            }
        }

    }
}