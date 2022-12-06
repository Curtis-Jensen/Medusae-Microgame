using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class CrystalResetter : MonoBehaviour
    {
        public SpawnPointScript[] crystals;

        /* Tells all of the beenShot booleans in the crystals to be false so that they are ready
         * to be shot again to activate for the next round.
         * 
         * It does this with a for loop
         * 
         * For cleaner code it's a good idea to change the renderers here as well
         */
        public void Reset()
        {
            for (int i = 0; i < crystals.Length; i++) crystals[i].beenShot = false;
        }
    }
}
