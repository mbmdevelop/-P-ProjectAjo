using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static HUDTurnBaseCombat;

public class UnitController : MonoBehaviour, IStateMachineStatus<UnitController.EStates>
{
    public enum EStates
    {
        Idle,
        ChoosingAction,
        Attacking,
        Dead
    }

    public delegate void OnReadyToChooseActionSignature();
    public event OnReadyToChooseActionSignature OnReadyToChooseActionDelegate;

    public delegate void OnActionSequenceEndedSignature();
    public event OnActionSequenceEndedSignature OnActionSequenceEndedDelegate;

    public delegate void OnDeathSignature(UnitController Instigator);
    public event OnDeathSignature OnDeathDelegate;


    [Header("PARAMETERS:")]
    [Header("DamageTextPopUp")]
    [SerializeField] private GameObject DamageTextPopUpPrefab = null;
    [SerializeField] private Vector2 SpawnOffset = Vector2.zero;

    private Animator AnimatorComp = null;
    private StatsComponent StatsComp = null;
    private DamageTextPopUpController DamageTextPopUpController = null;

    private EStates CurrentState = EStates.Idle;

    private List<Action> ActionSequence = new List<Action>();
    private int ExecutingActionIndex = 0;
    private bool IsExecutingAction = false;

    public StatsComponent GetStatsComponent()
    {
        return StatsComp;
    }

    private void Awake()
    {
        AnimatorComp = GetComponentInChildren<Animator>();

        StatsComp = GetComponentInChildren<StatsComponent>();
        StatsComp.OnHealthDepletedDelegate += OnHealthDepleted;
        StatsComp.OnHealthDownDelegate += OnHealthDown;
    }

    public void SetState(UnitController.EStates NewState)
    {
        if (CurrentState == NewState)
        {
            return;
        }

        CurrentState = NewState;

        switch (CurrentState)
        {
            case EStates.Idle: OnIdle(); break;
            case EStates.ChoosingAction: OnChoosingAction(); break;
            case EStates.Attacking: OnAttacking(); break;
            case EStates.Dead: OnDeath(); break;
        }
    }

    public void AddAction(Action NewAction)
    {
        ActionSequence.Add(NewAction);
    }

    private void BeginActionExecution()
    {
        IsExecutingAction = true;

        ActionSequence[ExecutingActionIndex].Execute();
    }

    public void EndActionExecution()
    {
        IsExecutingAction = false;

        ++ExecutingActionIndex;
        if (ExecutingActionIndex < ActionSequence.Count)
        {
            BeginActionExecution();
        }
        else
        {
            ExecutingActionIndex = 0;
            ActionSequence.Clear();
            OnActionSequenceEndedDelegate?.Invoke();
        }
    }

    public void ExecuteActionSequence()
    {
        BeginActionExecution();
    }

    public void BeginControlProcess()
    {
        ActionSequence.Clear();

        AnimatorComp.SetTrigger("TrgChoosingAction");
        AnimatorComp.SetBool("HasActionBeenChosen", false);
    }

    public void EndControlProcess()
    {
        AnimatorComp.SetBool("HasActionBeenChosen", true);
    }

    public void InitiateAttack()
    {
        AnimatorComp.SetTrigger("TrgAttacking");
    }

    public void ApplyAttack()
    {
        AttackAction ExecutingAttackAction = (AttackAction)ActionSequence[ExecutingActionIndex];

        StatsComponent TargetStatsComp = ExecutingAttackAction.GetAttackTarget().GetStatsComponent();
        TargetStatsComp.ApplyHealthChange(-StatsComp.GetAttack());
    }

    private void OnHealthDepleted()
    {
        AnimatorComp.SetTrigger("TrgDead");
    }

    private void OnHealthDown(int Damage)
    {
        Vector3 SpawnPos = new Vector3(transform.position.x + SpawnOffset.x, transform.position.y + SpawnOffset.y, transform.position.z);
        DamageTextPopUpController = Instantiate(DamageTextPopUpPrefab, SpawnPos, Quaternion.identity).GetComponentInChildren<DamageTextPopUpController>();

        DamageTextPopUpController.SetDamageText((Damage).ToString());
    }

    private void OnIdle()
    {
        if (IsExecutingAction)
        {
            EndActionExecution();
        }
    }

    private void OnChoosingAction()
    {
        OnReadyToChooseActionDelegate?.Invoke();
    }

    private void OnAttacking()
    {

    }

    private void OnDeath()
    {
        OnDeathDelegate?.Invoke(this);
    }
}