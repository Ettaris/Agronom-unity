using UnityEngine;

[CreateAssetMenu(fileName = "ModifierAssignmentConfig", menuName = "Game/Modifier Assignment Config")]
public class ModifierAssignmentConfig : ScriptableObject
{
    [Header("Permanent Modifier")]
    public bool assignPermanent = true;
    [Range(0f, 1f)]
    public float permanentChance = 0.25f;

    [Header("Second Modifier")]
    [Range(0f, 1f)]
    public float secondModifierChance = 0.5f;
    public bool allowDuplicate = false; // запрещаем дублирование по умолчанию

    [Header("Rarity Weights for Modifiers")]
    public int commonWeight = 50;
    public int uncommonWeight = 30;
    public int rareWeight = 15;
    public int epicWeight = 4;
    public int legendaryWeight = 1;
}