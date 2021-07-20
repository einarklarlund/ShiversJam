using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CanvasWorldCameraSetter : MonoBehaviour
{
    [Inject]
    GameManager _gameManager;

    void Start()
    {
        _gameManager.onGameStateChanged.AddListener((previousState, currentState) => SetCameras());
    }

    void SetCameras()
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
}
