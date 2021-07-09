using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using IntrovertStudios.Messaging;

public class GameManager : MonoBehaviour
{
    public enum Message
    {
        GameStateChanged
    }

    public enum GameState
    {
        MainMenu,
        Running,
        Paused,
        Loading
    }

    public string CurrentLevel { get; private set; }
    public GameState CurrentGameState { get; private set; }

    // these scenes need to be set in the inspector
    public SceneAsset bootScene;
    public SceneAsset mainMenuScene;
    public SceneAsset beginningScene;

    public IMessageHub<Message> hub;

    List<string> _loadedLevelNames;
    List<AsyncOperation> _loadOperations;
    List<AsyncOperation> _unloadOperations;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void Start()
    {
        if(!bootScene || !beginningScene || !mainMenuScene)
            Debug.LogError("[GameManager] Not all SceneAsset objects have been set in the inspector.");

        _loadOperations = new List<AsyncOperation>();
        _unloadOperations = new List<AsyncOperation>();
        _loadedLevelNames = new List<string>();
        
        // check if bootScene is loaded. if it is not, GetSceneByName will return an invalid scene
        // and boot scene will be loaded
        if(SceneManager.GetSceneByName(bootScene.name).name != bootScene.name)
        {
            // load the scene synchronously
            SceneManager.LoadScene(bootScene.name, LoadSceneMode.Additive);
            Debug.Log("[GameManager] Boot scene is unloaded. Loading boot scene...");
        }
        
        if(SceneManager.sceneCount != 1)
        {
            // add current scenes to loaded level names except for bootScene
            for(int i = SceneManager.sceneCount - 1; i >= 0; --i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                
                if(scene.name != bootScene.name)
                {
                    _loadedLevelNames.Add(scene.name);
                    // SceneManager.SetActiveScene(scene);
                    CurrentLevel = SceneManager.GetActiveScene().name;
                }
            }

            UpdateState(GameState.Running);
        }
        else
        {
            Debug.Log("[GameManager] Only boot scene is loaded. Loading main menu scene...");
            LoadLevel(mainMenuScene.name);
        }
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
    
    public void LoadLevel(string levelName)
    {
        // load the scene asynchronously using ao object
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        if(loadOperation == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }

        loadOperation.completed += OnLoadOperationCompleted;
        
        // change class vars
        _loadOperations.Add(loadOperation);
        _loadedLevelNames.Add(levelName);
        CurrentLevel = levelName;
        
        UpdateState(GameState.Loading);
    }

    public void UnloadLevel(string levelName)
    {
        // unload the scene asynchronously using ao object
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(levelName);
        if(unloadOperation == null)
        {
            Debug.LogError("[GameManager] Unable to unload level " + levelName);
            return;
        }

        // we must add our listener function (OnLoadCompleted) to the ao object so that it is called when the ao completes
        unloadOperation.completed += OnUnloadOperationCompleted;

        //change class vars
        _unloadOperations.Add(unloadOperation);
        _loadedLevelNames.Remove(levelName);
    }
    
    public void UnloadAllLevels()
    {
        for(int i = 0; i < _loadedLevelNames.Count; ++i)
            UnloadLevel(_loadedLevelNames[i]);
    }

    //update the game's state
    void UpdateState(GameState state)
    {  
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
        
        hub.Post(Message.GameStateChanged, (previousGameState, CurrentGameState));
    }

    void OnLoadOperationCompleted(AsyncOperation asyncOperation)
    {
        if(_loadOperations.Contains(asyncOperation))
            _loadOperations.Remove(asyncOperation);
        else
            Debug.LogWarning("[GameManager] Tried to remove a load operation from load operations list, but the operation was not in the list.");

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(CurrentLevel));

        if(_loadOperations.Count == 0)
            UpdateState(GameState.Running);
    }

    void OnUnloadOperationCompleted(AsyncOperation asyncOperation)
    {
        if(_unloadOperations.Contains(asyncOperation))
            _unloadOperations.Remove(asyncOperation);
        else
            Debug.LogWarning("[GameManager] Tried to remove an unload operation from load operations list, but the operation was not in the list.");
    }
}