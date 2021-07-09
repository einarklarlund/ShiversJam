using UnityEngine;
using Zenject;

public class PlayerRaycaster : MonoBehaviour
{
    public LayerMask layerMask;
    
    Transform _cameraTransform;
    Interactable _lastSelection;
    Interactor _interactor;

    void Start()
    {
        _cameraTransform = GetComponentInChildren<Camera>().transform;
        _interactor = GetComponent<Interactor>();
    }

    public bool TryInteractWithLastSelection()
    {
        if(_lastSelection && _lastSelection is Interactable && _lastSelection.Selected)
        {
            (_lastSelection as Interactable).Interact(_interactor);
            _lastSelection = null;
            return true;
        }
    
        return false;
    }
    
    void Update()
    {
        if(_lastSelection)
        {
            // check if the last interactable is in range, unselect it if isn't
            float distance = Vector3.Distance(_lastSelection.transform.position, transform.position);
            if(distance > _lastSelection.maxSelectionDistance)
            {
                _lastSelection.Unselect();
                _lastSelection = null;
            }
            else
            {
                // if the last interactable is in range, continue to raycast
                Raycast();
            }
        }
        else
        {
            Raycast();
        }
    }

    void Raycast()
    {
        RaycastHit hit;
        if(Physics.Raycast(_cameraTransform.position, _cameraTransform.transform.forward, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain", "Props", "Interactable")))
        {
            // get the Interactable component. All GameObjects in the Interactable layer should have one
            Interactable interactable = hit.transform.GetComponent<Interactable>();
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactable")
                && interactable)
            {
                // select the interactable if it's within maxSelectionDistance from the player
                float distance = Vector3.Distance(transform.position, interactable.transform.position);
                if(distance <= interactable.maxSelectionDistance)
                {
                    // if we're looking at a different interactable than the last selection,
                    // then unselect the last selection
                    if(_lastSelection && interactable.GetInstanceID() != _lastSelection.GetInstanceID())
                    {
                        _lastSelection.Unselect();
                    }

                    interactable.Select();
                    _lastSelection = interactable;
                }
            }
            else
            {
                if(_lastSelection)
                    _lastSelection.Unselect();

                _lastSelection = null;
            }
        }
        else
        {
            if(_lastSelection)
                _lastSelection.Unselect();

            _lastSelection = null;
        }
    }
}
