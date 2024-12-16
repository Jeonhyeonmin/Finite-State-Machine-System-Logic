using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput)),RequireComponent(typeof(CharacterController)), RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(LineRenderer))]
public class PlayerController : MonoBehaviour
{
    #region Variables

    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float dashDistance = 5f;

    private PlayerInput _playerInput;
    private CharacterController _characterController;
    private NavMeshAgent _navMeshAgent;
    private Camera _playerCamera;
    private PlayerCamera _playerCameraScript;

    [Header("Player Movement Variables")]
    [SerializeField] private Vector3 moveVelocity;

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

    [Space(30), Header("Player Navigation Variables")]
    [SerializeField] private GameObject mapTracker;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Color pathColor = Color.green;
    [SerializeField] private float lineWidth = 0.2f;

    [Space(30), Header("Player Animation")]
    private readonly float _animationSpeed = 0.15f;
    private readonly int _animationType = Animator.StringToHash("moveType");
    [SerializeField] private Animator _animator;
    private bool isSprint;

    #endregion Variables

    private void Start()
    {
        InitializeComponents();
        ConfigureGroundCheckSettings();

        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = true;

        mapTracker = Instantiate(mapTracker);
        mapTracker.SetActive(false);

        _lineRenderer = GetComponent<LineRenderer>();

        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = pathColor;
        _lineRenderer.endColor = pathColor;

        _playerInput.actions["Sprint"].performed += ctx => isSprint = true;
        _playerInput.actions["Sprint"].canceled += ctx => isSprint = false;
    }

    private void InitializeComponents()
    {
        _playerInput = GetComponent<PlayerInput>();
        _characterController = GetComponent<CharacterController>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _playerCamera = Camera.main;
        _playerCameraScript = Camera.main.GetComponent<PlayerCamera>();
    }

    private void ConfigureGroundCheckSettings()
    {
        Bounds playerBounds = _characterController.bounds;

        if (groundCheckAutoSettings)
        {
            groundCheckRadius = 0.07f;
            groundCheckDistance = 0;
            groundCheckOffset = playerBounds.extents.z - groundCheckRadius;
            groundCheckCount = 6;
        }
    }

    private void Update()
    {
        GroundCheck();
        MovePlayer();
        LineRendererPath();

        if (_playerInput.actions["Ctrl"].ReadValue<float>() > 0)
            _playerCameraScript.Angle += _playerInput.actions["Look"].ReadValue<Vector2>().x * _playerCameraScript.MouseSensitivity;
    }

    private void LateUpdate()
    {
        _navMeshAgent.nextPosition = _characterController.transform.position;
    }

    private void GroundCheck()
    {
        isGrounded = false;
        float angleStep = 360f / groundCheckCount;

        for (int i = 0; i < groundCheckCount; i++)
        {
            float angle = i * angleStep;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * groundCheckOffset + Vector3.down * groundCheckDistance;
            Vector3 spherePosition = _navMeshAgent.transform.position + offset;

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

    private void MovePlayer()
    {
        _navMeshAgent.speed = isSprint ? sprintSpeed : walkSpeed;

        if (isGrounded)
        {
            if (_playerInput.actions["Click"].triggered)
            {
                Ray ray = _playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100f, groundLayer))
                {
                    _navMeshAgent.SetDestination(hitInfo.point);
                }
            }

            if (_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
            {
                _characterController.Move(new Vector3(_navMeshAgent.velocity.x, moveVelocity.y, _navMeshAgent.velocity.z) * Time.deltaTime);
                mapTracker.SetActive(true);
                mapTracker.transform.position = _navMeshAgent.destination;
                _animator.SetFloat(_animationType, Mathf.Lerp(_animator.GetFloat(_animationType), isSprint ? 1 : 0.5f, _animationSpeed));
            }
            else
            {
                _characterController.Move(Vector3.zero);
                mapTracker.SetActive(false);
                _animator.SetFloat(_animationType, 0);
            }
        }
        else
        {
            _characterController.Move(new Vector3(_navMeshAgent.velocity.x / 2, moveVelocity.y, _navMeshAgent.velocity.z / 2) * Time.deltaTime);
            
            if (_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
            {
                mapTracker.SetActive(true);
                mapTracker.transform.position = _navMeshAgent.destination;
                _animator.SetFloat(_animationType, Mathf.Lerp(_animator.GetFloat(_animationType), isSprint ? 1 : 0.5f, _animationSpeed));
            }
            else
            {
                mapTracker.SetActive(false);
                _animator.SetFloat(_animationType, Mathf.Lerp(_animationType, 0, _animationSpeed));
            }
        }
    }

    private void LineRendererPath()
    {
        if (_navMeshAgent.hasPath)
        {
            DrawPath();
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }

    private void DrawPath()
    {
        NavMeshPath path = _navMeshAgent.path;

        if (path.corners.Length < 2) return;

        _lineRenderer.enabled = true;
        
        Vector3[] linePoints = path.corners;
        _lineRenderer.positionCount = linePoints.Length;

        for (int i = 0; i < linePoints.Length; i++)
        {
            _lineRenderer.SetPosition(i, linePoints[i]);
        }
    }

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
