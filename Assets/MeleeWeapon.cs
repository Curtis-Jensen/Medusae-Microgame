using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

public class MeleeWeapon : WeaponController
{
    void Start()
    {
        meleeWeapon = true;
    }

    public override bool HandleAttackInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        if (inputDown)
            return TryShoot();

        return false;
    }
}
