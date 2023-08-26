using UnityEngine;


    public class CompassElement : MonoBehaviour
    {
        [Tooltip("The marker on the compass for this element")]
        public CompassMarker CompassMarkerPrefab;

        [Tooltip("Text override for the marker, if it's a direction")]
        public string TextDirection;

        Compass compass;

        /* 1 Putting the markers as children of the compass is currently unnecessary 
         * because they are set under compass.RegisterCompassElement().
         * That may be a TODO, but at least the hierarchy is clean
         */
        void Awake()
        {
            compass = FindObjectOfType<Compass>();
            DebugUtility.HandleErrorIfNullFindObject<Compass, CompassElement>(compass, this);

            var markerInstance = Instantiate(CompassMarkerPrefab);//1

            markerInstance.Initialize(this, TextDirection);
            compass.RegisterCompassElement(transform, markerInstance);
        }

        void OnDestroy()
        {
            compass.UnregisterCompassElement(transform);
        }
    }
