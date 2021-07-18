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
    }

    void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState currentState)
    {
        // loading screen logic
        if(currentState == GameManager.GameState.Loading)
        {
            dummyCamera.gameObject.SetActive(true);
            loadingScreen.Show();
        }
        if((previousState == GameManager.GameState.Loading || previousState == GameManager.GameState.MainMenu) 
            && currentState != GameManager.GameState.Loading)
        {
            dummyCamera.gameObject.SetActive(false);
            loadingScreen.Hide();
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