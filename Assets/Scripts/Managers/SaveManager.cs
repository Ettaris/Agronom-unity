using UnityEngine;
using Infrastructure;
using Infrastructure.Events;
using Gameplay;
using Newtonsoft.Json;
using Data;
using System.Threading.Tasks;
using Systems;
using System.Collections.Generic;
using System;
using System.IO;

namespace Managers
{
    public class SaveManager : IGameSystem
    {
        private const string SAVE_KEY = "player_data";
        private const string JOURNAL_KEY = "journal";
        private const string RUN_KEY = "run";
        private ISaveProvider _provider;
        private RunManager _runManager;
        private JournalSystem _journalSystem;

        // ======================================================================
        //  Публичные асинхронные методы (использовать везде)
        // ======================================================================

        public async Task SaveJournalAsync(JournalData journal)
        {
            if (journal == null) return;
            string json = JsonConvert.SerializeObject(journal, Formatting.Indented);
            await _provider.SaveAsync(JOURNAL_KEY, json);
        }

        public async Task<JournalData> LoadJournalAsync()
        {
            var (success, json) = await _provider.LoadAsync(JOURNAL_KEY);
            if (!success || string.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject<JournalData>(json);
        }

        public async Task SaveRunAsync(RunData runData)
        {
            if (runData == null) return;
            var saveData = BuildSaveData(runData);
            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            await _provider.SaveAsync(RUN_KEY, json);
        }

        public async Task<RunData> LoadRunAsync()
        {
            var (success, json) = await _provider.LoadAsync(RUN_KEY);
            if (!success || string.IsNullOrEmpty(json)) return null;
            var saveData = JsonConvert.DeserializeObject<SaveData>(json,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            return RestoreRunData(saveData);
        }

        public async Task<bool> SaveGameAsync()
        {
            var runData = _runManager?.CurrentRunData;
            if (runData == null) return false;
            var saveData = BuildSaveData(runData);
            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            return await _provider.SaveAsync(SAVE_KEY, json);
        }

        public async Task<bool> LoadGameAsync()
        {
            var (success, json) = await _provider.LoadAsync(SAVE_KEY);
            if (!success || string.IsNullOrEmpty(json)) return false;

            SaveData saveData;
            try
            {
                saveData = JsonConvert.DeserializeObject<SaveData>(json,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            }
            catch
            {
                await _provider.DeleteAsync(SAVE_KEY);
                return false;
            }

            if (saveData == null)
            {
                await _provider.DeleteAsync(SAVE_KEY);
                return false;
            }

            var runData = RestoreRunData(saveData);
            if (runData == null)
            {
                await _provider.DeleteAsync(SAVE_KEY);
                return false;
            }

            var runManager = ServiceLocator.Get<RunManager>();
            runManager.LoadRunData(runData);
            EventBus.Publish(new HandUpdatedEvent());
            return true;
        }

        public async Task<bool> HasSaveAsync()
        {
            if (!_provider.HasSave(SAVE_KEY)) return false;
            try
            {
                var (success, json) = await _provider.LoadAsync(SAVE_KEY);
                if (!success || string.IsNullOrEmpty(json)) return false;
                var saveData = JsonConvert.DeserializeObject<SaveData>(json,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                return saveData != null;
            }
            catch
            {
                await _provider.DeleteAsync(SAVE_KEY);
                return false;
            }
        }

        public void SaveJournal(JournalData journal)
        {
            if (journal == null) return;
            string json = JsonConvert.SerializeObject(journal, Formatting.Indented);
            string path = GetJournalFilePath();
            File.WriteAllText(path, json);
        }

        public JournalData LoadJournal()
        {
            string path = GetJournalFilePath();
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<JournalData>(json);
        }

        private string GetJournalFilePath()
        {
            return Path.Combine(Application.persistentDataPath, "journal.json");
        }



        // ======== Вспомогательные методы ========

        private SaveData BuildSaveData(RunData runData)
        {
            if (runData == null) return null;

            var data = new SaveData
            {
                version = Application.version,
                saveTime = DateTime.Now,
                seed = runData.Seed,
                currentDay = runData.CurrentDay,
                calories = runData.Inventory.Calories,
                dailyQuota = runData.DailyQuota,
                isQuotaReached = runData.IsQuotaReached,
                journal = runData.Journal ?? new JournalData(),
                boardPlants = new List<SaveData.SerializedPlant>(),
                handItems = new List<SaveData.SerializedItem>(),
                deckItems = new List<SaveData.SerializedItem>()
            };

            // ---- Сохраняем поле ----
            var board = runData.Board;
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var cell = board.GetCell(x, y);
                    if (cell == null || cell.Plant == null) continue;

                    var plant = cell.Plant;
                    var sp = new SaveData.SerializedPlant
                    {
                        x = x,
                        y = y,
                        plantDataId = plant.PlantData.Id,
                        growthProgress = plant.GrowthProgress,
                        maxGenomeCapacity = plant.Genome.MaxCapacity,
                        currentGenomeWeight = plant.Genome.CurrentWeight,
                        properties = new List<SaveData.SerializedProperty>()
                    };

                    foreach (var prop in plant.Genome.Properties)
                    {
                        sp.properties.Add(new SaveData.SerializedProperty
                        {
                            propertyDataId = prop.Data.Id,
                            stacks = prop.Stacks
                        });
                    }

                    data.boardPlants.Add(sp);
                }
            }

            // ---- Сохраняем руку ----
            foreach (var item in runData.Hand.GetAll())
            {
                data.handItems.Add(SerializeItem(item));
            }

            // ---- Сохраняем колоду ----
            foreach (var item in runData.Deck.GetAllCards())
            {
                data.deckItems.Add(SerializeItem(item));
            }

            return data;
        }

        // Вспомогательный метод сериализации одного предмета
        private SaveData.SerializedItem SerializeItem(ItemInstance item)
        {
            if (item == null) return null;

            var si = new SaveData.SerializedItem
            {
                itemDataId = item.Data.Id,
                quantity = item.Quantity,
                isPlant = item is PlantInstance
            };

            if (item is PlantInstance plant)
            {
                si.plantData = new SaveData.SerializedPlant
                {
                    plantDataId = plant.PlantData.Id,
                    growthProgress = plant.GrowthProgress,
                    maxGenomeCapacity = plant.Genome.MaxCapacity,
                    currentGenomeWeight = plant.Genome.CurrentWeight,
                    properties = new List<SaveData.SerializedProperty>()
                };

                foreach (var prop in plant.Genome.Properties)
                {
                    si.plantData.properties.Add(new SaveData.SerializedProperty
                    {
                        propertyDataId = prop.Data.Id,
                        stacks = prop.Stacks
                    });
                }
            }

            return si;
        }


        private RunData RestoreRunData(SaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("RestoreRunData: data is null");
                return null;
            }

            var config = ServiceLocator.Get<GameConfig>();
            if (config == null)
            {
                Debug.LogError("RestoreRunData: GameConfig not found!");
                return null;
            }

            // Проверка пулов
            if (config.plantRarityPool == null || config.genomeRarityPool == null)
            {
                Debug.LogError("RestoreRunData: PlantPool or GenomePool is null in GameConfig!");
                return null;
            }

            // Создаём RunData
            var runData = new RunData(
                data.seed,
                config.boardWidth,
                config.boardHeight,
                config.maxHandSize,
                data.journal ?? new JournalData(),
                config.stages
            );

            // Восстанавливаем поле (с проверками)
            foreach (var sp in data.boardPlants ?? new List<SaveData.SerializedPlant>())
            {
                var plantData = config.plantRarityPool.commonPlants.Find(p => p.Id == sp.plantDataId);
                if (plantData == null)
                {
                    Debug.LogWarning($"PlantData with id {sp.plantDataId} not found");
                    continue;
                }

                var plant = new PlantInstance(plantData, sp.maxGenomeCapacity);
                plant.GrowthProgress = sp.growthProgress;

                //foreach (var spProp in sp.properties ?? new List<SaveData.SerializedProperty>())
                //{
                //    var propData = config.genomePool.genomeProperties.Find(p => p.Id == spProp.propertyDataId);
                //    if (propData == null) continue;
                //    var prop = new GenomePropertyInstance(propData, spProp.stacks);
                //    plant.AddGenomeProperty(prop);
                //}

                if (runData.Board.PlacePlant(plant, sp.x, sp.y))
                {
                    plant.CurrentCell = runData.Board.GetCell(sp.x, sp.y);
                    var resolver = ServiceLocator.Get<PropertyResolverSystem>();
                    resolver?.RegisterPlant(plant);
                }
            }

            // Восстанавливаем руку и колоду (аналогично с проверками)
            foreach (var si in data.handItems ?? new List<SaveData.SerializedItem>())
            {
                //var item = RestoreItem(si, config);
                //if (item != null) runData.Hand.Add(item);
            }

            foreach (var si in data.deckItems ?? new List<SaveData.SerializedItem>())
            {
                //var item = RestoreItem(si, config);
                //if (item != null) runData.Deck.Add(item);
            }

            // Восстанавливаем параметры
            runData.CurrentDay = data.currentDay;
            runData.Inventory.Calories = data.calories;
            runData.IsQuotaReached = data.isQuotaReached;
            runData.SetJournalData(data.journal ?? new JournalData());

            return runData;
        }


        public bool HasSave
        {
            get
            {
                if (!_provider.HasSave(SAVE_KEY)) return false;
                // Проверяем валидность файла
                try
                {
                    var (success, json) = _provider.LoadAsync(SAVE_KEY).Result;
                    if (!success || string.IsNullOrEmpty(json)) return false;
                    var saveData = JsonConvert.DeserializeObject<SaveData>(json,
                        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    if (saveData == null) return false;
                    // Можно также проверить, что восстановление RunData не падает с ошибкой
                    return true;
                }
                catch
                {
                    // Если файл повреждён, удаляем его и возвращаем false
                    _ = _provider.DeleteAsync(SAVE_KEY);
                    return false;
                }
            }
        }

        //private ItemInstance RestoreItem(SaveData.SerializedItem si, GameConfig config)
        //{
        //    // Ищем ItemData по id
        //    ItemData itemData = null;

        //    // Сначала ищем среди растений
        //    //itemData = config.plantPool?.plants.Find(p => p.Id == si.itemDataId);
        //    if (itemData == null)
        //        itemData = config.fermentPool?.ferments.Find(f => f.Id == si.itemDataId);
        //    if (itemData == null)
        //        itemData = config.batteryPool?.batteries.Find(b => b.Id == si.itemDataId);

        //    if (itemData == null)
        //    {
        //        Debug.LogWarning($"ItemData with id {si.itemDataId} not found");
        //        return null;
        //    }

        //    // Если это растение – создаём PlantInstance
        //    if (si.isPlant && si.plantData != null)
        //    {
        //        var plantData = config.plantPool.plants.Find(p => p.Id == si.plantData.plantDataId);
        //        if (plantData == null)
        //        {
        //            Debug.LogWarning($"PlantData with id {si.plantData.plantDataId} not found");
        //            return null;
        //        }

        //        var plant = new PlantInstance(plantData, si.plantData.maxGenomeCapacity);
        //        plant.GrowthProgress = si.plantData.growthProgress;
        //        foreach (var sp in si.plantData.properties)
        //        {
        //            var propData = config.genomePool.genomeProperties.Find(p => p.Id == sp.propertyDataId);
        //            if (propData == null) continue;
        //            var prop = new GenomePropertyInstance(propData, sp.stacks);
        //            plant.AddGenomeProperty(prop);
        //        }
        //        return plant;
        //    }
        //    else
        //    {
        //        // Обычный предмет (фермент, батарейка)
        //        return new ItemInstance(itemData, si.quantity);
        //    }
        //}

        // ======== IGameSystem ========
        public void Initialize()
        {
            _provider = new LocalFileSaveProvider();
            _runManager = ServiceLocator.Get<RunManager>();
            _journalSystem = ServiceLocator.Get<JournalSystem>();

            //EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
            //EventBus.Subscribe<RunEndedEvent>(OnRunEnded);
            //EventBus.Subscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
            //EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
            //EventBus.Subscribe<PlantHarvestedEvent>(OnPlantHarvested);
            //Application.quitting += OnApplicationQuitting;
        }

        public void Dispose()
        {
            //EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
            //EventBus.Unsubscribe<RunEndedEvent>(OnRunEnded);
            //EventBus.Unsubscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
            //EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
            //EventBus.Unsubscribe<PlantHarvestedEvent>(OnPlantHarvested);
            //Application.quitting -= OnApplicationQuitting;
        }

        // ======================================================================
        //  Обработчики событий (автосохранение)
        // ======================================================================

        //private async void OnDayEnded(DayEndedEvent evt) => await SaveGameAsync();
        //private async void OnRunEnded(RunEndedEvent evt) => await SaveGameAsync();
        //private async void OnGenomeDiscovered(GenomeDiscoveredEvent evt) => await SaveGameAsync();
        //private async void OnPlantPlaced(PlantPlacedEvent evt) => await SaveGameAsync();
        //private async void OnPlantHarvested(PlantHarvestedEvent evt) => await SaveGameAsync();
        //private async void OnApplicationQuitting() => await SaveGameAsync();
    }
}