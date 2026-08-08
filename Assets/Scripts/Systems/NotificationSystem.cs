using System.Collections.Generic;
using UnityEngine;
using Infrastructure;
using Infrastructure.Events;
using DG.Tweening;
using Data;

namespace Systems
{
    public class NotificationSystem : MonoBehaviour, IGameSystem
    {
        [Header("References")]
        [SerializeField] private Transform _container; // родительский объект для уведомлений
        [SerializeField] private GameObject _notificationPrefab;

        [Header("Settings")]
        [SerializeField] private int _poolSize = 5;
        [SerializeField] private float _spacing = 10f; // отступ между уведомлениями

        private Queue<NotificationData> _queue = new Queue<NotificationData>();
        private List<NotificationView> _activeNotifications = new List<NotificationView>();
        private Stack<NotificationView> _pool = new Stack<NotificationView>();

        private bool _isProcessing;

        public void Initialize()
        {
            // Находим ссылки, если не назначены в инспекторе
            if (_container == null)
            {
                GameObject containerObj = GameObject.Find("NotificationContainer");
                if (containerObj != null) _container = containerObj.transform;
                else
                {
                    containerObj = new GameObject("NotificationContainer");
                    _container = containerObj.transform;
                    _container.SetParent(GameObject.Find("Canvas")?.transform);
                }
            }

            if (_notificationPrefab == null)
            {
                Debug.LogError("NotificationSystem: Notification prefab not set!");
                return;
            }

            // Подписка на события
            EventBus.Subscribe<PlantGrownEvent>(OnPlantGrown);
            EventBus.Subscribe<PlantHarvestedEvent>(OnPlantHarvested);
            EventBus.Subscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
            EventBus.Subscribe<PlantKilledEvent>(OnPlantKilled);
            EventBus.Subscribe<HandFullEvent>(OnHandFull);
            EventBus.Subscribe<PlantKilledByCentrifugeEvent>(OnPlantKilledByCentrifuge);
        }

        private void Start()
        {

            for (int i = 0; i < _poolSize; i++)
            {
                CreatePooledObject();
            }
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<PlantGrownEvent>(OnPlantGrown);
            EventBus.Unsubscribe<PlantHarvestedEvent>(OnPlantHarvested);
            EventBus.Unsubscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
            EventBus.Unsubscribe<PlantKilledEvent>(OnPlantKilled);
            EventBus.Unsubscribe<HandFullEvent>(OnHandFull);
            EventBus.Unsubscribe<PlantKilledByCentrifugeEvent>(OnPlantKilledByCentrifuge);

            // Очищаем все активные уведомления
            foreach (var notification in _activeNotifications)
            {
                notification.ForceHide();
                ReturnToPool(notification);
            }
            _activeNotifications.Clear();
            _queue.Clear();
            _isProcessing = false;
        }

        private GameObject CreatePooledObject()
        {
            GameObject obj = Object.Instantiate(_notificationPrefab, _container);
            obj.SetActive(false);
            var view = obj.GetComponent<NotificationView>();
            if (view != null)
            {
                _pool.Push(view);
            }
            else
            {
                Debug.LogError("Notification prefab missing NotificationView component!");
            }
            return obj;
        }

        private NotificationView GetFromPool()
        {
            if (_pool.Count > 0)
            {
                var view = _pool.Pop();
                view.gameObject.SetActive(true);
                return view;
            }
            else
            {
                // Если пул пуст – создаём новый объект (можно увеличить пул)
                GameObject obj = Object.Instantiate(_notificationPrefab, _container);
                var view = obj.GetComponent<NotificationView>();
                if (view != null)
                {
                    return view;
                }
                else
                {
                    Debug.LogError("Created object without NotificationView!");
                    return null;
                }
            }
        }

        private void ReturnToPool(NotificationView view)
        {
            view.ForceHide();
            view.transform.SetParent(_container, false);
            _pool.Push(view);
        }

        private void EnqueueNotification(NotificationData data)
        {
            Debug.Log($"NotificationSystem: Enqueued notification: {data.Message}");
            _queue.Enqueue(data);
            if (!_isProcessing)
            {
                ProcessQueue();
            }
        }

        private void ProcessQueue()
        {
            if (_queue.Count == 0 || _isProcessing) return;

            _isProcessing = true;

            // Проверяем, есть ли свободные места (лимит активных)
            // Если активных слишком много – подождём
            if (_activeNotifications.Count >= _poolSize)
            {
                _isProcessing = false;
                Debug.LogError("Too much active notes");
                return;
            }

            var data = _queue.Dequeue();

            NotificationView view = GetFromPool();
            if (view == null)
            {
                Debug.LogError(view + " null view");
                _isProcessing = false;
                return;
            }

            // Позиционирование (выравнивание сверху вниз)
            view.transform.SetAsFirstSibling();
            view.transform.localPosition = new Vector3(0, -_activeNotifications.Count * (60 + _spacing), 0);


            view.Show(data);

            // По окончании анимации возвращаем в пул и продолжаем очередь
            DOVirtual.DelayedCall(data.Duration + 0.5f, () =>
            {
                ReturnToPool(view);
                _activeNotifications.Remove(view);
                _isProcessing = false;

                // Обновляем позиции остальных
                for (int i = 0; i < _activeNotifications.Count; i++)
                {
                    _activeNotifications[i].transform.DOLocalMoveY(-i * (60 + _spacing), 0.3f).SetEase(Ease.OutQuad);
                }

                ProcessQueue(); // Обрабатываем следующее
            });

            _activeNotifications.Add(view);
        }

        #region Обработчики событий

        private void OnPlantGrown(PlantGrownEvent evt)
        {
            //Debug.Log($"NotificationSystem: OnPlantGrown received for {evt.Plant.PlantData.itemName}");
            //EnqueueNotification(new NotificationData(
            //    $"{evt.Plant.PlantData.itemName} созрело!",
            //    evt.Plant.PlantData.icon,
            //    Color.green
            //));
        }

        private void OnPlantKilledByCentrifuge(PlantKilledByCentrifugeEvent evt)
        {
            EnqueueNotification(new NotificationData(
                $"{evt.Plant.PlantData.itemName} разложился в центрифуге!",
                evt.Plant.PlantData.icon,
                Color.red,
                3f
            ));
        }

        private void OnPlantHarvested(PlantHarvestedEvent evt)
        {
            //EnqueueNotification(new NotificationData(
            //    $"+{evt.CaloriesGained} калорий!",
            //    null,
            //    Color.yellow
            //));
        }

        private void OnGenomeDiscovered(GenomeDiscoveredEvent evt)
        {
            EnqueueNotification(new NotificationData(
                $"Открыто свойство: {evt.Property.Data.propertyName}!",
                evt.Property.Data.icon,
                Color.cyan,
                3f
            ));
        }


        private void OnPlantKilled(PlantKilledEvent evt)
        {
            EnqueueNotification(new NotificationData(
                $"{evt.Plant.PlantData.itemName} погиб{(evt.Reason != null ? $" ({evt.Reason})" : "")}",
                evt.Plant.PlantData.icon,
                Color.red,
                2.5f
            ));
        }

        private void OnHandFull(HandFullEvent evt)
        {
            EnqueueNotification(new NotificationData(
                "Рука полна!",
                null,
                Color.red
            ));
        }

        #endregion
    }


}