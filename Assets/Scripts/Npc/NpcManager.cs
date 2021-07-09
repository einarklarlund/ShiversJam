
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NpcManager : MonoBehaviour
{
    NpcController.Factory _npcControllerFactory;
    HashSet<NpcController> _npcControllers;
    int _spawnCount;

    [Inject]
    public void Construct(NpcController.Factory npcControllerFactory, GameManager gameManager)
    {
        _npcControllerFactory = npcControllerFactory;

        // gameManager.hub.Connect<(GameManager.GameState previous, GameManager.GameState current)>(GameManager.Message.GameStateChanged, OnGameStateChanged);
    }

    void Start()
    {
        _npcControllers = new HashSet<NpcController>();
    }

    public NpcController Create(NpcController npcPrefab, Vector3 initialPosition)
    {
        // spawn the prefab, rename it, set its position the spawner's position
        NpcController spawnedController = _npcControllerFactory.Create(npcPrefab);
        spawnedController.transform.position = initialPosition;
        spawnedController.name = $"{npcPrefab.name} {_spawnCount++}";
        
        _npcControllers.Add(spawnedController);

        return spawnedController;
    }

    public bool Remove(NpcController controller)
    {
        return _npcControllers.Remove(controller);
    }

    void OnGameStateChanged((GameManager.GameState previous, GameManager.GameState current) states)
    {
        if(states.previous == GameManager.GameState.Loading
            && states.current == GameManager.GameState.Running)
        {
            FindAllNpcsInScene();
        }
    }

    void FindAllNpcsInScene()
    {

    }
}