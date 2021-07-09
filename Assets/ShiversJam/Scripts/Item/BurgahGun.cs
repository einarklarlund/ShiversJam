using System;
using System.Collections.Generic;
using UnityEngine;
using Prime31.ZestKit;
using Zenject;

public class BurgahGun : Item
{
    [SerializeField]
    AnimationClip _fireAnimationClip = null;
    
    [SerializeField]
    AudioClip _fireAudioClip = null;

    [SerializeField]
    float _pitchModifier = 0.1f;

    [SerializeField]
    GameObject _projectilePrefab = null;

    Projectile.Factory _projectileFactory;
    EffectsController _effectsController;

    [Inject]
    public void Construct(Projectile.Factory projectileFactory)
    {
        _projectileFactory = projectileFactory;
    }

    protected override void OnInteracted(Interactor interactor)
    {
        base.OnInteracted(interactor);
    }

    protected override void OnUsed()
    {
        _effectsController.PlayAnimationClip(_fireAnimationClip);
        _effectsController.PlayAudioClip(_fireAudioClip, 1 + (0.5f - UnityEngine.Random.value) * _pitchModifier);
        SpawnProjectile();
    }

    new protected void Start()
    {
        base.Start();

        _effectsController = GetComponent<EffectsController>();
    }

    void SpawnProjectile()
    {
        // spawn projectile
        Projectile projectile = _projectileFactory.Create(_projectilePrefab);
        projectile.transform.position = transform.position;
        projectile.direction = transform.forward;
        projectile.owner = owner;
    }
}