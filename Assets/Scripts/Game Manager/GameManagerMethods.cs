using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameManagerMethods : MonoBehaviour
{
    [Inject]
    GameManager _gameManager;

    public void UpdateState(string newState)
    {
        var state = (GameManager.GameState) Enum.Parse(typeof(GameManager.GameState), newState);
        _gameManager.UpdateState(state);
    }

    
    public void SetLoadGameOnNextSceneLoad(bool setTo)
    {
        _gameManager.SetLoadGameOnNextSceneLoad(setTo);
    }

    public void BeginLoadTransitionTo(string newScene)
    {
        _gameManager.BeginLoadTransitionTo(newScene);
    }

    public void LoadNextScene()
    {
        _gameManager.LoadNextScene();
    }

    public void ResetGameState()
    {
        _gameManager.ResetGameState();
    }
}
