using UnityEngine;

public class UnitSpawnPointCore : MonoBehaviour
{
    public void OnValidate()
    {
        SpriteRenderer EditorSprite = gameObject.GetComponentInChildren<SpriteRenderer>();
        if (!EditorSprite)
        {
            Debug.LogError("Couldn't find a child SpriteRenderer Component.");
            return;
        }

        Color NewSpriteColor = Color.white;
        switch (SpawnUnitStatsData.Type)
        {
            case EUnitType.Ally: NewSpriteColor = Color.green; break;
            case EUnitType.Enemy: NewSpriteColor = Color.red; break;
            default: break;
        }

        EditorSprite.color = NewSpriteColor;
    }

    private void HideEditorSprite()
    {
        SpriteRenderer EditorSprite = gameObject.GetComponentInChildren<SpriteRenderer>();
        if (!EditorSprite)
        {
            Debug.LogError("Couldn't find a child SpriteRenderer Component.");
            return;
        }
        EditorSprite.enabled = false;
    }

    public void Start()
    {
        HideEditorSprite();
    }

    public SStatsData SpawnUnitStatsData;

    private SpriteRenderer EditorSprite;
}