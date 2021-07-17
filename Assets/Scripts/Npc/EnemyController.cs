using System;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(Animator))]
public abstract class EnemyController : NpcController
{
    // [Header("Enemy Controller Parameters")]
    public Damageable damageable;
    protected Interactable target;

    protected NpcMovementController npcMovementController;

    [Tooltip("When damaged, all enemies that are within a onDamageAlertDistance distance will be alerted")]
    public float onDamageAlertDistance = 15;

    public abstract void Attack();

    new protected void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        npcMovementController = GetComponent<NpcMovementController>();

        if(!damageable)
            damageable = this.FindComponent<Damageable>();
        if(!damageable)
            Debug.LogWarning($"[EnemyController] Enemy {name} needs a damageable component in its hierarchy.");

        damageable.hub.Connect<int>(Damageable.Message.Damaged, OnDamaged);
        damageable.hub.Connect(Damageable.Message.Killed, OnKilled);
    }
    
    public void Alert(Interactable target)
    {
        this.target = target;
        animator.SetTrigger("Alerted");
    }

    protected override void OnInteracted(Interactor interactor)
    {
        base.OnInteracted(interactor);

        // set target to be the owner of the attack
        if(interactor is Attack)
        {
            var attack = interactor as Attack;
            target = attack.owner.GetComponent<Interactable>();
        }
    }

    protected virtual void OnDamaged(int damage)
    {
        animator.SetTrigger("Damaged");
    }

    protected virtual void OnKilled()
    {
        animator.SetTrigger("Died");
        Debug.Log("died");

        // disable collision with damageable collider
        var damageable = this.FindComponent<Interactable>();
        if(damageable)
            damageable.GetComponent<Collider>().enabled = false;
    }
}
