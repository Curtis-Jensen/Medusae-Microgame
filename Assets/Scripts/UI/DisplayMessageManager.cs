using System.Collections.Generic;
using UnityEngine;

    public class DisplayMessageManager : MonoBehaviour
    {
        public UITable DisplayMessageRect;
        public NotificationToast MessagePrefab;

        List<(float timestamp, float delay, string message, NotificationToast notification)> pendingMessages;

        void Awake()
        {
            EventManager.AddListener<DisplayMessageEvent>(OnDisplayMessageEvent);
            pendingMessages = new List<(float, float, string, NotificationToast)>();
        }

        /* 1 Display the message under the DisplayMessageRect
         */
        void OnDisplayMessageEvent(DisplayMessageEvent evt)
        {
            NotificationToast notification =
                Instantiate(MessagePrefab, DisplayMessageRect.transform)
                .GetComponent<NotificationToast>();//1
            pendingMessages.Add((Time.time, evt.DelayBeforeDisplay, evt.Message, notification));
        }

        void Update()
        {
            foreach (var message in pendingMessages)
            {
                if (Time.time - message.timestamp > message.delay)
                {
                    message.Item4.Initialize(message.message);
                    DisplayMessage(message.notification);
                }
            }

            // Clear deprecated messages
            pendingMessages.RemoveAll(x => x.notification.Initialized);
        }

        void DisplayMessage(NotificationToast notification)
        {
            DisplayMessageRect.UpdateTable(notification.gameObject);
            //StartCoroutine(MessagePrefab.ReturnWithDelay(notification.gameObject, notification.TotalRunTime));
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<DisplayMessageEvent>(OnDisplayMessageEvent);
        }
    }
