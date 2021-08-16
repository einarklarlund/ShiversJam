using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PauseMenu : MonoBehaviour
{
    public Button resumeButton;

    public Button pauseButton;

    public Canvas canvas;
    
    public AudioClip pauseClip;
    public AudioClip unpauseClip;

    public AudioSource audioSource;

    [Inject]
    GameManager _gameManager;

    void Start()
    {
        canvas.enabled = false;
        _gameManager.onGameStateChanged.AddListener(OnGameStateChanged);
    }

    void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState currentState)
    {
        if(currentState == GameManager.GameState.Paused)
        {
            audioSource.PlayOneShot(pauseClip);
        }
        
        if(currentState == GameManager.GameState.Running && 
            previousState == GameManager.GameState.Paused)
        {
            audioSource.PlayOneShot(unpauseClip);
        }
    }
}
