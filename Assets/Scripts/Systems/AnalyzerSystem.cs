using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Data;
using Managers;

namespace Systems
{
    /// <summary>
    /// —истема анализа растений с помощью фермента.
    /// ћожет работать как с посаженными растени€ми (на поле), так и с карточками в руке.
    /// </summary>
    public class AnalyzerSystem : IGameSystem
    {
        private RunData _runData;
        private JournalSystem _journalSystem;
        private Hand _hand;

        public void Initialize()
        {
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);

        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);

        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in AnalyzerSystem!");
                return;
            }
            _journalSystem = ServiceLocator.Get<JournalSystem>();
            _hand = _runData.Hand;
        }

        /// <summary>
        /// јнализирует растение с использованием фермента.
        /// </summary>
        /// <param name="plant">–астение (может быть посажено или карточка в руке)</param>
        /// <param name="ferment">»спользуемый фермент (должен быть в руке)</param>
        /// <returns>true, если анализ успешен</returns>
        public bool AnalyzePlant(PlantInstance plant, FermentData ferment)
        {
            if (plant == null || ferment == null)
            {
                UnityEngine.Debug.LogWarning("AnalyzePlant: plant or ferment is null.");
                return false;
            }

            // ѕровер€ем, есть ли фермент в руке
            ItemInstance fermentItem = null;
            foreach (var item in _hand.GetAll())
            {
                if (item.Data is FermentData && item.Data == ferment)
                {
                    fermentItem = item;
                    break;
                }
            }

            if (fermentItem == null)
            {
                UnityEngine.Debug.LogWarning("AnalyzePlant: Ferment not found in hand.");
                return false;
            }

            // ѕровер€ем, есть ли у растени€ свойства дл€ анализа
            if (plant.Genome.Properties.Count == 0)
            {
                UnityEngine.Debug.Log($"Plant {plant.PlantData.itemName} has no properties to discover.");
                // ¬сЄ равно считаем, что анализ прошЄл, фермент тратитс€
                _hand.Remove(fermentItem);
                EventBus.Publish(new FermentUsedEvent { Target = plant, Ferment = ferment });
                return true;
            }

            // ќткрываем все свойства растени€ (добавл€ем в журнал)
            foreach (var prop in plant.Genome.Properties)
            {
                _journalSystem.DiscoverProperty(prop.Data);
                EventBus.Publish(new GenomeDiscoveredEvent
                {
                    Plant = plant,
                    Property = prop
                });
            }

            // ”дал€ем фермент из руки
            _hand.Remove(fermentItem);

            // ѕубликуем событие об использовании фермента
            EventBus.Publish(new FermentUsedEvent
            {
                Target = plant,
                Ferment = ferment
            });

            UnityEngine.Debug.Log($"Plant {plant.PlantData.itemName} analyzed. {plant.Genome.Properties.Count} properties discovered.");
            return true;
        }
    }
}