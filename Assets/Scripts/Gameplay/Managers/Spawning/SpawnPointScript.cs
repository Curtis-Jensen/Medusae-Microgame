using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class SpawnPointScript : MonoBehaviour
    {
        public SpawnManager spawnOwner;
        public Material glowingMaterial;
        public Material standardMaterial;

        internal static int totalSpawners;
        internal static int shotSpawners;
        internal static Renderer[] allRenderers = new Renderer[4];
        internal static bool[] beenShotBools = new bool[4];
        internal bool beenShot = false;
        Renderer ownRenderer;

        /* Gets the render component
         * 
         * Calculates the total amount of spawners to know when to call for the next wave
         * Uses the total spawner number as an ID so that an array of renderers can be held.
         */
        void Start()
        {
            ownRenderer = GetComponent<Renderer>();
            ownRenderer.enabled = true;

            allRenderers[totalSpawners] = ownRenderer;
            totalSpawners++;
        }

        /* If the crystal has not been shot yet make it enter a glowing state
         * 
         * When the last crystal has been shot it will tell all the other crystals to have a standard material
         * It also tells all the crystals that they haven't been shot again
         * 
         * It gets this informtaion by magic - Benjamin
         */
        private void OnCollisionEnter(Collision other)
        {
            if (beenShot == false)
            {
                ownRenderer.sharedMaterial = glowingMaterial;

                shotSpawners++;
                beenShot = true;

                if (shotSpawners >= totalSpawners)
                {
                    spawnOwner.EndWave();

                    for (int i = 0; i < 4; i++)
                    {
                        allRenderers[i].sharedMaterial = standardMaterial;
                    }
                    transform.parent.GetComponent<CrystalResetter>().Reset();
                    shotSpawners = 0;
                }
            }
        }
    }
}