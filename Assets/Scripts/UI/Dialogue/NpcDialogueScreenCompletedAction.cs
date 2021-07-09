using CleverCrow.Fluid.Dialogues.Actions;
using CleverCrow.Fluid.Dialogues;
using Zenject;

[CreateMenu("DialogueScreenCompletedAction")]
public class DialogueScreenCompletedAction : ActionDataBase {
    [Inject]
    UIManager _UIManager;


    public override void OnInit (IDialogueController dialogue) {
        // Run the first time the action is triggered
    }

    public override void OnStart () {
        // Runs when the action begins triggering
    }

    public override ActionStatus OnUpdate () {
        // Runs when the action begins triggering

        // Return continue to span multiple frames
        return ActionStatus.Success;
    }

    public override void OnExit () {
        // Runs when the actions `OnUpdate()` returns `ActionStatus.Success`
        _UIManager.hub.Post(UIManager.Message.NpcDialogueScreenCompleted);
    }

    public override void OnReset () {
        // Runs after a node has fully run through the start, update, and exit cycle
    }
}