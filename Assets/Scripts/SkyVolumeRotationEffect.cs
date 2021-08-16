using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;
using Prime31.ZestKit;
using HauntedPSX.RenderPipelines.PSX.Runtime;

public class SkyVolumeRotationEffect : MonoBehaviour, ITweenTarget<Vector3>
{
    public Volume volume;
    public ITween<Vector3> volumeEffectTween;

    private SkyVolume _skyVolume;
    private Vector3 _tweenInitialValue;

    // Start is called before the first frame update
    void Start()
    {
        if(!volume)
            volume = FindObjectOfType<Volume>();

        if(!volume)
            Debug.LogWarning($"[VolumeEffect] Could not find Volume component in the scene.");

        // get reference to the sky volume and store it 
        var profile = volume.profile;
        profile.TryGet<SkyVolume>(out _skyVolume);

        _skyVolume.skyRotation.overrideState = true;

        Play();
    }

    public void Play()
    {
        volumeEffectTween = new Vector3Tween(this, Vector3.zero, new Vector3(0, 359.999f, 0), 60 * 10)
            .setEaseType(EaseType.Linear)
            .setLoops(LoopType.RestartFromBeginning, int.MaxValue - 1);
        
        volumeEffectTween.start();
    }

    public void setTweenedValue(Vector3 value)
    {
        _skyVolume.skyRotation.value = value;
    }

    public Vector3 getTweenedValue()
    {
        return _skyVolume.skyRotation.value;
    }

    public object getTargetObject()
    {
        return this;
    }
}
