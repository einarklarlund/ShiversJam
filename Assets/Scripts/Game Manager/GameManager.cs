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
        Boot,
        MainMenu,
        Running,
        Paused,
        Loading
    }

    public GameState CurrentGameState { get; private set; } = GameState.Boot;
    public string nextScene => _nextScene;

    // these scenes need to be set in the inspector
    // public SceneAsset bootScene;
    // public SceneAsset mainMenuScene;
    // public SceneAsset beginningScene;

    [HideInInspector]
    public Camera mainCamera;
    [HideInInspector]
    public bool loadGameOnNextSceneLoad;
    public UnityEvent<GameState, GameState> onGameStateChanged;
    public UnityEvent onLoadSceneTransitionStart;
    public SaveSystemMethods saveSystemMethods;

    List<string> _loadedLevelNames;
    List<AsyncOperation> _loadOperations;
    List<AsyncOperation> _unloadOperations;

    [Inject]
    UIManager _UIManager;
    string _nextScene;

    void Awake()
    {
        if(onGameStateChanged == null)
            onGameStateChanged = new UnityEvent<GameState, GameState>();
    }

    void Start()
    {
        saveSystemMethods = GetComponent<SaveSystemMethods>();

        _nextScene = SceneManager.GetActiveScene().name;

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

    public void SetLoadGameOnNextSceneLoad(bool setTo)
    {
        loadGameOnNextSceneLoad = setTo;
    }

    public void BeginLoadTransitionTo(string sceneName)
    {
        _nextScene = sceneName;
        UpdateState(GameState.Loading);
        onLoadSceneTransitionStart.Invoke();
    }

    public void LoadNextScene()
    {
        if(loadGameOnNextSceneLoad)
        {
            Debug.Log($"[GameManager] loading game from save slot 1.");
            saveSystemMethods.LoadFromSlot(1);
        }
        else
        {        
            Debug.Log($"[GameManager] loading scene {_nextScene}.");
            saveSystemMethods.LoadScene(_nextScene);
        }
    }

    public void ResetGameState()
    {
        SaveSystem.ResetGameState();
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