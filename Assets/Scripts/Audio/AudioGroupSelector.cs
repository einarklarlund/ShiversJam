using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioGroupSelector : MonoBehaviour
{
    public SelectiveAudioSourceGroup activeAudioSourceGroup;
    
    // A list of trigger colliders that belong to the active audio source and which currently   
    // have the AudioGroupSelector inside.
    // When the list is emptied, the AudioGroupSelector has exited the activeAudioSourceGroup
    // and is no longer in a SelectiveAudioSourceGroup
    List<Collider> _activeAudioSourceGroupColliders;

    void Start()
    {
        _activeAudioSourceGroupColliders = new List<Collider>();
    }

    // When the player enters a trigger belonging to a new SelectiveAudioSourceGroup, the new audio source
    // group gets faded in and the previous SelectiveAudioSourceGroup gets faded out.
    // When the player enters a trigger collider that already belongs to the same activeAudioSourceGroup,
    // the collider is added to the list of _activeAudioSourceGroupColliders.
    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "AudioSourceGroup")
        {
            var audioSourceGroup = collider.GetComponent<SelectiveAudioSourceGroup>();
            // any gameobject with the AudioSourceGroup tag should have a SelectiveAudioSourceGroup component
            if(!audioSourceGroup)
                Debug.LogWarning($"GameObject {collider.name} has tag \"AudioSourceGroup\" but doesn't have a SelectiveAudioSourceGroup component");

            if(!activeAudioSourceGroup)
            {
                // if activeAudioSourceGroup hasn't been set (the AudioGroupSelector wasn't in a 
                // SelectiveAudioSourceGroup trigger), then set it as the new AudioSourceGroup and fade it in.
                activeAudioSourceGroup = audioSourceGroup;
                activeAudioSourceGroup.SetAudioSourcesEnabled(true, activeAudioSourceGroup.fadeInDuration);
            }
            else if(audioSourceGroup.GetInstanceID() == activeAudioSourceGroup.GetInstanceID())
            {
                // if the trigger collider that was entered is of the same AudioSourceGroup as the 
                // activeAudioSourceGroup, then add the collider to the list of activeAudioSourceGroup colliders.
                _activeAudioSourceGroupColliders.Add(collider);
            }
            else
            {
                // Otherwise, the AudioGroupSelector is exiting the activeAudioSourceGroup and entering a new one.
                // Therfore, the activeAudioSourceGroup must be faded out and be reset to the new audio source group.
                activeAudioSourceGroup.SetAudioSourcesEnabled(false, activeAudioSourceGroup.fadeOutDuration);

                // set the activeAudioSourceGroup to the new audio source group and fade it in
                activeAudioSourceGroup = audioSourceGroup;
                activeAudioSourceGroup.SetAudioSourcesEnabled(true, activeAudioSourceGroup.fadeInDuration);

                // The list of colliders must be reset to just include the collider of the new activeAudioSourceGroup 
                _activeAudioSourceGroupColliders = new List<Collider>() { collider };
            }
        }
    }

    // when the player exits a trigger collider belonging to an AudioSourceGroup, the
    // collider is removed from the list of _activeAudioSourceGroupColliders
    void OnTriggerExit(Collider collider)
    {
        if(collider.tag == "AudioSourceGroup")
        {
            // remove the exited collider from the current audio source colliders
            _activeAudioSourceGroupColliders.Remove(collider);

            // if AudioGroupSelector isn't inside of any triggers from the activeAudioSourceGroup
            // anymore, then the AudioGroupSelector has exited the activeAudioSourceGroup, and the
            // activeAudioSourceGroup must be faded out and reset to null
            if(_activeAudioSourceGroupColliders.Count == 0)
            {
                activeAudioSourceGroup.SetAudioSourcesEnabled(false, activeAudioSourceGroup.fadeOutDuration);
                activeAudioSourceGroup = null;
            }
        }
    }

}
