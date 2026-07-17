using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Gameplay;

/// <summary>
/// ќтвечает за визуализацию состо€ни€ растени€: рост и мутации.
/// ѕоддерживает комбинированные спрайты: мутационные спрайты разделены по стади€м роста.
/// </summary>
public class MutationView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _plantImage;
    [SerializeField] private Image _mutationOverlay; // опционально

    [Header("Animator")]
    [SerializeField] private Animator _mutationAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _spriteTransitionDuration = 0.3f;

    private PlantInstance _currentPlant;
    private BoardCellView _cellView;

    private void Awake()
    {
        _cellView = GetComponent<BoardCellView>();
        if (_cellView == null)
        {
            Debug.LogError("MutationView must be attached to BoardCellView");
            return;
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
        _currentPlant = plant;
        if (plant == null)
        {
            Clear();
            return;
        }
        UpdateVisuals(false);
    }

    private void UpdateVisuals(bool animate = true)
    {
        if (_currentPlant == null) return;

        // ѕолучаем стадию роста (индекс в массиве growthSprites)
        int growthStage = GetGrowthStageIndex();
        // ѕолучаем уровень мутации (0..N-1)
        int mutationLevel = GetMutationLevel();

        Sprite targetSprite = GetCombinedSprite(growthStage, mutationLevel);

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

        if (mutationLevel > 0)
        {
            _mutationAnimator.SetTrigger("Mutate");
        }

        UpdateGenomeVisuals();
    }

    private int GetGrowthStageIndex()
    {
        var data = _currentPlant.PlantData;
        if (data.growthSprites == null || data.growthSprites.Length == 0)
            return 0;

        float progress = _currentPlant.GrowthProgress;
        int stage = Mathf.FloorToInt(progress * (data.growthSprites.Length - 1));
        return Mathf.Clamp(stage, 0, data.growthSprites.Length - 1);
    }

    private int GetMutationLevel()
    {
        if (_currentPlant.Genome.Properties.Count == 0)
            return 0;

        var data = _currentPlant.PlantData;
        if (data.mutationStages == null || data.mutationStages.Length == 0)
            return 0;

        int fillPercent = _currentPlant.GetGenomeFillPercent();
        // ≈сли mutationStages задан как массив уровней мутации (без учЄта роста)
        // то возвращаем индекс от 0 до mutationStages.Length-1
        // ≈сли же он задан как комбинированный (все стадии роста * уровни мутации),
        // то это обрабатываетс€ в GetCombinedSprite, а здесь мы просто возвращаем уровень.
        // ƒл€ простоты считаем, что если длина массива кратна количеству стадий роста,
        // то это комбинированный, иначе Ц просто уровни.
        int stagesCount = data.growthSprites != null ? data.growthSprites.Length : 1;
        if (data.mutationStages.Length % stagesCount == 0)
        {
            int levelsPerStage = data.mutationStages.Length / stagesCount;
            return Mathf.Clamp(Mathf.FloorToInt(fillPercent / 100f * (levelsPerStage - 1)), 0, levelsPerStage - 1);
        }
        else
        {
            // ѕростой массив уровней мутации
            return Mathf.Clamp(Mathf.FloorToInt(fillPercent / 100f * (data.mutationStages.Length - 1)), 0, data.mutationStages.Length - 1);
        }
    }

    private Sprite GetCombinedSprite(int growthStage, int mutationLevel)
    {
        var data = _currentPlant.PlantData;

        // ≈сли есть мутационные спрайты
        if (data.mutationStages != null && data.mutationStages.Length > 0)
        {
            int stagesCount = data.growthSprites != null ? data.growthSprites.Length : 1;
            // ѕровер€ем, €вл€етс€ ли массив комбинированным (длина = стадии роста * уровни мутации)
            if (data.mutationStages.Length % stagesCount == 0)
            {
                int levelsPerStage = data.mutationStages.Length / stagesCount;
                int index = growthStage * levelsPerStage + mutationLevel;
                if (index >= 0 && index < data.mutationStages.Length)
                    return data.mutationStages[index];
            }
            else
            {
                // ≈сли массив не комбинированный, используем только уровень мутации
                int index = Mathf.Clamp(mutationLevel, 0, data.mutationStages.Length - 1);
                return data.mutationStages[index];
            }
        }

        // ≈сли мутации нет, используем спрайты роста
        if (data.growthSprites != null && data.growthSprites.Length > 0)
        {
            int index = Mathf.Clamp(growthStage, 0, data.growthSprites.Length - 1);
            return data.growthSprites[index];
        }

        // ≈сли ничего нет Ц возвращаем null (или дефолтный спрайт)
        return null;
    }

    private void UpdateGenomeVisuals()
    {
        if (_mutationOverlay != null && _currentPlant != null)
        {
            float fill = _currentPlant.GetGenomeFillPercent() / 100f;
            _mutationOverlay.color = new Color(1f, 1f, 1f, fill * 0.5f);
        }
    }

    private void Clear()
    {
        _plantImage.sprite = null;
        _plantImage.DOKill();
        if (_mutationOverlay != null) _mutationOverlay.color = Color.clear;
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