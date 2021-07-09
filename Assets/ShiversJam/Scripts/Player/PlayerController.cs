using Zenject;
using UnityEngine;
using IntrovertStudios.Messaging;
using TheFirstPerson;

[RequireComponent(typeof(PlayerInventory), typeof(PlayerRaycaster))]
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

    PlayerRaycaster _playerRaycaster;
    PlayerInventory _playerInventory;


    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        _playerRaycaster = GetComponent<PlayerRaycaster>();
        _playerInventory = GetComponent<PlayerInventory>();

        var damageable = GetComponent<Damageable>();
        damageable.hub.Connect<int>(Interactable.Message.Damaged, OnDamaged);
        damageable.hub.Connect(Interactable.Message.Killed, OnKilled);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(Input.GetButtonDown("Use"))
        {
            _playerRaycaster.TryInteractWithLastSelection();
        }
        else if(Input.GetButtonDown("Fire1"))
        {
            _playerInventory.UseCurrentItem();
        }
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
