using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.SceneManagement;
using PixelCrushers;

[RequireComponent(typeof(SaveSystemEvents))]
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Running,
        Paused,
        Loading
    }

    public string CurrentLevel { get; private set; }
    public GameState CurrentGameState { get; private set; } = GameState.MainMenu;

    // these scenes need to be set in the inspector
    // public SceneAsset bootScene;
    // public SceneAsset mainMenuScene;
    // public SceneAsset beginningScene;

    public UnityEvent<GameState, GameState> onGameStateChanged;

    List<string> _loadedLevelNames;
    List<AsyncOperation> _loadOperations;
    List<AsyncOperation> _unloadOperations;
    PlayerController _playerController;

    void Awake()
    {
        onGameStateChanged = new UnityEvent<GameState, GameState>();
    }

    void Start()
    {
        var saveSystemEvents = GetComponent<SaveSystemEvents>();
        saveSystemEvents.onSceneLoad.AddListener(() => UpdateState(GameState.Running));

        // update state to running if we're in a scene w the player controller
        _playerController = FindObjectOfType<PlayerController>();
        if(_playerController)
            UpdateState(GameState.Running);
    }

    void Update()
    {
        if((CurrentGameState == GameState.Running || CurrentGameState == GameState.Paused)
            && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    //change game state to paused/running
    public void TogglePause()
    {
        UpdateState(CurrentGameState == GameState.Running ? GameState.Paused : GameState.Running);
    }

    //update the game's state
    public void UpdateState(GameState state)
    {  
        Debug.Log($"[GameManager] Updating state from {CurrentGameState} to {state}");
        GameState previousGameState = CurrentGameState;
        CurrentGameState = state;

        switch(CurrentGameState)
        {           
            case GameState.Paused:
            case GameState.Loading:
                Time.timeScale = 0.0f;
                break;

            default:
                Time.timeScale = 1.0f;
                break;
        }

        if(CurrentGameState == GameState.Running)
        {
            _playerController = FindObjectOfType<PlayerController>();
        }
        
        onGameStateChanged.Invoke(previousGameState, CurrentGameState);
    }
}