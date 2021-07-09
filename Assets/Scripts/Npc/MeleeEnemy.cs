using System;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(Animator))]
public abstract class MeleeEnemy : EnemyController
{
    [HideInInspector]
    public Hitbox hitbox;

    [Tooltip("If the player is within attackRange units and the enemy is in the ChasePlayer state, the enemy will attack")]
    public float attackRange = 4;

    new protected void Start()
    {
        base.Start();

        hitbox = GetComponentInChildren<Hitbox>(true);
        
        if(!hitbox)
            Debug.LogWarning($"[MeleeEnemy] {name} needs to have a Hitbox component in its children.");
    }

    void Update()
    {
        if(!target)
            return;

        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Chase")
            && Vector3.Distance(target.transform.position, transform.position) <= attackRange)
        {
            animator.SetBool("CanAttack", true);
        }
        else if(animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")
            && Vector3.Distance(target.transform.position, transform.position) > attackRange)
        {
            animator.SetBool("CanAttack", false);
        }
    }

    public override void Attack()
    {
        hitbox.Activate();
    }
}
