using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using static HUDTurnBaseCombat;

public class GMTurnBaseCombat : GameModeBase
{
    [Header("PARAMETERS:")]
    [Header("Units")]
    [SerializeField] private GameObject AllyPrefab;
    [SerializeField] private GameObject EnemyPrefab;

    private IAA_MainInputActionAsset MainInputActionAsset = null;
    private InputAction CancelInputAction = null;

    private HUDTurnBaseCombat HUDTurnBaseCombat = null;

    private List<GameObject> UnitSpawnPoints = new List<GameObject>();
    private List<UnitController> AllyUnits = new List<UnitController>();
    private List<UnitController> EnemyUnits = new List<UnitController>();
    private List<UnitController> SpeedSortedUnits = new List<UnitController>();

    private UnitController ControlledUnit = null;
    private int ControlledUnitIndex = -1;

    private int ActingUnitIndex = 0;

    private void SetupInput()
    {
        if (MainInputActionAsset == null)
        {
            MainInputActionAsset = new IAA_MainInputActionAsset();
        }

        CancelInputAction = MainInputActionAsset.TurnBaseCombat.Cancel;
        CancelInputAction.Enable();
        CancelInputAction.performed += OnCancel;
    }

    private bool TryToGatherUnitSpawnPoints()
    {
        GameObject UnitDistributions = GameObject.FindWithTag("UnitDistributions");
        if (!UnitDistributions)
        {
            return false;
        }

        GameObject UnitDistributionToUse = null;
        foreach (Transform UnitDistributionTrans in UnitDistributions.transform)
        {
            GameObject UnitDistribution = UnitDistributionTrans.gameObject;
            if (UnitDistribution.activeSelf)
            {
                UnitDistributionToUse = UnitDistribution;
                break;
            }
        }

        if (!UnitDistributionToUse)
        {
            return false;
        }

        foreach (Transform UnitSpawnPointTrans in UnitDistributionToUse.transform)
        {
            GameObject UnitSpawnPoint = UnitSpawnPointTrans.gameObject;
            if (UnitSpawnPoint)
            {
                UnitSpawnPoints.Add(UnitSpawnPoint);
            }
        }

        return UnitSpawnPoints.Any();
    }

    private void SpawnUnits()
    {
        bool GatheringOfUnitSpawnPointsSuccessful = TryToGatherUnitSpawnPoints();
        if (!GatheringOfUnitSpawnPointsSuccessful)
        {
            Debug.LogError("TurnBaseCombatGameMode couldn't find any valid UnitSpawnPoint, the" +
                "units won't be spawned.");
            return;
        }

        foreach (GameObject UnitSpawnPoint in UnitSpawnPoints)
        {
            UnitSpawnPointCore UnitSpawnPointCore = UnitSpawnPoint.GetComponent<UnitSpawnPointCore>();
            if (!UnitSpawnPointCore)
            {
                continue;
            }

            SStatsData NewUnitStatsData = UnitSpawnPointCore.SpawnUnitStatsData;
            EUnitType NewUnitType = NewUnitStatsData.Type;
            GameObject NewUnitPrefab = NewUnitType == EUnitType.Ally ? AllyPrefab : EnemyPrefab;
            Transform SpawnTransform = UnitSpawnPoint.transform;
            SpawnTransform.localScale = Vector3.one;

            UnitController NewUnit = Instantiate(NewUnitPrefab, SpawnTransform).GetComponentInChildren<UnitController>();
            if (!NewUnit)
            {
                Debug.LogError("NewUnit could not be instantiated.");
                continue;
            }

            GameObject _Dynamic = GameObject.Find("_Dynamic");
            if (_Dynamic)
            {
                NewUnit.transform.SetParent(_Dynamic.transform, true);
            }
            else
            {
                Debug.LogError("The _Dynamic GameObject couldn't be found, the instantiated " +
                    "GameObject it will remain attached to the gameobject parent specified at " +
                    "the time of its creation");
            }

            StatsComponent NewUnitStatsComp = NewUnit.GetStatsComponent();
            if (NewUnitType == EUnitType.Ally)
            {
                NewUnitStatsData.Name = "Ally" + (AllyUnits.Count + 1);
                NewUnit.name = NewUnitStatsData.Name;

                NewUnitStatsComp.SetStatsData(NewUnitStatsData);

                AllyUnits.Add(NewUnit);

                NewUnit.OnReadyToChooseActionDelegate += OnControlledUnitReadyToChooseAction;
            }
            else
            {
                NewUnitStatsData.Name = "Enemy" + (EnemyUnits.Count + 1);
                NewUnit.name = NewUnitStatsData.Name;

                NewUnitStatsComp.SetStatsData(NewUnitStatsData);

                EnemyUnits.Add(NewUnit);
            }

            NewUnit.OnActionSequenceEndedDelegate += OnUnitActionSequenceEnded;
            NewUnit.OnDeathDelegate += OnUnitDeath;
        }
    }

    private void SetupHUD()
    {
        HUDTurnBaseCombat = FindFirstObjectByType<HUDTurnBaseCombat>();
        if (!HUDTurnBaseCombat)
        {
            Debug.LogError("TurnBaseCombatGameMode couldn't find any valid GameObject that has the" +
                "HUDTurnBaseCombat script, UI elements won't be shown.");
            return;
        }

        HUDTurnBaseCombat.Initialize(EnemyUnits.ToArray());
        HUDTurnBaseCombat.SetHUDEnable(false);

        HUDTurnBaseCombat.OnHUDButtonPressedDelegate += OnHUDButtonPressed;
    }

    private void Start()
    {
        SetupInput();

        SpawnUnits();

        SetupHUD();

        SwitchUnitControl();
    }

