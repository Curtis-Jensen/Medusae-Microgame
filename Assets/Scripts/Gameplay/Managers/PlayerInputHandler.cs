using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Tooltip("Sensitivity multiplier for moving the camera around")]
        public float LookSensitivity = 1f;

        [Tooltip("Additional sensitivity multiplier for WebGL")]
        public float WebglLookSensitivityMultiplier = 0.25f;

        [Tooltip("Limit to consider an input when using a trigger on a controller")]
        public float TriggerAxisThreshold = 0.4f;

        [Tooltip("Used to flip the vertical input axis")]
        public bool InvertYAxis = false;

        [Tooltip("Used to flip the horizontal input axis")]
        public bool InvertXAxis = false;

        GameFlowManager gameFlowManager;
        PlayerCharacterController playerCharacterController;
        bool fireInputWasHeld;

        void Start()
        {
            playerCharacterController = GetComponent<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerInputHandler>(
                playerCharacterController, this, gameObject);
            gameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, PlayerInputHandler>(gameFlowManager, this);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            fireInputWasHeld = GetFireInputHeld();
        }

        public bool CanProcessInput()
        {
            return Cursor.lockState == CursorLockMode.Locked && !gameFlowManager.GameIsEnding;
        }

        public Vector3 GetMoveInput()
        {
            if (CanProcessInput())
            {
                Vector3 move = new Vector3(Input.GetAxisRaw(InputNames.axisNameHorizontal), 0f,
                    Input.GetAxisRaw(InputNames.axisNameVertical));

                // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
                move = Vector3.ClampMagnitude(move, 1);

                return move;
            }

            return Vector3.zero;
        }

        public float GetLookInputsHorizontal()
        {
            return GetMouseOrStickLookAxis(InputNames.mouseAxisNameHorizontal,
                InputNames.axisNameJoystickLookHorizontal);
        }

        public float GetLookInputsVertical()
        {
            return GetMouseOrStickLookAxis(InputNames.mouseAxisNameVertical,
                InputNames.axisNameJoystickLookVertical);
        }

        public bool GetJumpInputDown()
        {
            if (CanProcessInput())
                return Input.GetButtonDown(InputNames.buttonNameJump);

            return false;
        }

        public bool GetJumpInputHeld()
        {
            if (CanProcessInput())
                return Input.GetButton(InputNames.buttonNameJump);

            return false;
        }

        public bool GetFireInputDown()
        {
            return GetFireInputHeld() && !fireInputWasHeld;
        }

        public bool GetFireInputReleased()
        {
            return !GetFireInputHeld() && fireInputWasHeld;
        }

        public bool GetFireInputHeld()
        {
            if (CanProcessInput())
            {
                bool isGamepad = Input.GetAxis(InputNames.buttonNameGamepadFire) != 0f;
                if (isGamepad)
                    return Input.GetAxis(InputNames.buttonNameGamepadFire) >= TriggerAxisThreshold;
                else
                    return Input.GetButton(InputNames.buttonNameFire);
            }

            return false;
        }

        public bool GetAimInputHeld()
        {
            if (CanProcessInput())
            {
                bool isGamepad = Input.GetAxis(InputNames.buttonNameGamepadAim) != 0f;
                bool i = isGamepad
                    ? (Input.GetAxis(InputNames.buttonNameGamepadAim) > 0f)
                    : Input.GetButton(InputNames.buttonNameAim);
                return i;
            }

            return false;
        }

        public bool GetSprintInputHeld()
        {
            if (CanProcessInput())
                return Input.GetButton(InputNames.buttonNameSprint);

            return false;
        }

        public bool GetCrouchInputDown()
        {
            if (CanProcessInput())
                return Input.GetButtonDown(InputNames.buttonNameCrouch);

            return false;
        }

        public bool GetCrouchInputReleased()
        {
            if (CanProcessInput())
                return Input.GetButtonUp(InputNames.buttonNameCrouch);

            return false;
        }

        public bool GetReloadButtonDown()
        {
            if (CanProcessInput())
                return Input.GetButtonDown(InputNames.buttonReload);

            return false;
        }

        public int GetSwitchWeaponInput()
        {
            if (CanProcessInput())
            {

                bool isGamepad = Input.GetAxis(InputNames.buttonNameGamepadSwitchWeapon) != 0f;
                string axisName = isGamepad
                    ? InputNames.buttonNameGamepadSwitchWeapon
                    : InputNames.buttonNameSwitchWeapon;

                if (Input.GetAxis(axisName) > 0f)
                    return -1;
                else if (Input.GetAxis(axisName) < 0f)
                    return 1;
                else if (Input.GetAxis(InputNames.buttonNameNextWeapon) > 0f)
                    return -1;
                else if (Input.GetAxis(InputNames.buttonNameNextWeapon) < 0f)
                    return 1;
            }

            return 0;
        }

        public int GetSelectWeaponInput()
        {
            if (CanProcessInput())
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    return 1;
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    return 2;
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    return 3;
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                    return 4;
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                    return 5;
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                    return 6;
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                    return 7;
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                    return 8;
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                    return 9;

            return 0;
        }

        float GetMouseOrStickLookAxis(string mouseInputName, string stickInputName)
        {
            if (CanProcessInput())
            {
                // Check if this look input is coming from the mouse
                bool isGamepad = Input.GetAxis(stickInputName) != 0f;
                float i = isGamepad ? Input.GetAxis(stickInputName) : Input.GetAxisRaw(mouseInputName);

                // handle inverting vertical input
                if (InvertYAxis)
                    i *= -1f;

                // apply sensitivity multiplier
                i *= LookSensitivity;

                if (isGamepad)
                    // since mouse input is already deltaTime-dependant, only scale input with frame time if it's coming from sticks
                    i *= Time.deltaTime;
                else
                {
                    // reduce mouse input amount to be equivalent to stick movement
                    i *= 0.01f;
#if UNITY_WEBGL
                    // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
                    i *= WebglLookSensitivityMultiplier;
#endif
                }

                return i;
            }

            return 0f;
        }
    }
}