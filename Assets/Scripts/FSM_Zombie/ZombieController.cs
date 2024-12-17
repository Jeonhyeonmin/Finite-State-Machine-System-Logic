using UnityEngine;

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

    #endregion Variables

    #region Unity Methods

    private void Start()
    {
        stateMachine = new StateMachine<ZombieController>(this, new MoveToWaypoint());
        stateMachine.AddState(new IdleState());
    }

    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
        Debug.Log(stateMachine.CurrentState);
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
}
