using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

/*
All this does is provide a way to find the NPC subtitle typewriter effect easily
through FindObjectOfType(NpcSubtitleText).typeWriterEffect
*/

[RequireComponent(typeof(UnityUITypewriterEffect))]
public class NpcSubtitleText : MonoBehaviour
{
    public AudioClip defaultTextScrollAudioClip;

    public UnityUITypewriterEffect typewriterEffect =>
        GetComponent<UnityUITypewriterEffect>();
}
