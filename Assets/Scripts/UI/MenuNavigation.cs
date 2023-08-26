using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class MenuNavigation : MonoBehaviour
    {
        public Selectable DefaultSelection;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (Input.GetButtonDown(InputNames.buttonNameSubmit)
                    || Input.GetAxisRaw(InputNames.axisNameHorizontal) != 0
                    || Input.GetAxisRaw(InputNames.axisNameVertical) != 0)
                {
                    EventSystem.current.SetSelectedGameObject(DefaultSelection.gameObject);
                }
            }
        }
    }
}