using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HUDTurnBaseCombat : HUDBase
{
    public enum ETBCHUDButtonActions
    {
        Attack,
        Defend,
        Skill
    }

    public delegate void OnHUDButtonPressedSignature(ETBCHUDButtonActions ButtonAction, UnitController EnemyUnit);
    public event OnHUDButtonPressedSignature OnHUDButtonPressedDelegate;

    [Header("PARAMETERS:")]
    [Header("Enemy Unit Select Button")]
    [SerializeField] private GameObject EnemyUnitSelectButtonPrefab = null;
    [SerializeField] private Vector3 EnemyUnitSelectButtonSpawningOffset = Vector3.zero;

    private Button[] TBCMenuButtons;
    private List<Button> EnemyUnitSelectButtons = new List<Button>();

    private void GatherAndBindTBCMenuButtons()
    {
        TBCMenuButtons = GetComponentsInChildren<Button>();
        if (TBCMenuButtons == null || TBCMenuButtons.Length == 0)
        {
            Debug.LogError("Unable to bind the TBCMenuButtons due to the Array is empty");
            return;
        }

        foreach (Button TBCMenuButton in TBCMenuButtons)
        {
            if (!TBCMenuButton)
            {
                Debug.LogError("Invalid TBCMenuButton(Button)");
                continue;
            }

            TBCMenuButton.onClick.AddListener(() => OnTBCMenuButtonPressed(TBCMenuButton.gameObject.name));
        }
    }

    private void CreateEnemyUnitSelectButtons(UnitController[] EnemyUnits)
    {
        foreach (UnitController EnemyUnit in EnemyUnits)
        {
            Vector3 SpawnPos = Camera.main.WorldToScreenPoint(EnemyUnit.transform.position);
            Button NewEnemyUnitSelectorButton = Instantiate(
                EnemyUnitSelectButtonPrefab,
                SpawnPos + EnemyUnitSelectButtonSpawningOffset,
                Quaternion.identity,
                gameObject.transform
                ).GetComponent<Button>();
            if (!NewEnemyUnitSelectorButton)
            {
                Debug.LogError("Invalid NewEnemyUnitSelectorButton(Button)");
                Destroy(NewEnemyUnitSelectorButton.gameObject);
                continue;
            }

            NewEnemyUnitSelectorButton.onClick.AddListener(() => OnEnemyUnitSelected(EnemyUnit));

            EnemyUnitSelectButtons.Add(NewEnemyUnitSelectorButton);
        }

        SetButtonsEnable(EnemyUnitSelectButtons.ToArray(), false);
    }

    public void Initialize(UnitController[] EnemyUnits)
    {
        GatherAndBindTBCMenuButtons();

        CreateEnemyUnitSelectButtons(EnemyUnits);
    }

    public void RemoveEnemyUnitSelectButtonByIndex(int EnemyUnitSelectButtonIndex)
    {
        Destroy(EnemyUnitSelectButtons[EnemyUnitSelectButtonIndex].gameObject);
        EnemyUnitSelectButtons.RemoveAt(EnemyUnitSelectButtonIndex);
    }

    public void ChangeFocusToTBCMenuButtonsButtons()
    {
        SetButtonsInteractableEnable(TBCMenuButtons, true);
        SetButtonsEnable(EnemyUnitSelectButtons.ToArray(), false);
        SetFocusOnButtons(TBCMenuButtons);
    }

    public bool AreEnemyUnitSelectButtonsFocused()
    {
        return AreButtonsFocused(EnemyUnitSelectButtons.ToArray());
    }

    private void OnTBCMenuButtonPressed(string TBCMenuButtonName)
    {
        if (TBCMenuButtonName == "AttackButton")
        {
            SetButtonsInteractableEnable(TBCMenuButtons, false);
            SetButtonsEnable(EnemyUnitSelectButtons.ToArray(), true);
            SetFocusOnButtons(EnemyUnitSelectButtons.ToArray());

            return;
        }
        else if (TBCMenuButtonName == "DefendButton")
        {
            OnHUDButtonPressedDelegate?.Invoke(ETBCHUDButtonActions.Defend, null);
        }
        else if (TBCMenuButtonName == "SkillsButton")
        {
            OnHUDButtonPressedDelegate?.Invoke(ETBCHUDButtonActions.Skill, null);
        }
    }

    private void OnEnemyUnitSelected(UnitController EnemyUnit)
    {
        OnHUDButtonPressedDelegate?.Invoke(ETBCHUDButtonActions.Attack, EnemyUnit);

        SetButtonsEnable(EnemyUnitSelectButtons.ToArray(), false);
        SetButtonsInteractableEnable(TBCMenuButtons, true);
        SetFocusOnButtons(TBCMenuButtons);
    }
}