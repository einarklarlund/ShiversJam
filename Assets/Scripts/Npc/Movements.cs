using System;
using UnityEngine;

public class Movements
{
    Func<Vector3, Vector3, Vector3> MoveToTarget => 
        (currentPosition, target) => target;

    Func<Vector3, Vector3, Vector3> StayStill => 
        (currentPosition, target) => currentPosition;
}