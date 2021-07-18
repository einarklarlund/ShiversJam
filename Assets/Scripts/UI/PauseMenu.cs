using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PauseMenu : MonoBehaviour
{
    [Inject]
    GameManager _gameManager;

    public Button resumeButton;
    public Button saveLoadButton;
    public Button quitButton;

    public void SetWorldSpaceCamera()
    {

    }
}