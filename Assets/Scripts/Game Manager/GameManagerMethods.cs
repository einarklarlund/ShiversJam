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

    public void LoadScene(string newScene)
    {
        _gameManager.LoadScene(newScene);
    }

    public void QueueNextScene(string nextScene)
    {
        _gameManager.QueueNextScene(nextScene);
    }

    public void LoadNextScene()
    {
        _gameManager.LoadNextScene();
    }
}
