using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class NpcSpawner : MonoBehaviour
{
    [SerializeField]
    [Label("NPC prefab")]
    protected NpcController _npcPrefab;

    [SerializeField]
    [Label("Number of NPCs")]
    int _numberOfNpcs = 1;

    [SerializeField]
    float _maxDistanceFromSpawner = 2;

    protected List<NpcController> npcControllers;

    NpcManager _npcManager;
 
    [Inject]
    public void Construct(NpcManager npcManager)
    {
        _npcManager = npcManager;
    }

    protected virtual NpcController Spawn()
    {
        NavMeshHit hit;
        Vector3 targetPosition;

        do
        {
            // spawn the controller at the spawner's position, displaced by a value
            // between -_maxDistanceFromCenter and +_maxDistanceFromCenter.
            targetPosition = transform.position + new Vector3(
                    (1 - 2 * UnityEngine.Random.value) * _maxDistanceFromSpawner,
                    (1 - 2 * UnityEngine.Random.value) * _maxDistanceFromSpawner,
                    (1 - 2 * UnityEngine.Random.value) * _maxDistanceFromSpawner
            );

        // find the NavMesh position that's closest to the target position
        } while(!NavMesh.SamplePosition(targetPosition, out hit, 10, NavMesh.AllAreas));

        // spawn the NPC controller prefab, rename it, set its position the spawner's position
        NpcController spawnedController = _npcManager.Create(_npcPrefab, hit.position);

        npcControllers.Add(spawnedController);
        
        // deactivate the mesh renderer.
        this.FindComponent<MeshRenderer>().enabled = false;

        return spawnedController;
    }

    public virtual void Remove(NpcController npcController)
    {
        npcControllers.Remove(npcController);
    }

    void Start()
    {
        npcControllers = new List<NpcController>();

        for(int i = 0; i < _numberOfNpcs; i++)
        {
            Spawn();
        }
    }
}
