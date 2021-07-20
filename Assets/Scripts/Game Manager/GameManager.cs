using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using PixelCrushers;
using Zenject;

[RequireComponent(typeof(SaveSystemMethods))]
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Running,
        Paused,
        Loading
    }

    public GameState CurrentGameState { get; private set; } = GameState.Loading;

    // these scenes need to be set in the inspector
    // public SceneAsset bootScene;
    // public SceneAsset mainMenuScene;
    // public SceneAsset beginningScene;

    [HideInInspector]
    public Camera mainCamera;
    public UnityEvent<GameState, GameState> onGameStateChanged;
    public SaveSystemMethods saveSystemMethods;

    List<string> _loadedLevelNames;
    List<AsyncOperation> _loadOperations;
    List<AsyncOperation> _unloadOperations;

    [Inject]
    UIManager _UIManager;
    string _nextScene;

    void Awake()
    {
        onGameStateChanged = new UnityEvent<GameState, GameState>();
    }

    void Start()
    {
        saveSystemMethods = GetComponent<SaveSystemMethods>();

        _nextScene = SceneManager.GetActiveScene().name;

        _UIManager.onLoadTransitionInComplete.AddListener(OnLoadTransitionInComplete);
        _UIManager.onLoadTransitionOutComplete.AddListener(OnLoadTransitionOutComplete);

        // check if we should be in main menu or running state
        if(_nextScene == "Main Menu")
            UpdateState(GameState.MainMenu);
        else
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
                Time.timeScale = 0.0f;
                break;

            default:
                Time.timeScale = 1.0f;
                break;
        }
        
        onGameStateChanged.Invoke(previousGameState, CurrentGameState);
    }

    public void LoadScene(string sceneName)
    {
        _nextScene = sceneName;
        UpdateState(GameState.Loading);
        _UIManager.BeginLoadTransitionIn();
    }

    // load the next scene once the loading screen has completely transitioned in
    void OnLoadTransitionInComplete()
    {
        Debug.Log($"[GameManager] loading scene {_nextScene}.");
        saveSystemMethods.LoadScene(_nextScene);
    }

    // set state and camera once the load transtion is complete
    void OnLoadTransitionOutComplete()
    {
        if(_nextScene == "Main Menu")
            UpdateState(GameState.MainMenu);
        else
            UpdateState(GameState.Running);
    }
}