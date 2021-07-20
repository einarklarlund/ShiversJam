using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text text;
    public Image backdrop;
    public Button newGameButton;
    public Button loadGameButton;
    public Animator animator;

    public UnityEvent onTransitionInComplete;
    public UnityEvent onTransitionOutComplete;

    void Awake()
    {
        onTransitionInComplete = new UnityEvent();
        onTransitionOutComplete = new UnityEvent();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Show()
    {
        Debug.Log("Showing main menu");
        gameObject.SetActive(true);
        animator.SetTrigger("TransitionIn");
    }

    public void Hide()
    {
        Debug.Log("Hiding main menu");
        animator.SetTrigger("TransitionOut");
    }

    // !! the following 2 methods MUST be called at the end of their
    // respective animation clips !!
    public void CompleteLoadTransitionIn()
    {
        onTransitionInComplete.Invoke();
    }

    public void CompleteLoadTransitionOut()
    {
        gameObject.SetActive(false);
        onTransitionOutComplete.Invoke();
    }
}