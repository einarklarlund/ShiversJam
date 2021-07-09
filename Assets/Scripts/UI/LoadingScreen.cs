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
        backdrop.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
    }

    public void Show()
    {
        Debug.Log("showing loading screen");
        backdrop.gameObject.SetActive(true);
        loadingText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Debug.Log("hiding loading screen");
        backdrop.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
    }
}
