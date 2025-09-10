using System;
using TMPro;
using UnityEngine;
using static HUDTurnBaseCombat;
using static UnityEngine.GraphicsBuffer;

public enum EUnitType
{
    Ally,
    Enemy
}

[Serializable]
public struct SStatsData
{
    [HideInInspector]
    public string Name;
    public EUnitType Type;
    public sbyte Level;
    public int ExpPoints;
    public int Health;
    public int Mana;
    public int Attack;
    public int Defense;
}

public class StatsComponent : MonoBehaviour
{
    public delegate void OnHealthDepletedSignature();
    public delegate void OnHealthDownSignature(int Delta);
    public event OnHealthDepletedSignature OnHealthDepletedDelegate;
    public event OnHealthDownSignature OnHealthDownDelegate;

    private SStatsData StatsData;

    public void ApplyHealthChange(int Delta)
    {
        StatsData.Health = Mathf.Max(0, StatsData.Health + Delta);

        if (Delta <= 0)
        {
            OnHealthDownDelegate?.Invoke(Delta);
        }

        if (StatsData.Health <= 0)
        {
            OnHealthDepletedDelegate?.Invoke();
        }
    }

    public void SetStatsData(SStatsData NewStatsData)
    {
        StatsData = NewStatsData;
    }

    public EUnitType GetUnitType()
    {
        return StatsData.Type;
    }

    public string GetName()
    {
        return StatsData.Name;
    }

    public int GetHealth()
    {
        return StatsData.Health;
    }

    public int GetAttack()
    {
        return StatsData.Attack;
    }

    public bool IsAlive()
    {
        return StatsData.Health > 0;
    }
}