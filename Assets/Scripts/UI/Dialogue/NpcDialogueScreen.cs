using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues;
using CleverCrow.Fluid.Dialogues.Choices;

public class NpcDialogueScreen : IDialogueScreen
{
    public IActor actor { get; set; }
    public string text { get; set; }
    public float textSpeed { get; set; }
    public List<CleverCrow.Fluid.Dialogues.Choices.IChoice> choices { get; set; }
    public NpcDialogueController npcDialogueController;

    public NpcDialogueScreen(NpcDialogueController npcDialogueController, IActor actor, string text, 
        float textSpeed, List<IChoice> choices)
    {
        this.actor = actor;
        this.text = text;
        this.choices = choices;
        this.textSpeed = textSpeed;
        this.npcDialogueController = npcDialogueController;
    }

    public void NextDialogueScreen()
    {
        npcDialogueController.NextDialogue();
    }

    public void ScrollText()
    {
        npcDialogueController.hub.Post(NpcDialogueController.Message.TextScrolled);
    }

    public void EndTextScroll()
    {
        npcDialogueController.hub.Post(NpcDialogueController.Message.TextScrollEnded);
    }

    public void SelectChoice(int choice)
    {
        npcDialogueController.SelectDialogueChoice(choice);
    }
}