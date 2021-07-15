using System.Collections;
using Zenject;
using UnityEngine;
using IntrovertStudios.Messaging;
using PixelCrushers.DialogueSystem;
using TheFirstPerson;
using Prime31.ZestKit;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerController : Interactor
{
    public enum Message
    {
        ItemPickedUp,
        CurrentItemDropped
    }

    public IMessageHub<Message> hub;
    public Camera playerCamera;
    public Damageable damageable;
    public DialogueSystemEvents dialogueSystemEvents;
    public FPSController fpsController;
    [HideInInspector]
    public bool speaking = false;

    PlayerInventory _playerInventory;

    // these 2 tweens will be used to look at view points during dialogue
    ITween<Vector3> _cameraRotationTween;
    ITween<Vector3> _playerRotationTween;


    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        _playerInventory = GetComponent<PlayerInventory>();

        var damageable = GetComponent<Damageable>();
        damageable.hub.Connect<int>(Interactable.Message.Damaged, OnDamaged);
        damageable.hub.Connect(Interactable.Message.Killed, OnKilled);

        dialogueSystemEvents = this.FindComponent<DialogueSystemEvents>();
        dialogueSystemEvents.conversationEvents.onConversationStart.AddListener(OnConversationStart);
        dialogueSystemEvents.conversationEvents.onConversationEnd.AddListener(OnConversationEnd);

        fpsController = GetComponent<FPSController>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            _playerInventory.UseCurrentItem();
        }
    }

    void OnConversationStart(Transform actor)
    {
        speaking = true;      

        // disable movement, mouse movement will be enabled after
        // LookAtViewPoint tween is finished
        fpsController.movementEnabled = false;
        fpsController.mouseLocked = false;
        fpsController.mouseLookEnabled = false;

        Debug.Log($"Conversation starting with {actor.name}");
        // if a dialogue view point exists, start the coroutine to look at it.
        var npcDialogueController = actor.GetComponent<NpcDialogueController>();
        if(npcDialogueController && npcDialogueController.viewPointTransform)
        {
            LookAtViewPoint(npcDialogueController.viewPointTransform);
        }
    }

    void OnConversationEnd(Transform actor)
    {
        speaking = false;

        // stop the camera and player rotation tweens
        if(_cameraRotationTween != default && _cameraRotationTween.isRunning())
            _cameraRotationTween.stop();
  
        if(_playerRotationTween != default && _playerRotationTween.isRunning())
            _playerRotationTween.stop();

        // disable the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // enable movement and disable the mouse
        fpsController.movementEnabled = true;
        fpsController.mouseLookEnabled = true;
        fpsController.mouseLocked = true;
    }

    void LookAtViewPoint(Transform viewPoint)
    {
        // find vector that points from camera position to viewpoint position
        Vector3 targetVector = viewPoint.position - playerCamera.transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(targetVector, transform.up);

        // create tweens for horizontal and vertical rotation
        _cameraRotationTween = playerCamera.transform.ZKlocalEulersTo(new Vector3(targetRotation.eulerAngles.x, 0, 0), 0.75f);
        _playerRotationTween = transform.ZKlocalEulersTo(new Vector3(0, targetRotation.eulerAngles.y, 0), 0.75f);

        // start tweens
        _cameraRotationTween.setEaseType(EaseType.QuadOut).start();
        _playerRotationTween.setEaseType(EaseType.QuadOut).start();
    }

    void OnDamaged(int damage)
    {
        Debug.Log($"took {damage} damage");
    }

    void OnKilled()
    {
        Debug.Log("died");
    }

    void OnTriggerEnter(Collider other)
    {
        var hitbox = other.GetComponent<Hitbox>();
        
        if(hitbox)
        {
            GetComponent<Interactable>().Interact(hitbox.onHitInteraction);
        }
    }
}