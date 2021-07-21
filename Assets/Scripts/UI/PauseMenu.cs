using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PauseMenu : MonoBehaviour
{
    public Canvas canvas;
    public Button resumeButton;
    public Button saveLoadButton;
    public Button quitButton;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
    }
}