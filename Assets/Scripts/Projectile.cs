using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Projectile : Attack
{
    public class Factory : PlaceholderFactory<UnityEngine.Object, Projectile> { }

    public float travelSpeed = 5;
    public float sphereCastRadius = 0.3f;
    public float lifetime = 10;
    public SpriteEffect onHitEffects;

    /*      ~~~~~       THE VARS BELOW SHOULD BE SET BY WHOEVER SPAWNS THE PROJECTILE       ~~~~~       */
    // the direction in which the projectile is traveling
    [HideInInspector]
    public Vector3 direction;
    // the item who shot this projectile
    [HideInInspector]
    public Item shooter;

    // the amount of distance that the spherecast will sweep over
    public float DistancePerFrame => travelSpeed * Time.fixedDeltaTime;

    float _timeSinceSpawned = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_timeSinceSpawned >= lifetime)
            Destroy(gameObject);

        bool wasHit = false;
        
        // spherecast will be done on interactable, terrain, and prop layers
        var layerMask = 1 << LayerMask.NameToLayer("Interactable") | 1 << LayerMask.NameToLayer("Terrain") | 1 << LayerMask.NameToLayer("Props");
        // sphere cast to find hits in the interactable layer, sweeping from the projectile's
        // position in the last frame to the position in the current frame.
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, sphereCastRadius, direction,
            DistancePerFrame, layerMask, QueryTriggerInteraction.Ignore);

        // if we hit something, tell the interactable to apply the shooter item and then remove the gameobject
        foreach(RaycastHit hit in hits)
        {
            var interactable = hit.transform.FindComponent<Interactable>();

            if(interactable)
                interactable.Interact(this);
            
            wasHit = true;
        }

        if(wasHit)
        {
            // activate effects and destroy the gameobject
            SpriteEffect effects = Instantiate(onHitEffects);
            effects.transform.position = transform.position;

            Destroy(gameObject);
            return;
        }

        // move the projectile in its direction and keep track of how long its been
        // since it spawned
        transform.position += direction * DistancePerFrame;
        _timeSinceSpawned += Time.fixedDeltaTime;
    }
}
