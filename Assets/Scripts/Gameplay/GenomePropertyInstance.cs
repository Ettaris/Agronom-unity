using Data;

namespace Gameplay
{
    public class GenomePropertyInstance
    {
        public GenomePropertyData Data { get; }
        public int Stacks { get; set; }

        public GenomePropertyInstance(GenomePropertyData data, int stacks = 1)
        {
            Data = data;
            Stacks = stacks;
        }

        public int GetGenomeCost() => Data.genomeCost;
    }
}