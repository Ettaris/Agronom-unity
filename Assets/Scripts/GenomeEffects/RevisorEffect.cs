using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Infrastructure.Events;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Managers;
using Systems;

namespace GenomeEffects
{
    /// <summary>
    /// При посадке: если на поле <= 5 растений, вырастить все остальные (кроме себя).
    /// Если на поле > 5 растений, убить половину поля (кроме себя) без начисления калорий.
    /// </summary>
    public class RevisorEffect : GenomeEffectBase, IOnPlantPlaced
    {
        public RevisorEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnPlantPlaced(PlantInstance plant, int x, int y, GridBoard board)
        {
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (runData == null) return;

            var allPlants = runData.Board.GetAllPlants();
            if (allPlants == null || allPlants.Count < 1) return;

            var others = new List<PlantInstance>();
            foreach (var p in allPlants)
            {
                if (p != plant) others.Add(p);
            }

            if (others.Count == 0) return;

            var resolver = ServiceLocator.Get<PropertyResolverSystem>();

            if (others.Count <= 5)
            {
                foreach (var other in others)
                {
                    other.GrowthProgress = 1f;
                    EventBus.Publish(new PlantGrownEvent { Plant = other });
                }
                Debug.Log($"Revisor: {others.Count} plants grown.");
            }
            else
            {
                int killCount = Mathf.FloorToInt(others.Count * 0.5f);
                if (killCount <= 0) return;

                var random = runData.Random;
                for (int i = others.Count - 1; i > 0; i--)
                {
                    int j = random.NextInt(i + 1);
                    var temp = others[i];
                    others[i] = others[j];
                    others[j] = temp;
                }

                int killed = 0;
                for (int i = 0; i < others.Count && killed < killCount; i++)
                {
                    var target = others[i];
                    if (target == null || target.CurrentCell == null) continue;

                    int cx = target.CurrentCell.X;
                    int cy = target.CurrentCell.Y;

                    board.RemovePlant(cx, cy);
                    target.CurrentCell = null;
                    resolver.UnregisterPlant(target);

                    EventBus.Publish(new PlantKilledEvent
                    {
                        Plant = target,
                        X = cx,
                        Y = cy,
                        Reason = "Revisor"
                    });

                    killed++;
                }
                Debug.Log($"Revisor: {killed} plants killed.");
            }
        }
    }
}