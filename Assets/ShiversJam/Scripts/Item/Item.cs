using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IntrovertStudios.Messaging;
using Zenject;

public abstract class Item : MonoBehaviour
{
    public enum Message
    {
        Interacted,
        Used
    }

    public bool IsHeld => isHeld;
    public IMessageHub<Message> hub;
    [HideInInspector]
    public Interactor owner;

    protected bool isHeld;
    protected ItemManager itemManager;
    protected Interactable interactable;
    protected Rigidbody itemRigidbody;
    protected Collider itemCollider;
    protected Camera playerCamera;

    [SerializeField]
    [Tooltip("Speed of the item when dropped by the player")]
    float _dropSpeed = 2;


    [Inject]
    public void Construct(ItemManager itemManager, PlayerController player)
    {
        this.itemManager = itemManager;
        playerCamera = player.playerCamera;
    }

    void Awake()
    {
        hub = new MessageHub<Message>();
    }
    
    protected void Start()
    {
        itemRigidbody = GetComponent<Rigidbody>();
        itemCollider = GetComponentInChildren<Collider>();
        interactable = GetComponentInChildren<Interactable>();
        if(interactable == null)
            Debug.Log(name);
        
        // connect listeners to hubs
        interactable.hub.Connect<Interactor>(Interactable.Message.Interacted, interactor => OnInteracted(interactor));
        hub.Connect(Message.Used, OnUsed);


        isHeld = false;
    }

    public virtual void PickUp()
    {
        // disable collisions with player
        var playerInventory = FindObjectOfType<PlayerInventory>();
        var playerCollider = playerInventory.GetComponent<Collider>();
        Physics.IgnoreCollision(itemCollider, playerCollider);

        // set parent and position so that the item follows
        // the player transform while its disabled
        transform.SetParent(playerInventory.ItemHoldTransform);
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localPosition = Vector3.zero;

        // set itemcollider gameobject's rotation
        itemCollider.transform.localRotation = Quaternion.Euler(Vector3.zero);

        // disable physics properties
        itemCollider.enabled = false;
        itemRigidbody.isKinematic = true;

        // add to the player's inventory
        playerInventory.Add(this);
        isHeld = true;
    }
    
    public virtual void Drop()
    {
        // set parent and enable gameobject
        transform.SetParent(itemManager.transform);
        gameObject.SetActive(true);

        // enable physics properties
        itemCollider.enabled = true;
        itemRigidbody.isKinematic = false;

        // remove from player inventory
        var playerInventory = FindObjectOfType<PlayerInventory>();
        playerInventory.Remove(this);
        isHeld = false;
        
        // for some reason, the Item stops listening to interactable.hub after
        // SetActive(false) is called. it also can't listen until SetActive(true)
        // is called again.
        // interactable.hub.Connect(Interactable.Message.Interacted, () => hub.Post(Message.Interacted));

        // add drop force to the rigidbody
        Vector3 direction = playerCamera.transform.forward.normalized;
        StartCoroutine("DropInDirection", direction);
    }

    protected abstract void OnUsed();
    protected virtual void OnInteracted(Interactor interactor)
    {
        if(interactor is PlayerController)
        {
            owner = interactor;
            PickUp();
        }
        else if(interactor is Projectile)
        {
            var projectileComponent = interactor as Projectile;
            var rigidbody = GetComponent<Rigidbody>();
            rigidbody.AddForce(projectileComponent.direction * rigidbody.mass * 4, ForceMode.Impulse);
        }
    }

    IEnumerator DropInDirection(Vector3 direction)
    {
        transform.position = playerCamera.transform.position + Vector3.up / 4;
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        
        itemRigidbody.AddForce(_dropSpeed * direction.normalized * itemRigidbody.mass, ForceMode.Impulse);
        
        // wait for 1 second before enabling collision w player
        yield return new WaitForSeconds(1);
        
        var playerCollider = FindObjectOfType<PlayerInventory>().GetComponent<Collider>();
        Physics.IgnoreCollision(itemCollider, playerCollider, false);
    }
}