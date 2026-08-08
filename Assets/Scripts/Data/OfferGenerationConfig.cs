using UnityEngine;

[CreateAssetMenu(fileName = "OfferGenerationConfig", menuName = "Game/Offer Generation Config")]
public class OfferGenerationConfig : ScriptableObject
{
    [Header("Количество карт в оффере")]
    public int cardsPerDay = 6;

    [Header("Сколько карт можно выбрать")]
    public int selectableCards = 2;

    [Header("Гарантированное количество растений")]
    public int guaranteedPlants = 2;

    /// <summary>
    /// Используются в WeightRandom в качестве процентов(шанс) выпадения.
    /// </summary>
    [Header("Веса типов предметов(Шанс выпадения)")]
    public int plantWeight = 70;
    public int fermentWeight = 20;
    public int batteryWeight = 10;
}