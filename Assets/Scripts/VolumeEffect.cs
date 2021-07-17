using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;
using Prime31.ZestKit;
using HauntedPSX.RenderPipelines.PSX.Runtime;

public class VolumeEffect : MonoBehaviour, ITweenTarget<float>
{
    public Volume volume;
    public UnityEvent onEffectFinished;
    public ITween<float> volumeEffectTween;

    private FogVolume _fogVolume;
    private float _tweenInitialValue;

    // Start is called before the first frame update
    void Start()
    {
        if(!volume)
            volume = FindObjectOfType<Volume>();

        if(!volume)
            Debug.LogWarning($"[VolumeEffect] Could not find Volume component in the scene.");

        var profile = volume.profile;
        // get reference to the fog volume and store it in fogVolume
        profile.TryGet<FogVolume>(out _fogVolume);

        _fogVolume.distanceMax.overrideState = true;
        _tweenInitialValue = _fogVolume.distanceMax.value;
    }

    public void Play()
    {
        volumeEffectTween = new FloatTween(this, _tweenInitialValue, -1, 2f)
            .setEaseType(EaseType.Linear)
            .setCompletionHandler(tween => StartCoroutine(WaitAndCallEvent()));
        
        volumeEffectTween.start();
    }

    public void setTweenedValue(float value)
    {
        _fogVolume.distanceMax.value = value;
    }

    public float getTweenedValue()
    {
        return _fogVolume.distanceMax.value;
    }

    public object getTargetObject()
    {
        return this;
    }

    IEnumerator WaitAndCallEvent()
    {
        yield return new WaitForSeconds(1f);

        onEffectFinished.Invoke();
    }
}
