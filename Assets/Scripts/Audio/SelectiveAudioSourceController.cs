using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Prime31.ZestKit;

[RequireComponent(typeof(AudioSource))]
public class SelectiveAudioSourceController : MonoBehaviour
{
    [Tooltip("Set this to true if this component should control audio source looping")]
    public bool controlAudioLooping;

    [ShowIf("controlAudioLooping")]
    public float loopInterval;

    [Tooltip("When the volume is faded out, it will be faded out to this value")]
    public float fadeOutVolume = 0;

    [Tooltip("When the volume is faded in, it will be faded in to this value")]
    public float fadeInVolume = 1;

    [HideInInspector]
    public AudioSource audioSource;

    ITween<float> _fadeInTween;
    ITween<float> _fadeOutTween;
    Coroutine _replayAudioCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // set volume to fadeout volume initially so that it can be faded in
        audioSource.volume = fadeOutVolume;
        
        if(controlAudioLooping)
        {
            audioSource.loop = false;

            // replay audio coroutine needs to be started if play on awake has been set
            if(audioSource.playOnAwake)
            {
                _replayAudioCoroutine = StartCoroutine(ReplayAudioCoroutine(loopInterval));
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
            _fadeInTween = audioSource.ZKvolumeTo(fadeInVolume, fadeInDuration);
            _fadeInTween.start();        
        }
        else
        {
            // immediately set the volume (since no fadein has been specified)
            audioSource.volume = fadeInVolume;
        }

        if(!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        
        if(controlAudioLooping)
        {
            // set audiosource.loop in case its value has changed after Start method was
            // invoked
            audioSource.loop = false;
            
            // start the replay audio coroutine if the _replayAudioCoroutine hasn't been started
            if(_replayAudioCoroutine == null)
                _replayAudioCoroutine = StartCoroutine(ReplayAudioCoroutine(loopInterval));
        }
    }

    // FadeOut audio, stop the audio source, and stop the replaying coroutine
    public void Stop(float fadeOutDuration = 0)
    {
        // stop any fadein/fadeout coroutines
        ZestKit.instance.stopAllTweensWithTarget(audioSource);

        if(fadeOutDuration == 0)
        {
            audioSource.volume = fadeOutVolume;

            // immediately stop audio source and replay coroutine
            if(fadeOutVolume == 0)
            {
                audioSource.Stop();
                StopCoroutine(_replayAudioCoroutine);
            }
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
                            StopCoroutine(_replayAudioCoroutine);
                        }
                    })
                .start();
        }
    }

    // The replaying coroutine.
    IEnumerator ReplayAudioCoroutine(float interval)
    {
        // wait for interval, then play the audio
        yield return new WaitForSeconds(interval);

        audioSource.Play();

        // replay
        StartCoroutine(ReplayAudioCoroutine(interval));
    }
}
