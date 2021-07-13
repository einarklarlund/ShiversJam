using UnityEngine;
using IntrovertStudios.Messaging;
using PixelCrushers.DialogueSystem;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    public enum Message
    {
        Selected,
        Unselected,
        Interacted,
        Damaged,
        Killed
    }
    public IMessageHub<Message> hub;
    
    // The Usable contains events (onSelect, onDeselect, onUse) that the interactable will use to
    // detect when it has been selected, used, etc.
    public Usable usable;

    public bool selectable = true;

    public bool Selected => _selected;

    protected bool _selected;
    
    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        // move all interactables (except the player) to the interactable layer
        // and make sure that they have a Usable component
        if(!GetComponent<PlayerController>())
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            if(!usable)
                usable = GetComponent<Usable>();
            if(!usable)
                usable = gameObject.AddComponent<Usable>();
                
            usable.events.onUse.AddListener(OnUse);
            usable.events.onSelect.AddListener(OnSelect);
            usable.events.onDeselect.AddListener(OnDeselect);
        }
    }

    public void OnSelect()
    {
        if(!selectable)
        {
            Debug.LogWarning($"[Interactable] Tried to select Interactable {name} but its selectable property has been set to false");
            return;
        }

        _selected = true;
        hub.Post(Message.Selected);
    }

    public void OnDeselect()
    {
        _selected = false;
        hub.Post(Message.Unselected);
    }

    public virtual void OnUse()
    {
        // whenever OnUse is called, it's because the player has selected and used the 
        // interactable. Therefore, we can use Interact(player) when usable.events.onUse
        // event is called.
        Interactor player = FindObjectOfType<PlayerController>();
        Interact(player);
    }

    public virtual void Interact(Interactor interactor)
    {
        hub.Post(Message.Interacted, interactor);
    }
}