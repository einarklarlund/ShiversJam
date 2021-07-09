using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues;
using CleverCrow.Fluid.Dialogues.Choices;

public interface IDialogueScreen
{
    
    IActor actor { get; set; } 
    string text { get; set; } 
    float textSpeed { get; set; } 
    List<IChoice> choices { get; set; } 

    void NextDialogueScreen();
    void ScrollText();
    void EndTextScroll();
    void SelectChoice(int choice);
}