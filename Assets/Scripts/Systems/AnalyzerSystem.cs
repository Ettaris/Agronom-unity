using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Data;
using Managers;
using System.Collections.Generic;

namespace Systems
{
    /// <summary>
    /// —истема анализа растений с помощью фермента.
    /// ћожет работать как с посаженными растени€ми (на поле), так и с карточками в руке.
    /// </summary>
    public class AnalyzerSystem : IRunAware
    {
        private Hand _hand;

        public void OnRunDataSetup(RunData runData)
        {
            _hand = runData.Hand;
            if (runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in AnalyzerSystem!");
                return;
            }
        }

        private bool IsPropertyPermanent(PlantInstance plant, GenomePropertyInstance prop)
        {
            if (plant.PermanentModifier != null && plant.PermanentModifier.Data == prop.Data)
                return true;

            if (plant.PlantData.fixedPermanentModifier == prop.Data)
                return true;

            return false;
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

            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (runData != null)
            {
                if (!runData.DiscoveredGenomes.ContainsKey(plant.PlantData))
                    runData.DiscoveredGenomes[plant.PlantData] = new List<GenomePropertyData>();

                foreach (var prop in plant.Genome.Properties)
                {
                    if (!runData.DiscoveredGenomes[plant.PlantData].Contains(prop.Data))
                        runData.DiscoveredGenomes[plant.PlantData].Add(prop.Data);
                }
            }

            if (plant.Genome.Properties.Count > 0)
            {
                foreach (var prop in plant.Genome.Properties)
                {
                    bool isPermanent = IsPropertyPermanent(plant, prop);
                    EventBus.Publish(new GenomeDiscoveredEvent
                    {
                        Plant = plant,
                        Property = prop,
                        isPermanent = isPermanent
                    });
                }
            }

            _hand.Remove(fermentItem);

            EventBus.Publish(new HandUpdatedEvent());
            EventBus.Publish(new PlantAnalyzedEvent { Plant = plant });

            UnityEngine.Debug.Log($"Plant {plant.PlantData.itemName} analyzed. {plant.Genome.Properties.Count} properties discovered.");
            return true;
        }
    }
}