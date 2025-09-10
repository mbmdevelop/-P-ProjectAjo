using UnityEngine;

public class GenericStateUpdater<T,TU> : StateMachineBehaviour where TU : IStateMachineStatus<T>
{
    [Header("PARAMETERS:")]
    [SerializeField] private T StateToUpdate;
    [SerializeField] bool WaitForTransitionToEnd = false;

    private TU StateMachineStatus;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (StateMachineStatus == null)
        {
            StateMachineStatus = animator.gameObject.GetComponentInChildren<TU>();
        }

        if (WaitForTransitionToEnd)
        {
            return;
        }

        StateMachineStatus.SetState(StateToUpdate);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!WaitForTransitionToEnd)
        {
            return;
        }

        if (stateInfo.normalizedTime <= 1.0f)
        {
            return;
        }

        StateMachineStatus.SetState(StateToUpdate);
    }
}
