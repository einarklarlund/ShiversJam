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

    [SerializeField]
    [Tooltip("Minimum amount of time to wait between typewriter onCharacter events for animation.")]
    float _typewriterAnimationInterval = 0.05f;

    [Inject]
    UIManager _UIManager;
    Animator _animator;
    TextMeshProTypewriterEffect _typewriterEffect;
    float _lastTypewriterAnimationTime;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
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

    // listen to TypeWriterEffect onCharacter events and set animation variables
    public void OnConversationStarted(Transform actor)
    {
        if(!canTalk)
            return;

        speaking = true;

        _animator.speed = 1;
        // _animator.SetBool("Moving", false);
        _animator.SetBool("Speaking", true);

        // find the npcSubtitleText so that we can find its typewriter effect
        var npcSubtitleText = FindObjectOfType<NpcSubtitleText>(true);
        if(!npcSubtitleText)
        {
            Debug.LogWarning($"[NpcDialogueController] NPC {name} couldn't find the NpcSubtitleText component in the hierarchy.");
            return;
        }

        // set _lastTypeWriterAnimationTime to an appropriately early time
        _lastTypewriterAnimationTime = Time.time - _typewriterAnimationInterval;

        _typewriterEffect = npcSubtitleText.GetComponent<TextMeshProTypewriterEffect>();
        // listen to text scroll events
        _typewriterEffect.onCharacter.AddListener(OnCharacter);
        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenOpened);
    }
    
    // animate the character speaking
    void OnCharacter()
    {
        // if the time between the onCharacter event and the last typewriter animation is less
        // than the minimum typewriter animation interval, then don't play any animation
        if(Time.time - _lastTypewriterAnimationTime < _typewriterAnimationInterval)
            return;

        _animator.SetTrigger("OnCharacter");
    }

    // set animation variables and stop listening to onCharacter event
    void OnConversationEnded(Transform actor)
    {
        _animator.SetBool("Speaking", false);

        speaking = false;

        _typewriterEffect.onCharacter.RemoveListener(OnCharacter);

        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenCompleted);
    }
}
