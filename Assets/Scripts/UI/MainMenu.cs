using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Prime31.ZestKit;

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

    public float tweenValue { get; private set; }

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
    public void TransitionIn()
    {
        Debug.Log("main menu transitioning in");
        animator.SetTrigger("TransitionIn");
    }
    
    public void TransitionOut()
    {
        animator.SetTrigger("TransitionOut");
    }

    // !! the following 2 methods MUST be called at the end of their
    // respective animation clips !!
    public void CompleteTransitionIn()
    {
        onTransitionInComplete.Invoke();
    }

    public void CompleteTransitionOut()
    {
        onTransitionOutComplete.Invoke();
    }
}