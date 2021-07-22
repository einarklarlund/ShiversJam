using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

public class MainMenu : MonoBehaviour
{
    public Canvas canvas;
    public Text text;
    public Image backdrop;
    public Button newGameButton;
    public Button loadGameButton;
    public Animator animator;

    public UnityEvent onTransitionInComplete;
    public UnityEvent onTransitionOutComplete;
    public bool loadGamePressed;

    public float tweenValue { get; private set; }

    [Inject]
    GameManager _gameManager;

    void Awake()
    {
        if(onTransitionInComplete == null)
            onTransitionInComplete = new UnityEvent();
        
        if(onTransitionOutComplete == null)
            onTransitionOutComplete = new UnityEvent();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        canvas = GetComponent<Canvas>();
        
        var color = backdrop.color;
        color.a = 1;

        // make sure backdrop doesn't lose alpha value after transitioning out
        onTransitionOutComplete.AddListener(() => 
        {
            Debug.Log("setting backdrop alpha value to 1");
            backdrop.color = color;
        });
    }

    // !! the following 2 methods MUST be called at the end of their
    // respective animation clips !!
    public void CompleteTransitionIn()
    {
        Debug.Log("[MainMenu] completing transition in");
        onTransitionInComplete.Invoke();
    }

    public void CompleteTransitionOut()
    {
        Debug.Log("[MainMenu] completing transition out");
        onTransitionOutComplete.Invoke();
    }
    
    // listens to UIManager.onMainMenuEnter
    public void TransitionIn()
    {
        Debug.Log("[MainMenu] transitioning in");
        animator.SetTrigger("TransitionIn");
    }

    // listens to newGameButton.onClick and loadGameButton.onClick
    public void TransitionOut()
    {
        Debug.Log("[MainMenu] transitioning out");
        animator.SetTrigger("TransitionOut");
    }

    // listens to UIManager.onMainMenuTransitionOutComplete
    public void ShowBackdrop()
    {
        Debug.Log($"[MainMenu] showing backdrop");
        animator.SetBool("ShowBackdrop", true);
    }

    // listens to UIManager.onLoadingScreenTransitionInComplete
    public void HideBackdrop()
    {
        Debug.Log($"[MainMenu] hiding backdrop");
        animator.SetBool("ShowBackdrop", false);
    }
}