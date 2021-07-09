using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Coffee.UIEffects;

public class DialoguePopUp : MonoBehaviour
{
    [HideInInspector]
    public bool finishedScrolling;

    [Inject]
    UIManager _UIManager;

    [SerializeField]
    Text _dialogueText = null;

    [SerializeField]
    Text _actorText = null;

    [SerializeField]
    List<DialogueButton> _choiceButtons = null;

    int _choice = -1;

    void Start()
    {
        gameObject.SetActive(false);
        _choiceButtons = new List<DialogueButton>(GetComponentsInChildren<DialogueButton>());
    }

    public void Show(IDialogueScreen dialogueScreen)
    {
        gameObject.SetActive(true);

        _actorText.text = $"{dialogueScreen.actor.DisplayName}:";

        if(dialogueScreen.choices != null)
        {
            for(int i = 0; i < dialogueScreen.choices.Count && i < _choiceButtons.Count; i++)
            {
                _choiceButtons[i].Show(dialogueScreen.choices[i].Text);
            }
        }
        else
        {
            foreach(var button in _choiceButtons)
            {
                button.gameObject.SetActive(false);
            }
        }

        StopAllCoroutines();
        StartCoroutine(ScrollText(dialogueScreen));
        StartCoroutine(WaitForScrollAndInput(dialogueScreen));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    IEnumerator ScrollText(IDialogueScreen dialogueScreen)
    {
        finishedScrolling = false;
        string dialogue = dialogueScreen.text;

        int dialogueIndex = 0;
        while(dialogueIndex < dialogue.Length)
        {
            // skip to the end of the dialogue if skipTextScroll has been set,
            // otherwise just display the next character.
            dialogueIndex = finishedScrolling ? dialogue.Length : dialogueIndex + 1;
            
            // set the on screen text
            _dialogueText.text = dialogue.Substring(0, dialogueIndex);

            // alert the npccontroller that the dialogue text has scrolled, so that the npccontroller
            // can do some fun animation shit and sound stuff
            char lastChar = _dialogueText.text[dialogueIndex - 1];
            if(!Char.IsPunctuation(lastChar) && !Char.IsSymbol(lastChar) && !Char.IsWhiteSpace(lastChar))
                dialogueScreen.ScrollText();

            // scroll the text at the rate of textSpeed chars per second 
            yield return new WaitForSeconds(1 / dialogueScreen.textSpeed);
        }

        finishedScrolling = true;
    }

    IEnumerator WaitForScrollAndInput(IDialogueScreen currentDialogueScreen)
    {
        yield return null;
        Debug.Log("waiting for scroll and input");

        // wait for the text scroll to finish or for the user input 
        while((!Input.GetButtonDown("Fire1") & !Input.GetButtonDown("Use")) && !finishedScrolling)
        {
            yield return null;
        }
        
        currentDialogueScreen.EndTextScroll();

        // turn on finishedScrolling in case Fire1/Use was called
        finishedScrolling = true;

        // wait for a frame before starting other coroutines so that the inputs from
        // the WaitForScrollAndInput coroutine aren't registered twice
        yield return null;
        
        if(currentDialogueScreen.choices != null)
        {
            // wait for player to select a choice if this dialogue screen has choices.
            StartCoroutine(WaitForChoiceInput(currentDialogueScreen));
        }
        else
        {
            // wait for player to press continue input (Fire1/Use) before going to next dialogue screen
            StartCoroutine(WaitForContinueInput(currentDialogueScreen));
        }
    }

    IEnumerator WaitForContinueInput(IDialogueScreen currentDialogueScreen)
    {
        while(!Input.GetButtonDown("Fire1") && !Input.GetButtonDown("Use"))
        {
            yield return null;
        }

        // start the next dialogue screen
        currentDialogueScreen.NextDialogueScreen();
    }

    IEnumerator WaitForChoiceInput(IDialogueScreen currentDialogueScreen)
    {
        int choice = -1;

        // wait for use to make a choice
        while(choice == -1)
        {
            // check each button to see if its been chosen
            for(int i = 0; i < _choiceButtons.Count; i++)
            {
                if(_choiceButtons[i].chosen)
                    choice = i;
            }

            yield return null;
        }
        
        // hide all the buttons
        for(int i = 0; i < _choiceButtons.Count; i++)
        {
            if(i != choice)
                _choiceButtons[i].Hide();
        }
        
        currentDialogueScreen.SelectChoice(1);
    }
}