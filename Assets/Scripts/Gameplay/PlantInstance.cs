using Data;
using UnityEngine;

namespace Gameplay
{
    public class PlantInstance : ItemInstance
    {
        public PlantData PlantData => Data as PlantData;
        public GenomeContainer Genome { get; private set; }
        public float GrowthProgress { get; set; }
        public bool IsGrown => GrowthProgress >= 1f;
        public Cell CurrentCell { get; set; }
        public Vector2Int Position { get; set; }
        public GenomePropertyInstance PermanentModifier { get; set; }

        public PlantInstance(PlantData data, int maxGenomeCapacity) : base(data)
        {
            Genome = new GenomeContainer(maxGenomeCapacity);
            GrowthProgress = 0f;
            CurrentCell = null;
        }

        // Обёртки для методов Genome с передачей this
        public bool CanAddGenomeProperty(GenomePropertyInstance property) => Genome.CanAddProperty(property);
        public bool AddGenomeProperty(GenomePropertyInstance property) => Genome.AddProperty(property, this);
        public GenomePropertyInstance RemoveGenomeProperty(GenomePropertyData propertyData) => Genome.RemoveProperty(propertyData, this);
        public void ClearGenomeProperties() => Genome.Clear();
        public int GetGenomeFillPercent() => Genome.GetFillPercent();

        public void SetCurrentCell(Cell cell)
        {
            CurrentCell = cell;
        }
    }
}