using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Prime31.ZestKit;
using PixelCrushers.DialogueSystem;

public class Periscope : MonoBehaviour
{
    public Transform cameraPositionTo;
    public Animator animator;
    public Usable usable;
    public string periscopeSceneName = "PeriscopeScene";
    public Transform periscopeLoweredPosition;
    public Transform endingNpcGroup;
    public Transform endingPlayerPosition;
    public GameObject modelGameObject;

    PlayerController _player;
    Camera _playerCamera;
    Camera _periscopeSceneCamera;
    ITween<Vector3> _cameraPositionTween;
    ITween<Quaternion> _cameraRotationTween;
    Vector3 _initialCameraPosition;

    void Start()
    {
        if(!usable)
            usable = this.FindComponent<Usable>();

        usable.enabled = false;

        var events = DialogueManager.instance.GetComponent<DialogueSystemEvents>();
        events.conversationEvents.onConversationEnd.AddListener(OnConversationEnded);
        }

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
        SceneManager.LoadSceneAsync(periscopeSceneName, LoadSceneMode.Additive);

        // disable collider so we can enable it on periscope exit and
        // activate the ending dialogue trigger
        var dialogueSystemTriggerTransform = transform.Find("Dialogue system trigger");
        dialogueSystemTriggerTransform.gameObject.SetActive(false);

        if(DialogueLua.GetVariable("Clock").asInt >= 6)
        {
            // disable music when the clock hits 6
            var musicAudioSource = GameObject.Find("Music Audio Source").GetComponent<AudioSource>();
            var musicVolumeTweener = musicAudioSource.GetComponent<AudioSourceVolumeTweener>();
            musicVolumeTweener.audioSource = musicAudioSource;
            musicVolumeTweener.tweenDuration = 1;
            musicVolumeTweener.TweenVolumeTo(0);
        }
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
            // place the player in the ending position
            var playerController = FindObjectOfType<PlayerController>();
            playerController.transform.position = endingPlayerPosition.transform.position;

            DialogueLua.SetVariable("ReadyForEnding", true);
            endingNpcGroup.gameObject.SetActive(true);
        }

        // enable the fog
        var fogEnabler = FindObjectOfType<FogVolumeEnabler>();
        fogEnabler.SetFogEnabled(true);
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

        // enable periscope mouse look
        var periscopeMouseLook = FindObjectOfType<PeriscopeMouseLook>();
        periscopeMouseLook.mouseLookEnabled = true;

        // disable the fog
        var fogEnabler = FindObjectOfType<FogVolumeEnabler>();
        fogEnabler.SetFogEnabled(false);
        
        StartCoroutine(WaitForInput());
    }

    void OnExitPeriscopeViewComplete()
    {
        // enable movement and pausing
        _player.SetMovementEnabled(true);
        _player.speaking = false;

        // asynchronously unload the periscope scene
        SceneManager.UnloadSceneAsync(periscopeSceneName);

        // enable collider so that we can activate the ending 
        // dialogue trigger OnTriggerEnter
        var dialogueSystemTriggerTransform = transform.Find("Dialogue system trigger");
        dialogueSystemTriggerTransform.gameObject.SetActive(true);

        // reset periscope camera rotation and disable persicope mouse look
        var periscopeMouseLook = FindObjectOfType<PeriscopeMouseLook>();
        periscopeMouseLook.transform.eulerAngles = Vector3.zero;
        periscopeMouseLook.mouseLookEnabled = false;
    }

    void OnConversationEnded(Transform actor)
    {
        if(DialogueLua.GetVariable("Clock").asInt >= 6)
        {
            modelGameObject.transform.ZKpositionTo(periscopeLoweredPosition.position, 2f)
                .setEaseType(EaseType.Linear)
                .start();
            usable.enabled = true;
        }
    }
}
