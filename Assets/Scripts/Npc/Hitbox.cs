using System.Collections;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    [HideInInspector]
    public Collider hitboxCollider;
    public Interactor onHitInteraction;
    public bool definedInFrames = false;
    float ActiveDuration => definedInFrames ? _activeFrames * Time.deltaTime : _activeDuration;
    int ActiveFrames => definedInFrames ? _activeFrames : (int) (_activeDuration / Time.deltaTime);

    bool DefinedInSeconds => !definedInFrames;
    [SerializeField]
    [ShowIf("DefinedInSeconds")]
    float _activeDuration; 

    [SerializeField]
    [ShowIf("definedInFrames")]
    int _activeFrames;

    public void Start()
    {
        hitboxCollider = GetComponent<Collider>();
        
        hitboxCollider.enabled = false;
        gameObject.SetActive(false);
    }

    // 
    public void Activate()
    {
        gameObject.SetActive(true);
        if(!onHitInteraction)
            Debug.LogWarning($"[Hitbox] {name} was activated but its onHitInteraction has not been set.");
        StartCoroutine(EnableForActiveDuration());   
    }
    
    IEnumerator EnableForActiveDuration()
    {
        hitboxCollider.enabled = true;

        yield return new WaitForSeconds(ActiveDuration);

        hitboxCollider.enabled = false;
        gameObject.SetActive(false);
    }
}