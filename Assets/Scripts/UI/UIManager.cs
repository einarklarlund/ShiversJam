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
        onLoadTransitionInComplete = new UnityEvent();
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
        loadingScreen.gameObject.SetActive(true);
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
        loadingScreen.animator.SetTrigger("TransitionOut");
        SetCanvasWorldCameras();
    }

    // bubble up this event from the loadings creen so that the GameManager
    // can listen
    public void OnLoadTransitionOutComplete()
    {
        loadingScreen.gameObject.SetActive(false);
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
        // show main menu (main menu is hidden when new game/load game button is pressed)
        if(currentState == GameManager.GameState.MainMenu)
        {
            mainMenu.gameObject.SetActive(true);
            mainMenu.Show();
        }
        
        // toggle/untoggle the pause menu
        if(currentState == GameManager.GameState.Paused)
        {
            pauseMenu.gameObject.SetActive(true);
        }
        else
        {
            pauseMenu.gameObject.SetActive(false);
        }
    }
}