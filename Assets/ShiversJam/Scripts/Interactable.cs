using UnityEngine;
using IntrovertStudios.Messaging;
using Zenject;

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
    
    [Tooltip("The maximum distance that the player can be from the selectable in order to select it")]
    public float maxSelectionDistance = 4f;

    public bool selectable = true;

    public bool Selected => _selected;

    protected bool _selected;
    
    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        // move all interactable except for the player's to the interactable layer
        if(!GetComponent<PlayerController>())
            gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void Select()
    {
        if(!selectable)
        {
            Debug.LogWarning($"[Interactable] Tried to select Interactable {name} but its selectable property has been set to false");
            return;
        }

        Debug.Log("selected");
        _selected = true;
        hub.Post(Message.Selected);
    }

    public void Unselect()
    {
        Debug.Log("unselected");
        _selected = false;
        hub.Post(Message.Unselected);
    }

    public virtual void Interact(Interactor interactor)
    {
        Unselect();
        hub.Post(Message.Interacted, interactor);
    }
}