using System;
using System.Collections.Generic;
using UnityEngine;
using Prime31.ZestKit;

public class LittleMan : Item
{
    bool _hasBeenPickedUp = false;
    bool _readyToBeDestroyed = false;

    object context = int.MaxValue / 2;

    protected override void OnInteracted(Interactor interactor)
    {
        base.OnInteracted(interactor);

        if(interactor is PlayerController)
        {
            _hasBeenPickedUp = true;
        }
    }

    protected override void OnUsed()
    {
        GetComponentInChildren<Billboard>().enabled = true;
        Drop();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(_hasBeenPickedUp)
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            TweenParty party = new TweenParty(2);

            foreach(var renderer in renderers)
            {
                party.addTween(
                    ZestKitExtensions.ZKfloatTo(renderer.material, 0, 3, "_GeoRes")
                        .setEaseType(EaseType.ExpoIn));
            }
                
            party.addTween(
                ZestKitExtensions.ZKlocalScaleTo(interactable.transform, new Vector3(10, 3, 10), 3)
                    .setEaseType(EaseType.ExpoIn));
            
            party.setCompletionHandler(tween => _readyToBeDestroyed = true);
            party.setContext(context);
            party.setEaseType(EaseType.QuadIn);
            party.start();
        }
    }

    void Update()
    {
        if(_readyToBeDestroyed)
        {
            Material material = GetComponentInChildren<MeshRenderer>().material;
            ZestKit.instance.stopAllTweensWithContext(context);

            itemManager.RemoveItem(this, Time.deltaTime);
            _readyToBeDestroyed = false;
        }
    }
}