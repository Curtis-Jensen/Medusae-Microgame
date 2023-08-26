using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


    public class LoadSceneButton : MonoBehaviour
    {
        public string SceneName = "";

        void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject
                && Input.GetButtonDown(InputNames.buttonNameSubmit))
                LoadTargetScene();
        }

        public void LoadTargetScene()
        {
            SceneManager.LoadScene(SceneName);
        }
    }
