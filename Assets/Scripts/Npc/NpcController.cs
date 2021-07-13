using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using NaughtyAttributes;
using IntrovertStudios.Messaging;

[RequireComponent(typeof(EffectsController))]
public abstract class NpcController : Interactor
{    
    public class Factory : PlaceholderFactory<UnityEngine.Object, NpcController> { }

    public enum Message
    {
        BehaviourStarted,
        BehaviourEnded,
        BehaviourChanged,
        AttackStarted,
        TargetAcquired,
        Selected,
        Unselected,
        Interacted,
    }

    
    [Header("NPC Controller Parameters")]
    public NpcDialogueController npcDialogueController;
    public Animator animator;
    public float pitchModifier;

    [Tooltip("if true, the NPC will turn to face the player when it is selected.")]
    public bool facePlayerOnSelect = true;
    
    [ShowIf("facePlayerOnSelect")]
    public float rotationSpeed = 5;

    [HideInInspector]
    public List<AudioClip> audioClips;
    public IMessageHub<NpcController.Message> hub;

    protected EffectsController effectsController;

    [Inject]
    NpcManager _npcManager;

    Interactable _interactable;
    PlayerController _player;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    protected void Start()
    {
        // get controllers that npc controller will communicate w/
        effectsController = GetComponent<EffectsController>();
        _interactable = this.FindComponent<Interactable>();
        if(!_interactable)
            Debug.LogWarning($"[NpcController] {name} needs to have an Interactable component in its hierarchy.");

        // connect the Interactable messages from the Interactable's hub to
        // the NpcController Messages in the NpcController's hub
        _interactable.hub.Connect(Interactable.Message.Selected, OnSelected);
        _interactable.hub.Connect(Interactable.Message.Unselected, OnUnselected);
        _interactable.hub.Connect<Interactor>(Interactable.Message.Interacted, OnInteracted);

        // connect OnInteracted handler
        hub.Connect<Interactor>(Message.Interacted, OnInteracted);

        // find player so that NPC can face the player when it's selected
        _player = FindObjectOfType<PlayerController>();
    }

    public void PlayRandomAudioClip(float pitchMod = 0.2f)
    {
        if(audioClips == null || audioClips.Count == 0)
        {
            Debug.LogError("[NpcController] The audioClips list has not been initialized correctly.");
        }
        
        var audioClip = audioClips[(int) (Random.value * audioClips.Count)];

        float pitch = 1 + (UnityEngine.Random.value - 0.5f) * pitchMod + pitchModifier;
        effectsController.PlayAudioClip(audioClip, pitch, true);
    }

    public void PlayAudioClip(AnimationEvent animationEvent)
    {
        float pitchMod = animationEvent.floatParameter == 0 ? 0.2f : animationEvent.floatParameter;
        string clipName = animationEvent.stringParameter;

        if(audioClips == null || audioClips.Count == 0)
        {
            Debug.LogError("[NpcController] The audioClips list has not been initialized correctly.");
        }

        var audioClip = audioClips.Find(clip => clip.name == clipName);

        if(audioClip)
        {
            float pitch = 1 + (UnityEngine.Random.value - 0.5f) * pitchMod + pitchModifier;
            effectsController.PlayAudioClip(audioClip, pitch);
        }
        else
        {
            Debug.LogError($"[NpcController] Could not find audio clip with name {name}.");
        }
    }

    protected virtual void OnSelected()
    {
        if(facePlayerOnSelect)
        {
            StartCoroutine(TurnTowardsPlayer());
        }

        hub.Post(NpcController.Message.Selected);
    }

    protected virtual void OnUnselected()
    {
        if(facePlayerOnSelect)
        {
            StopAllCoroutines();
        }

        hub.Post(NpcController.Message.Unselected);
    }

    protected virtual void OnInteracted(Interactor interactor)
    {
        Debug.Log($"NPC {name} recevied interaction from {interactor.name}.");
    }

    IEnumerator TurnTowardsPlayer()
    {
        while(_interactable.Selected)
        {
            // find vector that points from NPC position to player position
            Vector3 towardsPlayer = _player.transform.position - transform.position;
            // rotate the NPCs current forward direction towards the towardsPlayer vector
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, towardsPlayer, rotationSpeed / 100, 0);
            // apply the new rotation
            transform.rotation = Quaternion.LookRotation(newDirection);
            
            yield return null;
        }
    }
}
