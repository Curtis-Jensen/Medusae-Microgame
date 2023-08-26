using TMPro;
using UnityEngine;


    public class FramerateCounter : MonoBehaviour
    {
        [Tooltip("Delay between updates of the displayed framerate value")]
        public float PollingTime = 0.5f;

        [Tooltip("The text field displaying the framerate")]
        public TextMeshProUGUI UIText;

        float accumulatedDeltaTime = 0f;
        int accumulatedFrameCount = 0;

        void Update()
        {
            accumulatedDeltaTime += Time.deltaTime;
            accumulatedFrameCount++;

            if (accumulatedDeltaTime >= PollingTime)
            {
                int framerate = Mathf.RoundToInt((float)accumulatedFrameCount / accumulatedDeltaTime);
                UIText.text = framerate.ToString();

                accumulatedDeltaTime = 0f;
                accumulatedFrameCount = 0;
            }
        }
    }
