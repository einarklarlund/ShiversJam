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
    // dialogueSystemTrigger should be set in the inspector, but
    // NpcDialogueController will try to find it in the NPC's gameObject
    // hierarchy if it's not set in the inspector.
    public DialogueSystemTrigger dialogueSystemTrigger;
    // dialogueSystemEvents should also be set in the inspector, but
    // NpcDialogueController will try to find it in the NPC's gameObject
    // hierarchy if it's not set in the inspector.
    public DialogueSystemEvents dialogueSystemEvents;

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
    NpcSubtitleText _npcSubtitleText;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        _npcController = GetComponent<NpcController>();
        _effectsController = GetComponent<EffectsController>();
        _animator = GetComponent<Animator>();

        _npcSubtitleText = FindObjectOfType<NpcSubtitleText>();

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
        }

        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenOpened);
        
        _npcSubtitleText = FindObjectOfType<NpcSubtitleText>(true);
        Debug.Log($"_npcSubtitleText is null? {_npcSubtitleText == null}");
        Debug.Log($"onCharacter is null? {_npcSubtitleText.onCharacter == null}. onEnd is null? {_npcSubtitleText.onEnd == null}");
        // listen to text scroll events
        _npcSubtitleText.onCharacter.AddListener(OnTextScrolled);
        _npcSubtitleText.onEnd.AddListener(OnTextScrollEnded);
    }

    void OnConversationEnded(Transform actor)
    {
        if(_useAnimator){
            _animator.SetBool("Speaking", false);}
        else
            _effectsController.ChangeSpriteTo(_initialSprite);
        
        speaking = false;
        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenCompleted);
        Debug.Log("Dialogue screen completed");
        
        // stop listening to text scroll events
        _npcSubtitleText.onCharacter.RemoveAllListeners();
        _npcSubtitleText.onEnd.RemoveAllListeners();
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
