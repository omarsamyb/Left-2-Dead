using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : InfectedController
{
    protected override void Start()
    {
        base.Start();
        attackRoutine = AttackRoutine();
        attackDelay = new WaitForSeconds(attackDelayTime);
    }

    protected override void Attack()
    {
        base.Attack();
    }
    private IEnumerator AttackRoutine()
    {
        yield return null;
    }
}
