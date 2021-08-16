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

    [Header("Get Up From Bed Cutscene")]
    public Transform cameraInBedTransform;
    public bool playGetUpFromBedTweens = true;
    
    [HideInInspector]
    public bool speaking = false;

    PlayerInventory _playerInventory;

    // the position that the camera will tween to after getting up from bed
    Vector3 _uprightCameraPosition;

    // these 2 tweens will be used to look at view points during dialogue
    ITween<Vector3> _cameraRotationTween;
    ITween<Vector3> _playerRotationTween;

    [Inject]
    GameManager _gameManager;

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
        
        _gameManager.onGameStateChanged.AddListener(OnGameStateChanged);

        if(DialogueLua.GetVariable("NewGame").asBool == true)
        {   
            // set NewGame variable to false so that the get up from bed 
            // tweens aren't played on a loaded game
            DialogueLua.SetVariable("NewGame", false);

            if(playGetUpFromBedTweens)
            {
                // play the get up from bed tweens
                GetUpFromBed();
            }
        }
    }

    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            _playerInventory.UseCurrentItem();
        }
        
        if((_gameManager.CurrentGameState == GameManager.GameState.Running || 
                _gameManager.CurrentGameState == GameManager.GameState.Paused)
            && Input.GetKeyDown(KeyCode.Escape)
            && !speaking )
        {
            _gameManager.TogglePause();
        }
    }

    public void SetMovementEnabled(string setTo)
    {
        if(setTo == "false")
        {
            SetMovementEnabled(false);
        }
        else if(setTo == "true")
        {
            SetMovementEnabled(true);
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        // set movement and mouse look enablers
        fpsController.movementEnabled = enabled;
        fpsController.mouseLookEnabled = enabled;
        fpsController.mouseLocked = enabled;

        // disable selector
        var selector = GetComponentInChildren<Selector>();
        if(selector)
            selector.enabled = enabled;
    }

    public void GetUpFromBed()
    {
        // record the position that the player camera will tween to 
        // while gettin up from bed
        _uprightCameraPosition = playerCamera.transform.localPosition;

        // put the player in bed
        playerCamera.transform.position = cameraInBedTransform.position;
        playerCamera.transform.rotation = cameraInBedTransform.rotation;

        // create an empty tween so that the player gets out of bed a few seconds after starting the game
        ITween<Vector3> emptyTween = 
            playerCamera.transform.ZKpositionTo(playerCamera.transform.position, 2f);

        // after waiting a few seconds, the player will turn their head
        ITween<Vector3> turnHead = 
            playerCamera.transform.ZKlocalEulersTo(Vector3.up, 5f)
                .setEaseType(EaseType.QuartInOut);

        // then the player will get up from the bed
        ITween<Vector3> getUp = 
            playerCamera.transform.ZKlocalPositionTo(_uprightCameraPosition, 2f)
                .setEaseType(EaseType.BackInOut);

        // set tween order
        emptyTween.setNextTween(turnHead);
        turnHead.setNextTween(getUp);

        // disable the fpsController and enable it once the tweens are done
        fpsController.enabled = false;
        getUp.setCompletionHandler(iTween => fpsController.enabled = true);

        // start the sequence of tweens
        emptyTween.start();
    }

    public void LookAtViewPoint(Transform viewPoint, float duration = 0.75f)
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

    public void DestroySelector()
    {
        var selector = GetComponentInChildren<Selector>();
        Destroy(selector);
    }
    
    void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState currentState)
    {
        if(currentState == GameManager.GameState.Paused)
        {
            SetMovementEnabled(false);
        }
        else if(currentState == GameManager.GameState.Running)
        {
            SetMovementEnabled(true);
        }
    }

    void OnConversationStart(Transform actor)
    {
        if(speaking)
            return;

        speaking = true;      

        // disable movement, mouse movement will be enabled after
        // LookAtViewPoint tween is finished
        SetMovementEnabled(false);

        Debug.Log($"[PlayerController] Conversation starting with {actor.name}");
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

        // enable movement and disable the mouse
        SetMovementEnabled(true);
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
