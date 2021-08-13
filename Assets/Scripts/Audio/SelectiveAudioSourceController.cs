using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Prime31.ZestKit;

[RequireComponent(typeof(AudioSource))]
public class SelectiveAudioSourceController : MonoBehaviour
{
    [Tooltip("Set this to true if the audio source should only be heard when the player has entered the selective audio source group trigger")]
    public bool canOnlyBeHeardInGroupCollider;

    [Tooltip("Set this to true if this component should control audio source looping")]
    public bool controlAudioLooping;

    [ShowIf("controlAudioLooping")]
    public float loopInterval;

    [Tooltip("When the volume is faded out, it will be faded out to this value")]
    public float fadeOutVolume = 0;

    [HideInInspector]
    public AudioSource audioSource;

    float _initialVolume;
    ITween<float> _fadeInTween;
    ITween<float> _fadeOutTween;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        _initialVolume = audioSource.volume;

        // set volume to 0 so that the volume must be set by the Play() method
        if(canOnlyBeHeardInGroupCollider)
        {
            audioSource.volume = 0;
        }
        
        if(controlAudioLooping)
        {
            audioSource.loop = false;

            // replay audio coroutine needs to be started if play on awake has been set
            // and if the audio source should be heard outside of the collider
            if(audioSource.playOnAwake && !canOnlyBeHeardInGroupCollider)
            {
                StartCoroutine(ReplayAudioAfterInterval(loopInterval));
            }
        }
    }

    // Play the audio source and start replaying coroutine if looping is to
    // be controlled by this component isntead of by the audio source component.
    // The replaying coroutine will also call this method.
    public void Play(float fadeInDuration = 0)
    {
        Debug.Log($"playing {name}");

        // stop any fadein/fadeout tweens on the audio source
        ZestKit.instance.stopAllTweensWithTarget(audioSource);

        if(fadeInDuration > 0)
        {
            // start the fadein tween
            _fadeInTween = audioSource.ZKvolumeTo(_initialVolume, fadeInDuration);
            _fadeInTween.start();
        }
        else
        {
            // immediately set the volume (since no fadein has been specified)
            audioSource.volume = _initialVolume;
        }

        audioSource.Play();
        
        if(controlAudioLooping)
        {
            // set audiosource.loop in case its value has changed after Start method was
            // invoked
            audioSource.loop = false;
            
            // replay the audio after loopInterval seconds
            StartCoroutine(ReplayAudioAfterInterval(loopInterval));
        }
    }

    // FadeOut audio, stop the audio source, and stop the replaying coroutine
    public void Stop(float fadeOutDuration = 0)
    {
        // stop any fadein/fadeout tweens on the audio source
        ZestKit.instance.stopAllTweensWithTarget(audioSource);

        if(fadeOutDuration == 0)
        {
            audioSource.volume = fadeOutVolume;

            // immediately stop audio source and replay coroutine
            audioSource.Stop();
            StopAllCoroutines();
        }
        else
        {
            // if this audio source controller has been set to be silent on fadeout then 
            // stop the audiosource and stop replay coroutine after fade out
            audioSource.ZKvolumeTo(fadeOutVolume, fadeOutDuration)
                .setCompletionHandler(itween => 
                    {
                        if(fadeOutVolume == 0)
                        {
                            audioSource.Stop();
                            StopAllCoroutines();
                        }
                    })
                .start();
        }
    }

    // The replaying coroutine.
    IEnumerator ReplayAudioAfterInterval(float interval)
    {
        // wait for interval, then play the audio
        yield return new WaitForSeconds(interval);

        audioSource.Play();

        // replay
        StartCoroutine(ReplayAudioAfterInterval(interval));
    }
}
