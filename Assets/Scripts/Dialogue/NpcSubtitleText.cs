using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

/*
All this does is provide a way to find the NPC subtitle typewriter effect easily
through FindObjectOfType(NpcSubtitleText).typeWriterEffect
*/

[RequireComponent(typeof(TextMeshProTypewriterEffect))]
public class NpcSubtitleText : MonoBehaviour
{
    public AudioClip defaultTextScrollAudioClip;

    public TextMeshProTypewriterEffect typewriterEffect =>
        GetComponent<TextMeshProTypewriterEffect>();

    public UnityEvent onCharacter;

    void Awake()
    {
        onCharacter = new UnityEvent();
    }

    public void InvokeOnCharacter()
    {
        Debug.Log("NPC subtitle text OnCharacter invoked");
        onCharacter.Invoke();
    }
}
