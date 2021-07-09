using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Prime31.ZestKit;
using Coffee.UIEffects;
using UnityEngine.EventSystems;

public class DialogueButton : 
    MonoBehaviour, 
    ITweenTarget<float>, 
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Text text;
    public Color fadeInColor;
    public Color fadeOutColor;
    public Color highlightColor = Color.yellow;

    [HideInInspector]
    public bool chosen;

    Color _initialTextColor;
    Color _initialTextShadowColor;
    BaseMaterialEffect _currentEffect;
    UIShadow _textShadow;
    bool _selected;

    void Start()
    {
        _textShadow = text.GetComponent<UIShadow>();
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        _textShadow.effectColor = new Color(_textShadow.effectColor.r, _textShadow.effectColor.g, _textShadow.effectColor.b, 0);
    }

    void Update()
    {
        if(_selected && Input.GetButtonDown("Fire1"))
        {
            StartCoroutine(Dissolve(fadeInColor));
        }
    }

    public void Show(string displayText)
    {
        // turn on each button and set its text
        gameObject.SetActive(true);
        text.text = displayText;
        chosen = false;

        // Dissolve(fadeInColor, reverse: true);
        StartCoroutine(Dissolve(fadeInColor, reverse: true));
    }

    public void Hide()
    {
        StartCoroutine(Dissolve(fadeOutColor));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(Shine());
        _selected = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _selected = false;
    }

    public IEnumerator Dissolve(Color dissolveColor, float duration = 1, bool reverse = false, 
        float softness = 0.5f, float width = 0.5f, float from = 0, float to = 1)
    {
        var shinyEffect = GetComponent<UIShiny>();
        if(shinyEffect)
        {
            shinyEffect.Stop();
            Destroy(shinyEffect);
            Debug.Log(GetComponent<UIShiny>());
        }
        
        yield return new WaitForEndOfFrame();

        var dissolveEffect = GetComponent<UIDissolve>();
        if(!dissolveEffect)
            dissolveEffect = gameObject.AddComponent<UIDissolve>();
            
        dissolveEffect.Stop();

        dissolveEffect.color = dissolveColor;
        dissolveEffect.softness = softness;
        dissolveEffect.width = width;
        dissolveEffect.effectFactor = reverse ? to : from;

        _currentEffect = dissolveEffect;

        Debug.Log($"{from} {to}");

        FloatTween tween = reverse ? new FloatTween(this, to, from, duration) : new FloatTween(this, from, to, duration);
        tween.setCompletionHandler(tween => Destroy(dissolveEffect))
            .setEaseType(EaseType.Linear)
            .start();
    }

    public IEnumerator Shine(float duration = 0.4f, bool reverse = false, float softness = 0.215f, 
        float width = 0.25f, float from = 0, float to = 1, float brightness = 0.181f)
    {
        var dissolveEffect = GetComponent<UIDissolve>();
        if(dissolveEffect)
        {
            dissolveEffect.Stop();
            Destroy(dissolveEffect);
        }

        yield return new WaitForEndOfFrame();

        var shinyEffect = GetComponent<UIShiny>();
        if(!shinyEffect)
            shinyEffect = gameObject.AddComponent<UIShiny>();

        shinyEffect.softness = softness;
        shinyEffect.width = width;
        shinyEffect.effectFactor = reverse ? to : from;
        shinyEffect.brightness = brightness;

        _currentEffect = shinyEffect;

        FloatTween tween = reverse ? new FloatTween(this, to, from, duration) : new FloatTween(this, from, to, duration);
        tween.setCompletionHandler(tween => Destroy(shinyEffect))
            .setEaseType(EaseType.Linear)
            .start();
    }

    public void setTweenedValue(float value)
    {
        if(_currentEffect is UIDissolve)
        {
            (_currentEffect as UIDissolve).effectFactor = value;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1 - value);
            _textShadow.effectColor = new Color(_textShadow.effectColor.r, _textShadow.effectColor.g, _textShadow.effectColor.b, 1 - value);
        }
        else if(_currentEffect is UIEffect)
            (_currentEffect as UIEffect).effectFactor = value;

        else if(_currentEffect is UIShiny)
            (_currentEffect as UIShiny).effectFactor = value;
    }

    public float getTweenedValue()
    {
        if(_currentEffect is UIDissolve)
            return (_currentEffect as UIDissolve).effectFactor;

        if(_currentEffect is UIEffect)
            return (_currentEffect as UIEffect).effectFactor;

        if(_currentEffect is UIShiny)
            return (_currentEffect as UIShiny).effectFactor;

        return 0;
    }

    public object getTargetObject()
    {
        return this;
    }

}