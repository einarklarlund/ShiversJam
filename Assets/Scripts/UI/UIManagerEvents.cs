using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class UIManagerEvents : MonoBehaviour
{
    [Inject]
    UIManager _UIManager;

    [Header("Loading screen events")]
    public UnityEvent onLoadTransitionInStart;
    public UnityEvent onLoadTransitionInComplete;
    public UnityEvent onLoadTransitionOutStart;
    public UnityEvent onLoadTransitionOutComplete;
    public UnityEvent onQuitTransitionComplete;
    
    [Header("Main menu events")]
    public UnityEvent onMainMenuSceneLoaded;
    public UnityEvent onMainMenuEnter;
    public UnityEvent onMainMenuTransitionInStart;
    public UnityEvent onMainMenuTransitionInComplete;
    public UnityEvent onMainMenuTransitionOutStart;
    public UnityEvent onMainMenuTransitionOutComplete;

    // Start is called before the first frame update
    void Start()
    {
        _UIManager.onLoadTransitionInStart.AddListener(() =>
            onLoadTransitionInStart.Invoke());
            
        _UIManager.onLoadTransitionInComplete.AddListener(() =>
            onLoadTransitionInComplete.Invoke());
            
        _UIManager.onLoadTransitionOutStart.AddListener(() =>
            onLoadTransitionOutStart.Invoke());
            
        _UIManager.onLoadTransitionOutComplete.AddListener(() =>
            onLoadTransitionOutComplete.Invoke());

        _UIManager.onQuitTransitionComplete.AddListener(() =>
            onQuitTransitionComplete.Invoke());

            
        _UIManager.onMainMenuSceneLoaded.AddListener(() =>
            onMainMenuSceneLoaded.Invoke());

        _UIManager.onMainMenuEnter.AddListener(() =>
            onMainMenuEnter.Invoke());

        _UIManager.onMainMenuTransitionInStart.AddListener(() =>
            onMainMenuTransitionInStart.Invoke());

        _UIManager.onMainMenuTransitionInComplete.AddListener(() =>
            onMainMenuTransitionInComplete.Invoke());
            
        _UIManager.onMainMenuTransitionOutStart.AddListener(() =>
            onMainMenuTransitionOutStart.Invoke());

        _UIManager.onMainMenuTransitionOutComplete.AddListener(() =>
            onMainMenuTransitionOutComplete.Invoke());
    }
}
