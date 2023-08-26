using System.Collections.Generic;
using UnityEngine;


    public class Compass : MonoBehaviour
    {
        public RectTransform CompasRect;
        public float VisibilityAngle = 180f;
        public float HeightDifferenceMultiplier = 2f;
        public float MinScale = 0.5f;
        public float DistanceMinScale = 50f;
        public float CompasMarginRatio = 0.8f;

        public GameObject MarkerDirectionPrefab;

        Transform playerTransform;
        Dictionary<Transform, CompassMarker> elementsDictionnary = new Dictionary<Transform, CompassMarker>();

        float widthMultiplier;
        float heightOffset;

        void Awake()
        {
            PlayerCharacterController playerCharacterController = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, Compass>(playerCharacterController,
                this);
            playerTransform = playerCharacterController.transform;

            widthMultiplier = CompasRect.rect.width / VisibilityAngle;
            heightOffset = -CompasRect.rect.height / 2;
        }

        void Update()
        {
            // this is all very WIP, and needs to be reworked
            foreach (var element in elementsDictionnary)
            {
                float distanceRatio = 1;
                float heightDifference = 0;
                float angle;

                if (element.Value.IsDirection)
                {
                    angle = Vector3.SignedAngle(playerTransform.forward,
                        element.Key.transform.localPosition.normalized, Vector3.up);
                }
                else
                {
                    Vector3 targetDir = (element.Key.transform.position - playerTransform.position).normalized;
                    targetDir = Vector3.ProjectOnPlane(targetDir, Vector3.up);
                    Vector3 playerForward = Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up);
                    angle = Vector3.SignedAngle(playerForward, targetDir, Vector3.up);

                    Vector3 directionVector = element.Key.transform.position - playerTransform.position;

                    heightDifference = (directionVector.y) * HeightDifferenceMultiplier;
                    heightDifference = Mathf.Clamp(heightDifference, -CompasRect.rect.height / 2 * CompasMarginRatio,
                        CompasRect.rect.height / 2 * CompasMarginRatio);

                    distanceRatio = directionVector.magnitude / DistanceMinScale;
                    distanceRatio = Mathf.Clamp01(distanceRatio);
                }

                if (angle > -VisibilityAngle / 2 && angle < VisibilityAngle / 2)
                {
                    element.Value.CanvasGroup.alpha = 1;
                    element.Value.CanvasGroup.transform.localPosition = new Vector2(widthMultiplier * angle,
                        heightDifference + heightOffset);
                    element.Value.CanvasGroup.transform.localScale =
                        Vector3.one * Mathf.Lerp(1, MinScale, distanceRatio);
                }
                else
                    element.Value.CanvasGroup.alpha = 0;
            }
        }

        /// <summary>
        /// Puts all the compass markers under the compass game object as children
        /// </summary>
        /// <param name="element"></param>
        /// <param name="marker"></param>
        public void RegisterCompassElement(Transform element, CompassMarker marker)
        {
            marker.transform.SetParent(CompasRect);

            elementsDictionnary.Add(element, marker);
        }

        public void UnregisterCompassElement(Transform element)
        {
            if (elementsDictionnary.TryGetValue(element, out CompassMarker marker) && marker.CanvasGroup != null)
                Destroy(marker.CanvasGroup.gameObject);
            elementsDictionnary.Remove(element);
        }
    }
