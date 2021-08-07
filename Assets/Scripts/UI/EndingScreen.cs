using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingScreen : MonoBehaviour
{
    public Animator animator;

    public void IllustrationIn()
    {
        animator.SetTrigger("IllustrationIn");
    }

    public void EndImageIn()
    {
        animator.SetTrigger("EndImageIn");
    }
}
