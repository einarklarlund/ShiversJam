using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using CleverCrow.Fluid.Dialogues;
using CleverCrow.Fluid.Databases;
using CleverCrow.Fluid.Dialogues.Graphs;
using Zenject;
using IntrovertStudios.Messaging;

[RequireComponent(typeof(NpcController))]
public class NpcDialogueController : MonoBehaviour
{
    public DialogueGraph dialogueGraph;
    public DialogueController dialogueController;
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
    bool _useAnimator = false;
    bool _useSprite => !_useAnimator;

    [SerializeField]
    [ShowIf("_useSprite")]
    Sprite _textScrollSprite = null;

    [Inject]
    UIManager _UIManager;
    Animator _animator;
    NpcController _npcController;
    EffectsController _effectsController;
    NpcDialogueScreen _currentDialogueScreen;
    Sprite _initialSprite;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        _npcController = GetComponent<NpcController>();
        _effectsController = GetComponent<EffectsController>();
        _animator = GetComponent<Animator>();
        
        // create new database to store players choices
        var database = new DatabaseInstance();
        dialogueController = new DialogueController(database);

        // listen to dialogueController start and end events 
        dialogueController.Events.Speak.AddListener((actor, text) => OnDialogueStarted(actor, text));
        dialogueController.Events.Choice.AddListener((actor, text, choices) => OnDialogueStarted(actor, text, choices));
        dialogueController.Events.End.AddListener(OnDialogueEnded);

        // listen to text scroll events 
        hub.Connect(Message.TextScrolled, OnTextScrolled);
        hub.Connect(Message.TextScrollEnded, OnTextScrollEnded);

        if(_useSprite)
        {
            _initialSprite = _effectsController.GetCurrentSprite();
        }

    }

    public void BeginDialogue()
    {
        if(!canTalk)
            return;

        Debug.Log("beginning dialogue");
        speaking = true;

        if(_useAnimator)
        {
            _animator.speed = 1;
            _animator.SetBool("Walking", false);
            _animator.SetBool("Speaking", true);
        }

        dialogueController.Play(dialogueGraph);
    }

    public void NextDialogue()
    {
        Debug.Log("continuing to next dialogue");
        dialogueController.Next();
    }

    public void SelectDialogueChoice(int choice)
    {
        dialogueController.SelectChoice(choice);
    }

    void OnDialogueStarted(IActor actor, string text, 
        List<CleverCrow.Fluid.Dialogues.Choices.IChoice> choices = null)
    {
        _currentDialogueScreen = new NpcDialogueScreen(this, actor, text, _textScrollSpeed, choices);

        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenOpened, _currentDialogueScreen);
        hub.Post(Message.DialogueStarted, _currentDialogueScreen);
    }

    void OnDialogueEnded()
    {
        if(_useAnimator){
            _animator.SetBool("Speaking", false);}
        else
            _effectsController.ChangeSpriteTo(_initialSprite);
        
        speaking = false;
        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenCompleted, _currentDialogueScreen);
        Debug.Log("Dialogue screen completed");
    }

    void OnTextScrolled()
    {
        _effectsController.PlayAudioClip(_textScrollAudio, overlapSounds: true);

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