    private void OnDestroy()
    {
        CancelInputAction.performed -= OnCancel;
        CancelInputAction.Disable();
    }

    private void GenerateRandomEnemyUnitActions()
    {
        int NumAllyUnits = AllyUnits.Count;
        foreach (UnitController EnemyUnit in EnemyUnits)
        {
            int RandAllyUnitIndex = Random.Range(0, NumAllyUnits);
            EnemyUnit.AddAction(new AttackAction(EnemyUnit, AllyUnits[RandAllyUnitIndex]));
        }
    }

    private void ExecuteUnitActionSequences()
    {
        SpeedSortedUnits[ActingUnitIndex].ExecuteActionSequence();
    }

    private int GetNextValidUnitIndex(bool ReverseSearch)
    {
        int NextUnitIndex = AllyUnits.Count;
        if (ReverseSearch)
        {
            for (int i = ControlledUnitIndex - 1; i > -1; --i)
            {
                StatsComponent AllyUnitStatsComp = AllyUnits[i].GetStatsComponent();
                if (AllyUnitStatsComp.IsAlive())
                {
                    NextUnitIndex =  i;
                    break;
                }
            }
        }
        else
        {
            for (int i = ControlledUnitIndex + 1; i < AllyUnits.Count; ++i)
            {
                StatsComponent AllyUnitStatsComp = AllyUnits[i].GetStatsComponent();
                if (AllyUnitStatsComp.IsAlive())
                {
                    NextUnitIndex = i;
                    break;
                }
            }
        }

        return NextUnitIndex;
    }

    private int GetNextValidActingUnitIndex()
    {
        int NextActingUnitIndex = 0;
        for (int i = ActingUnitIndex + 1; i < SpeedSortedUnits.Count; ++i)
        {
            StatsComponent UnitStatsComp = SpeedSortedUnits[i].GetStatsComponent();
            if (UnitStatsComp.IsAlive())
            {
                NextActingUnitIndex = i;
                break;
            }
        }

        return NextActingUnitIndex;
    }

    private void OnCancel(InputAction.CallbackContext CallbackContext)
    {
        if (HUDTurnBaseCombat.AreEnemyUnitSelectButtonsFocused())
        {
            HUDTurnBaseCombat.ChangeFocusToTBCMenuButtonsButtons();
            return;
        }

        if (ControlledUnitIndex == 0 || !HUDTurnBaseCombat.gameObject.activeSelf)
        {
            return;
        }

        SwitchUnitControl(true);
    }

    private void OnHUDButtonPressed(ETBCHUDButtonActions ButtonAction, UnitController EnemyUnit)
    {
        switch (ButtonAction)
        {
            case ETBCHUDButtonActions.Attack: ControlledUnit.AddAction(new AttackAction(ControlledUnit, EnemyUnit)); break;
            case ETBCHUDButtonActions.Defend: ControlledUnit.AddAction(new DefendAction(ControlledUnit)); break;
            case ETBCHUDButtonActions.Skill: ControlledUnit.AddAction(new SkillAction(ControlledUnit)); break;
        }

        SwitchUnitControl();
    }

    private void OnUnitActionSequenceEnded()
    {
        if (ActingUnitIndex < SpeedSortedUnits.Count - 1)
        {
            ActingUnitIndex = GetNextValidActingUnitIndex();
            ExecuteUnitActionSequences();
        }
        else
        {
            ActingUnitIndex = 0;

            System.Action Func = () => { SwitchUnitControl(); };
            StartCoroutine(ExecuteFuncDeferredRoutine(1.0f, Func));
        }
    }

    private void OnUnitDeath(UnitController UnitCtrl)
    {
        UnitCtrl.OnDeathDelegate -= OnUnitDeath;

        StatsComponent UnitStatsComp = UnitCtrl.GetStatsComponent();
        if (UnitStatsComp.GetUnitType() == EUnitType.Ally)
        {
            AllyUnits.Remove(UnitCtrl);
        }
        else
        {
            HUDTurnBaseCombat.RemoveEnemyUnitSelectButtonByIndex(EnemyUnits.IndexOf(UnitCtrl));
            EnemyUnits.Remove(UnitCtrl);
        }

        Destroy(UnitCtrl.gameObject);
    }

    private void OnControlledUnitReadyToChooseAction()
    {
        HUDTurnBaseCombat.SetHUDEnable(true);
    }

    private void SwitchUnitControl(bool ToPreviousUnit = false)
    {
        HUDTurnBaseCombat.SetHUDEnable(false);

        if (ControlledUnit != null)
        {
            ControlledUnit.EndControlProcess();
        }

        ControlledUnitIndex = GetNextValidUnitIndex(ToPreviousUnit);
        if (ControlledUnitIndex < AllyUnits.Count)
        {
            ControlledUnit = AllyUnits[ControlledUnitIndex];
            ControlledUnit.BeginControlProcess();
        }
        else
        {
            ControlledUnit = null;
            ControlledUnitIndex = -1;

            GenerateRandomEnemyUnitActions();

            UnifyUnitLists();

            StartCoroutine(ExecuteFuncDeferredRoutine(0.5f, ExecuteUnitActionSequences));
        }
    }

    private void UnifyUnitLists()
    {
        SpeedSortedUnits.Clear();
        SpeedSortedUnits.AddRange(AllyUnits);
        SpeedSortedUnits.AddRange(EnemyUnits);
    }

    IEnumerator ExecuteFuncDeferredRoutine(float WaitingTime, System.Action Func)
    {
        yield return new WaitForSeconds(WaitingTime);

        Func?.Invoke();
    }
}