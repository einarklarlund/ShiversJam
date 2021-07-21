using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
using Coffee.UIEffects;

public class LoadingScreen : MonoBehaviour
{
    public Canvas canvas;
    public Text loadingText;
    public Image backdrop;
    public Animator animator;

    public UnityEvent onLoadTransitionInComplete;
    public UnityEvent onLoadTransitionOutComplete;

    void Awake()
    {
        onLoadTransitionInComplete = new UnityEvent();
        onLoadTransitionOutComplete = new UnityEvent();
    }

    void Start()
    {
        if(!animator)
        {
            animator = GetComponent<Animator>();
            if(!animator)
            {
                Debug.LogWarning("[LoadingScreen] Could not find Animator component.");
            }
        }

        canvas = GetComponent<Canvas>();
        canvas.enabled = false;

        var newColor = backdrop.color;
        newColor.a = 0;
        backdrop.color = newColor;
    }

    // !! the load transition end animation MUST call this method using
    // an animation event at the end of the transition !!
    public void CompleteLoadTransitionIn()
    {
        onLoadTransitionInComplete.Invoke();
    }


    // !! the load transition out animation MUST call this method using
    // an animation event at the end of the transition !!
    public void CompleteLoadTransitionOut()
    {
        onLoadTransitionOutComplete.Invoke();
    }
}
