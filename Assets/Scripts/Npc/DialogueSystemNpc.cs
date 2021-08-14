using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystemNpc : NpcController
{
    public void SetFacePlayerOnSelect(string facePlayer)
    {
        if(facePlayer == "true")
        {
            facePlayerOnSelect = true;
        }
        else if(facePlayer == "false")
        {
            facePlayerOnSelect = false;
        }
    }
}
