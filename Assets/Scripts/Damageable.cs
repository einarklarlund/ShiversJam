using UnityEngine;
using IntrovertStudios.Messaging;

public class Damageable : Interactable
{
    public bool invincible = false;
    public int maxHealth = 20;
    public int health = 20;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    public override void Interact(Interactor interactor)
    {
        if(interactor is Attack)
        {
            TakeDamage((interactor as Attack).damage);
        }

        base.Interact(interactor);
    }

    void TakeDamage(int damage)
    {
        if(invincible || health <= 0)
            return;

        health -= damage;

        if(health <= 0)
            hub.Post(Message.Killed);
        else
            hub.Post(Message.Damaged, damage);
    }
}