﻿using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class InGameMenuManager : MonoBehaviour
    {
        [Tooltip("Root GameObject of the menu used to toggle its activation")]
        public GameObject pauseMenuRoot;
        internal static bool isPaused;

        [Tooltip("Master volume when menu is open")]
        [Range(0.001f, 1f)]
        public float VolumeWhenMenuOpen = 0.5f;

        [Tooltip("Slider component for look sensitivity")]
        public Slider LookSensitivitySlider;

        [Tooltip("Toggle component for shadows")]
        public Toggle ShadowsToggle;

        [Tooltip("Toggle component for invincibility")]
        public Toggle InvincibilityToggle;

        [Tooltip("Toggle component for framerate display")]
        public Toggle FramerateToggle;

        [Tooltip("GameObject for the controls")]
        public GameObject ControlImage;

        //TODO: remove pause prompt after first pause
        [Tooltip("For getting a refference to the pause prompt so it can be disabled.")]
        public GameObject pauseButtonPrompt;

        PlayerInputHandler playerInputsHandler;
        Health playerHealth;
        FramerateCounter framerateCounter;

        void Start()
        {
            playerInputsHandler = FindObjectOfType<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerInputHandler, InGameMenuManager>(playerInputsHandler,
                this);

            playerHealth = playerInputsHandler.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, InGameMenuManager>(playerHealth, this, gameObject);

            framerateCounter = FindObjectOfType<FramerateCounter>();
            DebugUtility.HandleErrorIfNullFindObject<FramerateCounter, InGameMenuManager>(framerateCounter, this);

            pauseMenuRoot.SetActive(false);

            LookSensitivitySlider.value = playerInputsHandler.LookSensitivity;
            LookSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

            ShadowsToggle.isOn = QualitySettings.shadows != ShadowQuality.Disable;
            ShadowsToggle.onValueChanged.AddListener(OnShadowsChanged);

            InvincibilityToggle.isOn = playerHealth.Invincible;
            InvincibilityToggle.onValueChanged.AddListener(OnInvincibilityChanged);

            FramerateToggle.isOn = framerateCounter.UIText.gameObject.activeSelf;
            FramerateToggle.onValueChanged.AddListener(OnFramerateCounterChanged);
        }

        void Update()
        {
            // Lock cursor when clicking outside of menu
            if (!pauseMenuRoot.activeSelf && Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            Pause();

            //I have little idea as to what this does so I'll just remove it
            //if (Input.GetAxisRaw(InputNames.axisNameVertical) != 0)
            //{
            //    if (EventSystem.current.currentSelectedGameObject == null)
            //    {
            //        EventSystem.current.SetSelectedGameObject(null);
            //        LookSensitivitySlider.Select();
            //    }
            //}
        }

        public void Pause()
        {
            bool pauseButtonPressed = Input.GetButtonDown(InputNames.buttonNamePauseMenu);
            bool cancelButtonPressed = Input.GetButtonDown(InputNames.buttonNameCancel);

            if (pauseButtonPressed
                || (pauseMenuRoot.activeSelf && cancelButtonPressed))
            {
                if (ControlImage.activeSelf)
                {
                    ControlImage.SetActive(false);
                    return;
                }

                SetPauseMenuActivation(!pauseMenuRoot.activeSelf);
            }
        }

        public void ClosePauseMenu()
        {
            SetPauseMenuActivation(false);
        }

        void SetPauseMenuActivation(bool active)
        {
            pauseMenuRoot.SetActive(active);
            Cursor.visible = active;
            isPaused = active;

            if (pauseMenuRoot.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
                AudioUtility.SetMasterVolume(VolumeWhenMenuOpen);

                //EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1f;
                AudioUtility.SetMasterVolume(1);
            }

        }

        void OnMouseSensitivityChanged(float newValue)
        {
            playerInputsHandler.LookSensitivity = newValue;
        }

        void OnShadowsChanged(bool newValue)
        {
            QualitySettings.shadows = newValue ? ShadowQuality.All : ShadowQuality.Disable;
        }

        void OnInvincibilityChanged(bool newValue)
        {
            playerHealth.Invincible = newValue;
        }

        void OnFramerateCounterChanged(bool newValue)
        {
            framerateCounter.UIText.gameObject.SetActive(newValue);
        }

        public void OnShowControlButtonClicked(bool show)
        {
            ControlImage.SetActive(show);
        }
    }
}