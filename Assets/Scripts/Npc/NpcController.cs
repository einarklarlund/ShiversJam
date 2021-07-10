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

    [HideInInspector]
    public List<AudioClip> audioClips;
    public IMessageHub<NpcController.Message> hub;

    protected EffectsController effectsController;

    [Inject]
    NpcManager _npcManager;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    protected void Start()
    {
        // get controllers that npc controller will communicate w/
        effectsController = GetComponent<EffectsController>();
        Interactable interactable = this.FindComponent<Interactable>();
        if(!interactable)
            Debug.LogWarning($"[NpcController] {name} needs to have an Interactable component in its hierarchy.");

        // connect the Interactable messages from the Interactable's hub to
        // the NpcController Messages in the NpcController's hub
        interactable.hub.Connect(Interactable.Message.Selected, () => hub.Post(NpcController.Message.Selected));
        interactable.hub.Connect(Interactable.Message.Unselected, () => hub.Post(NpcController.Message.Unselected));
        interactable.hub.Connect<Interactor>(Interactable.Message.Interacted, OnInteracted);

        // connect OnInteracted handler
        hub.Connect<Interactor>(Message.Interacted, OnInteracted);
    }
    
    protected virtual void OnInteracted(Interactor interactor)
    {
        Debug.Log($"NPC {name} recevied interaction from {interactor.name}.");
        if(npcDialogueController && interactor is PlayerController
            && !npcDialogueController.speaking)
        {
            npcDialogueController.BeginDialogue();
        }
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
}
