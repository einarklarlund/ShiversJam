using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text text;
    public Image backdrop;
    public Button newGameButton;
    public Button loadGameButton;

    public void Show()
    {
        Debug.Log("Showing main menu");
        gameObject.SetActive(true);
        backdrop.enabled = true;
        text.enabled = true;
    }

    public void Hide()
    {
        Debug.Log("Hiding main menu");
        gameObject.SetActive(false);
        backdrop.enabled = false;
        text.enabled = false;
    }
}