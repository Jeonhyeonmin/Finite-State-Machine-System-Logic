using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    #region Variables

    protected StateMachine<ZombieController> stateMachine;
    public StateMachine<ZombieController> StateMachine => stateMachine;
    
    public Transform target;
    public float attackRange;
    public Transform[] waypoints;
    public Transform targetWaypoint = null;
    private int waypointIndex = 0;

    private CharacterController controller;
    private NavMeshAgent agent;

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

    #endregion Variables

    #region Unity Methods

    private void Start()
    {
        stateMachine = new StateMachine<ZombieController>(this, new MoveToWaypoint());
        stateMachine.AddState(new IdleState());

        controller = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();

        if (groundCheckAutoSettings)
        {
            ConfigureGroundCheckSettings();
        }
    }

    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
        Debug.Log(stateMachine.CurrentState);

        GroundCheck();
        controller.Move(moveVelocity * Time.deltaTime);
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
            Vector3 spherePosition = agent.transform.position + offset;

            if (Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore))
            {
                isGrounded = true;
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
            Debug.Log("공격 가능 여부 : " + (distance <= attackRange));
            return (distance <= attackRange);
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
