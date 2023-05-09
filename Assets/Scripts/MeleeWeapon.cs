using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using System;

public class MeleeWeapon : WeaponController
{
    [Tooltip("Damage of the melee weapon")]
    public float Damage = 40f;

    GameObject parent;

    void Start()
    {
        meleeWeapon = true;
        parent = gameObject.transform.root.gameObject;
    }

    public override bool HandleAttackInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        if (inputDown)
            return TryAttack();

        return false;
    }

    private bool TryAttack()
    {

        //Not yet implemented
        return true;
    }

    public override float AttackAnimation()
    {
        //Not yet implemented
        return 0;
    }

    private void OnTriggerEnter(Collider collision)
    {
        var damagable = collision.gameObject.GetComponent<Damageable>();

        damagable.InflictDamage(Damage, false, parent);
    }
}
