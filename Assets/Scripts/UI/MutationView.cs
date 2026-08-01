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
    [SerializeField] private float _spriteTransitionDuration = 0.3f;

    private PlantInstance _currentPlant;
    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = _plantImage.transform.localScale;
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
        _currentPlant = plant;
        if (plant == null)
        {
            Clear();
            return;
        }
        ResetVisuals();
        UpdateVisuals(false);
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
        _plantImage.transform.localScale = Vector3.one;
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

        // ћутации Ц если есть свойства
        if (_currentPlant.Genome.Properties.Count > 0)
        {
            var mutationSprite = GetMutationSprite();
            if (mutationSprite != null)
            {
                _mutationAnimator.SetTrigger("Mutate");
            }
        }
    }

    private Sprite GetGrowthSprite()
    {
        Debug.Log("Get current sprite for plant from mutation");
        if (_currentPlant == null) return null;
        var data = _currentPlant.PlantData;
        if (data.growthSprites == null || data.growthSprites.Length == 0)
        {
            Debug.Log("null sprite");
            return null;
        }

        float progress = _currentPlant.GrowthProgress;
        int stage = Mathf.FloorToInt(progress * (data.growthSprites.Length - 1));
        stage = Mathf.Clamp(stage, 0, data.growthSprites.Length - 1);
        return data.growthSprites[stage];
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
        Debug.Log("Mutation Clear view");
        _plantImage.transform.localScale = Vector3.one;
        _plantImage.color = Color.white;
        DOTween.Kill(_plantImage);
        DOTween.Kill(_plantImage.transform);
        if (_mutationOverlay != null)
            _mutationOverlay.color = Color.clear;
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

    public void Refresh() => UpdateVisuals(false);
}