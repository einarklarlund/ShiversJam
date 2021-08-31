using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

// cycles through the materials list and applies the next material to the meshrenderer
// when the Clock dialogue variable is changed
public class MaterialCycler : MonoBehaviour
{
    public List<Material> materials;
    public MeshRenderer meshRenderer;
    
    DialogueSystemEvents _dialogueSystemEvents;
    int _clockValue;
    int _materialIndex;

    // Start is called before the first frame update
    void Start()
    {
        if(!meshRenderer)
            meshRenderer = GetComponent<MeshRenderer>();

        _dialogueSystemEvents = DialogueManager.instance.GetComponent<DialogueSystemEvents>();
        _dialogueSystemEvents.conversationEvents.onConversationEnd.AddListener(OnConversationEnd);

        _clockValue = DialogueLua.GetVariable("Clock").asInt;
    }

    void OnConversationEnd(Transform actor)
    {
        Debug.Log("OnConversationEnd");
        // get the clock value from Dialogue system
        var newClockValue = DialogueLua.GetVariable("Clock").asInt;
        if(newClockValue != _clockValue)
        {
            // update the meshrenderer's material
            if(_materialIndex < materials.Count)
            {
                meshRenderer.material = materials[_materialIndex];
                _materialIndex++;
            }

            // update cloack value var
            _clockValue = newClockValue;
        }
    }
}