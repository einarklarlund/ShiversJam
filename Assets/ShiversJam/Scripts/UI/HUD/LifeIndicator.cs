using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Prime31.ZestKit;
using Coffee.UIEffects;
using Zenject;

public class LifeIndicator : MonoBehaviour, ITweenTarget<float>
{
    [Inject]
    GameManager _gameManager;
    UIDissolve _UIDissolve;
    Text _healthText;
    Damageable _playerDamageable;

    public void OnEnable()
    {
        _gameManager.hub.Connect<(GameManager.GameState previousState, GameManager.GameState currentState)>(GameManager.Message.GameStateChanged, OnGameStateChanged);
    }

    // Start is called before the first frame update
    void Start()
    {
        _UIDissolve = GetComponentInChildren<UIDissolve>();
        _healthText = GetComponentInChildren<Text>();
    }

    void OnGameStateChanged((GameManager.GameState previousState, GameManager.GameState currentState) states)
    {
        if(states.currentState == GameManager.GameState.Running)
        {
            if(_playerDamageable)
                _playerDamageable.hub.DisconnectAll();

            _playerDamageable = FindObjectOfType<PlayerController>()
                .GetComponent<Damageable>();

            // _healthText.text = $"{_playerDamageable.health}";

            _playerDamageable.hub.Connect<int>(Interactable.Message.Damaged, OnPlayerDamaged);
        }
    }

    public void setTweenedValue(float value)
    {
        _UIDissolve.effectFactor = value;
        // _healthText.text = $"{value}";
    }

    public float getTweenedValue()
    {
        return _UIDissolve.effectFactor;
    }

    public object getTargetObject()
    {
        return this;
    }

    void OnPlayerDamaged(int damage)
    {
        Debug.Log("damaged");
        ZestKit.instance.stopAllTweensWithTarget(this);
        // _healthText.text = $"{_playerDamageable.health}";

        var targetValue = 0.19f + 0.81f * (1 - (float) _playerDamageable.health / _playerDamageable.maxHealth);
        Debug.Log($"{targetValue} {_UIDissolve.effectFactor}");
        new FloatTween(this, _UIDissolve.effectFactor, targetValue, 0.2f)
            .setEaseType(EaseType.QuadIn)
            .start();
    }
}
