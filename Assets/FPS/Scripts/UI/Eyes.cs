using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Unity.FPS.UI;
using Unity.FPS.Game;

namespace Unity.FPS.UI
{
    public class Eyes : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("The black part of the screen when the player blinks")]
        public GameObject eyeLids;
        [Tooltip("Whether Debug.Logs() and errors should be logged when testing how the player looks at the enemy")]
        public bool testing;
        [Tooltip("How high above the center of the collider the player will look.  An attempt at debugging the eyes glitch.")]
        public float lookHeight;

        [Header("Static")]
        [Tooltip("The effect that covers the screen when looking at medusae")]
        public Image staticImage;
        [Tooltip("Where the static sound effect comes from")]
        public AudioSource effectNoise;
        [Tooltip("The volume that static that changes when looking at or not looking at medusae")]
        public float effectVolume;
        [Tooltip("The intensity of the static effect on screen")]
        public float staticIntensity;
        [Tooltip("The maximum intensity of the static effect on screen")]
        public float maxStaticIntensity;
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
        Health health;
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

            health = gameObject.GetComponentInParent<Health>();
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

                var colliderBullsEye = viewableList[i].viewableTarget.transform.position;
                colliderBullsEye = new Vector3(colliderBullsEye.x, colliderBullsEye.y + lookHeight, colliderBullsEye.z);
                var screenPos = cam.WorldToScreenPoint(colliderBullsEye);

                bool withinWidth =  screenPos.x > 0 && screenPos.x < camWidth;
                bool withinHeight = screenPos.y > 0 && screenPos.y < camHeight;

                if (withinHeight && withinWidth)
                {
                    var sightLine = cam.ScreenPointToRay(screenPos);

                    Debug.DrawRay(sightLine.origin, colliderBullsEye - sightLine.origin, Color.magenta);
                    CheckForObstructions(viewableList[i], sightLine);
                }
            }
        }

        /* Step 3 THIS IS WHERE THE EYES GLITCH RESIDES
         * The order of the results in RaycastAll() is uNdEfInEd so sometimes the wall randomly pops in front of the medusa.
         * Need to sort by distance from the player or something
         * 
         * if the ray hits a non viewable, it stops looking for stuff
         * if the ray hits a different viewable with a tag it just keeps going
         * If the ray hits the enemy it was looking for it gives the medusa effect.
         */
        void CheckForObstructions(Viewable targetInFrame, Ray sightLine)
        {
            var hits = Physics.RaycastAll(sightLine);

            hits = SortRaycasts(hits);

            foreach (RaycastHit item in hits)
            {
                if (item.transform.CompareTag("Untagged")) return;

                if (item.collider.Equals(targetInFrame.viewableTarget.GetComponentInChildren<Collider>()))
                {
                    AddLookTime(targetInFrame.effectMultiplier);
                    return;
                }
            }
        }

        private RaycastHit[] SortRaycasts(RaycastHit[] hits)
        {
            bool sorted;

            do
            {
                sorted = true;
                for (int i = 0; i < hits.Length - 1; i++)
                {
                    if (hits[i].distance > hits[i + 1].distance)
                    {
                        var temp = hits[i];
                        hits[i] = hits[i + 1];
                        hits[i + 1] = temp;

                        sorted = false;
                    }
                }

            } while (sorted == false);

            return hits;
        }

        /* Step 4 ☣
         * This is meant to be the place to decide what will happen when medusae are looked at
         * 
         * Gives damage and healing through the look timer since that is already going up and down
         */
        void AddLookTime(float effectMultiplier)
        {
            eyesViewing = true;

            lookTimer += Time.deltaTime * effectMultiplier;
            if (lookTimer > 1)
            {
                health.TakeDamage(lookTimer, gameObject);
                lookTimer = 0;
            }            
            else if (lookTimer < 1)
            {
                health.Heal(-lookTimer);
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