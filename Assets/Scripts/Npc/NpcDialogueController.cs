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
    float _textScrollSpeed = 20;

    [SerializeField]
    AudioClip _textScrollAudio = null;

    [SerializeField]
    bool _useAnimator = true;
    bool _useSprite => !_useAnimator;

    [SerializeField]
    [ShowIf("_useSprite")]
    Sprite _textScrollSprite = null;

    [Inject]
    UIManager _UIManager;
    Animator _animator;
    NpcController _npcController;
    EffectsController _effectsController;
    Sprite _initialSprite;
    UnityUITypewriterEffect _typewriterEffect;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        _npcController = GetComponent<NpcController>();
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

        if(_useSprite)
        {
            _initialSprite = _effectsController.GetCurrentSprite();
        }
    }

    public void OnConversationStarted(Transform actor)
    {
        if(!canTalk)
            return;

        Debug.Log("beginning dialogue");
        speaking = true;

        if(_useAnimator)
        {
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
            // add typewriter audio clip
            if(_textScrollAudio)
                _typewriterEffect.audioClip = _textScrollAudio;
            else
                _typewriterEffect.audioClip = npcSubtitleText.defaultTextScrollAudioClip;
        }

        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenOpened);
    }

    void OnConversationEnded(Transform actor)
    {
        if(_useAnimator)
        {
            _animator.SetBool("Speaking", false);
        
            // stop listening to text scroll events
            _typewriterEffect.onCharacter.RemoveAllListeners();
            _typewriterEffect.onEnd.RemoveAllListeners();
        }
        else
            _effectsController.ChangeSpriteTo(_initialSprite);
        
        speaking = false;

        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenCompleted);
        Debug.Log("Dialogue screen completed");
    }

    void OnTextScrolled()
    {
        if(_useAnimator)
        {
            _animator.SetTrigger("TextScrolled");
        }
        else
        {
            if(_effectsController.GetCurrentSprite() == _initialSprite)
            {
                _effectsController.ChangeSpriteTo(_textScrollSprite);
            }
            else
            {
                _effectsController.ChangeSpriteTo(_initialSprite);
            }
        }
    }

    void OnTextScrollEnded()
    {
        Debug.Log("End text scroll");
        if(_useSprite)
            _effectsController.ChangeSpriteTo(_initialSprite);
    }
}
