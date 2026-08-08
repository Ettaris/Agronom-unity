using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Gameplay;

public class MutationView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _plantImage;
    [SerializeField] private Image _mutationOverlay;
    [SerializeField] private Animator _mutationAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _spriteTransitionDuration = 0.3f;

    private PlantInstance _currentPlant;
    private BoardRoot _boardRoot;

    private void Awake()
    {
        if (_plantImage != null)
        {
            // Настраиваем pivot и anchors для центрирования
            var rect = _plantImage.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
        }


        EventBus.Subscribe<PlantGrownEvent>(OnPlantGrown);
        EventBus.Subscribe<GenomeChangedEvent>(OnGenomeChanged);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlantGrownEvent>(OnPlantGrown);
        EventBus.Unsubscribe<GenomeChangedEvent>(OnGenomeChanged);
    }

    public void Initialize(PlantInstance plant)
    {
        _boardRoot = ServiceLocator.TryGet<BoardRoot>(out var br) ? br : null;
        if (_boardRoot == null)
            Debug.LogError("MutationView: BoardRoot not found!");
        _currentPlant = plant;
        if (plant == null)
        {
            Clear();
            return;
        }
        ResetVisuals();
        // Обязательно обнуляем локальную позицию – мы дочерний объект опорной клетки
        transform.localPosition = Vector3.zero;
        PositionAndScale();
        UpdateVisuals(false);
    }

    private void PositionAndScale()
    {
        Debug.Log("Go to Pos and Scale");
        if (_currentPlant == null || _boardRoot == null) {
            Debug.Log($"Current plant == {_currentPlant} or _board root = {_boardRoot}");
            return;
        }

        Vector2Int size = _currentPlant.PlantData.size;
        Vector2Int pos = _currentPlant.Position;

        var anchorCell = _boardRoot.GetCellView(pos.x, pos.y);
        if (anchorCell == null)
        {
            Debug.LogError($"MutationView: No cell view at ({pos.x},{pos.y})");
            return;
        }

        // Получаем размер опорной клетки
        RectTransform cellRect = anchorCell.GetComponent<RectTransform>();
        float cellWidth = cellRect.rect.width;
        float cellHeight = cellRect.rect.height;

        // Обнуляем локальные координаты и масштаб
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        // Настраиваем RectTransform изображения
        RectTransform imgRect = _plantImage.rectTransform;
        imgRect.anchorMin = new Vector2(0.5f, 0.5f);
        imgRect.anchorMax = new Vector2(0.5f, 0.5f);
        imgRect.pivot = new Vector2(0.5f, 0.5f);

        // Размер изображения = размер клетки × размер растения
        imgRect.sizeDelta = new Vector2(cellWidth * size.x, cellHeight * size.y);

        // Смещение, чтобы центр изображения совпал с центром группы клеток
        float offsetX = cellWidth * (size.x - 1) / 2f;
        float offsetY = -cellHeight * (size.y - 1) / 2f;
        imgRect.anchoredPosition = new Vector2(offsetX, offsetY);

        Debug.Log($"MutationView: size={size}, cellSize=({cellWidth},{cellHeight}), sizeDelta={imgRect.sizeDelta}, anchoredPos={imgRect.anchoredPosition}");
    }

    private void ResetVisuals()
    {
        _plantImage.transform.localScale = Vector3.one;
        _plantImage.color = Color.white;
        _plantImage.gameObject.SetActive(true);
        DOTween.Kill(_plantImage);
        DOTween.Kill(_plantImage.transform);
        if (_mutationOverlay != null)
        {
            _mutationOverlay.color = Color.clear;
            DOTween.Kill(_mutationOverlay);
        }
    }

    private void UpdateVisuals(bool animate = true)
    {
        if (_currentPlant == null) return;

        Sprite targetSprite = GetGrowthSprite();
        if (targetSprite == null)
        {
            _plantImage.gameObject.SetActive(false);
            return;
        }

        _plantImage.gameObject.SetActive(true);
        _plantImage.color = Color.white;

        if (animate && targetSprite != _plantImage.sprite)
        {
            _plantImage.DOFade(0f, _spriteTransitionDuration / 2f).OnComplete(() =>
            {
                _plantImage.sprite = targetSprite;
                _plantImage.DOFade(1f, _spriteTransitionDuration / 2f);
            });
        }
        else
        {
            _plantImage.sprite = targetSprite;
        }

        if (_currentPlant.Genome.Properties.Count > 0)
        {
            var mutationSprite = GetMutationSprite();
            if (mutationSprite != null)
                _mutationAnimator.SetTrigger("Mutate");
        }
    }

    private Sprite GetGrowthSprite()
    {
        if (_currentPlant == null) return null;
        var data = _currentPlant.PlantData;
        if (data.growthSprites != null && data.growthSprites.Length > 0)
        {
            float progress = _currentPlant.GrowthProgress;
            int stage = Mathf.FloorToInt(progress * (data.growthSprites.Length - 1));
            stage = Mathf.Clamp(stage, 0, data.growthSprites.Length - 1);
            return data.growthSprites[stage];
        }
        return data.icon;
    }

    private Sprite GetMutationSprite()
    {
        if (_currentPlant == null) return null;
        var data = _currentPlant.PlantData;
        if (data.mutationStages == null || data.mutationStages.Length == 0)
            return null;

        int fillPercent = _currentPlant.GetGenomeFillPercent();
        int stage = Mathf.FloorToInt(fillPercent / 100f * (data.mutationStages.Length - 1));
        stage = Mathf.Clamp(stage, 0, data.mutationStages.Length - 1);
        return data.mutationStages[stage];
    }

    private void Clear()
    {
        _plantImage.sprite = null;
        _plantImage.gameObject.SetActive(false);
        _plantImage.transform.localScale = Vector3.one;
        _plantImage.color = Color.white;
        DOTween.Kill(_plantImage);
        DOTween.Kill(_plantImage.transform);
        if (_mutationOverlay != null)
            _mutationOverlay.color = Color.clear;
    }

    public void Refresh()
    {
        if (_currentPlant != null)
            UpdateVisuals(false);
    }

    private void OnPlantGrown(PlantGrownEvent evt)
    {
        if (evt.Plant == _currentPlant)
            UpdateVisuals(true);
    }

    private void OnGenomeChanged(GenomeChangedEvent evt)
    {
        if (evt.Plant == _currentPlant)
            UpdateVisuals(true);
    }
}