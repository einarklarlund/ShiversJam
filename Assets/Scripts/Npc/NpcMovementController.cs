using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

[RequireComponent(typeof(NpcController), typeof(NavMeshAgent))]
public class NpcMovementController : MonoBehaviour
{
    public enum MovementType
    {
        Still,
        FollowPlayer,
        Wander,
        WanderAroundInitialPosition,
        FollowTarget,
        FreakOut,
        FloatAway,
        Fly
    }

    [Tooltip("Transform that the NpcMovementController will target if the movement requires a target")]
    public Transform target;

    [Tooltip("Determines whether the controller will ping the animator with walkcycle events")]
    public bool useStepAnimation = false;
    [Tooltip("When using a wander movement type, the NPC will wait for this amount of time between movements")]
    public float waitDuration = 3f;
    public float distancePerStep = 0.5f;
    public float rotationSpeed = 7;

    [HideInInspector]
    public bool canMove = false;

    public Coroutine _walkCycleCoroutine;

    NavMeshAgent _agent;
    Animator _animator;
    Transform _playerTransform;
    Vector3 _initialPosition;
    MovementType _currentMovementType;
    float _displacementMagnitude;

    [Inject]
    public void Construct(PlayerController player)
    {
        _playerTransform = player.transform;
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        // listen for the TargetAcquired message
        GetComponent<NpcController>().hub.Connect<Transform>(NpcController.Message.TargetAcquired, OnTargetAcquired);
        
        _initialPosition = transform.position;

        _agent.updateRotation = false;
    }

    public void Move()
    {
        if(useStepAnimation)
            _animator.SetBool("Moving", true);

        switch(_currentMovementType)
        {
            case MovementType.Still:        
                // _animator.SetBool("Moving", false);
                _agent.speed = 0;
                break;

            case MovementType.FollowPlayer:  
                MoveToPosition(_playerTransform.position);
                break;

            case MovementType.Wander:
                MoveToPosition(transform.position, _displacementMagnitude);
                break;

            case MovementType.WanderAroundInitialPosition:
                MoveToPosition(_initialPosition, _displacementMagnitude);
                break;

            case MovementType.FollowTarget:
                if(target)
                    MoveToPosition(target.position, _displacementMagnitude);
                break;

            case MovementType.FreakOut:
                break;

            case MovementType.FloatAway:
                break;

            case MovementType.Fly:
                break;
        }
    }

    public void ChangeMovement(MovementType movementType, float speed, float magnitude)
    {
        canMove = movementType != MovementType.Still;
        _agent.speed = speed;
        _currentMovementType = movementType;
        _displacementMagnitude = magnitude;

        Move();
    }

    public void StopMoving()
    {
        if(useStepAnimation && _walkCycleCoroutine != default)
            StopCoroutine(_walkCycleCoroutine);

        _agent.isStopped = true;
    }

    public void ResumeMoving()
    {
        _agent.isStopped = false;

        _walkCycleCoroutine = StartCoroutine(MoveAndWait());
    }

    void MoveToPosition(Vector3 position, float displacement = 0)
    {
        // set the destination to be the given position 
        // plus some random displacement between -displacement/2 and +displacement/2
        Vector3 targetPosition = position + new Vector3(
            displacement * (Random.value - 0.5f),
            displacement * (Random.value - 0.5f), 
            displacement * (Random.value - 0.5f));

        _agent.destination = targetPosition;

        _walkCycleCoroutine = StartCoroutine(MoveAndWait());
    }

    IEnumerator MoveAndWait()
    {
        if(useStepAnimation)
            _animator.SetBool("Moving", true);

        var lastStepPosition = transform.position;

        // wait for allow NPC to accelerate before checking if it has stopped
        yield return new WaitForSeconds(0.1f);

        // rotate the NPC and check if NPC has taken a step in the walk cycle
        while(_agent.remainingDistance > 0 && _agent.desiredVelocity.magnitude > 0)
        {
            if(useStepAnimation && Vector3.Distance(lastStepPosition, transform.position) >= distancePerStep)
            {
                _animator.SetTrigger("Stepped");
                lastStepPosition = transform.position;
            }

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, _agent.desiredVelocity, 0.05f, 0);
            transform.rotation = Quaternion.LookRotation(newDirection);
            
            yield return null;
        }
        
        if(useStepAnimation)
            _animator.SetBool("Moving", false);
            
        // wait for waitDuration after movement is finished
        yield return new WaitForSeconds(waitDuration);

        // decide whether or not to move again (based on current movement type)
        switch(_currentMovementType)
        {
            case MovementType.Wander:
            case MovementType.WanderAroundInitialPosition:
                Move();
                break;
        }
    }

    void OnTargetAcquired(Transform target)
    {
        this.target = target;
    }
}