using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class GameManagerEvents : MonoBehaviour
{
    public UnityEvent<GameManager.GameState, GameManager.GameState> onGameStateChanged;

    [Inject]
    GameManager _gameManager;

    void Start()
    {
        _gameManager.onGameStateChanged.AddListener(
            (GameManager.GameState previousState, GameManager.GameState currentState) => 
                onGameStateChanged.Invoke(previousState, currentState));
    }
}
