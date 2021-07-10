using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Prime31.ZestKit;

public class Burgahperson : EnemyController
{
    Billboard _billboard;

    new protected void Start()
    {
        base.Start();

        damageable.hub.Connect(Interactable.Message.Selected, OnSelected);
        damageable.hub.Connect(Interactable.Message.Unselected, OnUnselected);

        _billboard = GetComponent<Billboard>();
        _billboard.enabled = false;
    }

    public override void Attack()
    {
        throw new NotImplementedException();
    }

    void OnSelected()
    {
        _billboard.enabled = true;

        animator.SetBool("Moving", false);

        transform.rotation = Quaternion.Euler(Vector3.zero);

        npcMovementController.StopMoving();
    }

    void OnUnselected()
    {
        _billboard.enabled = false;
        
        npcMovementController.ResumeMoving();

        Debug.Log("unselected");
    }
}
