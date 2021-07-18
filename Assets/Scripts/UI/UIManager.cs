using System.Collections;
using IntrovertStudios.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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
    public Camera dummyCamera;

    [Inject]
    GameManager _gameManager;

    void Awake()
    {
        hub = new MessageHub<Message>();
    }

    void OnEnable()
    {
        _gameManager.onGameStateChanged.AddListener(OnGameStateChanged);
    }

    void Start()
    {
        pauseMenu.gameObject.SetActive(false);
        pauseMenu.resumeButton.onClick.AddListener(_gameManager.TogglePause);

        dummyCamera.enabled = false;
    }

    void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState currentState)
    {
        // show/hide Main Menu
        if(currentState == GameManager.GameState.MainMenu)
        {
            mainMenu.Show();
        }
        else if(previousState == GameManager.GameState.MainMenu &&
            currentState != GameManager.GameState.MainMenu)
        {
            mainMenu.Hide();
        }

        // show/hide loading screen
        if(currentState == GameManager.GameState.Loading)
        {
            loadingScreen.Show();
            dummyCamera.enabled = true;
        }
        else if(previousState == GameManager.GameState.Loading &&
            currentState != GameManager.GameState.Loading)
        {
            loadingScreen.Hide();
            dummyCamera.enabled = false;
        }
        
        // set the pause menu's world space camera
        if(currentState == GameManager.GameState.Loading ||
            currentState == GameManager.GameState.MainMenu ||
            currentState == GameManager.GameState.Running)
        {
            pauseMenu.SetWorldSpaceCamera();
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