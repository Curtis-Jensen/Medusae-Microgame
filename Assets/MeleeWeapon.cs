using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using System;

public class MeleeWeapon : WeaponController
{
    void Start()
    {
        meleeWeapon = true;
    }

    public override bool HandleAttackInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        if (inputDown)
            return TryAttack();

        return false;
    }

    private bool TryAttack()
    {
        throw new NotImplementedException();
    }

    public override float AttackAnimation()
    {
        return 0;
    }
}
