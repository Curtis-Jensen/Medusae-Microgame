using UnityEngine;

namespace Unity.FPS.UI
{
    public class Viewable : MonoBehaviour //What will be accessed by Eyes.cs to be looked at
    {
        [Tooltip("By how much this object will increase or decrease effects (such as giving damage or healing)")]
        public float damageMultiplier;

        internal GameObject viewableTarget;//The object itself that is being looked

        void Start()
        {
            //This script should be tied to a gameobject with a collider to be viewed so this line will work
            viewableTarget = gameObject;

            //Searches for any instance of Eyes.cs to tell them about themselves
            Eyes[] eyesList = FindObjectsOfType<Eyes>();

            foreach (Eyes eye in eyesList)
                eye.viewableList.Add(this);
        }
    }
}
