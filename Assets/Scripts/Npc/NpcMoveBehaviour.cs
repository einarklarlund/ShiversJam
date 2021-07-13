using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcMoveBehaviour : StateMachineBehaviour
{
    public NpcMovementController.MovementType movementType;
    public float movementSpeed = 3;
    public float movementMagnitude = 3;
    NpcMovementController _npcMovementController;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(!_npcMovementController)
            _npcMovementController = animator.GetComponent<NpcMovementController>();
        
        if(!_npcMovementController)
            Debug.LogError($"[NpcMoveBehaviour] Could not find NpcMovementController script on {animator.gameObject.name}.");
        else
            _npcMovementController.ChangeMovement(movementType, movementSpeed, movementMagnitude);
    }
}
