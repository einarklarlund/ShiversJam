using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

/*
this singleton class bubbles up the UnityUITypewriterEffect events of the Npc's Subtitle Text.
This way, any gameobject can find the NpcSubtitleText through the singleton, and listen to
the typewriter events.
*/

[RequireComponent(typeof(UnityUITypewriterEffect))]
public class NpcSubtitleText : MonoBehaviour
{
    public UnityUITypewriterEffect typewriterEffect;
    public UnityEvent onCharacter;
    public UnityEvent onEnd;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        typewriterEffect = GetComponent<UnityUITypewriterEffect>();

        onCharacter = new UnityEvent();
        onEnd = new UnityEvent();

        typewriterEffect.onCharacter.AddListener(() => onCharacter.Invoke());
        typewriterEffect.onEnd.AddListener(() => onEnd.Invoke());
    }
}
