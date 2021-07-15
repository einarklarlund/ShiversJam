using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Zenject;
using IntrovertStudios.Messaging;
using PixelCrushers.DialogueSystem;

[RequireComponent(typeof(NpcController))]
public class NpcDialogueController : MonoBehaviour
{
    // The following variables *should* be set in the inspector for
    // performance reasons, but GameObject.FindComponent will still
    // find them if they aren't set (and still exist in the NPC's
    // hierarchy).
    // Every NPC with the DialogueController needs a dialogueSystemTrigger
    public DialogueSystemTrigger dialogueSystemTrigger;
    // the DialogueSystemEvents will tell the NpcDialogueController when to 
    // start animating, when to talk to the NPC subtitle text gameobject, etc.
    public DialogueSystemEvents dialogueSystemEvents;
    [Tooltip("The transform that the player will look at when talking to an NPC")]
    public Transform viewPointTransform;

    public enum Message
    {
        DialogueStarted,
        TextScrolled,
        TextScrollEnded,
        DialogueEnded
    }

    public IMessageHub<Message> hub;
    public bool canTalk { get; private set; } = true;
    public bool speaking { get; private set; } = false;

    [SerializeField]
    [Tooltip("While the NPC is in dialogue, its dialogue text will scroll at this speed")]
    float _textScrollSpeed = 20;

    [Header("Audio settings")]

    [SerializeField]
    [Tooltip("While the NPC is in dialogue, these audio clips will be used while their dialogue text is scrolling")]
    List<AudioClip> _textScrollAudioClips = null;
    
    [SerializeField]
    [Tooltip("NPC audio scroll clips will be chose randomly if true, or sequentially if false")]
    bool _chooseAudioClipsRandomly;

    [SerializeField]
    [Tooltip("True if the AudioSource.PlayOneShot() method is to be used. Otherwise, AudioSource.Play() will be used")]
    bool _playOneShot = true;

    [SerializeField]
    [Tooltip("True if a new audio clip should interrupt the previous one")]
    bool _interruptAudioClip = false;

    [Inject]
    UIManager _UIManager;
    Animator _animator;
    EffectsController _effectsController;
    UnityUITypewriterEffect _typewriterEffect;
    int _currentAudioClipIndex;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        _effectsController = GetComponent<EffectsController>();
        _animator = GetComponent<Animator>();

        if(!dialogueSystemTrigger)
            dialogueSystemTrigger = this.FindComponent<DialogueSystemTrigger>();
        if(!dialogueSystemTrigger)
            Debug.LogWarning($"[NpcDialogueController] NPC {name} has a NpcDialogueController, but doesn't have a DialogueSystemTrigger in its hierarchy");

        if(!dialogueSystemEvents)
            dialogueSystemEvents = this.FindComponent<DialogueSystemEvents>();
        if(!dialogueSystemEvents)
            Debug.LogWarning($"[NpcDialogueController] NPC {name} has a NpcDialogueController, but doesn't have a DialogueSystemEvents in its hierarchy");

        // listen to conversation events
        dialogueSystemEvents.conversationEvents.onConversationStart.AddListener(OnConversationStarted);
        dialogueSystemEvents.conversationEvents.onConversationEnd.AddListener(OnConversationEnded);
    }

    public void OnConversationStarted(Transform actor)
    {
        if(!canTalk)
            return;

        Debug.Log("beginning dialogue");
        speaking = true;

        _animator.speed = 1;
        _animator.SetBool("Moving", false);
        _animator.SetBool("Speaking", true);

        // find the npcSubtitleText so that we can find its typewriter effect
        var npcSubtitleText = FindObjectOfType<NpcSubtitleText>(true);
        if(!npcSubtitleText)
        {
            Debug.LogWarning($"[NpcDialogueController] NPC {name} couldn't find the NpcSubtitleText component in the hierarchy.");
            return;
        }

        _typewriterEffect = npcSubtitleText.typewriterEffect;
        // listen to text scroll events
        _typewriterEffect.onCharacter.AddListener(OnTextScrolled);
        _typewriterEffect.onEnd.AddListener(OnTextScrollEnded);
        
        // if text scroll audio clips have been defined, the typewriter won't play any sounds.
        // the npc will play the sounds instead.
        if(_textScrollAudioClips != null && _textScrollAudioClips.Count > 0)
        {
            // disable typewriter audio clip because now it'll just be the npc who plays sounds
            _typewriterEffect.audioClip = null;
        }
        else
        {
            // if no text scroll audio clips have been defined, the typewriter effect will use 
            // the default audio clip.
            _typewriterEffect.audioClip = npcSubtitleText.defaultTextScrollAudioClip;
        }

        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenOpened);
    }

    void OnConversationEnded(Transform actor)
    {
        _animator.SetBool("Speaking", false);
    
        // stop listening to text scroll events
        _typewriterEffect.onCharacter.RemoveAllListeners();
        _typewriterEffect.onEnd.RemoveAllListeners();

        // remove audio clip because it'll play on the first character that
        // the typewriter does next
        _typewriterEffect.audioClip = null;
        _typewriterEffect.audioSource.clip = null;
        
        speaking = false;

        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenCompleted);
        Debug.Log("Dialogue screen completed");
    }

    void OnTextScrolled()
    {
        _animator.SetTrigger("TextScrolled");

        // if no audio clips have been defined, the npc wont play any sounds
        if(_textScrollAudioClips == null || _textScrollAudioClips.Count == 0)
            return;

        // wrap audio clip index back to 0 if it has gone out of bounds
        _currentAudioClipIndex = 
            (_currentAudioClipIndex >= _textScrollAudioClips.Count) ?
                0 : _currentAudioClipIndex;

        // choose a random audio clip or sequentially scroll thru the audio clip list, then play it
        AudioClip audioClip;
        if(_chooseAudioClipsRandomly)
            audioClip = _textScrollAudioClips[Random.Range(0, _textScrollAudioClips.Count - 1)];
        else
            audioClip = _textScrollAudioClips[_currentAudioClipIndex++];
        
        _effectsController.PlayAudioClip(audioClip, interruptAudioClip: _interruptAudioClip, playOneShot: _playOneShot);
    }

    void OnTextScrollEnded()
    {
        Debug.Log("End text scroll");
    }
}