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
        _gameManager.hub.Connect<(GameManager.GameState previousState, GameManager.GameState currentState)>(
            GameManager.Message.GameStateChanged, OnGameStateChanged);
        
    }
    void OnGameStateChanged((GameManager.GameState previousState, GameManager.GameState currentState) states)
    {
        Debug.Log($"{states.previousState} {states.currentState}");
        if(states.currentState == GameManager.GameState.Loading)
        {
            dummyCamera.gameObject.SetActive(true);
            loadingScreen.Show();
        }
        if((states.previousState == GameManager.GameState.Loading || states.previousState == GameManager.GameState.MainMenu) 
            && states.currentState != GameManager.GameState.Loading)
        {
            dummyCamera.gameObject.SetActive(false);
            loadingScreen.Hide();
        }
    }
}