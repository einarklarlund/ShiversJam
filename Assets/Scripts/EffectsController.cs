using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectsController : MonoBehaviour
{
    
    [Header("Animation")]
    public List<AnimationClip> animationClips = null;
    public float loopEvery;
    public Action animationCompletedHandler;
    
    Transform _baseTransform;
    Vector3 _initialLocalScale;
    Vector3 _initialLocalPosition;

    void Start()
    {
        _baseTransform = transform.Find("Npc Base");
        if(!_baseTransform)
            return;
        // Component baseComponent = this.FindComponent<SpriteRenderer>();
        // if(!baseComponent)
        //     baseComponent = this.FindComponent<MeshRenderer>();
            
        // _baseTransform = baseComponent.transform;   
                
        _initialLocalScale = _baseTransform.localScale;
        _initialLocalPosition = _baseTransform.localPosition;
    }

    public void ResetEffects()
    {
        _baseTransform.localPosition = _initialLocalPosition;
        _baseTransform.localScale = _initialLocalScale;

        var spriteRenderer = this.FindComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;   
        // var meshRenderer = this.FindComponent<MeshRenderer>(); 
    }

    public void PlayAudioClip(AudioClip clip, float pitch = 1, bool overlapSounds = false)
    {
        var audioSource = this.FindComponent<AudioSource>();

        if(!clip || !audioSource)
        {
            Debug.LogError("[EffectsController] In PlayAudioClip(), the clip parameter must not be null.");
            return;
        }
        if(!audioSource)
        {
            Debug.LogError("[EffectsController] The AudioSource component could not be found.");
            return;
        }

        // stop the audiosource from playing - will prevent sounds from
        // overlapping 
        if(!overlapSounds)
            audioSource.Stop();

        // set the pitch and play the clip
        // audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip);
    }
    
    public void PlayAnimationClip(AnimationClip clip)
    {
        var animation = this.FindComponent<Animation>();

        if(!clip)
        {
            Debug.LogError("[EffectsController] In PlayAnimationClip(), the clip parameter must not be null.");
            return;
        }
        if(!animation)
        {
            Debug.LogError("[EffectsController] The Animation component could not be found.");
            return;
        }

        // stop the current animation so that we can play another one
        animation.Stop();

        // set the animation's clip
        var clipExists = animation.GetClip(clip.name) == clip;
        if(!clipExists)
            animation.AddClip(clip, clip.name);

        animation.clip = clip;

        // play animation
        if(animation.clip)
            animation.Play();
    }

    public void ChangeSpriteTo(Sprite sprite)
    {
        var spriteRenderer = this.FindComponent<SpriteRenderer>();
        if(sprite)
            spriteRenderer.sprite = sprite;
    }

    public Sprite GetCurrentSprite()
    {
        var spriteRenderer = this.FindComponent<SpriteRenderer>();
        return spriteRenderer.sprite;
    }
}