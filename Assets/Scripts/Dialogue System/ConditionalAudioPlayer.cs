using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class ConditionalAudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    [Tooltip("The name of the variable that will be used as a condition to play this ")]
    public string dialogueLuaConditionVariable;
    public bool playOnlyOnce = true;

    DialogueSystemEvents _dialogueSystemEvents;
    bool _hasPlayed;

    // Start is called before the first frame update
    void Start()
    {
        _dialogueSystemEvents = DialogueManager.instance.GetComponent<DialogueSystemEvents>();
        _dialogueSystemEvents.conversationEvents.onConversationLine.AddListener(OnConversationLine);
    }

    void OnConversationLine(Subtitle subtitle)
    {
        var condition = DialogueLua.GetVariable(dialogueLuaConditionVariable);
        
        if(!condition.hasReturnValue)
        {
            Debug.LogWarning($"[ConditionalAudioPlayer] Couldn't find Dialogue Lua variable {dialogueLuaConditionVariable}");
            return;
        }

        if(condition.asBool && (!playOnlyOnce || (playOnlyOnce && !_hasPlayed)))
        {
            audioSource.PlayOneShot(audioSource.clip);
            DialogueLua.SetVariable(dialogueLuaConditionVariable, true);
            _hasPlayed = true;
        }
    }
}
