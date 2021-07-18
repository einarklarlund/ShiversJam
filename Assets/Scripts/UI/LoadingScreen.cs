using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Coffee.UIEffects;

public class LoadingScreen : MonoBehaviour
{
    public Text loadingText;
    public Image backdrop;

    void Start()
    {
        Hide();
    }

    public void Show()
    {
        Debug.Log("showing loading screen");
        gameObject.SetActive(true);
        loadingText.enabled = true;
        backdrop.enabled = true;
    }

    public void Hide()
    {
        Debug.Log("hiding loading screen");
        gameObject.SetActive(false);
        loadingText.enabled = false;
        backdrop.enabled = false;
    }
}
