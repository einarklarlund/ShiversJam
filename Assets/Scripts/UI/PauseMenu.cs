using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Button resumeButton;

    public Button pauseButton;

    public Canvas canvas;

    void Start()
    {
        canvas.enabled = false;
    }
}
