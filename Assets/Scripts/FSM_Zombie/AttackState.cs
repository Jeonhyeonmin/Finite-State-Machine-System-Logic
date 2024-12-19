using UnityEngine;

public class AttackState : State<ZombieController>
{
    private Animator anim;
    private int hashAttack = Animator.StringToHash("Attack");
    private FieldOfView fieldOfView;

    public override void OnInitialzed()
    {
        anim = context.GetComponentInChildren<Animator>();
        fieldOfView = context.GetComponent<FieldOfView>();
    }

    public override void OnEnter()
    {
        fieldOfView.viewAngle = 360f;

        if (context.IsAvailableAttack)
        {
            if (context.IsCheckTargetAngle)
            {
                anim?.SetTrigger(hashAttack);
                Debug.Log("공격");
            }
            else
            {
                Debug.Log("이동 필요");
                anim?.SetTrigger(hashAttack);
            }
        }
        else
        {
            stateMachine.ChangeState<IdleState>();
        }   
    }

    public override void Update(float delta)
    {
        //throw new System.NotImplementedException();
    }
}
