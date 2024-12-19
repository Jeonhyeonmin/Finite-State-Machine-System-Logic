using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    #region Variables

    protected StateMachine<ZombieController> stateMachine;
    public StateMachine<ZombieController> StateMachine => stateMachine;
    
    public Transform target => fieldOfView?.NearestTarget;
    public float attackRange;
    public Transform[] waypoints;
    public Transform targetWaypoint = null;
    private int waypointIndex = 0;

    private CharacterController controller;
    private NavMeshAgent agent;

    private FieldOfView fieldOfView;

    private Vector3 moveVelocity = Vector3.zero;

    #region Ground Check Variables

    [Space(30), Header("Ground Check Variables")]
    [SerializeField] private bool groundCheckAutoSettings;
    [SerializeField] private float groundCheckRadius = 0.02f;
    [SerializeField] private float groundCheckDistance = 0f;
    [SerializeField] private float groundCheckOffset = 0f;
    [SerializeField] private int groundCheckCount = 0;
    [SerializeField] private bool isGrounded;
    [SerializeField] private LayerMask groundLayer;

    #endregion Ground Check Variables

    [SerializeField] private float patrolStopDistance = 0.1f;
    [SerializeField] private float attackStopDistance = 0.5f;

    #endregion Variables

    #region Unity Methods

    private void Start()
    {
        stateMachine = new StateMachine<ZombieController>(this, new MoveToWaypoint());
        stateMachine.AddState(new IdleState());
        stateMachine.AddState(new MoveState());
        stateMachine.AddState(new AttackState());

        controller = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();

        if (groundCheckAutoSettings)
        {
            ConfigureGroundCheckSettings();
        }

        fieldOfView = GetComponent<FieldOfView>();
    }

    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
        Debug.Log(stateMachine.CurrentState);

        GroundCheck();
        controller.Move(moveVelocity * Time.deltaTime);

        if (target != null)
        {
            agent.stoppingDistance = attackStopDistance;
        }
        else
        {
            agent.stoppingDistance = patrolStopDistance;
        }
    }

    private void LateUpdate()
    {
        //agent.nextPosition = controller.transform.position;
    }

    private void ConfigureGroundCheckSettings()
    {
        Bounds playerBounds = controller.bounds;

        if (groundCheckAutoSettings)
        {
            groundCheckRadius = 0.07f;
            groundCheckDistance = 0;
            groundCheckOffset = playerBounds.extents.z - groundCheckRadius;
            groundCheckCount = 6;
        }
    }

    private void GroundCheck()
{
    isGrounded = false;
    float angleStep = 360f / groundCheckCount;

    for (int i = 0; i < groundCheckCount; i++)
    {
        float angle = i * angleStep;
        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * groundCheckOffset + Vector3.down * groundCheckDistance;
        Vector3 spherePosition = controller.transform.position + offset;

        Debug.DrawRay(spherePosition, Vector3.down * groundCheckDistance, Color.red, 1f);

        if (Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            Debug.Log("Is Grounded");
            moveVelocity.y = Mathf.MoveTowards(moveVelocity.y, -2f, Mathf.Abs(Physics.gravity.y * Time.deltaTime));
            break;
        }
    }

    if (!isGrounded)
    {
        moveVelocity.y += Physics.gravity.y * Time.deltaTime;
        moveVelocity.y = Mathf.Clamp(moveVelocity.y, Physics.gravity.y, moveVelocity.y);
    }
}

    #region Other Methods

    public bool IsAvailableAttack
    {
        get 
        {
            if (!target)
            {
                return false;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            return (distance <= attackRange);
        }
    }

    public bool IsCheckTargetAngle
    {
        get
        {
            if (!target)
            {
                return false;
            }

            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle <= 10)
            {
                return true;
            }
            else
            {
                StopCoroutine(transformRotation());
                StartCoroutine(transformRotation());
                return false;
            }
        }
    }

    private IEnumerator transformRotation()
    {
        while (true)
        {
            if (target)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dirToTarget.x, 0, dirToTarget.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 2f * Time.deltaTime);
            }

            yield return null;
        }
        
    }

    public Transform SearchEnemy()
    {
        return target;
    }

    public Transform FindNextWaypoint()
    {
        targetWaypoint = null;

        if (waypoints.Length > 0)
        {
            targetWaypoint = waypoints[waypointIndex];
        }
        Debug.Log("다음 목적지 : " + targetWaypoint);
        waypointIndex = (waypointIndex + 1) % waypoints.Length;

        return targetWaypoint;
    }

    public void StartFieldOfViewCoroutine()
    {
        StopCoroutine(FieldOfViewCoroutine());
        StartCoroutine(FieldOfViewCoroutine());
    }

    public IEnumerator FieldOfViewCoroutine()
    {
        yield return new WaitForSeconds(2.0f);
        fieldOfView.viewAngle = 90f;
    }

    #endregion Other Methods

    #endregion Unity Methods

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float angleStep = 360f / groundCheckCount;

        for (int i = 0; i < groundCheckCount; i++)
        {
            float angle = i * angleStep;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * groundCheckOffset + Vector3.down * groundCheckDistance;
            Vector3 spherePosition = transform.position + offset;

            Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
        }
    }
}
