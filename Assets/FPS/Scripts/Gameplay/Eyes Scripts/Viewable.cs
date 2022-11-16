using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Viewable //This script is mainly used to hold attributes of things being looked at.
{
    public GameObject viewableTarget;//The object itself that is being looked
    //By how much this object will increase or decrease effects (such as giving damage or healing)
    public float effectMultiplier = 0;

    public Viewable(GameObject viewableTarget, float effectMultiplier)
    {
        this.viewableTarget = viewableTarget;
        this.effectMultiplier = effectMultiplier;

        //Debug.Log("The actual object of the viewable object is: " + viewableTarget.name + "\n and the effectMultiplier is: "
        //    + effectMultiplier);
    }
}
