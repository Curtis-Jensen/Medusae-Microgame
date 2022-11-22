using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Unity.FPS.UI;

namespace Unity.FPS.UI
{
    public class Eyes : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("The black part of the screen when the player blinks")]
        public GameObject eyeLids;
        [Tooltip("The effect that covers the screen when looking at medusae")]
        public Image staticImage;
        public AudioSource effectNoise;
        [Tooltip("The volume that static that changes when looking at or not looking at medusae")]
        public float effectVolume;
        [Tooltip("The intensity of the static effect on screen")]
        public float staticIntensity;
        public float maxStaticIntensity;
        public bool testing;
        #endregion
        #region Private Variables
        bool eyesViewing;
        float lookTimer = 0; //However long the player has been looking at the enemy.
        float medusaeVisualized;
        float staticVisualEffect;//Determines by how much the screen will be staticy
        internal List<Viewable> viewableList;
        Camera cam;
        int camWidth;
        int camHeight;
        Vector2 camCenter;
        #endregion

        void Awake()
        {
            viewableList = new List<Viewable>();
        }

        void Start()
        {
            cam = gameObject.GetComponent<Camera>();
            camWidth = cam.pixelWidth;
            camHeight = cam.pixelHeight;
            camCenter = new Vector2(camWidth / 2, camHeight / 2);
        }

        #region👁Viewing Steps
        /* Step 0 🕶
         * If the player is blinking:
         * Keep note that no medusae are being looked at and exit the fixed update.
         */
        bool Blinking()
        {
            bool blinking = false;
            if (InGameMenuManager.isPaused)   blinking = true;
            else if (Input.GetMouseButton(2)) blinking = true;

            eyeLids.SetActive(blinking);
            return blinking;
        }

        /* Step 1 👁
         * 
         * The update function calls everything
         */
        void FixedUpdate()
        {
            if (Blinking()) return;

            eyesViewing = false;
            ScanFrameForViewables();

            SetStaticIntensity();
            RenderStatic();
            if(testing)
                EyesTest();
        }

        /* Step 2 ⬜
         * For each enemy (determined by SpawnOwner adding to the visuals group):
         * If that enemy is not dead, Turns the position of the enemy into pixel units
         * 
         * If the enemy is on the screen
         * draw a ray from the camera's view
         */
        void ScanFrameForViewables()
        {
            for (int i = 0; i < viewableList.Count; i++)
            {
                if (viewableList[i] == null) continue;

                var screenPos = cam.WorldToScreenPoint(viewableList[i].viewableTarget.transform.position);

                bool withinWidth = screenPos.x > 0 && screenPos.x < camWidth;
                bool withinHeight = screenPos.y > 0 && screenPos.y < camHeight;

                if (withinHeight && withinWidth)
                {
                    var sightLine = cam.ScreenPointToRay(screenPos);
                    Debug.DrawRay(sightLine.origin, sightLine.direction * 100, Color.magenta);
                    CheckForObstructions(viewableList[i], sightLine);
                }
            }
        }

        /* Step 3
         * Creates a ray to look for the enemy (also makes a ray for the scene to visualize)
         * 
         * (visualizes the ray when in debug)
         * 
         * if the ray hits a wall, it stops looking for stuff
         * if the ray hits an enemy it keeps looking for stuff (doesn't acknowledge)
         * If the ray hits the enemy it was looking for it gives the medusa effect.
         */
        void CheckForObstructions(Viewable targetInFrame, Ray sightLine)
        {
            //Debug.Log("The target to view is within frame!");
            foreach (RaycastHit item in Physics.RaycastAll(sightLine))
            {
                if (item.transform.CompareTag("Untagged")) return;

                if (item.collider.Equals(targetInFrame.viewableTarget.GetComponentInChildren<Collider>()))
                {
                    //Debug.Log("Found: " + targetInFrame.viewableTarget.name);
                    AddLookTime(targetInFrame.effectMultiplier);
                    return;
                }
                //else Debug.Log("The current collider is is: " + item.collider +
                //   ", and the target collider is: " + targetInFrame.viewableTarget.GetComponentInChildren<Collider>());
            }
        }

        /* Step 4 ☣
         * This is meant to be the place to decide what will happen when medusae are looked at
         * 
         * Gives damage and healing through the look timer since that is already going up and down
         */
        void AddLookTime(float effectMultiplier)
        {
            eyesViewing = true;

            //Debug.DrawRay(sightLine.origin, sightLine.direction * 100, lineColor);

            lookTimer += Time.deltaTime * effectMultiplier;
            if (Math.Abs(lookTimer) > 1)
            {
                //TODO add in reference to player's health script
                //DecreaseHealth(lookTimer);

                lookTimer = 0;
            }
            medusaeVisualized += effectMultiplier;
        }
        #endregion

        /* Shoots out a very simple ray directly in front of the player to see if anything is there.
         * If there is something there, but the main eyes methods don't see it, then there is something wrong
         * and an error is thrown.  Hopefully with some detail as to the error.
         * 
         * For each item in viewableList (unless it's null)
         * Get the collider for the viewable target
         * See if it's what we got (If it's a different item it doesn't matter)
         * If it is what we got, set redundantEyesViewing to true so the eyes test passes
         * 
         * If this test sees something but the main code does not, throw an error
         */
        void EyesTest()
        {
            var redundantEyesViewing = false;
            var inFront = cam.ScreenPointToRay(camCenter);
            Collider item = null;
            Physics.Raycast(inFront, out RaycastHit hit);

            for (int i = 0; i < viewableList.Count; i++)
            {
                if (viewableList[i] == null) continue;

                item = viewableList[i].viewableTarget.GetComponentInChildren<Collider>();
                if (item == hit.collider)
                {
                    redundantEyesViewing = true;
                    break;
                }
            }

            if (redundantEyesViewing && !eyesViewing)
                throw new UnityException(item.name + " should be visible, but it is not.");
            else if (redundantEyesViewing && eyesViewing)
                Debug.Log(item.name + " is VISIBLE");
            else if (!redundantEyesViewing && !eyesViewing)
                Debug.Log("Nothing to view");
            else if (!redundantEyesViewing && eyesViewing)
                Debug.Log(item.name + " is VISIBLE, but not in center frame");
        }

        #region📺Static
        /* Determines by how much the screen should be staticy
         * Caps out the static effect if it's too much
         */
        void SetStaticIntensity()
        {
            staticVisualEffect = medusaeVisualized * staticIntensity;//To be sure that everything is still visible
            if (staticVisualEffect > maxStaticIntensity) staticVisualEffect = maxStaticIntensity;
        }

        /* Set the volume and opacity of the static effect based on how many medusae are being looked at,
         * which is calculated in SetStaticIntensity().
         */
        void RenderStatic()
        {
            effectNoise.volume = medusaeVisualized * effectVolume;
            staticImage.color = new Color(staticImage.color.r, staticImage.color.g, staticImage.color.b,
                staticVisualEffect);
            medusaeVisualized = 0;
            staticVisualEffect = 0;
        }
        #endregion
    }
}