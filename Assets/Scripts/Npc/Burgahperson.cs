using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Prime31.ZestKit;

public class Burgahperson : EnemyController
{
    new protected void Start()
    {
        base.Start();

        damageable.hub.Connect(Interactable.Message.Selected, OnSelected);
        damageable.hub.Connect(Interactable.Message.Unselected, OnUnselected);
    }

    public override void Attack()
    {
        throw new NotImplementedException();
    }
}
