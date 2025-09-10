using TMPro;
using TMPro.EditorUtilities;
using UnityEditor.Search;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Action
{

    public Action(UnitController InInstigator)
    {
        Instigator = InInstigator;
    }
    public virtual void Execute() {}

    protected UnitController Instigator;
}

public class AttackAction : Action
{
    public AttackAction(UnitController InInstigator, UnitController InTarget)
        : base(InInstigator)
    {
        Target = InTarget;
    }

    public override void Execute()
    {
        if (!Target)
        {
            return;
        }

        StatsComponent TargetStatsComp = Target.GetStatsComponent();
        if (!TargetStatsComp || !TargetStatsComp.IsAlive())
        {
            Debug.Log("The TargetUnit has been Killed so the Action will be consumed.");

            Instigator.EndActionExecution();

            return;
        }

        Instigator.InitiateAttack();
    }

    public UnitController GetAttackTarget()
    {
        return Target;
    }

    private UnitController Target;
}

public class DefendAction : Action
{
    public DefendAction(UnitController InInstigator) : base(InInstigator) {}

    public override void Execute()
    {
        StatsComponent InstigatorStatsComp = Instigator.GetStatsComponent();

        Debug.Log(InstigatorStatsComp.GetName() + ": defends himself");
    }
}
public class SkillAction : Action
{
    public SkillAction(UnitController InInstigator) : base(InInstigator) {}

    public override void Execute()
    {
        Debug.Log("WIP!(What did you think was going to happen?-.-)");
    }
}
