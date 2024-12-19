using UnityEngine;
using UnityEngine.AI;

public class MoveState : State<ZombieController>
{
    private Animator anim;
    private CharacterController controller;
    private NavMeshAgent agent;

    private int hashMove = Animator.StringToHash("Move");

    public override void OnInitialzed()
    {
        anim = context.GetComponentInChildren<Animator>();
        controller = context.GetComponent<CharacterController>();
        agent = context.GetComponent<NavMeshAgent>();
    }

    public override void OnEnter()
    {
        agent?.SetDestination(context.target.position);
        anim?.SetBool(hashMove, true);   
    }

    public override void Update(float delta)
    {
        Transform enemy = context.SearchEnemy();

        if (enemy)
        {
            Debug.Log("적 발견");
            agent.SetDestination(context.target.position);

            if (agent.remainingDistance > agent.stoppingDistance)
            {
                controller.Move(new Vector3(agent.velocity.x, 0, agent.velocity.z) * delta);
                return;
            }
        }

        if (enemy && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("사정거리 접촉");
            stateMachine.ChangeState<IdleState>();
        }

        if (!enemy)
        {
            Debug.Log("적 없음");
            stateMachine.ChangeState<IdleState>();
        }
    }

    public override void OnExit()
    {
        anim?.SetBool(hashMove, false);
        agent.ResetPath();
    }
}
