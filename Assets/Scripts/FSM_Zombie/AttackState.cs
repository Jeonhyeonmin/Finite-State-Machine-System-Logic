using UnityEngine;

public class AttackState : State<ZombieController>
{
    private Animator anim;
    private int hashAttack = Animator.StringToHash("Attack");

    public override void OnInitialzed()
    {
        anim = context.GetComponentInChildren<Animator>();
    }

    public override void OnEnter()
    {
        if (context.IsAvailableAttack)
        {
            anim?.SetTrigger(hashAttack);
            Debug.Log("공격");
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
