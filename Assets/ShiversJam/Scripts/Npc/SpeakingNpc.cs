using System;
using UnityEngine;
using Zenject;
using NaughtyAttributes;
using IntrovertStudios.Messaging;

[RequireComponent(typeof(NpcDialogueController))]
public class SpeakingNpc : NpcController
{
    protected override void OnInteracted(Interactor interactor)
    {
        base.OnInteracted(interactor);

        if(interactor is PlayerController)
        {
            GetComponent<NpcDialogueController>().BeginDialogue();
        }
    }
}