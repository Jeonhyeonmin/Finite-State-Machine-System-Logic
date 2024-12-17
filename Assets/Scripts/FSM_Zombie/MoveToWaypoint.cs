using UnityEngine;
using UnityEngine.AI;

public class MoveToWaypoint : State<ZombieController>
{
    private Animator anim;
    private CharacterController controller;
    private NavMeshAgent agent;

    protected int hashMove = Animator.StringToHash("Move");

    public override void OnInitialzed()
    {
        anim = context.GetComponentInChildren<Animator>();
        controller = context.GetComponent<CharacterController>();
        agent = context.GetComponent<NavMeshAgent>();
    }

    public override void OnEnter()
    {
        if (context.targetWaypoint == null)
        {
            context.FindNextWaypoint();
        }

        if (context.targetWaypoint)
        {
            agent?.SetDestination(context.targetWaypoint.position);
           anim?.SetBool(hashMove, true);
        }   
    }

    public override void Update(float deltaTime)
    {
        Transform enemy = context.SearchEnemy();
        Debug.Log("Update");
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
        else
        {
            Debug.Log("이동");
            if (!agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance))
            {
                Transform nextDest = context.FindNextWaypoint();
                Debug.Log("다음 목적지 : " + nextDest);
                if (nextDest)
                {
                    agent.SetDestination(nextDest.position);
                }

                //stateMachine.ChangeState<IdleState>();
            }
            else
            {
                controller.Move(agent.velocity * deltaTime);
                //anim.SetFloat(hashMove, agent.velocity.magnitude / agent.speed, 1f, deltaTime);
            }
        }
    }

    public override void OnExit()
    {
        anim?.SetBool(hashMove, false);
        agent.ResetPath();
    }
}
