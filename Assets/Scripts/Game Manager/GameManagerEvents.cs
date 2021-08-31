using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class GameManagerEvents : MonoBehaviour
{
    public UnityEvent<GameManager.GameState, GameManager.GameState> onGameStateChanged;
    public UnityEvent onLoadSceneTransitionStart;

    public UnityEvent onMainMenuStateBegan;
    public UnityEvent onRunningStateBegan;
    public UnityEvent onLoadingStateBegan;

    public UnityEvent onLoadMainMenuTransitionStart;

    [Inject]
    GameManager _gameManager;

    void Start()
    {
        _gameManager.onGameStateChanged.AddListener(OnGameStateChanged);

        _gameManager.onLoadSceneTransitionStart.AddListener(() =>
            onLoadSceneTransitionStart.Invoke());
    }

    public void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState currentState)
    {
        switch(currentState)
        {
            case GameManager.GameState.MainMenu:
                onMainMenuStateBegan.Invoke();
                break;

            case GameManager.GameState.Loading:
                onLoadingStateBegan.Invoke();
                break;

            case GameManager.GameState.Running:
                onRunningStateBegan.Invoke();
                break;
        }
    }
}
