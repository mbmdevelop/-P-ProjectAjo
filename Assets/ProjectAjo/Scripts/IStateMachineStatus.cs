using UnityEngine;

public interface IStateMachineStatus<T>
{
    public void SetState(T NewState);
}