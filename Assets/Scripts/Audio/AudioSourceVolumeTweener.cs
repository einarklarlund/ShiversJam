using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31.ZestKit;

public class AudioSourceVolumeTweener : MonoBehaviour
{
    public AudioSource audioSource;

    public float tweenDuration = 4;

    // Start is called before the first frame update
    void Start()
    {
        if(!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    public void TweenVolumeTo(float tweenTo)
    {
        ZestKit.instance.stopAllTweensWithTarget(audioSource);

        SelectiveAudioSourceController controller =  GetComponent<SelectiveAudioSourceController>();
        if(controller)
        {
            controller.StopAllCoroutines();
        }

        audioSource.ZKvolumeTo(tweenTo, tweenDuration).start();
    }
}
