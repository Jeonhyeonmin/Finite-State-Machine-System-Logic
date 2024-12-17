using UnityEngine;

public class IdleState : State<ZombieController>
{
    public bool isPatrol = true;
    private float minIdleTime = 0.0f;
    private float maxIdleTime = 3.0f;
    private float idleTime = 0.0f;

    private Animator anim;
    private CharacterController controller;
    
    protected int hashMove = Animator.StringToHash("Move");

    public override void OnInitialzed()
    {
        anim = context.GetComponentInChildren<Animator>();
        controller = context.GetComponent<CharacterController>();
    }

    public override void OnEnter()
    {
        anim?.SetBool(hashMove, false);
        controller?.Move(Vector3.zero);

        if (isPatrol)
        {
            idleTime = Random.Range(minIdleTime, maxIdleTime);
        }
    }

    public override void Update(float delta)
    {
        Transform enemy = context.SearchEnemy();

        if (enemy)
        {
            if (context.IsAvailableAttack)
            {
                Debug.Log("공격");
                //stateMachine.ChangeState<AttackState>();
            }
            else
            {
                Debug.Log("이동");
                //stateMachine.ChangeState<MoveState>();
            }
        }

        else if (isPatrol && stateMachine.ElapsedTimeInState > idleTime)    // 순찰 상태에서 일정 시간이 지나면 다음 순찰로 이동
        {
            stateMachine.ChangeState<MoveToWaypoint>();
        }
    }

    public override void OnExit()
    {
        
    }
}
