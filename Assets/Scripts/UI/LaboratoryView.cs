using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using Data;
using Managers;

/// <summary>
/// UI лаборатории. Поддерживает режимы анализа (фермент) и центрифуги (батарейка).
/// Использует слоты для расходников и растений, кнопку выполнения.
/// </summary>
public class LaboratoryView : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private LaboratorySlotView _consumableSlot;
    [SerializeField] private LaboratorySlotView _plantSlotA;
    [SerializeField] private LaboratorySlotView _plantSlotB;

    [Header("Buttons")]
    [SerializeField] private Button _actionButton;

    [Header("Info Panel")]
    [SerializeField] private PlantInfoView _plantInfo;

    [Header("Animator")]
    [SerializeField] private Animator _labAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _slotPulseDuration = 0.5f;

    private RunData _runData;
    private ItemInstance _consumableItem;
    private PlantInstance _plantA;
    private PlantInstance _plantB;
    private bool _isCentrifugeMode;

    private void Awake()
    {
        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
        if (_runData == null) Debug.LogError("LaboratoryView: RunData is null");

        EventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
        EventBus.Subscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Subscribe<FermentUsedEvent>(OnFermentUsed);
        EventBus.Subscribe<BatteryUsedEvent>(OnBatteryUsed);
        EventBus.Subscribe<GenomeTransferredEvent>(OnGenomeTransferred);

        _actionButton.onClick.AddListener(OnActionButtonClicked);
        _actionButton.interactable = false;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
        EventBus.Unsubscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Unsubscribe<FermentUsedEvent>(OnFermentUsed);
        EventBus.Unsubscribe<BatteryUsedEvent>(OnBatteryUsed);
        EventBus.Unsubscribe<GenomeTransferredEvent>(OnGenomeTransferred);
    }

    #region Event Handlers

    private void OnCardSelected(CardSelectedEvent evt)
    {
        if (!gameObject.activeInHierarchy) return;

        // Если слот расходника пуст и предмет является ферментом или батарейкой
        if (_consumableSlot.IsEmpty && (evt.Item.Data is FermentData || evt.Item.Data is BatteryData))
        {
            SetConsumableSlot(evt.Item);
            return;
        }

        // Если расходник уже есть, пытаемся вставить растение
        if (!_consumableSlot.IsEmpty && evt.Item is PlantInstance plant)
        {
            if (_isCentrifugeMode)
            {
                // Режим центрифуги: два слота
                if (_plantSlotA.IsEmpty)
                {
                    SetPlantSlotA(plant);
                    return;
                }
                if (_plantSlotB.IsEmpty && _plantSlotA.Item != plant)
                {
                    SetPlantSlotB(plant);
                    return;
                }
            }
            else
            {
                // Режим анализа: один слот
                if (_plantSlotA.IsEmpty)
                {
                    SetPlantSlotA(plant);
                    return;
                }
            }
        }
        // Если не удалось вставить – отрицательная обратная связь (можно добавить анимацию)
        Debug.Log("No suitable slot for this item");
    }

    private void OnHandUpdated(HandUpdatedEvent evt)
    {
        // Если предмет в слоте расходника был удалён из руки (использован вне лаборатории) – очищаем всё
        if (_consumableItem != null)
        {
            bool exists = false;
            foreach (var item in _runData.Hand.GetAll())
            {
                if (item == _consumableItem)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                ClearSlots();
            }
        }
    }

    /// <summary>
    /// Обрабатывает бросок предмета в лабораторию.
    /// Распределяет предмет по свободным слотам в зависимости от типа.
    /// </summary>
    /// <returns>true, если предмет был размещён</returns>
    public bool OnItemDropped(ItemInstance item)
    {
        if (item == null) return false;

        // Если предмет – растение
        if (item is PlantInstance plant)
        {
            // Проверяем, есть ли свободный слот для растения
            if (_plantSlotA.IsEmpty)
            {
                SetPlantSlotA(plant);
                return true;
            }
            if (_isCentrifugeMode && _plantSlotB.IsEmpty)
            {
                SetPlantSlotB(plant);
                return true;
            }
            // Если слоты заняты – не можем разместить
            return false;
        }

        // Если предмет – фермент или батарейка (расходник)
        if (item.Data is FermentData || item.Data is BatteryData)
        {
            if (_consumableSlot.IsEmpty)
            {
                SetConsumableSlot(item);
                return true;
            }
            return false;
        }

        return false;
    }

    private void OnFermentUsed(FermentUsedEvent evt) { /* Можно добавить анимацию или логику, но необязательно */ }
    private void OnBatteryUsed(BatteryUsedEvent evt) { /* Можно добавить анимацию */ }
    private void OnGenomeTransferred(GenomeTransferredEvent evt)
    {
        // Анимация успешного переноса
        _labAnimator.SetTrigger("Success");
    }

    #endregion

    #region Slot Management

    private void SetConsumableSlot(ItemInstance item)
    {
        _consumableItem = item;
        _consumableSlot.SetItem(item);

        // Определяем режим
        if (item.Data is BatteryData)
        {
            _isCentrifugeMode = true;
            _plantSlotB.gameObject.SetActive(true);
            _labAnimator.SetBool("CentrifugeMode", true);
        }
        else if (item.Data is FermentData)
        {
            _isCentrifugeMode = false;
            _plantSlotB.gameObject.SetActive(false);
            _labAnimator.SetBool("CentrifugeMode", false);
        }

        _consumableSlot.Pulse();
        UpdateActionButton();
    }

    private void SetPlantSlotA(PlantInstance plant)
    {
        _plantA = plant;
        _plantSlotA.SetItem(plant);
        _plantSlotA.Pulse();
        _plantInfo.ShowPlant(plant);
        UpdateActionButton();
    }

    private void SetPlantSlotB(PlantInstance plant)
    {
        _plantB = plant;
        _plantSlotB.SetItem(plant);
        _plantSlotB.Pulse();
        UpdateActionButton();
    }

    private void ClearSlots()
    {
        _consumableItem = null;
        _plantA = null;
        _plantB = null;
        _consumableSlot.Clear();
        _plantSlotA.Clear();
        _plantSlotB.Clear();
        _plantInfo.Clear();
        _actionButton.interactable = false;
        _labAnimator.SetTrigger("Clear");
    }

    private void UpdateActionButton()
    {
        bool canExecute = false;
        if (_isCentrifugeMode)
        {
            canExecute = _consumableItem != null && _plantA != null && _plantB != null;
        }
        else
        {
            canExecute = _consumableItem != null && _plantA != null;
        }
        _actionButton.interactable = canExecute;
    }

    #endregion

    #region Execution

    private void OnActionButtonClicked()
    {
        if (!_actionButton.interactable) return;

        _labAnimator.SetTrigger("Execute");

        if (_isCentrifugeMode)
        {
            var battery = _consumableItem.Data as BatteryData;
            if (battery != null && _plantA != null && _plantB != null)
            {
                CommandProcessor.Execute(new TransferGenomeCommand
                {
                    Donor = _plantA,
                    Target = _plantB,
                    Battery = battery
                });
            }
        }
        else
        {
            var ferment = _consumableItem.Data as FermentData;
            if (ferment != null && _plantA != null)
            {
                CommandProcessor.Execute(new AnalyzeCommand
                {
                    Plant = _plantA,
                    Ferment = ferment
                });
            }
        }

        // Очищаем слоты после выполнения (расходник удалён, растения могли быть уничтожены или изменены)
        ClearSlots();
    }

    #endregion

    #region Public Methods

    public void OpenLab()
    {
        gameObject.SetActive(true);
        _labAnimator.SetTrigger("Open");
        ClearSlots();
    }

    public void CloseLab()
    {
        _labAnimator.SetTrigger("Close");
        DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
    }

    #endregion
}