using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using Data;
using Managers;

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

        // Подписка на события (только когда окно открыто)
        EventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
        EventBus.Subscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Subscribe<FermentUsedEvent>(OnFermentUsed);
        EventBus.Subscribe<BatteryUsedEvent>(OnBatteryUsed);
        EventBus.Subscribe<GenomeTransferredEvent>(OnGenomeTransferred);

        _actionButton.onClick.AddListener(OnActionButtonClicked);
        _actionButton.interactable = false;

        // По умолчанию окно закрыто
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
        EventBus.Unsubscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Unsubscribe<FermentUsedEvent>(OnFermentUsed);
        EventBus.Unsubscribe<BatteryUsedEvent>(OnBatteryUsed);
        EventBus.Unsubscribe<GenomeTransferredEvent>(OnGenomeTransferred);
    }

    #region Открытие/закрытие

    public void OpenLab()
    {
        gameObject.SetActive(true);
        _labAnimator.SetTrigger("Open");
        ClearSlots();
    }

    public void CloseLab()
    {
        _labAnimator.SetTrigger("Close");
        ClearSlots();
        DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
    }

    #endregion

    #region Обработчики событий

    private void OnCardSelected(CardSelectedEvent evt)
    {
        // Игнорируем выбор, если окно не активно – только Drag&Drop
        if (!gameObject.activeInHierarchy) return;
        // Никакой автоматической вставки!
        Debug.Log($"LaboratoryView: Card {evt.Item.Data.itemName} selected, but drag&drop only.");
    }

    private void OnHandUpdated(HandUpdatedEvent evt)
    {
        // Если предмет в слоте был удалён из руки (использован) – очищаем слоты
        if (_consumableItem != null)
        {
            bool exists = false;
            foreach (var item in _runData.Hand.GetAll())
            {
                if (item == _consumableItem) { exists = true; break; }
            }
            if (!exists) ClearSlots();
        }
    }

    private void OnFermentUsed(FermentUsedEvent evt) { }
    private void OnBatteryUsed(BatteryUsedEvent evt) { }
    private void OnGenomeTransferred(GenomeTransferredEvent evt)
    {
        _labAnimator.SetTrigger("Success");
    }

    #endregion

    #region Управление слотами (только через Drag&Drop)

    public bool OnItemDropped(ItemInstance item)
    {
        if (!gameObject.activeInHierarchy) return false;

        // Если это растение
        if (item is PlantInstance plant)
        {
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
            return false;
        }

        // Если это фермент или батарейка
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

    private void SetConsumableSlot(ItemInstance item)
    {
        _consumableItem = item;
        _consumableSlot.SetItem(item);

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

    #region Выполнение действия

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

        ClearSlots();
    }

    #endregion
}