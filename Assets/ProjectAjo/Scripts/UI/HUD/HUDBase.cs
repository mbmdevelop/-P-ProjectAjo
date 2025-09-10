using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDBase : MonoBehaviour
{
    private EventSystem EventSystem = null;

    private void Awake()
    {
        EventSystem = FindFirstObjectByType<EventSystem>();
    }

    public void SetHUDEnable(bool NewValue)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(NewValue);
        }
    }
    
    protected void SetFocusOnButtons(Button[] Buttons)
    {
        Buttons[0].Select();
    }

    protected void SetButtonsInteractableEnable(Button[] Buttons, bool NewValue)
    {
        foreach (Button Button in Buttons)
        {
            Button.interactable = NewValue;
        }
    }

    protected void SetButtonsEnable(Button[] Buttons, bool NewValue)
    {
        foreach (Button Button in Buttons)
        {
            Button.gameObject.SetActive(NewValue);
        }
    }

    protected bool AreButtonsFocused(Button[] Buttons)
    {
        foreach (Button Button in Buttons)
        {
            if (EventSystem.currentSelectedGameObject == Button.gameObject)
            {
                return true;
            }
        }

        return false;
    }
}
