using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Prime31.ZestKit;

public class Periscope : MonoBehaviour
{
    public Transform cameraPositionTo;
    public Animator animator;
    public SceneAsset periscopeScene;

    PlayerController _player;
    Camera _camera;
    ITween<Vector3> _cameraPositionTween;
    ITween<Quaternion> _cameraRotationTween;
    Vector3 _initialCameraPosition;

    public void EnterPeriscopeView()
    {
        _player = FindObjectOfType<PlayerController>();
        _camera = _player.playerCamera;

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
        _initialCameraPosition = _camera.transform.localPosition;
        _cameraPositionTween = _camera.transform.ZKpositionTo(cameraPositionTo.position, 1f);
        _cameraPositionTween.setEaseType(EaseType.QuadOut)
            .setCompletionHandler(tween => OnEnterPeriscopeViewComplete())
            .start();

        // start periscope animation
        animator.SetTrigger("Enter");

        // asynchronously load the periscope scene
        SceneManager.LoadSceneAsync(periscopeScene.name, LoadSceneMode.Additive);
    }

    // !!!!! the Periscope_Exit animation MUST call this method after it finishes !!!!!
    public void ExitPeriscopeView()
    {
        // enable camera
        _camera.enabled = true;

        // start periscope animation
        Debug.Log("exit periscope");

        // tween the player camera rotation

        // tween the player camera position
        _cameraPositionTween = _camera.transform.ZKlocalPositionTo(_initialCameraPosition, 0.75f);
        _cameraPositionTween.setEaseType(EaseType.QuadOut)
            .setCompletionHandler(tween => OnExitPeriscopeViewComplete())
            .start();
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
        _camera.enabled = false;
        _camera.GetComponent<AudioListener>().enabled = false;
        GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
        StartCoroutine(WaitForInput());
    }

    void OnExitPeriscopeViewComplete()
    {
        // enable movement and pausing
        _player.SetMovementEnabled(true);
        _player.speaking = false;

        // asynchronously unload the periscope scene
        SceneManager.UnloadSceneAsync(periscopeScene.name);
    }
}
