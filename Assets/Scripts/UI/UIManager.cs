using System.Collections;
using IntrovertStudios.Messaging;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using PixelCrushers;
using PixelCrushers.DialogueSystem;

[RequireComponent(typeof(GameManagerEvents))]
public class UIManager : MonoBehaviour
{
    public enum Message
    {
        Alerted,
        NpcDialogueScreenOpened,
        NpcDialogueScreenCompleted,
        PlayerDamaged,
        ItemChanged
    }

    public IMessageHub<Message> hub;

    [Header("UI objects")]
    public MainMenu mainMenu;
    public PauseMenu pauseMenu;
    public HUD HUD;
    public LoadingScreen loadingScreen;

    [Header("Loading screen events")]
    public UnityEvent onLoadTransitionInStart;
        // ~~~~~ GOES TO onLoadTransitionInStart ~~~~~
        // loadingScreen.canvas.enabled = true;
        // loadingScreen.TransitionIn();

    public UnityEvent onLoadTransitionInComplete;
        // ~~~~~ GOES TO onLoadTransitionInComplete ~~~~~
        // mainMenu.HideBackdrop()

    public UnityEvent onLoadTransitionOutStart;
        // ~~~~~ GOES TO onLoadTransitionOutStart ~~~~~~
        // loadingScreen.TransitionOut()
        // this.SetCanvasWorldCameras()

    public UnityEvent onLoadTransitionOutComplete;
        // ~~~~~ GOES INTO onLoadTransitionOutComplete ~~~~~
        // loadingScreen.canvas.enabled = false;
    
    [Header("Main menu events")]
    public UnityEvent onMainMenuSceneLoaded;
    public UnityEvent onMainMenuEnter;
            // ~~~~~ GOES INTO onMainMenuEnter ~~~~~
            //     mainMenu.TransitionIn();
    public UnityEvent onMainMenuTransitionInComplete;
    public UnityEvent onMainMenuTransitionOutComplete;


    [Inject]
    GameManager _gameManager;

    void Awake()
    {
        hub = new MessageHub<Message>();

        // load transition in
        if(onLoadTransitionInStart == null)
            onLoadTransitionInStart = new UnityEvent();

        if(onLoadTransitionInComplete == null)
            onLoadTransitionInComplete = new UnityEvent();
        // load transition out
        if(onLoadTransitionOutStart == null)
            onLoadTransitionOutStart = new UnityEvent();

        if(onLoadTransitionOutComplete == null)
            onLoadTransitionOutComplete = new UnityEvent();

        // main menu
        if(onMainMenuSceneLoaded == null)
            onMainMenuSceneLoaded = new UnityEvent();

        if(onMainMenuEnter == null)
            onMainMenuEnter = new UnityEvent();
        
        if(onMainMenuTransitionInComplete == null)
            onMainMenuTransitionInComplete = new UnityEvent();

        if(onMainMenuTransitionOutComplete == null)
            onMainMenuTransitionOutComplete = new UnityEvent();
        
    }

    void OnEnable()
    {
        _gameManager.onGameStateChanged.AddListener(OnGameStateChanged);
    }

    void Start()
    {
        // the end load transition will play after the savesystem finishes loading
        var saveSystemEvents = GetComponent<SaveSystemEvents>();

        pauseMenu.resumeButton.onClick.AddListener(_gameManager.TogglePause);

        saveSystemEvents.onSceneLoad.AddListener(OnSceneLoad);

        // bubble up the load transition events from the loading screen
        loadingScreen.onLoadTransitionInComplete.AddListener(OnLoadTransitionInComplete);
        loadingScreen.onLoadTransitionOutComplete.AddListener(OnLoadTransitionOutComplete);

        // bubble up the transition events from Main Menu
        mainMenu.onTransitionInComplete.AddListener(() => onMainMenuTransitionInComplete.Invoke());
        mainMenu.onTransitionOutComplete.AddListener(() => onMainMenuTransitionOutComplete.Invoke());

        SetCanvasWorldCameras();
    }

    // listens to gameManager.onLoadSceneTransitionStart
    public void BeginLoadTransitionIn()
    {
        onLoadTransitionInStart.Invoke();
    }

    // bubble up this event from the loading screen so that GameManager
    // can listen
    public void OnLoadTransitionInComplete()
    {
        onLoadTransitionInComplete.Invoke();
    }

    // transition in from the loading screen after the save system has
    // finished loading a scene
    public void OnSceneLoad()
    {
        if(_gameManager.nextScene == "Main Menu")
            onMainMenuSceneLoaded.Invoke();

        onLoadTransitionOutStart.Invoke();
    }

    // bubble up this event from the loadings creen so that the GameManager
    // can listen
    public void OnLoadTransitionOutComplete()
    {
        onLoadTransitionOutComplete.Invoke();
    }
    
    public void SetCanvasWorldCameras()
    {
        Camera camera;
        var canvases = GetComponentsInChildren<Canvas>(true);

        var playerController = FindObjectOfType<PlayerController>();
        if(playerController)
            camera = playerController.playerCamera;
        else
            camera = GameObject.Find("Main Camera").GetComponent<Camera>();

        foreach(Canvas canvas in canvases)
        {
            canvas.worldCamera = camera;
        }
    }

    void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState currentState)
    {   
        if(currentState == GameManager.GameState.MainMenu &&
            previousState == GameManager.GameState.Loading)
        {
            onMainMenuEnter.Invoke();
        }

        // toggle/untoggle the pause menu
        if(currentState == GameManager.GameState.Paused)
        {
            pauseMenu.canvas.enabled = true;
        }
        else
        {
            pauseMenu.canvas.enabled = false;
        }
    }
}