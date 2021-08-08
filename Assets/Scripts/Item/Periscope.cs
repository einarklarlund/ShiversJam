using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Prime31.ZestKit;
using PixelCrushers.DialogueSystem;

public class Periscope : MonoBehaviour
{
    public Transform cameraPositionTo;
    public Animator animator;
    public SceneAsset periscopeScene;
    public Transform endingNpcGroup;

    PlayerController _player;
    Camera _playerCamera;
    Camera _periscopeSceneCamera;
    ITween<Vector3> _cameraPositionTween;
    ITween<Quaternion> _cameraRotationTween;
    Vector3 _initialCameraPosition;

    public void EnterPeriscopeView()
    {
        _player = FindObjectOfType<PlayerController>();
        _playerCamera = _player.playerCamera;

        // disable movement and pausing
        _player.SetMovementEnabled(false);
        _player.speaking = true;

        // tween the player camera  rotation
        // find vector that points from camera position to viewpoint position
        Vector3 targetVector = cameraPositionTo.transform.forward;
        Quaternion targetRotation = Quaternion.LookRotation(targetVector, transform.up);

        _cameraRotationTween = _player.playerCamera.transform.ZKrotationTo(targetRotation, 1f);
        _cameraRotationTween.setEaseType(EaseType.QuadOut)
            .start();

        // tween the player camera position
        _initialCameraPosition = _playerCamera.transform.localPosition;
        _cameraPositionTween = _playerCamera.transform.ZKpositionTo(cameraPositionTo.position, 1f);
        _cameraPositionTween.setEaseType(EaseType.QuadOut)
            .setCompletionHandler(tween => OnEnterPeriscopeViewComplete())
            .start();

        // start periscope animation
        animator.SetTrigger("Enter");

        // asynchronously load the periscope scene
        SceneManager.LoadSceneAsync(periscopeScene.name, LoadSceneMode.Additive);

        // disable collider so we can enable it on periscope exit and
        // activate the ending dialogue trigger
        var dialogueSystemTriggerTransform = transform.Find("Dialogue system trigger");
        dialogueSystemTriggerTransform.gameObject.SetActive(false);
    }

    // !!!!! the Periscope_Exit animation MUST call this method after it finishes !!!!!
    public void ExitPeriscopeView()
    {
        // disable periscope scene camera and its audio listener
        _periscopeSceneCamera.enabled = false;
        _periscopeSceneCamera.GetComponent<AudioListener>().enabled = false;

        // enable player camera and its audio listener
        _playerCamera.enabled = true;
        _playerCamera.GetComponent<AudioListener>().enabled = true;

        // tween the player camera position
        _cameraPositionTween = _playerCamera.transform.ZKlocalPositionTo(_initialCameraPosition, 0.75f);
        _cameraPositionTween.setEaseType(EaseType.QuadOut)
            .setCompletionHandler(tween => OnExitPeriscopeViewComplete())
            .start();

        // set ReadyForEnding variable once the periscope has been exited
        // after the clock hits 6
        if(DialogueLua.GetVariable("Clock").asInt >= 6)
        {
            DialogueLua.SetVariable("ReadyForEnding", true);
            endingNpcGroup.gameObject.SetActive(true);
        }
    }

    IEnumerator WaitForInput()
    {
        while(!Input.GetButtonDown("Fire1") && !Input.GetButtonDown("Use"))
        {
            yield return null;
        }

        animator.SetTrigger("Exit");
    }

    void OnEnterPeriscopeViewComplete()
    {
        // disable player camera and player camera audio listener
        _playerCamera.enabled = false;
        _playerCamera.GetComponent<AudioListener>().enabled = false;
        
        // enable periscope scene camera and its audio listener
        _periscopeSceneCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _periscopeSceneCamera.enabled = true;
        _periscopeSceneCamera.GetComponent<AudioListener>().enabled = true;
        
        StartCoroutine(WaitForInput());
    }

    void OnExitPeriscopeViewComplete()
    {
        // enable movement and pausing
        _player.SetMovementEnabled(true);
        _player.speaking = false;

        // asynchronously unload the periscope scene
        SceneManager.UnloadSceneAsync(periscopeScene.name);

        // enable collider so that we can activate the ending 
        // dialogue trigger OnTriggerEnter
        var dialogueSystemTriggerTransform = transform.Find("Dialogue system trigger");
        dialogueSystemTriggerTransform.gameObject.SetActive(true);
    }
}
