using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class Eyes : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("The black part of the screen when the player blinks")]
        public GameObject eyeLids;
        [Tooltip("Whether Debug.Logs() and errors should be logged when testing how the player looks at the enemy")]
        public bool deepTesting;
        [Tooltip("How high above the center of the collider the player will look.  An attempt at debugging the eyes glitch.")]
        public float lookHeight;
        [Tooltip("The choral sound effect when looking at the healing cross")]
        public AudioClip choralSound;

        [Header("Static")]
        [Tooltip("The static sound effect when looking at medusae")]
        public AudioClip staticSound;
        [Tooltip("The effect that covers the screen when looking at medusae")]
        public Image staticImage;
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
        float effectMagnitude;
        float staticVisualEffect;//Determines by how much the screen will be staticy
        internal List<Viewable> viewableList;
        Camera cam;
        int camWidth;
        int camHeight;
        Vector2 camCenter;
        Health health;
        AudioSource effectSoundSource; //Where the static sound effect comes from
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
            effectSoundSource = gameObject.GetComponent<AudioSource>();

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
            if (InGameMenuManager.isPaused)                       blinking = true;
            else if (Input.GetButton(InputNames.buttonCloseEyes)) blinking = true;

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

            var viewablesInRange = ScanFrameForViewables();
            foreach (var viewable in viewablesInRange)
            {
                var hit = CheckForObstructions(viewable);
                if (hit != null)
                    AddLookTime(viewable.damageMultiplier);
            }

            SetStaticIntensity();
            RenderStatic();
            EyesTest();
        }

        /* Step 2 ⬜
         * For each enemy (determined by SpawnOwner adding to the visuals group):
         * If that enemy is not dead, Turns the position of the enemy into pixel units
         * 
         * If the enemy is on the screen
         * draw a ray from the camera's view
         */
        List<Viewable> ScanFrameForViewables()
        {
            var viewablesInRange = new List<Viewable>();
            for (int i = 0; i < viewableList.Count; i++)
            {
                if (viewableList[i] == null) continue;

                var colliderBullsEye = viewableList[i].viewableTarget.transform.position;
                colliderBullsEye = new Vector3(colliderBullsEye.x, colliderBullsEye.y + lookHeight, colliderBullsEye.z);
                var screenPos = cam.WorldToScreenPoint(colliderBullsEye);

                bool withinWidth = screenPos.x > 0 && screenPos.x < camWidth;
                bool withinHeight = screenPos.y > 0 && screenPos.y < camHeight;

                if (withinHeight && withinWidth)
                {
                    viewablesInRange.Add(viewableList[i]);
                }
            }
            return viewablesInRange;
        }

        /* Step 3 
         * 1 if the ray hits a non viewable, it stops looking for stuff
         * 2 if the ray hits a different viewable with a tag it just keeps going
         * 3 If the ray hits the enemy it was looking for it gives the medusa effect.
         */
        RaycastHit? CheckForObstructions(Viewable viewable)
        {
            var colliderBullsEye = viewable.viewableTarget.transform.position;
            colliderBullsEye = new Vector3(colliderBullsEye.x, colliderBullsEye.y + lookHeight, colliderBullsEye.z);
            var screenPos = cam.WorldToScreenPoint(colliderBullsEye);

            var sightLine = cam.ScreenPointToRay(screenPos);

            var hits = Physics.RaycastAll(sightLine);
            Array.Sort(hits, (a, b) => (a.distance.CompareTo(b.distance)));

            foreach (RaycastHit item in hits)
            {
                if (item.transform.CompareTag("Untagged")) return null;//1

                //2

                if (item.collider.Equals(viewable.viewableTarget.GetComponentInChildren<Collider>()))//3
                {
                    return item;
                }
            }

            return null;
        }

        /* Step 4 ☣
         * This is meant to be the place to decide what will happen when medusae are looked at
         * 
         * Gives damage and healing through the look timer since that is already going up and down
         */
        void AddLookTime(float effectMultiplier)
        {
            eyesViewing = true;
            effectMagnitude += effectMultiplier;

            lookTimer += Time.deltaTime * effectMultiplier;

            if (lookTimer > 0)
                health.TakeDamage(lookTimer, gameObject);
            else if (lookTimer < 0)
                health.Heal(-lookTimer);

            lookTimer = 0;
        }
        #endregion

        /* Shoots out a very simple ray directly in front of the player to see if anything is there.
         * If there is something there, but the main eyes methods don't see it, then there is something wrong
         * and an error is thrown.
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

            if (deepTesting) 
            {
                if (redundantEyesViewing && eyesViewing)
                    Debug.Log(item.name + " is VISIBLE");
                else if (!redundantEyesViewing && !eyesViewing)
                    Debug.Log("Nothing to view");
                else if (!redundantEyesViewing && eyesViewing)
                    Debug.Log(item.name + " is VISIBLE, but not in center frame");
            }
        }

        #region📺Static
        /* Determines by how much the screen should be staticy
         * Caps out the static effect if it's too much
         */
        void SetStaticIntensity()
        {
            staticVisualEffect = effectMagnitude * staticIntensity;//To be sure that everything is still visible
            if (staticVisualEffect > maxStaticIntensity) staticVisualEffect = maxStaticIntensity;
        }

        /* Set the volume and opacity of the static effect based on how many medusae are being looked at,
         * which is calculated in SetStaticIntensity().
         */
        void RenderStatic()
        {
            if (effectMagnitude > 0 && effectSoundSource.clip != staticSound)
            {
                effectSoundSource.clip = staticSound;
                effectSoundSource.Play();
            }
            else if (effectMagnitude <= 0 && effectSoundSource.clip != choralSound)
            {
                effectSoundSource.clip = choralSound;
                effectSoundSource.Play();
            }

            effectSoundSource.volume = Mathf.Abs(effectMagnitude) * effectVolume;
            staticImage.color = new Color(staticImage.color.r, staticImage.color.g, staticImage.color.b,
                staticVisualEffect);

            effectMagnitude = 0;
            staticVisualEffect = 0;
        }
        #endregion
    }
}