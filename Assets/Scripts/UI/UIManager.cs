using System.Collections;
using IntrovertStudios.Messaging;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using PixelCrushers;
using PixelCrushers.DialogueSystem;

[RequireComponent(typeof(SaveSystemEvents))]
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
    public MainMenu mainMenu;
    public PauseMenu pauseMenu;
    public HUD HUD;
    public LoadingScreen loadingScreen;

    public UnityEvent onLoadTransitionInComplete;
    public UnityEvent onLoadTransitionOutComplete;


    [Inject]
    GameManager _gameManager;

    void Awake()
    {
        hub = new MessageHub<Message>();

        if(onLoadTransitionInComplete == null)
            onLoadTransitionInComplete = new UnityEvent();
        
        if(onLoadTransitionOutComplete == null)
            onLoadTransitionOutComplete = new UnityEvent();
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

        SetCanvasWorldCameras();
    }

    // called by the GameManager when it decides to load a scene
    // ie. in LoadScene()
    public void BeginLoadTransitionIn()
    {
        Debug.Log("Loading screen transitioning in");
        loadingScreen.canvas.enabled = true;
        loadingScreen.animator.SetTrigger("TransitionIn");
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
        // hide the main menu's backdrop if transitioning to main menu scene
        // show the backdrop if transitioning to main menu scene
        if(_gameManager.nextScene != "Main Menu")
        {
            mainMenu.animator.SetTrigger("HideBackdrop");
        }
        else
        {
            Debug.Log("main menu showing backdrop");
            mainMenu.animator.SetTrigger("ShowBackdrop");
        }

        // enable main menu canvas if transitioning to Main Menu scene
        mainMenu.canvas.enabled = _gameManager.nextScene == "Main Menu";

        Debug.Log("transitioning loadingscreen out");
        loadingScreen.animator.SetTrigger("TransitionOut");
        SetCanvasWorldCameras();
    }

    // bubble up this event from the loadings creen so that the GameManager
    // can listen
    public void OnLoadTransitionOutComplete()
    {
        // transition in the main menu if necessary
        if(_gameManager.nextScene == "Main Menu")
        {
            Debug.Log("main menu transition in");
            mainMenu.canvas.enabled = true;
            mainMenu.TransitionIn();
        }

        loadingScreen.canvas.enabled = false;
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