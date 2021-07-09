using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls _playerControls;

    public Vector2 GetPlayerMovement()
    {
        return _playerControls.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseMovement()
    {
        return _playerControls.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerJumpedThisFrame()
    {
        return _playerControls.Player.Jump.triggered;
    }

    void Awake()
    {
        _playerControls = new PlayerControls();
    }    

    void OnEnable()
    {
        _playerControls.Enable();
    }

    void OnDisable()
    {
        _playerControls.Disable();
    }
}
