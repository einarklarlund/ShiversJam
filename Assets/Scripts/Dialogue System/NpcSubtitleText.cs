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
    public AudioSource audioSource;

    public TextMeshProTypewriterEffect typewriterEffect =>
        GetComponent<TextMeshProTypewriterEffect>();

        
    [SerializeField]
    [Tooltip("Minimum amount of time to wait between typewriter sfx. Must be set to an appropriately high amount or else sounds will overlap too much.")]
    float _mininumTypewriterSFXDuration = 0.075f;

    DialogueSystemEvents _dialogueSystemEvents;
    string _currentSpeaker;
    List<AudioClip> _currentSpeakerVoiceClips;
    float _lastTypewriterSFXTime;

    void Start()
    {
        // add dialogue system events and listen to OnConversationStart
        _dialogueSystemEvents = gameObject.AddComponent<DialogueSystemEvents>();
        _dialogueSystemEvents.conversationEvents.onConversationStart.AddListener(OnConversationStart);
        _dialogueSystemEvents.conversationEvents.onConversationLine.AddListener(OnConversationLine);
        _dialogueSystemEvents.conversationEvents.onConversationEnd.AddListener(OnConversationEnd);
    }

    // set the current speaker and its voice clips once the conversation starts
    void OnConversationStart(Transform actor)
    {
        _currentSpeaker = actor.FindComponent<DialogueActor>().actor;
        SetSpeakerVoiceClips();

        // set _lastTypewriterSFXTime so that sfx will play on the first character
        _lastTypewriterSFXTime = Time.time - _mininumTypewriterSFXDuration;

        // listen to onCharacter event
        typewriterEffect.onCharacter.AddListener(OnCharacter);
    }

    // current speaker and voice clips must also be set on each convresation line,
    // in case the speaker changes during the conversation
    void OnConversationLine(Subtitle subtitle)
    {
        // do nothing if the speaker hasn't changed
        if(_currentSpeaker == subtitle.speakerInfo.Name)
            return;

        // otherwise set the current speaker and its voice clips
        _currentSpeaker = subtitle.speakerInfo.Name;
        SetSpeakerVoiceClips();

        // set _lastTypewriterSFXTime so that sfx will play on the first character
        _lastTypewriterSFXTime = Time.time - _mininumTypewriterSFXDuration;
    }

    // find the VoiceNumber field of the current speaker and search thru Resources
    // to find their voice clips
    void SetSpeakerVoiceClips()
    {
        // find the speaker's VoiceNumber field
        var voiceNumber = 
            DialogueLua.GetActorField(_currentSpeaker, "VoiceNumber").asInt;

        // set typewriter audio clip to defaultTextScrollAudioClip if we couldn't find the VoiceNumber
        if(voiceNumber == 0)
        {
            Debug.Log($"[NpcSubtitleText] Couldn't find VoiceNumber field for actor {_currentSpeaker}");
            _currentSpeakerVoiceClips = new List<AudioClip>()  { defaultTextScrollAudioClip };
            return;
        }

        // load a list of voice clips from Resources using the speaker's VoiceNumber field
        List<AudioClip> voiceClips = new List<AudioClip>();
        AudioClip nextVoiceClip;
        int i = 1;
        do
        {
            nextVoiceClip = (AudioClip) Resources.Load($"Voice {voiceNumber} ({i++})");
            if(nextVoiceClip)
                voiceClips.Add(nextVoiceClip);
        } while (nextVoiceClip != null);
        
        // set _currentSpeakerVoiceClips to defaultTextScrollAudioClip if we couldn't find any voice clips
        if(voiceClips.Count == 0)
        {
            Debug.LogWarning($"[NpcSubtitleText] Couldn't find any voice clips for Voice {voiceNumber}");
            _currentSpeakerVoiceClips = new List<AudioClip>() { defaultTextScrollAudioClip };
            return;
        }

        // otherwise, set the _currentSpeakerVoiceClips to the loaded resources
        _currentSpeakerVoiceClips = voiceClips;
    }

    // play the voice clip onCharacter, but not too frequently
    void OnCharacter()
    {
        // if the time between the onCharacter event and the last typewriter sfx is less
        // than the minimum typewriter sfx duration, then don't play any sounds
        if(Time.time - _lastTypewriterSFXTime < _mininumTypewriterSFXDuration)
            return;

        // otherwise, SFX will play on this onCharacter event
        _lastTypewriterSFXTime = Time.time;
        
        // play a random audio clip from the list of the speaker's audio clips
        int randomIndex = Random.Range(0, _currentSpeakerVoiceClips.Count);
        audioSource.PlayOneShot(_currentSpeakerVoiceClips[randomIndex]);
    }

    // reset current speaker variables and stop listening to onCharacter
    void OnConversationEnd(Transform actor)
    {
        _currentSpeaker = null;
        _currentSpeakerVoiceClips = null;

        typewriterEffect.onCharacter.RemoveListener(OnCharacter);
    }

    void SetTypeWriterAudioClipToDefault()
    {
        typewriterEffect.audioClip = defaultTextScrollAudioClip;
        typewriterEffect.alternateAudioClips = null;
    }
}
