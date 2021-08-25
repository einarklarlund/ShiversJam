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

    [Tooltip("Fade the audio in and out at a specified period")]
    public bool fadesInOut = false;

    [ShowIf("fadesInOut")]
    [Tooltip("The duration of which one sequence of fading in and out lasts")]
    public float fadeInOutDuration;

    [ShowIf("fadesInOut")]
    [Tooltip("The period at which the fade-in-and-out effect repeats")]
    public float fadeInOutPeriod = 5;

    [ShowIf("fadesInOut")]
    [Tooltip("A random deviation for the fade-in-and-out period")]
    public float periodDeviation = 1;

    // the base volume that will be tweened by FadeCoroutine.
    // does not represent the final volume value that audioSource will have if
    // the FadeInOutCoroutine is being used (if fadesInOut is true)
    float _baseVolume;
    Coroutine _fadeAudioCoroutine;
    Coroutine _fadeAudioInOutCoroutine;
    Coroutine _replayAudioCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // set volume to fadeout volume initially so that it can be faded in
        audioSource.volume = fadeOutVolume;
        _baseVolume = fadeOutVolume;
        
        if(controlAudioLooping)
        {
            audioSource.loop = false;

            // replay audio coroutine needs to be started if play on awake has been set
            if(audioSource.playOnAwake)
            {
                _replayAudioCoroutine = StartCoroutine(ReplayAudioCoroutine(loopInterval));
            }
        }

        if(fadesInOut)
        {
            _fadeAudioInOutCoroutine = StartCoroutine(FadeAudioInOutCouroutine());
        }
    }

    // Play the audio source and start replaying coroutine if looping is to
    // be controlled by this component isntead of by the audio source component.
    // The replaying coroutine will also call this method.
    public void Play(float fadeInDuration = 0)
    {
        Debug.Log($"playing {name}");

        // stop any fadein/fadeout coroutines 
        if(_fadeAudioCoroutine != null)
            StopCoroutine(_fadeAudioCoroutine);  
        // ZestKit.instance.stopAllTweensWithTarget(audioSource);

        if(fadeInDuration > 0)
        {
            // start the fadein coroutine
            _fadeAudioCoroutine = StartCoroutine(FadeAudioCoroutine(true, fadeInDuration));
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

        if(fadesInOut)
        {
            // start the fadeInOut coroutine if it hasn't been started
            if(_fadeAudioInOutCoroutine == null)
                _fadeAudioInOutCoroutine = StartCoroutine(FadeAudioInOutCouroutine());
        }
    }

    // FadeOut audio, stop the audio source, and stop the replaying coroutine
    public void Stop(float fadeOutDuration = 0)
    {
        // stop any fadein/fadeout coroutines
        StopCoroutine(_fadeAudioCoroutine);
        // ZestKit.instance.stopAllTweensWithTarget(audioSource);

        if(fadeOutDuration == 0)
        {
            audioSource.volume = fadeOutVolume;

            // immediately stop audio source and replay coroutine
            audioSource.Stop();
            StopCoroutine(_replayAudioCoroutine);
        }
        else
        {
            // audioSource.ZKvolumeTo(fadeOutVolume, fadeOutDuration)
            //     .setCompletionHandler(itween => 
            //         {
            //             if(fadeOutVolume == 0)
            //             {
            //                 audioSource.Stop();
            //                 StopCoroutine(_replayAudioCoroutine);
            //             }
            //         })
            //     .start();
            _fadeAudioCoroutine = StartCoroutine(FadeAudioCoroutine(false, fadeOutDuration));
        }
    }

    IEnumerator FadeAudioCoroutine(bool fadeIn, float fadeDuration)
    {
        int totalFrames = (int) (fadeDuration / Time.fixedDeltaTime); // duration of fade in frames
        var fadeVolumeTo = fadeIn ? fadeInVolume : fadeOutVolume; // volume to tween to
        // change in volume per frame (total change in volume divided by total amount of frames)
        var deltaVolume = (fadeVolumeTo - audioSource.volume) / totalFrames;
        Debug.Log($"totalFrames {totalFrames}");

        while(totalFrames-- > 0)
        {
            // tween _baseVolume if the this SelectiveAudioSourceController has been
            // set to fade in and out (final volume will be set in FadeAudioInOutCoroutine).
            // othewise, tween the audiosource volume instead
            if(fadesInOut)
                _baseVolume += deltaVolume;
            else
                audioSource.volume += deltaVolume;

            yield return new WaitForFixedUpdate();
        }
        
        // if this audio source controller has been set to be silent on fadeout then 
        // stop the audiosource and stop replay coroutine after fade out
        if(fadeVolumeTo == 0)
        {
            audioSource.Stop();
            StopCoroutine(_replayAudioCoroutine);
        }
    }

    IEnumerator FadeAudioInOutCouroutine()
    {
        int totalFrames = Mathf.RoundToInt(fadeInOutDuration / Time.fixedDeltaTime); // duration of fade in frames
        // magnitude of change in volume per frame (volume changes from 0-to-_baseVolume using half of total frames
        // then from fadeInVolume-to-0 using remaining total frames) 
        var deltaVolume = _baseVolume / (totalFrames / 2);
        int framesLeft = totalFrames;

        Debug.Log($"{name} totalFrames {totalFrames}");

        // for half of the total frames, audioSource.volume goes from 0 to baseVolume
        audioSource.volume = 0;
        while(framesLeft-- > totalFrames / 2)
        {
            audioSource.volume += deltaVolume;
            yield return new WaitForFixedUpdate();
        }

        // for the rest of the total frames, audioSource.volume goes from baseVolume to 0
        while(framesLeft-- > 0)
        {
            audioSource.volume -= deltaVolume;
            yield return new WaitForFixedUpdate();
        }

        audioSource.volume = 0;

        // wait and then restart the coroutine
        yield return new WaitForSeconds(fadeInOutPeriod - fadeInOutDuration + 2 * (0.5f - Random.value) * periodDeviation);
        _fadeAudioInOutCoroutine = StartCoroutine(FadeAudioInOutCouroutine());
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
